using System.Drawing.Imaging;
using System.IO;

namespace MDTracer
{
    public partial class Form_VDP_Screen : Form
    {
        private int g_screen_xsize_change;
        private int g_screen_ysize_change;
        private int g_content_xsize_change;
        private int g_content_ysize_change;
        public int g_screen_xpos;
        public int g_screen_ypos;
        private string g_screen_type;
        private readonly object g_bitmapLock = new object();
        private readonly object g_videoRecordLock = new object();
        private Bitmap? g_source_bitmap;
        private int g_source_xsize;
        private int g_source_ysize;
        private Form_Main_AviRecorder? g_videoRecorder;
        private string g_videoRecordPath = "";
        private bool g_sizeChanging;
        private volatile bool g_gridEnabled;
        private volatile bool g_screenshotEnabled;
        private volatile bool g_videoRecordingEnabled;
        private volatile bool g_viewActive;
        //----------------------------------------------------------------
        //form
        //----------------------------------------------------------------
        public Form_VDP_Screen()
        {
            InitializeComponent();
            screenshotToolStripMenuItem.Click += vdpMenuSettingToolStripMenuItem_Click;
            videoRecordingToolStripMenuItem.Click += vdpMenuSettingToolStripMenuItem_Click;
            VisibleChanged += (sender, e) => g_viewActive = Visible;
        }
        //----------------------------------------------------------------
        //initialize
        //----------------------------------------------------------------
        public void initialize(string in_type, int in_screen_xsize, int in_screen_ysize, string in_title)
        {
            g_screen_xsize_change = in_screen_xsize;
            g_screen_ysize_change = in_screen_ysize;
            g_screen_type = in_type;
            this.Text = in_title;
        }
        //----------------------------------------------------------------
        //Event Handling: Screen Operations
        //----------------------------------------------------------------
        private void Form_VDP_Screen_FormClosing(object sender, FormClosingEventArgs e)
        {
            g_viewActive = false;
            SyncVideoRecordingStop();
            switch (g_screen_type)
            {
                case "A": md_main.g_screenA_enable = false; break;
                case "B": md_main.g_screenB_enable = false; break;
                case "W": md_main.g_screenW_enable = false; break;
                case "S": md_main.g_screenS_enable = false; break;
            }
            md_main.g_form_setting.update();
            md_main.write_setting();
            e.Cancel = true;
        }
        private void Form_VDP_Screen_ResizeEnd(object sender, EventArgs e)
        {
            var currentPosition = this.Location;
            g_screen_xpos = currentPosition.X;
            g_screen_ypos = currentPosition.Y;
            ResizeClientToImageAspect();
            g_content_xsize_change = panel_screen.ClientSize.Width;
            g_content_ysize_change = panel_screen.ClientSize.Height;
            md_main.write_setting();
        }
        private void Form_VDP_Screen_Shown(object sender, EventArgs e)
        {
            g_viewActive = true;
            this.Location = new System.Drawing.Point(g_screen_xpos, g_screen_ypos);
            g_content_xsize_change = panel_screen.ClientSize.Width;
            g_content_ysize_change = panel_screen.ClientSize.Height;
            ForceClientSizeToImageSize();
        }
        private void ResizeClientToImageAspect()
        {
            if (WindowState != FormWindowState.Normal) return;
            if (g_sizeChanging == true) return;

            int w_imageWidth = (g_source_xsize > 0) ? g_source_xsize : g_screen_xsize_change;
            int w_imageHeight = (g_source_ysize > 0) ? g_source_ysize : g_screen_ysize_change;
            if (w_imageWidth <= 0 || w_imageHeight <= 0) return;

            int w_contentWidth = panel_screen.ClientSize.Width;
            int w_contentHeight = panel_screen.ClientSize.Height;
            if (w_contentWidth <= 0 || w_contentHeight <= 0) return;

            int w_width = Size.Width;
            int w_height = Size.Height;
            int w_nonContentWidth = Width - w_contentWidth;
            int w_nonContentHeight = Height - w_contentHeight;
            if (g_content_xsize_change <= 0 || g_content_ysize_change <= 0)
            {
                g_content_xsize_change = w_contentWidth;
                g_content_ysize_change = w_contentHeight;
            }

            if (w_contentWidth != g_content_xsize_change)
            {
                w_height = (int)(w_imageHeight * (w_contentWidth / (double)w_imageWidth)) + w_nonContentHeight;
            }
            else
            if (w_contentHeight != g_content_ysize_change)
            {
                w_width = (int)(w_imageWidth * (w_contentHeight / (double)w_imageHeight)) + w_nonContentWidth;
            }

            if (w_width == Size.Width && w_height == Size.Height) return;

            g_sizeChanging = true;
            try
            {
                Size = new Size(w_width, w_height);
            }
            finally
            {
                g_sizeChanging = false;
            }
        }
        //----------------------------------------------------------------
        //Event Handling: Painting
        //----------------------------------------------------------------
        private void UpdatePictureBox(Bitmap in_bitmap)
        {
            QueueDisplayUpdate(in_bitmap, false);
        }

        private void QueueDisplayUpdate(Bitmap in_bitmap, bool in_screenSizeChanged)
        {
            if (IsDisposed == true)
            {
                in_bitmap.Dispose();
                return;
            }

            try
            {
                if (IsHandleCreated == false)
                {
                    in_bitmap.Dispose();
                    return;
                }
                BeginInvoke(new Action<Bitmap, bool>(ApplyDisplayUpdate), in_bitmap, in_screenSizeChanged);
            }
            catch (ObjectDisposedException)
            {
                in_bitmap.Dispose();
            }
            catch (InvalidOperationException)
            {
                in_bitmap.Dispose();
            }
        }

        private void ApplyDisplayUpdate(Bitmap in_bitmap, bool in_screenSizeChanged)
        {
            if (IsUpdateTargetAvailable() == false || Visible == false || WindowState == FormWindowState.Minimized)
            {
                in_bitmap.Dispose();
                return;
            }

            pictureBox_screen.Image?.Dispose();
            pictureBox_screen.Image = in_bitmap;
            if (in_screenSizeChanged == true)
            {
                ForceClientSizeToImageSize();
            }
            Invalidate();
        }

        public void picture_update(Bitmap in_bitmap, int in_screen_xsize, int in_screen_ysize)
        {
            if (IsDisposed == true || g_viewActive == false) return;

            bool w_screenSizeChanged;
            lock (g_bitmapLock)
            {
                w_screenSizeChanged = g_source_xsize != in_screen_xsize || g_source_ysize != in_screen_ysize;
                g_source_bitmap?.Dispose();
                g_source_bitmap = new Bitmap(in_bitmap);
                g_source_xsize = in_screen_xsize;
                g_source_ysize = in_screen_ysize;
            }

            Bitmap bmp_dst = CreateDisplayBitmap(in_bitmap, in_screen_xsize, in_screen_ysize);
            videoRecordingAddFrame(bmp_dst);
            QueueDisplayUpdate(bmp_dst, w_screenSizeChanged);
        }

        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            g_gridEnabled = gridToolStripMenuItem.Checked;
            RefreshImageFromSource();
            md_main.write_setting();
        }

        private void vdpMenuSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            g_screenshotEnabled = screenshotToolStripMenuItem.Checked;
            g_videoRecordingEnabled = videoRecordingToolStripMenuItem.Checked;
            md_main.write_setting();
        }

        public void SetMenuSetting(bool in_grid, bool in_screenshot, bool in_videoRecording)
        {
            g_gridEnabled = in_grid;
            g_screenshotEnabled = in_screenshot;
            g_videoRecordingEnabled = in_videoRecording;
            gridToolStripMenuItem.Checked = in_grid;
            screenshotToolStripMenuItem.Checked = in_screenshot;
            videoRecordingToolStripMenuItem.Checked = in_videoRecording;
        }

        public string GetMenuSettingText()
        {
            return ((g_gridEnabled == true) ? "1" : "0")
                + ":" + ((g_screenshotEnabled == true) ? "1" : "0")
                + ":" + ((g_videoRecordingEnabled == true) ? "1" : "0");
        }

        public void SyncScreenshot(string in_timeStamp)
        {
            if (g_screenshotEnabled == false) return;

            try
            {
                Image? w_image = pictureBox_screen.Image;
                if (w_image == null) return;

                string w_directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MDTracer", "Screenshot");
                Directory.CreateDirectory(w_directoryPath);

                string w_filePath = CreateCaptureFilePath(w_directoryPath, "screenshot", in_timeStamp, ".png");
                using Bitmap w_bitmap = new Bitmap(w_image);
                w_bitmap.Save(w_filePath, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "VDP Screenshot");
            }
        }

        public void SyncVideoRecordingStart(string in_timeStamp)
        {
            if (g_videoRecordingEnabled == false) return;

            try
            {
                Image? w_image = pictureBox_screen.Image;
                int w_width = w_image?.Width ?? 0;
                int w_height = w_image?.Height ?? 0;
                if (w_width <= 0 || w_height <= 0) return;

                string w_directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MDTracer", "Video");
                Directory.CreateDirectory(w_directoryPath);

                string w_filePath = CreateCaptureFilePath(w_directoryPath, "video", in_timeStamp, ".avi");
                lock (g_videoRecordLock)
                {
                    g_videoRecorder?.Dispose();
                    g_videoRecorder = new Form_Main_AviRecorder(w_filePath, w_width, w_height, 60);
                    g_videoRecordPath = w_filePath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "VDP Video Recording");
            }
        }

        public void SyncVideoRecordingStop()
        {
            Form_Main_AviRecorder? w_recorder;
            lock (g_videoRecordLock)
            {
                if (g_videoRecorder == null) return;

                w_recorder = g_videoRecorder;
                g_videoRecorder = null;
                g_videoRecordPath = "";
            }

            w_recorder.Dispose();
        }

        public void SyncVideoRecordingAddAudioSamples(byte[] in_buffer, int in_offset, int in_count)
        {
            Form_Main_AviRecorder? w_recorder;
            lock (g_videoRecordLock)
            {
                w_recorder = g_videoRecorder;
            }
            if (w_recorder == null) return;

            try
            {
                w_recorder.AddAudioSamples(in_buffer, in_offset, in_count);
            }
            catch
            {
                lock (g_videoRecordLock)
                {
                    if (ReferenceEquals(g_videoRecorder, w_recorder) == true)
                    {
                        g_videoRecorder = null;
                        g_videoRecordPath = "";
                    }
                }
                w_recorder.Dispose();
            }
        }

        private void RefreshImageFromSource()
        {
            Bitmap? w_source;
            int w_xsize;
            int w_ysize;
            lock (g_bitmapLock)
            {
                if (g_source_bitmap == null) return;
                w_source = new Bitmap(g_source_bitmap);
                w_xsize = g_source_xsize;
                w_ysize = g_source_ysize;
            }

            using (w_source)
            {
                UpdatePictureBox(CreateDisplayBitmap(w_source, w_xsize, w_ysize));
            }
        }

        private Bitmap CreateDisplayBitmap(Bitmap in_bitmap, int in_screen_xsize, int in_screen_ysize)
        {
            int w_width = Math.Min(in_screen_xsize, in_bitmap.Width);
            int w_height = Math.Min(in_screen_ysize, in_bitmap.Height);
            if (w_width <= 0) w_width = in_bitmap.Width;
            if (w_height <= 0) w_height = in_bitmap.Height;

            Bitmap w_sourceBitmap = new Bitmap(w_width, w_height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(w_sourceBitmap))
            {
                g.DrawImage(in_bitmap, new Rectangle(0, 0, w_width, w_height), new Rectangle(0, 0, w_width, w_height), GraphicsUnit.Pixel);
                if (g_gridEnabled == true)
                {
                    using Pen w_pen = new Pen(Color.FromArgb(128, 255, 255, 255), 1);
                    for (int x = 0; x < w_width; x += 8) g.DrawLine(w_pen, x, 0, x, w_height - 1);
                    for (int y = 0; y < w_height; y += 8) g.DrawLine(w_pen, 0, y, w_width - 1, y);
                }
            }
            return w_sourceBitmap;
        }

        private bool IsUpdateTargetAvailable()
        {
            return IsDisposed == false
                && Disposing == false
                && pictureBox_screen.IsDisposed == false
                && pictureBox_screen.Disposing == false
                && IsHandleCreated == true;
        }

        private void ForceClientSizeToImageSize()
        {
            if (IsUpdateTargetAvailable() == false) return;
            int w_imageWidth = (g_source_xsize > 0) ? g_source_xsize : g_screen_xsize_change;
            int w_imageHeight = (g_source_ysize > 0) ? g_source_ysize : g_screen_ysize_change;
            if (w_imageWidth <= 0 || w_imageHeight <= 0) return;
            if (WindowState != FormWindowState.Normal) return;

            g_sizeChanging = true;
            try
            {
                ClientSize = new Size(w_imageWidth, w_imageHeight + menuStrip1.Height);
                g_content_xsize_change = panel_screen.ClientSize.Width;
                g_content_ysize_change = panel_screen.ClientSize.Height;
            }
            finally
            {
                g_sizeChanging = false;
            }
            RefreshImageFromSource();
        }

        private void videoRecordingAddFrame(Bitmap in_bitmap)
        {
            Form_Main_AviRecorder? w_recorder;
            lock (g_videoRecordLock)
            {
                w_recorder = g_videoRecorder;
            }
            if (w_recorder == null) return;

            try
            {
                w_recorder.AddFrame(in_bitmap);
            }
            catch
            {
                lock (g_videoRecordLock)
                {
                    if (ReferenceEquals(g_videoRecorder, w_recorder) == true)
                    {
                        g_videoRecorder = null;
                        g_videoRecordPath = "";
                    }
                }
                w_recorder.Dispose();
            }
        }

        private string CreateCaptureFilePath(string in_directoryPath, string in_captureType, string in_timeStamp, string in_extension)
        {
            string w_romName = md_main.g_state_capture_rom_file_name;
            if (string.IsNullOrEmpty(w_romName) == true) w_romName = in_captureType;
            foreach (char w_char in Path.GetInvalidFileNameChars())
            {
                w_romName = w_romName.Replace(w_char, '_');
            }

            string w_screenName = string.IsNullOrEmpty(g_screen_type) ? "screen" : "screen" + g_screen_type;
            string w_filePrefix = w_romName + "_" + in_captureType + "_" + in_timeStamp + "_" + w_screenName;
            string w_filePath = Path.Combine(in_directoryPath, w_filePrefix + in_extension);
            int w_suffix = 1;
            while (File.Exists(w_filePath) == true)
            {
                w_filePath = Path.Combine(in_directoryPath, w_filePrefix + "_" + w_suffix + in_extension);
                w_suffix++;
            }
            return w_filePath;
        }
    }
}
