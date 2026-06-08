using System.Reflection;

namespace GenesisEmu.Frontend.Windows
{
    public static class WinFormsAboutDialog
    {
        private const string AboutText =
            "GenesisEmu is a Sega Genesis / Mega Drive emulator written in C# and .NET.\r\n\r\n" +
            "This software is not intended for playing games illegally. It exists to help " +
            "users understand and appreciate the engineering behind this hardware and the " +
            "software that runs on it.\r\n\r\n" +
            "Originally based on MDTracer by sasayaki-japan.\r\n\r\n" +
            "Licensed under the MIT License.\r\n" +
            "Copyright (c) Stephane Dallongeville and contributors.";

        public static void Show(IWin32Window? in_owner)
        {
            string w_version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "";

            using Form w_form = new Form
            {
                Text = "About GenesisEmu",
                ClientSize = new Size(420, 280),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent,
            };

            var w_title = new Label
            {
                AutoSize = true,
                Location = new Point(16, 16),
                Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 12, FontStyle.Bold),
                Text = "GenesisEmu",
            };
            var w_versionLabel = new Label
            {
                AutoSize = true,
                Location = new Point(16, 42),
                Text = string.IsNullOrEmpty(w_version) ? "" : "Version " + w_version,
            };
            var w_body = new TextBox
            {
                Location = new Point(16, 68),
                Size = new Size(388, 160),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                Text = AboutText,
            };
            var w_close = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Location = new Point(324, 238),
                Size = new Size(80, 28),
            };

            w_form.Controls.AddRange(new Control[] { w_title, w_versionLabel, w_body, w_close });
            w_form.AcceptButton = w_close;
            WinFormsDebugTheme.Apply(w_form);
            w_form.ShowDialog(in_owner);
        }
    }
}
