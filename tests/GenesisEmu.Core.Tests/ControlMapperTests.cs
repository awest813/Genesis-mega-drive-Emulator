using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    public class ControlMapperTests
    {
        private static void SetupMachine(byte[] rom)
        {
            md_main.g_md_cartridge = new md_cartridge
            {
                g_file = rom,
                g_file_size = rom.Length
            };
            md_main.g_md_m68k = new md_m68k();
            md_main.g_md_sram = new md_sram
            {
                g_present = true,
                g_enabled = true,
                g_start = 0x200000,
                g_end = 0x20FFFF,
                g_data = new byte[0x10000]
            };
            md_main.g_md_mapper = new md_mapper();
            md_main.g_md_mapper.configure(md_main.g_md_cartridge);
            md_main.g_md_control = new md_control();
            md_main.g_md_z80 = new md_z80();
            md_main.g_md_m68k.reset();
        }

        [Fact]
        public void Write16_A130F0_UpdatesSramGateAndMapperBank()
        {
            byte[] rom = new byte[10 * md_mapper.BANK_SIZE];
            for (int i = 0; i < rom.Length; i++) rom[i] = (byte)(i / md_mapper.BANK_SIZE);
            SetupMachine(rom);

            md_main.g_md_control.write16(0xA130F0, 0x0008); // low byte -> A130F1, bank1 -> page8

            Assert.False(md_main.g_md_sram.g_enabled);
            Assert.Equal((byte)8, md_main.g_md_mapper.g_bank_pages[1]);
            Assert.Equal(8, md_main.g_md_m68k.g_memory[0x080000]);
        }

        [Fact]
        public void Write16_A130F2_UpdatesBank1()
        {
            byte[] rom = new byte[10 * md_mapper.BANK_SIZE];
            for (int i = 0; i < rom.Length; i++) rom[i] = (byte)(i / md_mapper.BANK_SIZE);
            SetupMachine(rom);

            md_main.g_md_control.write16(0xA130F2, 0x0007); // low byte -> A130F3 maps bank1 page7

            Assert.Equal((byte)7, md_main.g_md_mapper.g_bank_pages[1]);
            Assert.Equal(7, md_main.g_md_m68k.g_memory[0x080000]);
        }
    }
}
