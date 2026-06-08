using GenesisEmu.Frontend.Windows;
using MDTracer;

namespace GenesisEmu.Game
{
    //----------------------------------------------------------------
    // Selects which DirectInput gamepad device GenesisEmu listens to.
    //----------------------------------------------------------------
    internal sealed class GameGamepadDialog : Form
    {
        public GameGamepadDialog()
        {
            Text = "Gamepad Device";
            ClientSize = new Size(360, 130);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var w_label = new Label
            {
                AutoSize = true,
                Location = new Point(12, 16),
                Text = "Active gamepad (both players share this device):",
            };
            var w_combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 40),
                Size = new Size(336, 23),
            };
            var w_rescanButton = new Button
            {
                Text = "Rescan",
                Location = new Point(12, 72),
                Size = new Size(80, 28),
            };
            var w_closeButton = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Location = new Point(268, 72),
                Size = new Size(80, 28),
            };

            void RefreshList()
            {
                w_combo.Items.Clear();
                IReadOnlyList<string> w_devices = md_main.g_md_io.g_joy_name_list;
                if (w_devices.Count == 0)
                {
                    w_combo.Items.Add("(no gamepads found)");
                    w_combo.SelectedIndex = 0;
                    w_combo.Enabled = false;
                    return;
                }

                w_combo.Enabled = true;
                int w_selected = 0;
                for (int i = 0; i < w_devices.Count; i++)
                {
                    w_combo.Items.Add(w_devices[i]);
                    if (w_devices[i] == md_main.g_md_io.g_joy_name)
                    {
                        w_selected = i;
                    }
                }
                w_combo.SelectedIndex = w_selected;
            }

            w_rescanButton.Click += (_, _) =>
            {
                md_main.g_md_io.rescan();
                RefreshList();
            };
            w_combo.SelectedIndexChanged += (_, _) =>
            {
                if (w_combo.Enabled == false) return;
                md_main.g_md_io.select_joy_device(w_combo.SelectedIndex);
                md_main.write_setting();
            };
            w_closeButton.Click += (_, _) => Close();

            Controls.AddRange(new Control[] { w_label, w_combo, w_rescanButton, w_closeButton });
            AcceptButton = w_closeButton;
            Load += (_, _) => RefreshList();
            WinFormsDebugTheme.Apply(this);
        }
    }
}
