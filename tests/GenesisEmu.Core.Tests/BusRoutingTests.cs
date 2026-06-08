using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// Integration tests for md_bus cartridge-range routing: SRAM gate and mapper banks.
    /// </summary>
    public class BusRoutingTests
    {
        private const int BANK = md_mapper.BANK_SIZE;

        private static byte[] PagedRom(int in_pages)
        {
            byte[] rom = new byte[in_pages * BANK];
            for (int i = 0; i < rom.Length; i++) rom[i] = (byte)(i / BANK);
            return rom;
        }

        private static void SetupMachine(byte[] in_rom)
        {
            md_main.g_md_bus = new md_bus();
            md_main.g_md_m68k = new md_m68k();
            md_main.g_md_m68k.initialize();
            md_main.g_md_m68k.g_bus = md_main.g_md_bus;
            md_main.g_md_cartridge = new md_cartridge
            {
                g_file = in_rom,
                g_file_size = in_rom.Length
            };
            md_main.g_md_sram = new md_sram
            {
                g_present = true,
                g_enabled = true,
                g_start = 0x200000,
                g_end = 0x2003FF,
                g_data = new byte[0x400]
            };
            md_main.g_md_mapper = new md_mapper();
            md_main.g_md_mapper.configure(md_main.g_md_cartridge);
            md_main.g_md_control = new md_control();
            md_main.g_md_z80 = new md_z80();
            md_main.g_md_m68k.reset();
        }

        [Fact]
        public void Read8_MapperBankSwitch_ReturnsMappedPageThroughBus()
        {
            SetupMachine(PagedRom(10));
            md_main.g_md_control.write8(0xA130F3, 9);

            Assert.Equal(9, md_main.g_md_bus.read8(0x080000));
            Assert.Equal(9, md_main.g_md_bus.read8(0x080000 + BANK / 2));
        }

        [Fact]
        public void Read16_SramGateEnabled_ReturnsSramNotRomMirror()
        {
            SetupMachine(PagedRom(4));
            md_main.g_md_m68k.g_memory[0x200000] = 0x11;
            md_main.g_md_m68k.g_memory[0x200001] = 0x22;
            md_main.g_md_sram.write16(0x200000, 0xAABB);

            Assert.Equal((ushort)0xAABB, md_main.g_md_bus.read16(0x200000));
        }

        [Fact]
        public void Read16_SramGateDisabled_ReturnsRomMirror()
        {
            SetupMachine(PagedRom(4));
            md_main.g_md_m68k.g_memory[0x200000] = 0x11;
            md_main.g_md_m68k.g_memory[0x200001] = 0x22;
            md_main.g_md_sram.write16(0x200000, 0xAABB);
            md_main.g_md_sram.set_enabled(false);

            Assert.Equal((ushort)0x1122, md_main.g_md_bus.read16(0x200000));
        }

        [Fact]
        public void Write8_SramGateEnabled_UpdatesSramBuffer()
        {
            SetupMachine(PagedRom(4));
            md_main.g_md_bus.write8(0x200100, 0xCD);

            Assert.Equal(0xCD, md_main.g_md_sram.read8(0x200100));
            Assert.True(md_main.g_md_sram.g_dirty);
        }
    }
}
