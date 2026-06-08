using System.Threading;

namespace MDTracer
{
    public partial class Form_Pattern : Form
    {
        public int g_screen_xpos;
        public int g_screen_ypos;
        public static int CHAR_MAX = 112;
        public static int g_cur_char;
        //----------------------------------------------------------------
        //form
        //----------------------------------------------------------------
        public Form_Pattern()
        {
            InitializeComponent();

            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            label_num.Text = "0";

            hScrollBar_picturebox.Minimum = 0;
            hScrollBar_picturebox.Maximum = CHAR_MAX - 1;
            hScrollBar_picturebox.LargeChange = 1;
        }
        //----------------------------------------------------------------
        //Event Handling: Screen Operations
        //----------------------------------------------------------------
        private void hScrollBar_picturebox_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type != ScrollEventType.EndScroll)
            {
                int w_cur = e.NewValue;
                if (w_cur < 0)
                {
                    w_cur = 0;
                }
                else
                if (w_cur >= CHAR_MAX)
                {
                    w_cur = CHAR_MAX - 1;
                }
                g_cur_char = w_cur;
                label_num.Text = g_cur_char.ToString();
                this.Invalidate();
            }
        }
        private void Form_Pattern_FormClosing(object sender, FormClosingEventArgs e)
        {
            md_main.g_debugView.pattern_enable = false;
            md_main.g_frontendSettings.NotifyDebugWindowLayoutChanged();
            md_main.write_setting();
            e.Cancel = true;
        }
        private void Form_Pattern_ResizeEnd(object sender, EventArgs e)
        {
            var currentPosition = this.Location;
            g_screen_xpos = currentPosition.X;
            g_screen_ypos = currentPosition.Y;
            md_main.write_setting();
        }
        private void Form_Pattern_Shown(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(g_screen_xpos, g_screen_ypos);
        }
        //----------------------------------------------------------------
        //Event Handling: Painting
        //----------------------------------------------------------------
        private void Form_Pattern_Paint(object sender, PaintEventArgs e)
        {
            pictureBox_pattern.Invalidate();
        }
        private void UpdatePictureBox(Bitmap in_bitmap)
        {
            if (pictureBox_pattern.IsDisposed == true || pictureBox_pattern.IsHandleCreated == false)
            {
                in_bitmap.Dispose();
                return;
            }

            if (pictureBox_pattern.InvokeRequired == true)
            {
                try
                {
                    pictureBox_pattern.BeginInvoke(new Action<Bitmap>(UpdatePictureBox), in_bitmap);
                }
                catch
                {
                    in_bitmap.Dispose();
                }
                return;
            }

            pictureBox_pattern.Image?.Dispose();
            pictureBox_pattern.Image = in_bitmap;
        }

        public void picture_update(uint[] in_pixels, int in_width, int in_height)
        {
            if (IsDisposed == true || IsHandleCreated == false || Visible == false) return;
            if (in_width < 128 || in_height < 128) return;

            Bitmap bmp_dst = WinFormsVdpDebugBitmap.CreatePatternTileView(
                in_pixels, in_width, in_height, g_cur_char << 3);
            UpdatePictureBox(bmp_dst);
        }
    }
}
