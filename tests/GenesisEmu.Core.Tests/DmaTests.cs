using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// VDP DMA: instant-transfer completion must not leave a post-hoc CPU stall,
    /// and memory-to-VRAM DMA must read through the system bus (SRAM gate).
    /// </summary>
    public class DmaTests
    {
        private const uint DATA = 0xC00000;
        private const uint CTRL = 0xC00004;

        private static void SetupBusMachine(uint in_sramAddress = 0x200000)
        {
            md_main.g_md_bus = new md_bus();
            md_main.g_md_m68k = new md_m68k();
            md_main.g_md_m68k.initialize();
            md_main.g_md_m68k.g_bus = md_main.g_md_bus;
            md_main.g_md_cartridge = new md_cartridge
            {
                g_file = new byte[0x300000],
                g_file_size = 0x300000
            };
            md_main.g_md_sram = new md_sram
            {
                g_present = true,
                g_enabled = true,
                g_start = in_sramAddress,
                g_end = in_sramAddress + 0x3FF,
                g_data = new byte[0x400]
            };
            md_main.g_md_mapper = new md_mapper();
            md_main.g_md_control = new md_control();
            md_main.g_md_z80 = new md_z80();
        }

        // Issues a memory-to-VRAM DMA (mode 0) for <in_wordCount> words from
        // <in_srcByteAddress> into VRAM address <in_vramDest>.
        private static void TriggerMemoryDma(md_vdp in_vdp, uint in_srcByteAddress, uint in_vramDest, int in_wordCount)
        {
            in_vdp.g_vdp_reg_1_4_dma = 1;
            in_vdp.g_vdp_reg_15_autoinc = 2;
            in_vdp.g_vdp_reg_23_dma_mode = 0;
            uint w_srcWord = in_srcByteAddress >> 1;
            in_vdp.g_vdp_reg_21_dma_source_low = (byte)(w_srcWord & 0xff);
            in_vdp.g_vdp_reg_22_dma_source_mid = (byte)((w_srcWord >> 8) & 0xff);
            in_vdp.g_vdp_reg_23_5_dma_high = (byte)((w_srcWord >> 16) & 0x7f);
            in_vdp.g_vdp_reg_19_dma_counter_low = (byte)(in_wordCount & 0xff);
            in_vdp.g_vdp_reg_20_dma_counter_high = (byte)((in_wordCount >> 8) & 0xff);

            // Transfer code 0x21 = VRAM write with DMA bit set.
            ushort w1 = (ushort)((1 << 14) | (in_vramDest & 0x3FFF));
            ushort w2 = (ushort)((8 << 4) | ((in_vramDest >> 14) & 0x03));
            in_vdp.write16(CTRL, w1);
            in_vdp.write16(CTRL, w2);
        }

        private static ushort ReadVramWord(md_vdp in_vdp, uint in_address)
        {
            ushort w1 = (ushort)((0 << 14) | (in_address & 0x3FFF));
            ushort w2 = (ushort)((in_address >> 14) & 0x03);
            in_vdp.write16(CTRL, w1);
            in_vdp.write16(CTRL, w2);
            return in_vdp.read16(DATA);
        }

        [Fact]
        public void MemoryDma_CompletesWithoutPostTransferCpuStall()
        {
            SetupBusMachine();
            const uint w_src = 0x000100;
            md_main.g_md_m68k.g_memory[w_src] = 0xDE;
            md_main.g_md_m68k.g_memory[w_src + 1] = 0xAD;

            var w_vdp = new md_vdp();
            TriggerMemoryDma(w_vdp, w_src, 0x0000, 1);

            Assert.Equal((byte)0, w_vdp.g_vdp_status_1_dma);
            Assert.Equal(0, w_vdp.dma_status_update());
            Assert.Equal((ushort)0xDEAD, ReadVramWord(w_vdp, 0x0000));
        }

        [Fact]
        public void MemoryDma_ReadsSramThroughBusWhenGateEnabled()
        {
            const uint w_sramAddr = 0x200000;
            SetupBusMachine(w_sramAddr);
            md_main.g_md_m68k.g_memory[w_sramAddr] = 0x11;
            md_main.g_md_m68k.g_memory[w_sramAddr + 1] = 0x22;
            md_main.g_md_sram.write16(w_sramAddr, 0xAABB);

            var w_vdp = new md_vdp();
            TriggerMemoryDma(w_vdp, w_sramAddr, 0x0100, 1);

            Assert.Equal((ushort)0xAABB, ReadVramWord(w_vdp, 0x0100));
        }

        [Fact]
        public void MemoryDma_WrapsSourceWithin128KWindow()
        {
            SetupBusMachine();
            const uint w_near_end = 0x1FFFE;
            md_main.g_md_m68k.g_memory[w_near_end] = 0xBE;
            md_main.g_md_m68k.g_memory[w_near_end + 1] = 0xEF;
            md_main.g_md_m68k.g_memory[0] = 0xCA;
            md_main.g_md_m68k.g_memory[1] = 0xFE;

            var w_vdp = new md_vdp();
            TriggerMemoryDma(w_vdp, w_near_end, 0x0000, 2);

            Assert.Equal((ushort)0xBEEF, ReadVramWord(w_vdp, 0x0000));
            Assert.Equal((ushort)0xCAFE, ReadVramWord(w_vdp, 0x0002));
        }

        [Fact]
        public void MemoryDma_ReadsRomMirrorWhenSramGateDisabled()
        {
            const uint w_sramAddr = 0x200000;
            SetupBusMachine(w_sramAddr);
            md_main.g_md_m68k.g_memory[w_sramAddr] = 0x11;
            md_main.g_md_m68k.g_memory[w_sramAddr + 1] = 0x22;
            md_main.g_md_sram.write16(w_sramAddr, 0xAABB);
            md_main.g_md_sram.set_enabled(false);

            var w_vdp = new md_vdp();
            TriggerMemoryDma(w_vdp, w_sramAddr, 0x0100, 1);

            Assert.Equal((ushort)0x1122, ReadVramWord(w_vdp, 0x0100));
        }
    }
}
