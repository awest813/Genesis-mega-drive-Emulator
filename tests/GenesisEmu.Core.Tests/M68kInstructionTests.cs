using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// Instruction-level tests for the MC68000 core.
    ///
    /// These exercise real opcode handlers end to end via md_m68k.step(), which
    /// fetches/decodes/executes a single instruction from the CPU's own memory
    /// without the tracer, VDP or DMA machinery of the full run() loop.
    ///
    /// Memory operands route through the injected IM68kBus seam; the test wires in
    /// <see cref="FlatMemoryBus"/>, which simply forwards to the CPU's flat 16 MB
    /// memory. This is what the bus/tracer decoupling was for: no md_main, no UI.
    /// </summary>
    public class M68kInstructionTests
    {
        // Minimal bus that maps every access straight onto the CPU's flat memory
        // (matching how md_bus routes ROM/RAM ranges back to the 68000 in the real
        // machine), so operand and immediate accesses work without a full bus.
        private sealed class FlatMemoryBus : IM68kBus
        {
            private readonly md_m68k _cpu;
            public FlatMemoryBus(md_m68k cpu) => _cpu = cpu;
            public byte read8(uint a) => _cpu.read8(a);
            public ushort read16(uint a) => _cpu.read16(a);
            public uint read32(uint a) => _cpu.read32(a);
            public void write8(uint a, byte v) => _cpu.write8(a, v);
            public void write16(uint a, ushort v) => _cpu.write16(a, v);
            public void write32(uint a, uint v) => _cpu.write32(a, v);
        }

        private const uint CodeBase = 0x000400;

        private static md_m68k NewCpu()
        {
            var cpu = new md_m68k();
            cpu.g_bus = new FlatMemoryBus(cpu);
            return cpu;
        }

        // Places a single opcode word at CodeBase, points PC at it, and executes it.
        private static void Exec(md_m68k cpu, ushort opcode)
        {
            cpu.g_reg_PC = CodeBase;
            cpu.write16(CodeBase, opcode);
            cpu.step();
        }

        // ---- MOVEQ (no operand fetch, no bus) ---------------------------------

        [Fact]
        public void Nop_AdvancesPcAndReportsCycles()
        {
            var cpu = NewCpu();
            cpu.g_reg_PC = CodeBase;
            cpu.write16(CodeBase, 0x4E71); // NOP

            int cycles = cpu.step();

            Assert.Equal(4, cycles);
            Assert.Equal(CodeBase + 2, cpu.g_reg_PC);
        }

        [Fact]
        public void Moveq_SignExtendsNegativeImmediate()
        {
            var cpu = NewCpu();
            Exec(cpu, 0x76FF);  // MOVEQ #-1, D3

            Assert.Equal(0xFFFFFFFFu, cpu.g_reg_data[3].l);
            Assert.True(cpu.g_status_N);
            Assert.False(cpu.g_status_Z);
            Assert.False(cpu.g_status_V);
            Assert.False(cpu.g_status_C);
        }

        [Fact]
        public void Moveq_Zero_SetsZeroFlag()
        {
            var cpu = NewCpu();
            Exec(cpu, 0x7600);  // MOVEQ #0, D3

            Assert.Equal(0x00000000u, cpu.g_reg_data[3].l);
            Assert.False(cpu.g_status_N);
            Assert.True(cpu.g_status_Z);
        }

        [Fact]
        public void Moveq_PositiveImmediate_ZeroExtends()
        {
            var cpu = NewCpu();
            Exec(cpu, 0x787F);  // MOVEQ #0x7F, D4

            Assert.Equal(0x0000007Fu, cpu.g_reg_data[4].l);
            Assert.False(cpu.g_status_N);
            Assert.False(cpu.g_status_Z);
        }

        // ---- ADD.W Dn,Dn (register direct, no bus) ----------------------------

        [Fact]
        public void AddW_RegToReg_Adds()
        {
            var cpu = NewCpu();
            cpu.g_reg_data[0].l = 0x00000005;
            cpu.g_reg_data[1].l = 0x00000003;
            Exec(cpu, 0xD041);  // ADD.W D1, D0

            Assert.Equal((ushort)0x0008, cpu.g_reg_data[0].w);
            Assert.False(cpu.g_status_Z);
            Assert.False(cpu.g_status_C);
            Assert.False(cpu.g_status_V);
        }

        [Fact]
        public void AddW_Overflow_SetsCarryZeroAndExtend()
        {
            var cpu = NewCpu();
            cpu.g_reg_data[0].l = 0x0000FFFF;
            cpu.g_reg_data[1].l = 0x00000001;
            Exec(cpu, 0xD041);  // ADD.W D1, D0

            Assert.Equal((ushort)0x0000, cpu.g_reg_data[0].w);
            Assert.True(cpu.g_status_Z);
            Assert.True(cpu.g_status_C);
            Assert.True(cpu.g_status_X);
            Assert.False(cpu.g_status_V);
        }

        // ---- SUB.W / AND.W Dn,Dn ---------------------------------------------

        [Fact]
        public void SubW_RegToReg_Subtracts()
        {
            var cpu = NewCpu();
            cpu.g_reg_data[0].l = 0x00000008;
            cpu.g_reg_data[1].l = 0x00000003;
            Exec(cpu, 0x9041);  // SUB.W D1, D0

            Assert.Equal((ushort)0x0005, cpu.g_reg_data[0].w);
            Assert.False(cpu.g_status_Z);
            Assert.False(cpu.g_status_C);
        }

        [Fact]
        public void AndW_RegToReg_MasksBits()
        {
            var cpu = NewCpu();
            cpu.g_reg_data[0].l = 0x000000FF;
            cpu.g_reg_data[1].l = 0x00000F0F;
            Exec(cpu, 0xC041);  // AND.W D1, D0

            Assert.Equal((ushort)0x000F, cpu.g_reg_data[0].w);
            Assert.False(cpu.g_status_Z);
            Assert.False(cpu.g_status_V);
            Assert.False(cpu.g_status_C);
        }

        // ---- SWAP Dn ----------------------------------------------------------

        [Fact]
        public void Swap_ExchangesWordHalves()
        {
            var cpu = NewCpu();
            cpu.g_reg_data[2].l = 0x12345678;
            Exec(cpu, 0x4842);  // SWAP D2

            Assert.Equal(0x56781234u, cpu.g_reg_data[2].l);
            Assert.False(cpu.g_status_N);
        }

        // ---- MOVE.W through the injected bus (memory operands) ----------------

        [Fact]
        public void MoveW_RegToMemory_WritesThroughBus()
        {
            var cpu = NewCpu();
            cpu.g_reg_addr[0].l = 0x00FF1000;   // A0 -> work RAM
            cpu.g_reg_data[1].w = 0x1234;       // D1
            Exec(cpu, 0x3081);  // MOVE.W D1, (A0)

            // The store routed through FlatMemoryBus into the CPU's memory.
            Assert.Equal((ushort)0x1234, cpu.read16(0x00FF1000));
            Assert.False(cpu.g_status_Z);
        }

        [Fact]
        public void MoveW_MemoryToReg_ReadsThroughBus()
        {
            var cpu = NewCpu();
            cpu.write16(0x00FF2000, 0xABCD);
            cpu.g_reg_addr[0].l = 0x00FF2000;   // A0 -> work RAM
            Exec(cpu, 0x3010);  // MOVE.W (A0), D0

            Assert.Equal((ushort)0xABCD, cpu.g_reg_data[0].w);
            Assert.True(cpu.g_status_N);        // 0xABCD has bit 15 set
        }
    }
}
