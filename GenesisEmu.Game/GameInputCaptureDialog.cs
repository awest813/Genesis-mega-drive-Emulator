using System.ComponentModel;
using MDTracer;

namespace GenesisEmu.Game
{
    //----------------------------------------------------------------
    // Captures the next keyboard key or gamepad button for input mapping.
    //----------------------------------------------------------------
    internal sealed class GameInputCaptureDialog : Form
    {
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private readonly BackgroundWorker g_worker = new();
        private volatile bool g_closingByUser;

        internal int g_captureMode;
        internal int g_result = -1;

        public GameInputCaptureDialog(int in_captureMode)
        {
            g_captureMode = in_captureMode;
            Text = "Input Capture";
            ClientSize = new Size(320, 90);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;

            var w_label = new Label
            {
                AutoSize = true,
                Location = new Point(12, 12),
                Text = "Press the button or key you want to assign",
            };
            var w_clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(60, 48),
                Size = new Size(75, 23),
            };
            var w_cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(182, 48),
                Size = new Size(75, 23),
            };

            w_clearButton.Click += (_, _) => CloseWithResult(-2);
            w_cancelButton.Click += (_, _) => CloseWithResult(-1);

            Controls.Add(w_label);
            Controls.Add(w_clearButton);
            Controls.Add(w_cancelButton);

            g_worker.WorkerSupportsCancellation = true;
            g_worker.DoWork += Worker_DoWork;
            Shown += (_, _) =>
            {
                if (g_worker.IsBusy == false) g_worker.RunWorkerAsync();
                ActiveControl = null;
            };
            FormClosing += (_, _) =>
            {
                g_closingByUser = true;
                if (g_worker.IsBusy == true) g_worker.CancelAsync();
            };
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            WaitForInputRelease();
            while (g_worker.CancellationPending == false && g_closingByUser == false)
            {
                int w_ret = g_captureMode == 0
                    ? md_main.g_md_io.read_device_joystick_for_setting()
                    : md_main.g_md_io.read_device_keyboard_for_setting();
                if (w_ret != -1)
                {
                    g_result = w_ret;
                    BeginInvoke(Close);
                    return;
                }
                Thread.Sleep(10);
            }
        }

        private void WaitForInputRelease()
        {
            while (g_worker.CancellationPending == false && g_closingByUser == false)
            {
                int w_ret = g_captureMode == 0
                    ? md_main.g_md_io.read_device_joystick_for_setting()
                    : md_main.g_md_io.read_device_keyboard_for_setting();
                if (w_ret == -1) return;
                Thread.Sleep(10);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (g_captureMode == 1 && TryGetDirectInputKeyNo(msg, out int w_keyNo) == true)
            {
                CloseWithResult(w_keyNo);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void CloseWithResult(int in_result)
        {
            g_result = in_result;
            g_closingByUser = true;
            if (g_worker.IsBusy == true) g_worker.CancelAsync();
            Close();
        }

        private static bool TryGetDirectInputKeyNo(Message in_msg, out int out_keyNo)
        {
            out_keyNo = -1;
            if (in_msg.Msg != WM_KEYDOWN && in_msg.Msg != WM_SYSKEYDOWN) return false;

            Keys w_virtualKey = (Keys)(int)in_msg.WParam;
            if (w_virtualKey == Keys.Pause)
            {
                out_keyNo = (int)SharpDX.DirectInput.Key.Pause;
                return true;
            }

            long w_lParam = in_msg.LParam.ToInt64();
            int w_scanCode = (int)((w_lParam >> 16) & 0xff);
            if (w_scanCode == 0) return false;

            bool w_extended = (w_lParam & (1L << 24)) != 0;
            out_keyNo = w_extended == true ? w_scanCode | 0x80 : w_scanCode;
            return true;
        }
    }
}
