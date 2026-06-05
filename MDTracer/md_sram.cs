using System.Diagnostics;
using System.IO;

namespace MDTracer
{
    //----------------------------------------------------------------
    // Battery-backed cartridge SRAM.
    //
    // Many Genesis carts include a small static RAM kept alive by a coin
    // battery, used for save games and high-score tables. It is wired into the
    // 68000 cartridge address space (the window is declared in the ROM header,
    // typically 0x200000-0x20FFFF) and, on most carts, gated by bit 0 of the
    // 0xA130F1 control register: when set, the window reads/writes SRAM; when
    // clear, it reads ROM.
    //
    // This implementation keeps a flat byte buffer indexed by (address - start).
    // That trivially round-trips both 16-bit SRAM and the common 8-bit-on-odd-
    // bytes wiring (the game only touches the addresses it owns), at the cost of
    // a buffer that mirrors the declared window size. Contents are persisted to
    // a ".srm" file sitting next to the ROM.
    //----------------------------------------------------------------
    internal class md_sram
    {
        // Largest window we are willing to back with a buffer. Real carts top
        // out at 64 KB; anything larger is treated as a malformed header.
        private const int MAX_SIZE = 0x20000;

        public bool g_present;   // cartridge declares a valid SRAM window
        public bool g_enabled;   // window currently maps SRAM (vs ROM)
        public uint g_start;     // first SRAM address (inclusive)
        public uint g_end;       // last SRAM address (inclusive)
        public byte[] g_data = System.Array.Empty<byte>();
        public bool g_dirty;     // unsaved writes pending

        private string g_path = "";

        private static void report(string in_message)
        {
            Debug.WriteLine("[SRAM] " + in_message);
        }

        // Resets to "no SRAM" without touching disk. Used when loading a cart
        // that has no battery backup.
        public void clear()
        {
            g_present = false;
            g_enabled = false;
            g_start = 0;
            g_end = 0;
            g_data = System.Array.Empty<byte>();
            g_dirty = false;
            g_path = "";
        }

        // Configures the SRAM window from the cartridge header and computes the
        // ".srm" path next to the ROM. Does not read the file (call load()).
        public void configure(md_cartridge in_cart, string in_rom_path)
        {
            clear();

            byte[] w_file = in_cart.g_file;
            if (w_file == null || in_cart.g_file_size <= 0x1bc) return;

            // The "RA" marker at 0x1B0 indicates external RAM is present.
            bool w_marker = (w_file[0x1b0] == (byte)'R') && (w_file[0x1b1] == (byte)'A');
            if (!w_marker) return;

            uint w_start = in_cart.g_extra_memory_start;
            uint w_end = in_cart.g_extra_memory_end;

            // Sanity-check the declared window: it must be a non-empty range that
            // lives inside the cartridge address space and is not absurdly large.
            if (w_end < w_start) return;
            if (w_start < 0x200000 || w_end > 0x3fffff) return;
            long w_size = (long)w_end - w_start + 1;
            if (w_size <= 0 || w_size > MAX_SIZE) return;

            g_present = true;
            g_enabled = true;          // lenient default: map SRAM so saves work
            g_start = w_start;
            g_end = w_end;
            g_data = new byte[w_size];
            g_path = Path.ChangeExtension(in_rom_path, ".srm");
        }

        // Updates the window-enable bit driven by writes to 0xA130F1.
        public void set_enabled(bool in_enabled)
        {
            if (g_present) g_enabled = in_enabled;
        }

        // True when the address should currently be serviced by SRAM rather than
        // ROM. Kept cheap; called on every cartridge-range bus access.
        public bool contains(uint in_address)
        {
            return g_present && g_enabled && (in_address >= g_start) && (in_address <= g_end);
        }

        //----------------------------------------------------------------
        // Byte/word/long access. Word and long accesses are big-endian, matching
        // the 68000 bus. Index is bounds-checked so a straddling access near the
        // end of the window cannot throw.
        //----------------------------------------------------------------
        public byte read8(uint in_address)
        {
            int w_index = (int)(in_address - g_start);
            if ((uint)w_index >= (uint)g_data.Length) return 0;
            return g_data[w_index];
        }

        public ushort read16(uint in_address)
        {
            return (ushort)((read8(in_address) << 8) | read8(in_address + 1));
        }

        public uint read32(uint in_address)
        {
            return (uint)((read8(in_address) << 24)
                        | (read8(in_address + 1) << 16)
                        | (read8(in_address + 2) << 8)
                        | read8(in_address + 3));
        }

        public void write8(uint in_address, byte in_data)
        {
            int w_index = (int)(in_address - g_start);
            if ((uint)w_index >= (uint)g_data.Length) return;
            if (g_data[w_index] != in_data)
            {
                g_data[w_index] = in_data;
                g_dirty = true;
            }
        }

        public void write16(uint in_address, ushort in_data)
        {
            write8(in_address, (byte)(in_data >> 8));
            write8(in_address + 1, (byte)in_data);
        }

        public void write32(uint in_address, uint in_data)
        {
            write8(in_address, (byte)(in_data >> 24));
            write8(in_address + 1, (byte)(in_data >> 16));
            write8(in_address + 2, (byte)(in_data >> 8));
            write8(in_address + 3, (byte)in_data);
        }

        //----------------------------------------------------------------
        // Persistence
        //----------------------------------------------------------------
        public void load()
        {
            if (!g_present || g_path.Length == 0) return;
            try
            {
                if (!File.Exists(g_path)) return;
                byte[] w_saved = File.ReadAllBytes(g_path);
                int w_len = System.Math.Min(w_saved.Length, g_data.Length);
                System.Array.Copy(w_saved, g_data, w_len);
                g_dirty = false;
            }
            catch (IOException ex)
            {
                report("load failed: " + ex.Message);
            }
        }

        // Writes the buffer back to disk if there are unsaved changes.
        public void save()
        {
            if (!g_present || g_path.Length == 0 || !g_dirty) return;
            try
            {
                File.WriteAllBytes(g_path, g_data);
                g_dirty = false;
            }
            catch (IOException ex)
            {
                report("save failed: " + ex.Message);
            }
        }
    }
}
