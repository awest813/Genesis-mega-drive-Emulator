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
        public bool load(string in_romname)
        {
            try
            {
                g_file = File.ReadAllBytes(in_romname);
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
                        g_file = memoryStream.ToArray();
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
