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
            md_main.g_pattern_enable = false;
            md_main.g_form_setting.update();
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

        public void picture_update(Bitmap in_bitmap)
        {
            if (IsDisposed == true || IsHandleCreated == false || Visible == false) return;
            if (in_bitmap.Width < 128 || in_bitmap.Height < 128) return;

            int w_y = g_cur_char << 3;
            int w_maxY = in_bitmap.Height - 128;
            if (w_y < 0)
            {
                w_y = 0;
            }
            else if (w_y > w_maxY)
            {
                w_y = w_maxY;
            }

            Rectangle rect = new Rectangle(0, w_y, 128, 128);
            Bitmap bmp_dst = in_bitmap.Clone(rect, in_bitmap.PixelFormat);
            UpdatePictureBox(bmp_dst);
        }
    }
}
