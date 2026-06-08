using System.Drawing.Imaging;

namespace GenesisEmu.Frontend.Windows
{
    //----------------------------------------------------------------
    // Saves the current game-screen bitmap to the user's local app data.
    //----------------------------------------------------------------
    public static class WinFormsGameScreenshot
    {
        public static string SavePng(Image in_image, string in_appFolderName, string? in_romName)
        {
            if (in_image == null) throw new ArgumentNullException(nameof(in_image));

            string w_directoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                in_appFolderName,
                "Screenshot");
            Directory.CreateDirectory(w_directoryPath);

            string w_romName = SanitizeFileName(string.IsNullOrEmpty(in_romName) ? "screenshot" : in_romName);
            string w_timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string w_filePrefix = w_romName + "_screenshot_" + w_timeStamp + "_screen";
            string w_filePath = Path.Combine(w_directoryPath, w_filePrefix + ".png");
            int w_suffix = 1;
            while (File.Exists(w_filePath) == true)
            {
                w_filePath = Path.Combine(w_directoryPath, w_filePrefix + "_" + w_suffix + ".png");
                w_suffix++;
            }

            using Bitmap w_bitmap = new Bitmap(in_image);
            w_bitmap.Save(w_filePath, ImageFormat.Png);
            return w_filePath;
        }

        public static string GetScreenshotFolder(string in_appFolderName)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                in_appFolderName,
                "Screenshot");
        }

        private static string SanitizeFileName(string in_name)
        {
            string w_name = in_name;
            foreach (char w_char in Path.GetInvalidFileNameChars())
            {
                w_name = w_name.Replace(w_char, '_');
            }
            return w_name;
        }
    }
}
