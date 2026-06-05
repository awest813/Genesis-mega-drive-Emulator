using System.Text;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// Tests for md_cartridge header-field parsing.
    ///
    /// The Genesis cartridge header lives at 0x100-0x1FF and is fixed-layout
    /// big-endian. CartridgeTests covers system-type validation and the low-level
    /// get_string/get_uint helpers; this suite verifies that load() decodes every
    /// individual header field at the correct offset.
    /// </summary>
    public class CartridgeHeaderTests
    {
        // Writes ASCII text into rom at offset, without a terminator (header fields
        // are fixed-width and space-padded, never null-terminated).
        private static void PutAscii(byte[] rom, int offset, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            Array.Copy(bytes, 0, rom, offset, bytes.Length);
        }

        private static void PutUint(byte[] rom, int offset, uint value)
        {
            rom[offset + 0] = (byte)(value >> 24);
            rom[offset + 1] = (byte)(value >> 16);
            rom[offset + 2] = (byte)(value >> 8);
            rom[offset + 3] = (byte)value;
        }

        // Builds a minimal-but-complete header with distinct, recognisable values
        // in every field so an offset mistake in the parser is easy to spot.
        private static byte[] BuildHeader()
        {
            byte[] rom = new byte[0x200];
            PutAscii(rom, 0x100, "SEGA MEGA DRIVE".PadRight(0x10)); // system type (0x100-0x10F)
            PutAscii(rom, 0x110, "(C)SEGA 1991.DEC");           // copyright   (0x110-0x11F)
            // Title fields are 48 bytes wide; space-pad so the unused tail is
            // 0x20 (trimmable) rather than 0x00 (which TrimEnd would not strip).
            PutAscii(rom, 0x120, "TEST GAME TITLE DOMESTIC".PadRight(0x30)); // title 1 (0x120-0x14F)
            PutAscii(rom, 0x150, "TEST GAME TITLE OVERSEAS".PadRight(0x30)); // title 2 (0x150-0x17F)
            PutAscii(rom, 0x180, "GM 00000000-00");             // serial      (0x180-0x18D), exactly 14 wide
            PutAscii(rom, 0x190, "J".PadRight(0x10));           // device sup  (0x190-0x19F)
            PutUint(rom, 0x1A0, 0x00000000);                    // rom start
            PutUint(rom, 0x1A4, 0x000FFFFF);                    // rom end
            PutUint(rom, 0x1A8, 0x00FF0000);                    // ram start
            PutUint(rom, 0x1AC, 0x00FFFFFF);                    // ram end
            rom[0x1B2] = 0x52;                                  // extra memory type ('R')
            PutUint(rom, 0x1B4, 0x00200000);                    // extra memory start
            PutUint(rom, 0x1B8, 0x0020FFFF);                    // extra memory end
            PutAscii(rom, 0x1F0, "JUE");                        // country     (0x1F0-0x1F2)
            return rom;
        }

        // Loads the given bytes through the real file path (load reads from disk),
        // returning the populated cartridge.
        private static MDTracer.md_cartridge LoadFromBytes(byte[] rom, string name)
        {
            var cart = new MDTracer.md_cartridge();
            string tmpFile = Path.Combine(Path.GetTempPath(), name);
            try
            {
                File.WriteAllBytes(tmpFile, rom);
                Assert.True(cart.load(tmpFile));
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
            return cart;
        }

        [Fact]
        public void Load_ParsesTextFields()
        {
            var cart = LoadFromBytes(BuildHeader(), "hdr_text.bin");

            Assert.Equal("SEGA MEGA DRIVE", cart.g_system_type);   // trimmed by load()
            Assert.Equal("(C)SEGA 1991.DEC", cart.g_copyright);
            Assert.Equal("TEST GAME TITLE DOMESTIC", cart.g_game_title1.TrimEnd());
            Assert.Equal("TEST GAME TITLE OVERSEAS", cart.g_game_title2.TrimEnd());
            Assert.Equal("GM 00000000-00", cart.g_serial_number);
            Assert.Equal("J", cart.g_device_support.TrimEnd());
            Assert.Equal("JUE", cart.g_country);
        }

        [Fact]
        public void Load_ParsesRomAndRamRanges()
        {
            var cart = LoadFromBytes(BuildHeader(), "hdr_ranges.bin");

            Assert.Equal(0x00000000u, cart.g_rom_start);
            Assert.Equal(0x000FFFFFu, cart.g_rom_end);
            Assert.Equal(0x00FF0000u, cart.g_ram_start);
            Assert.Equal(0x00FFFFFFu, cart.g_ram_end);
        }

        [Fact]
        public void Load_ParsesExtraMemoryFields()
        {
            var cart = LoadFromBytes(BuildHeader(), "hdr_extra.bin");

            Assert.Equal((byte)0x52, cart.g_extra_memory_type);
            Assert.Equal(0x00200000u, cart.g_extra_memory_start);
            Assert.Equal(0x0020FFFFu, cart.g_extra_memory_end);
        }

        [Fact]
        public void Load_FileExactlyAtHeaderBoundary_ReturnsFalse()
        {
            // load() requires more than 0x1F2 bytes so the country field is present.
            var cart = new MDTracer.md_cartridge();
            byte[] rom = new byte[0x1F2];
            PutAscii(rom, 0x100, "SEGA MEGA DRIVE ");

            string tmpFile = Path.Combine(Path.GetTempPath(), "hdr_boundary.bin");
            try
            {
                File.WriteAllBytes(tmpFile, rom);
                Assert.False(cart.load(tmpFile));
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        [Fact]
        public void GetString_StartBeyondFile_ReturnsEmpty()
        {
            var cart = new MDTracer.md_cartridge();
            cart.g_file = Encoding.ASCII.GetBytes("ABCD");
            cart.g_file_size = cart.g_file.Length;

            Assert.Equal("", cart.get_string(10, 12));
        }

        [Fact]
        public void GetString_EndBeyondFile_ClampsToAvailableBytes()
        {
            var cart = new MDTracer.md_cartridge();
            cart.g_file = Encoding.ASCII.GetBytes("ABCD");
            cart.g_file_size = cart.g_file.Length;

            // Requests 6 chars starting at index 2 but only "CD" exists.
            Assert.Equal("CD", cart.get_string(2, 7));
        }
    }
}
