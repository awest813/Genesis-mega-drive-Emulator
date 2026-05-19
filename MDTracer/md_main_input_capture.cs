using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MDTracer
{
    internal partial class md_main
    {
        internal sealed class InputRecordEntry
        {
            public InputRecordEntry(string filePath)
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

        internal static class InputRecordStore
        {
            private const string FileExtension = ".mdi";
            private const int MaxEntryCountPerRom = 100;

            public static string DirectoryPath
            {
                get
                {
                    string w_basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    return Path.Combine(w_basePath, "MDTracer", "InputRecordHistory");
                }
            }

            public static IReadOnlyList<InputRecordEntry> GetEntries()
            {
                string w_directoryPath = GetCurrentRomDirectoryPath();
                if (string.IsNullOrEmpty(w_directoryPath) == true) return Array.Empty<InputRecordEntry>();
                if (Directory.Exists(w_directoryPath) == false) return Array.Empty<InputRecordEntry>();

                return Directory.GetFiles(w_directoryPath, "*")
                    .Select(in_file => new InputRecordEntry(in_file))
                    .OrderByDescending(in_entry => in_entry.CreatedAt)
                    .ThenByDescending(in_entry => Path.GetFileName(in_entry.FilePath))
                    .ToArray();
            }

            public static InputRecordEntry? GetLatestEntry()
            {
                return GetEntries().FirstOrDefault();
            }

            public static string CreateNewFilePath()
            {
                string w_directoryPath = GetCurrentRomDirectoryPath();
                if (string.IsNullOrEmpty(w_directoryPath) == true)
                {
                    throw new InvalidDataException("Input record ROM file name is empty.");
                }
                Directory.CreateDirectory(w_directoryPath);

                string w_filePrefix = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
                string w_filePath = Path.Combine(w_directoryPath, w_filePrefix + FileExtension);
                int w_suffix = 1;
                while (File.Exists(w_filePath) == true)
                {
                    w_filePath = Path.Combine(w_directoryPath, w_filePrefix + "_" + w_suffix + FileExtension);
                    w_suffix++;
                }
                return w_filePath;
            }

            public static void Delete(InputRecordEntry in_entry)
            {
                if (File.Exists(in_entry.FilePath) == true)
                {
                    File.Delete(in_entry.FilePath);
                }
            }

            public static void DeleteAll()
            {
                foreach (InputRecordEntry w_entry in GetEntries())
                {
                    Delete(w_entry);
                }
            }

            public static void DeleteOldEntriesOverLimit()
            {
                foreach (InputRecordEntry w_entry in GetEntries().Skip(MaxEntryCountPerRom))
                {
                    Delete(w_entry);
                }
            }

            public static bool IsAvailable()
            {
                return string.IsNullOrEmpty(g_state_capture_rom_file_name) == false;
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
