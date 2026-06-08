using GenesisEmu.Frontend.Windows;
using MDTracer;

namespace GenesisEmu.Game
{
    //----------------------------------------------------------------
    // Player 1 and player 2 keyboard/gamepad mapping for the game frontend.
    //----------------------------------------------------------------
    internal sealed class GameControlsDialog : Form
    {
        public GameControlsDialog()
        {
            Text = "Controller Settings";
            ClientSize = new Size(440, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var w_tabs = new TabControl { Dock = DockStyle.Fill };
            w_tabs.TabPages.Add(BuildPlayerTab(
                "Player 1",
                md_main.g_md_io.g_key_allocation,
                md_main.g_md_io.g_joy_allocation));
            w_tabs.TabPages.Add(BuildPlayerTab(
                "Player 2",
                md_main.g_md_io.g_key_allocation2,
                md_main.g_md_io.g_joy_allocation2));

            var w_hint = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 36,
                Padding = new Padding(8, 8, 8, 0),
                Text = "Both players can use keyboard and the same gamepad (map different buttons per player).",
            };

            var w_closePanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
            };
            var w_closeButton = new Button
            {
                Text = "Close",
                Size = new Size(80, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            w_closeButton.Location = new Point(w_closePanel.Width - w_closeButton.Width - 12, 6);
            w_closeButton.Click += (_, _) => Close();
            w_closePanel.Resize += (_, _) =>
            {
                w_closeButton.Left = w_closePanel.ClientSize.Width - w_closeButton.Width - 12;
            };
            w_closePanel.Controls.Add(w_closeButton);

            Controls.Add(w_tabs);
            Controls.Add(w_closePanel);
            Controls.Add(w_hint);
        }

        private TabPage BuildPlayerTab(string in_title, int[] in_keyAllocation, int[] in_joyAllocation)
        {
            var w_tab = new TabPage(in_title)
            {
                Padding = new Padding(4),
            };

            var w_layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = GenesisInputKeyNames.PadButtons.Length + 1,
            };
            w_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            w_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            w_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            w_layout.Controls.Add(new Label { Text = "Button", AutoSize = true, Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold) }, 0, 0);
            w_layout.Controls.Add(new Label { Text = "Keyboard", AutoSize = true, Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold) }, 1, 0);
            w_layout.Controls.Add(new Label { Text = "Gamepad", AutoSize = true, Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold) }, 2, 0);

            for (int i = 0; i < GenesisInputKeyNames.PadButtons.Length; i++)
            {
                int w_row = i + 1;
                int w_index = i;

                w_layout.Controls.Add(new Label
                {
                    Text = GenesisInputKeyNames.PadButtons[i],
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                }, 0, w_row);

                var w_keyboardButton = CreateMappingButton(in_keyAllocation[i], true);
                w_keyboardButton.Click += (_, _) => CaptureKeyboard(in_keyAllocation, w_index, w_keyboardButton);
                w_layout.Controls.Add(w_keyboardButton, 1, w_row);

                var w_gamepadButton = CreateMappingButton(in_joyAllocation[i], false);
                w_gamepadButton.Click += (_, _) => CaptureGamepad(in_joyAllocation, w_index, w_gamepadButton);
                w_layout.Controls.Add(w_gamepadButton, 2, w_row);
            }

            w_tab.Controls.Add(w_layout);
            return w_tab;
        }

        private static Button CreateMappingButton(int in_value, bool in_isKeyboard)
        {
            string w_text = in_isKeyboard
                ? GenesisInputKeyNames.GetKeyName(in_value)
                : GenesisInputKeyNames.GetJoystickName(in_value);
            if (string.IsNullOrEmpty(w_text) == true) w_text = "(none)";

            return new Button
            {
                Text = w_text,
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
            };
        }

        private void CaptureKeyboard(int[] in_keyAllocation, int in_index, Button in_button)
        {
            using var w_dialog = new GameInputCaptureDialog(1);
            w_dialog.ShowDialog(this);
            if (w_dialog.g_result == -1) return;

            if (w_dialog.g_result == -2)
            {
                in_keyAllocation[in_index] = 0;
                in_button.Text = "(none)";
            }
            else if (w_dialog.g_result >= 0)
            {
                in_keyAllocation[in_index] = w_dialog.g_result;
                in_button.Text = GenesisInputKeyNames.GetKeyName(w_dialog.g_result);
            }
            md_main.write_setting();
        }

        private void CaptureGamepad(int[] in_joyAllocation, int in_index, Button in_button)
        {
            using var w_dialog = new GameInputCaptureDialog(0);
            w_dialog.ShowDialog(this);
            if (w_dialog.g_result == -1) return;

            if (w_dialog.g_result == -2)
            {
                in_joyAllocation[in_index] = -1;
                in_button.Text = "(none)";
            }
            else if (w_dialog.g_result >= 0)
            {
                in_joyAllocation[in_index] = w_dialog.g_result;
                in_button.Text = GenesisInputKeyNames.GetJoystickName(w_dialog.g_result);
            }
            md_main.write_setting();
        }
    }
}
