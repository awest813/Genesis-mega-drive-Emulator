using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MDTracer
{
    internal partial class md_main
    {
        internal sealed class StateListEntry
        {
            public StateListEntry(string filePath)
            {
                FilePath = filePath;
                CreatedAt = File.GetCreationTime(filePath);
                LastWriteTime = File.GetLastWriteTime(filePath);
            }

            public string FilePath { get; }
            public DateTime CreatedAt { get; }
            public DateTime LastWriteTime { get; }
            public string DisplayName => LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
                + "  "
                + Path.GetFileName(FilePath);
        }

        internal static class StateStore
        {
            private const string FileExtension = ".bin";
            private const int StateVersion = 5;
            private const int MaxEntryCountPerRom = 100;
            private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes("MDT68KST");

            public static string DirectoryPath
            {
                get
                {
                    string w_basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    return Path.Combine(w_basePath, "MDTracer", "ListState");
                }
            }

            public static IReadOnlyList<StateListEntry> GetEntries()
            {
                string w_directoryPath = GetCurrentRomDirectoryPath();
                if (string.IsNullOrEmpty(w_directoryPath) == true) return Array.Empty<StateListEntry>();
                if (Directory.Exists(w_directoryPath) == false) return Array.Empty<StateListEntry>();

                return Directory.GetFiles(w_directoryPath, "*")
                    .Select(in_file => new StateListEntry(in_file))
                    .OrderByDescending(in_entry => in_entry.CreatedAt)
                    .ThenByDescending(in_entry => Path.GetFileName(in_entry.FilePath))
                    .ToArray();
            }

            public static StateListEntry? GetLatestEntry()
            {
                return GetEntries().FirstOrDefault();
            }

            public static StateListEntry? GetEntryByFileNameWithoutExtension(string in_fileNameWithoutExtension)
            {
                return GetEntries().FirstOrDefault(in_entry =>
                    string.Equals(Path.GetFileNameWithoutExtension(in_entry.FilePath), in_fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase) == true);
            }

            public static StateListEntry Save()
            {
                string w_filePrefix = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
                return Save(w_filePrefix);
            }

            public static StateListEntry Save(string in_filePrefix)
            {
                string w_directoryPath = GetCurrentRomDirectoryPath();
                if (string.IsNullOrEmpty(w_directoryPath) == true)
                {
                    throw new InvalidDataException("MD state capture ROM file name is empty.");
                }
                Directory.CreateDirectory(w_directoryPath);

                string w_filePrefix = SanitizeFilePrefix(in_filePrefix);
                string w_filePath = Path.Combine(w_directoryPath, w_filePrefix + FileExtension);
                int w_suffix = 1;
                while (File.Exists(w_filePath) == true)
                {
                    w_filePath = Path.Combine(w_directoryPath, w_filePrefix + "_" + w_suffix + FileExtension);
                    w_suffix++;
                }

                using (FileStream w_stream = new FileStream(w_filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                using (BinaryWriter w_writer = new BinaryWriter(w_stream))
                {
                    w_writer.Write(MagicBytes);
                    w_writer.Write(StateVersion);
                    g_md_m68k.write_state(w_writer);
                    g_md_vdp.write_state(w_writer);
                    g_md_music.write_state(w_writer);
                    g_md_z80.write_state(w_writer);
                    g_md_sram.write_state(w_writer);
                }

                DeleteOldEntriesOverLimit();
                return new StateListEntry(w_filePath);
            }

            public static void Restore(StateListEntry in_entry)
            {
                using FileStream w_stream = new FileStream(in_entry.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (w_stream.Length == md_m68k.StateWorkRamSize)
                {
                    g_md_m68k.restore_work_ram_raw_state(w_stream);
                    return;
                }

                using BinaryReader w_reader = new BinaryReader(w_stream);
                byte[] w_magic = w_reader.ReadBytes(MagicBytes.Length);
                if (w_magic.SequenceEqual(MagicBytes) == false)
                {
                    throw new InvalidDataException("MD state capture format is invalid.");
                }

                int w_version = w_reader.ReadInt32();
                if ((w_version < 1) || (w_version > StateVersion))
                {
                    throw new InvalidDataException("MD state capture version is unsupported: " + w_version);
                }

                g_md_m68k.restore_state(w_reader, w_version >= 4);
                if (w_version >= 2)
                {
                    g_md_vdp.restore_state(w_reader);
                    g_md_music.restore_state(w_reader);
                }
                if (w_version >= 3)
                {
                    g_md_z80.restore_state(w_reader);
                }
                if (w_version >= 5)
                {
                    g_md_sram.restore_state(w_reader);
                }

                if (w_version < 4)
                {
                    g_md_m68k.restore_work_ram_state(w_reader);
                }
            }

            public static void Delete(StateListEntry in_entry)
            {
                if (File.Exists(in_entry.FilePath) == true)
                {
                    File.Delete(in_entry.FilePath);
                }
            }

            public static void DeleteAll()
            {
                foreach (StateListEntry w_entry in GetEntries())
                {
                    Delete(w_entry);
                }
            }

            private static void DeleteOldEntriesOverLimit()
            {
                foreach (StateListEntry w_entry in GetEntries().Skip(MaxEntryCountPerRom))
                {
                    Delete(w_entry);
                }
            }

            public static bool IsAvailable()
            {
                return (g_md_m68k?.g_memory != null)
                    && (g_md_vdp != null)
                    && (g_md_music != null)
                    && (g_md_z80 != null)
                    && (string.IsNullOrEmpty(g_state_capture_rom_file_name) == false);
            }

            private static string SanitizeFilePrefix(string in_filePrefix)
            {
                string w_filePrefix = in_filePrefix;
                if (string.IsNullOrEmpty(w_filePrefix) == true)
                {
                    w_filePrefix = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
                }
                foreach (char w_char in Path.GetInvalidFileNameChars())
                {
                    w_filePrefix = w_filePrefix.Replace(w_char, '_');
                }
                return w_filePrefix;
            }

            private static string GetCurrentRomDirectoryPath()
            {
                string w_romFolderName = GetCurrentRomFolderName();
                if (string.IsNullOrEmpty(w_romFolderName) == true) return "";
                return Path.Combine(DirectoryPath, w_romFolderName);
            }

            private static string GetCurrentRomFolderName()
            {
                string w_rom_file_name = Path.GetFileNameWithoutExtension(g_state_capture_rom_file_name);
                if (string.IsNullOrEmpty(w_rom_file_name) == true) return "";

                foreach (char w_char in Path.GetInvalidFileNameChars())
                {
                    w_rom_file_name = w_rom_file_name.Replace(w_char, '_');
                }
                return w_rom_file_name;
            }

        }
    }
}
