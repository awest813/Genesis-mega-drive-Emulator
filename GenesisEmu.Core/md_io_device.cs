namespace MDTracer
{
    internal partial class md_io
    {
        public void rescan()
        {
            g_inputBackend.Rescan(true);
        }

        public void select_joy_device(int in_index)
        {
            g_inputBackend.SelectJoyDevice(in_index);
        }

        private void rescan_joystick_if_needed()
        {
            if (md_main.is_clock_wait_skip() == true) return;

            long w_now = Environment.TickCount64;
            if (w_now - g_joy_last_rescan_time < JOY_RESCAN_INTERVAL_MS) return;
            g_joy_last_rescan_time = w_now;
            if (Interlocked.CompareExchange(ref g_joy_rescan_in_progress, 1, 0) != 0) return;

            Task.Run(() =>
            {
                try
                {
                    g_inputBackend.Rescan(false);
                }
                finally
                {
                    Interlocked.Exchange(ref g_joy_rescan_in_progress, 0);
                }
            });
        }

        public int read_device_joystick()
        {
            int w_out = g_inputBackend.ReadJoystick(g_joy_status);
            if (w_out < 0)
            {
                rescan_joystick_if_needed();
            }
            return w_out;
        }

        public int read_device_joystick_for_setting()
        {
            return g_inputBackend.ReadJoystick(null);
        }

        public int read_device_keyboard()
        {
            return g_inputBackend.ReadKeyboard(g_key_status);
        }

        public int read_device_keyboard_for_setting()
        {
            return g_inputBackend.ReadKeyboard(null);
        }
    }
}
