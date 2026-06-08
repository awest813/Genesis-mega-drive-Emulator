using GenesisEmu.Frontend.Windows;
using MDTracer;

namespace GenesisEmu.Game
{
    //----------------------------------------------------------------
    // Player 1 keyboard and gamepad mapping for the game frontend.
    //----------------------------------------------------------------
    internal sealed class GameControlsDialog : Form
    {
        private const int PadButtonCount = 12;
        private readonly Button[] g_keyboardButtons = new Button[PadButtonCount];
        private readonly Button[] g_gamepadButtons = new Button[PadButtonCount];

        public GameControlsDialog()
        {
            Text = "Controller Settings — Player 1";
            ClientSize = new Size(420, 360);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var w_layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = GenesisInputKeyNames.PadButtons.Length + 1,
                Padding = new Padding(8),
            };
            w_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            w_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            w_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            w_layout.Controls.Add(new Label { Text = "Button", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
            w_layout.Controls.Add(new Label { Text = "Keyboard", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 1, 0);
            w_layout.Controls.Add(new Label { Text = "Gamepad", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 2, 0);

            for (int i = 0; i < GenesisInputKeyNames.PadButtons.Length; i++)
            {
                int w_row = i + 1;
                w_layout.Controls.Add(new Label
                {
                    Text = GenesisInputKeyNames.PadButtons[i],
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                }, 0, w_row);

                var w_keyboardButton = CreateMappingButton(
                    md_main.g_md_io.g_key_allocation[i],
                    true);
                w_keyboardButton.Click += (_, _) => CaptureKeyboard(i, w_keyboardButton);
                g_keyboardButtons[i] = w_keyboardButton;
                w_layout.Controls.Add(w_keyboardButton, 1, w_row);

                var w_gamepadButton = CreateMappingButton(
                    md_main.g_md_io.g_joy_allocation[i],
                    false);
                w_gamepadButton.Click += (_, _) => CaptureGamepad(i, w_gamepadButton);
                g_gamepadButtons[i] = w_gamepadButton;
                w_layout.Controls.Add(w_gamepadButton, 2, w_row);
            }

            var w_closeButton = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Size = new Size(80, 28),
            };
            w_closeButton.Location = new Point(ClientSize.Width - w_closeButton.Width - 16, ClientSize.Height - 40);
            w_closeButton.Click += (_, _) => Close();

            Controls.Add(w_layout);
            Controls.Add(w_closeButton);
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

        private void CaptureKeyboard(int in_index, Button in_button)
        {
            using var w_dialog = new GameInputCaptureDialog(1);
            w_dialog.ShowDialog(this);
            if (w_dialog.g_result == -1) return;

            if (w_dialog.g_result == -2)
            {
                md_main.g_md_io.g_key_allocation[in_index] = 0;
                in_button.Text = "(none)";
            }
            else if (w_dialog.g_result >= 0)
            {
                md_main.g_md_io.g_key_allocation[in_index] = (byte)w_dialog.g_result;
                in_button.Text = GenesisInputKeyNames.GetKeyName(w_dialog.g_result);
            }
            md_main.write_setting();
        }

        private void CaptureGamepad(int in_index, Button in_button)
        {
            using var w_dialog = new GameInputCaptureDialog(0);
            w_dialog.ShowDialog(this);
            if (w_dialog.g_result == -1) return;

            if (w_dialog.g_result == -2)
            {
                md_main.g_md_io.g_joy_allocation[in_index] = -1;
                in_button.Text = "(none)";
            }
            else if (w_dialog.g_result >= 0)
            {
                md_main.g_md_io.g_joy_allocation[in_index] = w_dialog.g_result;
                in_button.Text = GenesisInputKeyNames.GetJoystickName(w_dialog.g_result);
            }
            md_main.write_setting();
        }
    }
}
