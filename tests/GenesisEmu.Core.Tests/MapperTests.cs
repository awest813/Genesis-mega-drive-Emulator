using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// Tests for the Sega cartridge memory mapper (md_mapper): activation based
    /// on ROM size and the page-into-bank copy that realises a bank switch.
    /// </summary>
    public class MapperTests
    {
        private const int BANK = 0x80000;   // md_mapper.BANK_SIZE

        // ROM where every 512 KB page is filled with its own page index, so a
        // mapped bank's contents identify which physical page landed there.
        private static byte[] PagedRom(int pages)
        {
            byte[] rom = new byte[pages * BANK];
            for (int i = 0; i < rom.Length; i++) rom[i] = (byte)(i / BANK);
            return rom;
        }

        [Fact]
        public void Configure_SmallRom_Inactive()
        {
            var cart = new md_cartridge { g_file = new byte[0x10], g_file_size = 0x400000 };
            var mapper = new md_mapper();
            mapper.configure(cart);
            Assert.False(mapper.g_active);   // exactly 4 MB needs no banking
        }

        [Fact]
        public void Configure_LargeRom_Active()
        {
            var cart = new md_cartridge { g_file = new byte[0x10], g_file_size = 0x400001 };
            var mapper = new md_mapper();
            mapper.configure(cart);
            Assert.True(mapper.g_active);
        }

        [Fact]
        public void MapBank_CopiesSelectedPageIntoWindow()
        {
            byte[] rom = PagedRom(10);             // 5 MB, pages 0..9
            byte[] mem = new byte[0x400000];

            md_mapper.map_bank(rom, mem, 1, 8);    // page 8 -> bank 1 (0x080000)

            Assert.Equal(8, mem[0x080000]);
            Assert.Equal(8, mem[0x080000 + BANK / 2]);
            Assert.Equal(8, mem[0x080000 + BANK - 1]);
            // Neighbouring banks are untouched.
            Assert.Equal(0, mem[0x100000]);
        }

        [Fact]
        public void MapBank_PagePartlyBeyondRom_ZeroFillsTail()
        {
            // ROM ends 0x100 bytes into page 9.
            byte[] rom = new byte[9 * BANK + 0x100];
            for (int i = 0; i < rom.Length; i++) rom[i] = 0x5A;
            byte[] mem = new byte[0x400000];

            md_mapper.map_bank(rom, mem, 2, 9);    // page 9 -> bank 2 (0x100000)

            Assert.Equal(0x5A, mem[0x100000]);            // copied region
            Assert.Equal(0x5A, mem[0x100000 + 0xFF]);
            Assert.Equal(0, mem[0x100000 + 0x100]);       // zero-filled tail
            Assert.Equal(0, mem[0x100000 + BANK - 1]);
        }

        [Fact]
        public void MapBank_PageBeyondRom_ClearsWindow()
        {
            byte[] rom = new byte[BANK];                  // single page
            byte[] mem = new byte[0x400000];
            for (int i = 0; i < BANK; i++) mem[0x180000 + i] = 0xFF;   // dirty the window

            md_mapper.map_bank(rom, mem, 3, 200);         // page far past ROM end

            Assert.Equal(0, mem[0x180000]);
            Assert.Equal(0, mem[0x180000 + BANK - 1]);
        }
    }
}
