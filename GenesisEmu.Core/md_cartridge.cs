using System.IO.Compression;
using System.Diagnostics;
using System.Text;

namespace MDTracer
{
    internal class md_cartridge
    {
        public byte[] g_file;            //ROM File data
        public int g_file_size;          //ROM File data size
        public string g_system_type;
        public string g_copyright;
        public string g_game_title1;
        public string g_game_title2;
        public string g_serial_number;
        public string g_device_support;
        public uint g_rom_start;
        public uint g_rom_end;
        public uint g_ram_start;
        public uint g_ram_end;
        public byte g_extra_memory_type;
        public uint g_extra_memory_start;
        public uint g_extra_memory_end;
        public string g_country;
        public bool g_pal_inferred;

        private static bool LooksLikeGenesis(byte[] in_data)
        {
            if (in_data == null || in_data.Length < 0x104) return false;
            return in_data[0x100] == (byte)'S'
                && in_data[0x101] == (byte)'E'
                && in_data[0x102] == (byte)'G'
                && in_data[0x103] == (byte)'A';
        }

        // SMD interleave format support adapted from Sandopolis (MIT License).
        private static byte[]? TryDeinterleaveSmd(byte[] in_raw, int in_payloadOffset)
        {
            const int w_block_size = 16 * 1024;
            if (in_payloadOffset < 0 || in_payloadOffset >= in_raw.Length) return null;

            int w_payload_len = in_raw.Length - in_payloadOffset;
            if (w_payload_len < w_block_size) return null;

            int w_full_blocks = w_payload_len / w_block_size;
            int w_remainder = w_payload_len % w_block_size;
            int w_out_len = w_full_blocks * w_block_size + ((w_remainder > 0) ? w_block_size : 0);
            byte[] w_out = new byte[w_out_len];

            int w_src = in_payloadOffset;
            int w_dst = 0;
            for (int w_block = 0; w_block < w_full_blocks; w_block++)
            {
                int w_half = w_block_size / 2;
                for (int i = 0; i < w_half; i++)
                {
                    w_out[w_dst + i * 2] = in_raw[w_src + i];
                    w_out[w_dst + i * 2 + 1] = in_raw[w_src + i + w_half];
                }
                w_src += w_block_size;
                w_dst += w_block_size;
            }

            if (w_remainder > 0)
            {
                byte[] w_padded = new byte[w_block_size];
                Array.Copy(in_raw, w_src, w_padded, 0, w_remainder);
                int w_half = w_block_size / 2;
                for (int i = 0; i < w_half; i++)
                {
                    w_out[w_dst + i * 2] = w_padded[i];
                    w_out[w_dst + i * 2 + 1] = w_padded[i + w_half];
                }
            }

            return w_out;
        }

        private static byte[] NormalizeRomBytes(byte[] in_raw)
        {
            if (LooksLikeGenesis(in_raw)) return in_raw;

            if (in_raw.Length > 512)
            {
                byte[]? w_smd = TryDeinterleaveSmd(in_raw, 512);
                if (w_smd != null && LooksLikeGenesis(w_smd)) return w_smd;
            }

            byte[]? w_plain = TryDeinterleaveSmd(in_raw, 0);
            if (w_plain != null && LooksLikeGenesis(w_plain)) return w_plain;

            return in_raw;
        }

        public bool load(string in_romname)
        {
            try
            {
                g_file = NormalizeRomBytes(File.ReadAllBytes(in_romname));
                g_file_size = g_file.Length;
            }
            catch (IOException ex)
            {
                Debug.WriteLine("[Cartridge] ROM load failed: " + ex.Message);
                return false;
            }
            if (g_file_size < 4) return false;
            if ((g_file[0] == 0x50) && (g_file[1] == 0x4b) && (g_file[2] == 0x03) && (g_file[3] == 0x04))
            {
                using (FileStream fileStream = new FileStream(in_romname, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Length == 0) continue;

                        using Stream zipEntryStream = entry.Open();
                        using MemoryStream memoryStream = new MemoryStream((int)entry.Length);
                        zipEntryStream.CopyTo(memoryStream);
                        g_file = NormalizeRomBytes(memoryStream.ToArray());
                        g_file_size = g_file.Length;
                        break;
                    }
                }
            }
            if (g_file_size <= 0x1f2) return false;

            g_system_type = get_string(0x100, 0x10f).Trim();
            if ((g_system_type != "SEGA MEGA DRIVE") &&
                (g_system_type != "SEGA GENESIS"))
            {
                return false;
            }
            g_copyright = get_string(0x110, 0x11f);
            g_game_title1 = get_string(0x120, 0x14f);
            g_game_title2 = get_string(0x150, 0x17f);
            g_serial_number = get_string(0x180, 0x18d);
            g_device_support = get_string(0x190, 0x19f);
            g_rom_start = get_uint(0x1a0);
            g_rom_end = get_uint(0x1a4);
            g_ram_start = get_uint(0x1a8);
            g_ram_end = get_uint(0x1ac);
            g_extra_memory_type = get_byte(0x1b2);
            g_extra_memory_start = get_uint(0x1b4);
            g_extra_memory_end = get_uint(0x1b8);
            g_country = get_string(0x1f0, 0x1f2);
            g_pal_inferred = md_rom_metadata.InferPalModeFromCountryCodes(g_country) == true;
            return true;
        }
        public string get_string(int in_start, int in_end)
        {
            if (in_start < 0 || in_start >= g_file_size) return "";

            int w_length = Math.Min(in_end - in_start + 1, g_file_size - in_start);
            return Encoding.ASCII.GetString(g_file, in_start, w_length);
        }
        public uint get_uint(int in_start)
        {
            uint w_val = 0;
            for (int i = 0; i < 4; i++)
            {
                w_val = (w_val << 8) + g_file[in_start + i];
            }
            return w_val;
        }
        public byte get_byte(int in_start)
        {
            return g_file[in_start];
        }
    }
}
