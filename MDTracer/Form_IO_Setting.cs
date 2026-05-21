using System.ComponentModel;

namespace MDTracer
{
    public partial class Form_IO_Setting : Form
    {
        public int g_mode;
        public int g_result;
        private volatile bool g_closingByUser;
        public Form_IO_Setting()
        {
            g_result = -1;
            InitializeComponent();
            backgroundWorker1.WorkerSupportsCancellation = true;
            FormClosing += Form_IO_Setting_FormClosing;
        }

        private void Form_IO_Setting_Shown(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy == false)
            {
                backgroundWorker1.RunWorkerAsync();
            }

            this.ActiveControl = null;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int w_ret = -1;
            WaitForInputRelease();
            do
            {
                if (backgroundWorker1.CancellationPending == true || g_closingByUser == true)
                {
                    return;
                }
                if(g_mode == 0)
                {
                    w_ret = md_main.g_md_io.read_device_joystick_for_setting();
                    if (w_ret != -1)
                    {
                        break;
                    }
                }
                if (g_mode == 1)
                {
                    w_ret = md_main.g_md_io.read_device_keyboard_for_setting();
                    if (w_ret != -1)
                    {
                        break;
                    }
                }
                Thread.Sleep(10);
            } while (true);
            if (backgroundWorker1.CancellationPending == true || g_closingByUser == true) return;

            g_result = w_ret;
            if (!IsDisposed && IsHandleCreated)
            {
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        Close();
                    }));
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private void WaitForInputRelease()
        {
            while (backgroundWorker1.CancellationPending == false && g_closingByUser == false)
            {
                int w_ret = g_mode == 0
                    ? md_main.g_md_io.read_device_joystick_for_setting()
                    : md_main.g_md_io.read_device_keyboard_for_setting();

                if (w_ret == -1) return;
                Thread.Sleep(10);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            g_closingByUser = true;
            if (backgroundWorker1.IsBusy == true) backgroundWorker1.CancelAsync();
            Close();
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            g_closingByUser = true;
            g_result = -2;
            if (backgroundWorker1.IsBusy == true) backgroundWorker1.CancelAsync();
            Close();
        }

        private void Form_IO_Setting_FormClosing(object? sender, FormClosingEventArgs e)
        {
            g_closingByUser = true;
            if (backgroundWorker1.IsBusy == true) backgroundWorker1.CancelAsync();
        }
    }

}
