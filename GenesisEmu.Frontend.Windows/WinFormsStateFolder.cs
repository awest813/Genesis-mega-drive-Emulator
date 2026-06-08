using System.Diagnostics;
using MDTracer;

namespace GenesisEmu.Frontend.Windows
{
    public static class WinFormsStateFolder
    {
        public static string? GetCurrentRomStateDirectory()
        {
            return md_main.StateStore.CurrentRomDirectoryPath;
        }

        public static void OpenCurrentRomStateDirectory()
        {
            string? w_folder = GetCurrentRomStateDirectory();
            if (string.IsNullOrEmpty(w_folder) == true)
            {
                throw new InvalidOperationException("Load a ROM before opening the save-state folder.");
            }

            Directory.CreateDirectory(w_folder);
            Process.Start(new ProcessStartInfo
            {
                FileName = w_folder,
                UseShellExecute = true,
            });
        }
    }
}
