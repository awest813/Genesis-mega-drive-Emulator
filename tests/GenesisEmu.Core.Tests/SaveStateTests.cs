using System.IO;
using System.Text;
using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// Save-state v6 round-trip: CPU PC, mapper bank pages, and SRAM contents.
    /// </summary>
    public class SaveStateTests
    {
        private const int BANK = md_mapper.BANK_SIZE;
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes("MDT68KST");

        private static byte[] PagedRom(int in_pages)
        {
            byte[] rom = new byte[in_pages * BANK];
            for (int i = 0; i < rom.Length; i++) rom[i] = (byte)(i / BANK);
            return rom;
        }

        private static void SetupMachine(byte[] in_rom)
        {
            md_main.initialize();
            md_main.g_md_cartridge.g_file = in_rom;
            md_main.g_md_cartridge.g_file_size = in_rom.Length;
            md_main.g_md_sram.g_present = true;
            md_main.g_md_sram.g_enabled = true;
            md_main.g_md_sram.g_start = 0x200000;
            md_main.g_md_sram.g_end = 0x2003FF;
            md_main.g_md_sram.g_data = new byte[0x400];
            md_main.g_md_sram.write8(0x200000, 0xBE);
            md_main.g_md_sram.write8(0x200001, 0xEF);
            md_main.g_md_mapper.configure(md_main.g_md_cartridge);
            md_main.g_md_m68k.reset();
            md_main.g_md_control.write8(0xA130F3, 9);
            md_main.g_md_m68k.g_reg_PC = 0x080456;
        }

        private static byte[] CaptureStateV6()
        {
            using var w_stream = new MemoryStream();
            using var w_writer = new BinaryWriter(w_stream);
            w_writer.Write(MagicBytes);
            w_writer.Write(6);
            md_main.g_md_m68k.write_state(w_writer);
            md_main.g_md_vdp.write_state(w_writer);
            md_main.g_md_music.write_state(w_writer);
            md_main.g_md_z80.write_state(w_writer);
            md_main.g_md_sram.write_state(w_writer);
            md_main.g_md_mapper.write_state(w_writer);
            return w_stream.ToArray();
        }

        private static void RestoreStateV6(byte[] in_blob)
        {
            using var w_reader = new BinaryReader(new MemoryStream(in_blob));
            byte[] w_magic = w_reader.ReadBytes(MagicBytes.Length);
            Assert.True(w_magic.AsSpan().SequenceEqual(MagicBytes));
            int w_version = w_reader.ReadInt32();
            Assert.Equal(6, w_version);

            md_main.g_md_m68k.restore_state(w_reader, w_version >= 4);
            md_main.g_md_vdp.restore_state(w_reader);
            md_main.g_md_music.restore_state(w_reader);
            md_main.g_md_z80.restore_state(w_reader);
            md_main.g_md_sram.restore_state(w_reader);
            md_main.g_md_mapper.restore_state(w_reader);
        }

        [Fact]
        public void StateV6_RoundTrip_PreservesPcMapperAndSram()
        {
            SetupMachine(PagedRom(10));
            byte[] w_blob = CaptureStateV6();

            md_main.g_md_m68k.g_reg_PC = 0;
            md_main.g_md_mapper.reset();
            md_main.g_md_m68k.reset();
            md_main.g_md_sram.write8(0x200000, 0);

            RestoreStateV6(w_blob);

            Assert.Equal(0x080456u, md_main.g_md_m68k.g_reg_PC);
            Assert.Equal((byte)9, md_main.g_md_mapper.g_bank_pages[1]);
            Assert.Equal(9, md_main.g_md_bus.read8(0x080000));
            Assert.Equal(0xBE, md_main.g_md_sram.read8(0x200000));
            Assert.Equal(0xEF, md_main.g_md_sram.read8(0x200001));
        }
    }
}
