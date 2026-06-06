namespace MDTracer
{
    //----------------------------------------------------------------
    // Sega cartridge memory mapper (the "SSF2" mapper).
    //
    // The 68000 can only see 4 MB of cartridge (0x000000-0x3FFFFF), but a few
    // titles ship larger ROMs (Super Street Fighter 2 is ~5 MB). Those carts
    // divide the 4 MB window into eight 512 KB banks and expose seven bank
    // registers at odd addresses 0xA130F3..0xA130FF; writing a physical page
    // number to a register swaps that 512 KB page into the corresponding bank.
    // Bank 0 (0x000000-0x07FFFF) is fixed to physical page 0.
    //
    //   0xA130F3 -> bank 1 (0x080000)   0xA130FB -> bank 5 (0x280000)
    //   0xA130F5 -> bank 2 (0x100000)   0xA130FD -> bank 6 (0x300000)
    //   0xA130F7 -> bank 3 (0x180000)   0xA130FF -> bank 7 (0x380000)
    //   0xA130F9 -> bank 4 (0x200000)
    //
    // In this emulator the 68000 reads ROM straight out of its flat memory
    // array, so a bank switch is realised by copying the selected 512 KB page
    // from the full ROM image into the bank's window. Reset reloads the ROM
    // linearly, which is exactly the identity mapping, so no work is needed
    // until a game first writes a bank register.
    //----------------------------------------------------------------
    internal class md_mapper
    {
        public const int BANK_SIZE = 0x80000;   // 512 KB per bank
        public const int BANK_COUNT = 8;

        // True only when the ROM is larger than the 4 MB window and therefore
        // actually needs banking; otherwise bank-register writes are ignored.
        public bool g_active;

        public void configure(md_cartridge in_cart)
        {
            g_active = (in_cart.g_file != null) && (in_cart.g_file_size > BANK_SIZE * BANK_COUNT);
        }

        // Handles a write to a 0xA130F3..0xA130FF bank register.
        public void write_control(uint in_address, byte in_data)
        {
            if (!g_active) return;
            if (in_address < 0xa130f3 || in_address > 0xa130ff) return;

            int w_bank = (int)((in_address - 0xa130f1) >> 1);   // F3->1 .. FF->7
            if (w_bank < 1 || w_bank >= BANK_COUNT) return;

            map_bank(md_main.g_md_cartridge.g_file, md_main.g_md_m68k.g_memory, w_bank, in_data);
        }

        // Copies physical 512 KB page <in_page> into bank <in_bank>'s window of
        // <in_mem>. Any part of the window past the end of the ROM is zero-filled.
        // Pure and side-effect-free apart from writing <in_mem>, so it is unit
        // testable without standing up the rest of the machine.
        public static void map_bank(byte[] in_rom, byte[] in_mem, int in_bank, int in_page)
        {
            int w_dst = in_bank * BANK_SIZE;
            int w_src = in_page * BANK_SIZE;

            int w_count = 0;
            if (w_src < in_rom.Length)
            {
                w_count = System.Math.Min(BANK_SIZE, in_rom.Length - w_src);
            }
            if (w_count > 0)
            {
                System.Array.Copy(in_rom, w_src, in_mem, w_dst, w_count);
            }
            if (w_count < BANK_SIZE)
            {
                System.Array.Clear(in_mem, w_dst + w_count, BANK_SIZE - w_count);
            }
        }
    }
}
