using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// Tests for the MC68000 memory subsystem (md_m68k_memory).
    ///
    /// The 68000 sees a flat 16 MB address space backed by a single byte array.
    /// These tests pin down the three behaviours that subtle ROM bugs tend to hide in:
    ///   * big-endian byte ordering for 16/32-bit accesses,
    ///   * 24-bit address masking (the 68000 only drives A0-A23), and
    ///   * mirroring of the upper address range onto the 64 KB work RAM at 0xFF0000.
    /// </summary>
    public class M68kMemoryTests
    {
        // The constructor only builds the opcode/flag tables and allocates the
        // 16 MB memory array; it does not touch any static md_main/UI state, so a
        // bare instance is safe to use directly in a unit test.
        private static MDTracer.md_m68k NewCpu() => new MDTracer.md_m68k();

        [Fact]
        public void Write16_StoresBigEndian()
        {
            var cpu = NewCpu();
            cpu.write16(0x000100, 0x1234);

            Assert.Equal(0x12, cpu.read8(0x000100));
            Assert.Equal(0x34, cpu.read8(0x000101));
        }

        [Fact]
        public void Write32_StoresBigEndian()
        {
            var cpu = NewCpu();
            cpu.write32(0x000200, 0xDEADBEEF);

            Assert.Equal(0xDE, cpu.read8(0x000200));
            Assert.Equal(0xAD, cpu.read8(0x000201));
            Assert.Equal(0xBE, cpu.read8(0x000202));
            Assert.Equal(0xEF, cpu.read8(0x000203));
        }

        [Fact]
        public void Read16_AssemblesBigEndian()
        {
            var cpu = NewCpu();
            cpu.write8(0x000300, 0xAB);
            cpu.write8(0x000301, 0xCD);

            Assert.Equal(0xABCD, cpu.read16(0x000300));
        }

        [Fact]
        public void Read32_AssemblesBigEndian()
        {
            var cpu = NewCpu();
            cpu.write8(0x000400, 0x01);
            cpu.write8(0x000401, 0x02);
            cpu.write8(0x000402, 0x03);
            cpu.write8(0x000403, 0x04);

            Assert.Equal(0x01020304u, cpu.read32(0x000400));
        }

        [Theory]
        [InlineData(0x0000FFu)]
        [InlineData(0x00FFFFu)]
        [InlineData(0xFFFFFFu)]
        public void ReadWrite8_RoundTrips(uint address)
        {
            var cpu = NewCpu();
            cpu.write8(address, 0x5A);
            Assert.Equal(0x5A, cpu.read8(address));
        }

        [Fact]
        public void Address_MaskedTo24Bits()
        {
            var cpu = NewCpu();
            // The high byte (0x01) is outside A0-A23 and must be ignored, so this
            // aliases to 0x000100.
            cpu.write8(0x01000100, 0xAB);

            Assert.Equal(0xAB, cpu.read8(0x000100));
        }

        [Fact]
        public void UpperRange_MirrorsOntoWorkRam()
        {
            var cpu = NewCpu();
            // 0xFF0010 is the canonical work-RAM address.
            cpu.write8(0xFF0010, 0x55);

            // 0xE00000 and above all fold to (addr & 0xFFFF) | 0xFF0000, so these
            // read back the value written at 0xFF0010.
            Assert.Equal(0x55, cpu.read8(0xE00010));
            Assert.Equal(0x55, cpu.read8(0xFE0010));
            Assert.Equal(0x55, cpu.read8(0xFF0010));
        }

        [Fact]
        public void WorkRam_WriteThroughMirror_VisibleAtCanonicalAddress()
        {
            var cpu = NewCpu();
            // Writing through a mirror must update the same backing byte.
            cpu.write16(0xE00020, 0xBEEF);

            Assert.Equal(0xBEEF, cpu.read16(0xFF0020));
        }
    }
}
