namespace GenesisEmu.Frontend.Windows
{
    //----------------------------------------------------------------
    // Toggles borderless maximized fullscreen for WinForms main windows.
    //----------------------------------------------------------------
    public sealed class WinFormsFullscreenHelper
    {
        private FormWindowState g_prevWindowState;
        private FormBorderStyle g_prevBorderStyle;
        private Rectangle g_prevBounds;
        private bool g_fullscreen;

        public bool IsFullscreen => g_fullscreen;

        public void Toggle(Form in_form, params Control[] in_hideWhenFullscreen)
        {
            if (g_fullscreen == true)
            {
                Exit(in_form, in_hideWhenFullscreen);
            }
            else
            {
                Enter(in_form, in_hideWhenFullscreen);
            }
        }

        public void Enter(Form in_form, params Control[] in_hideWhenFullscreen)
        {
            if (g_fullscreen == true || in_form.WindowState == FormWindowState.Minimized) return;

            g_prevWindowState = in_form.WindowState;
            g_prevBorderStyle = in_form.FormBorderStyle;
            g_prevBounds = in_form.Bounds;

            foreach (Control w_control in in_hideWhenFullscreen)
            {
                w_control.Hide();
            }

            in_form.FormBorderStyle = FormBorderStyle.None;
            in_form.WindowState = FormWindowState.Maximized;
            g_fullscreen = true;
        }

        public void Exit(Form in_form, params Control[] in_showWhenWindowed)
        {
            if (g_fullscreen == false) return;

            in_form.FormBorderStyle = g_prevBorderStyle;
            in_form.WindowState = FormWindowState.Normal;
            in_form.Bounds = g_prevBounds;
            if (g_prevWindowState == FormWindowState.Maximized)
            {
                in_form.WindowState = FormWindowState.Maximized;
            }

            foreach (Control w_control in in_showWhenWindowed)
            {
                w_control.Show();
            }

            g_fullscreen = false;
        }
    }
}
