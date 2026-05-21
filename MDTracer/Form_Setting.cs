using MDTracer;

namespace MDTracer
{
    public partial class Form_Setting : Form
    {
        public bool g_view_screenA = true;
        public bool g_view_screenB = true;
        public bool g_view_screenW = true;
        public bool g_view_screenS = true;
        public bool g_screenA_High = false;
        public bool g_screenB_High = false;
        public bool g_screenW_High = false;
        public bool g_screenS_High = false;
        public bool g_screenA_Low = false;
        public bool g_screenB_Low = false;
        public bool g_screenW_Low = false;
        public bool g_screenS_Low = false;
        private bool g_updating;
        //----------------------------------------------------------------
        //form
        //----------------------------------------------------------------
        public Form_Setting()
        {
            InitializeComponent();
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
        }

        //----------------------------------------------------------------
        //Event Handling: Screen Operations
        //----------------------------------------------------------------
        private void Form_Setting_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        private void comboBox_videoformat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_tvmode_req = comboBox_videoformat.SelectedIndex;
            md_main.write_setting();
        }
        private void comboBox_rendering_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_gpu_req = comboBox_rendering.SelectedIndex;
            md_main.write_setting();
        }
        private void checkBox_screenA_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_screenA_enable = checkBox_screenA.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_screenB_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_screenB_enable = checkBox_screenB.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_screenW_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_screenW_enable = checkBox_screenW.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_screenS_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_screenS_enable = checkBox_screenS.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_pattern_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_pattern_enable = checkBox_pattern.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_pallete_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_pallete_enable = checkBox_pallete.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_code_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_code_enable = checkBox_code.Checked;
            show_window();
            md_main.write_setting();
        }
        private void checkBox_io_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_io_enable = checkBox_io.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_music_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_music_enable = checkBox_music.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_register_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_registry_enable = checkBox_register.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_flow_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_flow_enable = checkBox_flow.Checked;
            show_window();
            md_main.write_setting();
        }

        private void checkBox_fsb_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_trace_fsb = checkBox_fsb.Checked;
            md_main.write_setting();
        }

        private void checkBox_sip_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            md_main.g_trace_sip = checkBox_sip.Checked;
            md_main.write_setting();
        }
        //----------------------------------------------------------------
        //sub function
        //----------------------------------------------------------------
        public void update()
        {
            g_updating = true;
            try
            {
                checkBox_screenA.Checked = md_main.g_screenA_enable;
                checkBox_screenB.Checked = md_main.g_screenB_enable;
                checkBox_screenW.Checked = md_main.g_screenW_enable;
                checkBox_screenS.Checked = md_main.g_screenS_enable;
                checkBox_pattern.Checked = md_main.g_pattern_enable;
                checkBox_pallete.Checked = md_main.g_pallete_enable;
                checkBox_code.Checked = md_main.g_code_enable;
                checkBox_io.Checked = md_main.g_io_enable;
                checkBox_music.Checked = md_main.g_music_enable;
                checkBox_register.Checked = md_main.g_registry_enable;
                checkBox_flow.Checked = md_main.g_flow_enable;
                checkBox_fsb.Checked = md_main.g_trace_fsb;
                checkBox_sip.Checked = md_main.g_trace_sip;
                comboBox_videoformat.SelectedIndex = md_main.g_md_vdp.g_vdp_status_0_tvmode;
                comboBox_rendering.SelectedIndex = (md_main.g_md_vdp.rendering_gpu == false) ? 0 : 1;
                checkBox_viewA.Checked = g_view_screenA;
                checkBox_viewB.Checked = g_view_screenB;
                checkBox_viewW.Checked = g_view_screenW;
                checkBox_viewS.Checked = g_view_screenS;
                checkBox_A_High.Checked = g_screenA_High;
                checkBox_A_Low.Checked = g_screenA_Low;
                checkBox_B_High.Checked = g_screenB_High;
                checkBox_B_Low.Checked = g_screenB_Low;
                checkBox_W_High.Checked = g_screenW_High;
                checkBox_W_Low.Checked = g_screenW_Low;
                checkBox_S_High.Checked = g_screenS_High;
                checkBox_S_Low.Checked = g_screenS_Low;
            }
            finally
            {
                g_updating = false;
            }
            show_window();
        }
        public void show_window()
        {
            if (md_main.g_screenA_enable == true) { md_main.g_form_screenA.Show(); } else { md_main.g_form_screenA.Hide(); }
            if (md_main.g_screenB_enable == true) { md_main.g_form_screenB.Show(); } else { md_main.g_form_screenB.Hide(); }
            if (md_main.g_screenW_enable == true) { md_main.g_form_screenW.Show(); } else { md_main.g_form_screenW.Hide(); }
            if (md_main.g_screenS_enable == true) { md_main.g_form_screenS.Show(); } else { md_main.g_form_screenS.Hide(); }
            if (md_main.g_pattern_enable == true) { md_main.g_form_pattern.Show(); } else { md_main.g_form_pattern.Hide(); }
            if (md_main.g_pallete_enable == true) { md_main.g_form_pallete.Show(); } else { md_main.g_form_pallete.Hide(); }
            if (md_main.g_code_enable == true) { md_main.g_form_code.Show(); } else { md_main.g_form_code.Hide(); }
            if (md_main.g_io_enable == true) { md_main.g_form_io.Show(); } else { md_main.g_form_io.Hide(); }
            if (md_main.g_music_enable == true) { md_main.g_form_music.Show(); } else { md_main.g_form_music.Hide(); }
            if (md_main.g_registry_enable == true) { md_main.g_form_registry.Show(); } else { md_main.g_form_registry.Hide(); }
            if (md_main.g_flow_enable == true) { md_main.g_form_flow.Show(); } else { md_main.g_form_flow.Hide(); }
        }

        private void checkBox_viewA_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_view_screenA = checkBox_viewA.Checked;
            md_main.write_setting();
        }

        private void checkBox_viewB_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_view_screenB = checkBox_viewB.Checked;
            md_main.write_setting();
        }

        private void checkBox_viewW_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_view_screenW = checkBox_viewW.Checked;
            md_main.write_setting();
        }

        private void checkBox_viewS_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_view_screenS = checkBox_viewS.Checked;
            md_main.write_setting();
        }

        private void checkBox_A_High_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_screenA_High = checkBox_A_High.Checked;
            md_main.write_setting();
        }

        private void checkBox_B_High_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_screenB_High = checkBox_B_High.Checked;
            md_main.write_setting();
        }

        private void checkBox_W_High_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_screenW_High = checkBox_W_High.Checked;
            md_main.write_setting();
        }

        private void checkBox_S_High_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_screenS_High = checkBox_S_High.Checked;
            md_main.write_setting();
        }

        private void checkBox_A_Low_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_screenA_Low = checkBox_A_Low.Checked;
            md_main.write_setting();
        }

        private void checkBox_B_Low_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_screenB_Low = checkBox_B_Low.Checked;
            md_main.write_setting();
        }

        private void checkBox_W_Low_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_screenW_Low = checkBox_W_Low.Checked;
            md_main.write_setting();
        }

        private void checkBox_S_Low_CheckedChanged(object sender, EventArgs e)
        {
            if (g_updating == true) return;
            g_screenS_Low = checkBox_S_Low.Checked;
            md_main.write_setting();
        }
    }
}
