using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
namespace MDTracer
{
    public partial class Form_Main : Form
    {
        public static Form_Main Instance { get; private set; }
        public static int g_screen_size_x;
        public static int g_screen_size_y;
        public static int g_screen_xpos;
        public static int g_screen_ypos;
        public static int g_mouseclick_pos_x;
        public static int g_mouseclick_pos_y;
        public static bool g_mouseclick_interrupt;
        public static string[] g_file_name;
        private static bool g_filelist_view;
        private Bitmap g_work_bitmap = null;
        private int g_work_bitmapW = -1;
        private int g_work_bitmapH = -1;
        private readonly object g_videoRecordLock = new object();
        private Form_Main_AviRecorder? g_videoRecorder;
        private string g_videoRecordPath = "";

        //----------------------------------------------------------------
        //form
        //----------------------------------------------------------------
        public Form_Main()
        {
            InitializeComponent();
            Instance = this;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
        }
        //----------------------------------------------------------------
        //Event Handling: Screen Operations
        //----------------------------------------------------------------
        private void Form_Main_Load(object sender, EventArgs e)
        {
            g_file_name = new string[9];
            pictureBox_game.Image = new Bitmap(320, 240);
            pictureBox_game.BackColor = Color.Black;
            md_main.initialize();
            md_main.read_setting();
            md_main.g_form_setting.update();
            md_main.g_form_setting.show_window();
            md_main.g_form_screenA.initialize("A", 256, 256, "screen A");
            md_main.g_form_screenB.initialize("B", 256, 256, "screen B");
            md_main.g_form_screenW.initialize("W", 256, 256, "screen W");
            md_main.g_form_screenS.initialize("S", 512, 512, "screen S");
            md_main.g_form_io.initialize();
            md_main.g_form_music.initialize();
            md_main.g_md_music.setting();
            md_main.g_form_io.rescan();
            g_filelist_view = true;
            this.Location = new System.Drawing.Point(g_screen_xpos, g_screen_ypos);
            BringToFront();
        }

        private void Form_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            videoRecordingStop(false);
            vdpScreensVideoRecordingStop();
            md_main.g_md_io.input_record_stop();
            md_main.g_md_io.input_replay_stop();
        }
        private void Form_Main_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Normal) return;
            if (g_filelist_view == false)
            {
                int w_x = this.Size.Width;
                int w_y = this.Size.Height;
                if ((w_x - 16) != g_screen_size_x)
                {
                    w_y = (int)(224 * ((w_x - 16) / 320.0f)) + 85;
                }
                else
                    if ((w_y - 85) != g_screen_size_y)
                    {
                        w_x = (int)(320 * ((w_y - 85) / 224.0f)) + 16;
                    }
                this.Size = new Size(w_x, w_y);
            }
        }
        private void Form_Main_ResizeEnd(object sender, EventArgs e)
        {
            if (g_filelist_view == false)
            {
                g_screen_xpos = this.Location.X;
                g_screen_ypos = this.Location.Y;
                g_screen_size_x = this.Width - 16;
                g_screen_size_y = this.Height - 85;
                md_main.write_setting();
            }
        }

        private void pictureBox_game_Paint(object sender, PaintEventArgs e)
        {
            if (g_filelist_view == true)
            {
                Font wfont = new Font("ÇlÇr ÉSÉVÉbÉN", 10);
                Brush wbrush = Brushes.White;
                e.Graphics.DrawString("file select", wfont, wbrush, new PointF(20, 20));
                e.Graphics.DrawString("(F key: Select in File Explorer)", wfont, wbrush, new PointF(30, 40));
                for (int i = 0; i < 9; i++)
                {
                    string w_filename = Path.GetFileName(g_file_name[i]);
                    e.Graphics.DrawString((i + 1) + ": " + w_filename, wfont, wbrush, new PointF(30, 60 + i * 15));
                }
            }
        }

        //----------------------------------------------------------------
        //Event Handling: key operations
        //----------------------------------------------------------------
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (g_filelist_view == false)
            {
                switch (keyData)
                {
                    case Keys.Escape:
                        stopGame();
                        return true;
                    case Keys.F1:
                        StateSave();
                        return true;
                    case Keys.F2:
                        StateRestoreLatest();
                        return true;
                    case Keys.F3:
                        state_capture_list();
                        return true;
                    case Keys.F5:
                        if (md_main.g_md_io.g_input_replaying == true)
                        {
                            input_capture_Stop();
                        }
                        else if (md_main.g_md_io.g_input_recording == true)
                        {
                            input_capture_Stop();
                        }
                        else
                        {
                            input_capture_Start();
                        }
                        return true;
                    case Keys.F6:
                        if (md_main.g_md_io.g_input_replaying == true)
                        {
                            input_capture_Stop();
                        }
                        else
                        {
                            input_capture_ReplayLatest();
                        }
                        return true;
                    case Keys.F7:
                        input_capture_list();
                        return true;

                    case Keys.F9:
                        frameAdvance();
                        return true;
                    case Keys.F10:
                        screenshot();
                        return true;
                    case Keys.F11:
                        videoRecordingToggle();
                        return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void Form_Main_KeyDown(object sender, KeyEventArgs e)
        {
            string w_filename = "";
            switch (e.KeyCode)
            {
                case Keys.F:
                    if (g_filelist_view == true)
                    {
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            w_filename = openFileDialog1.FileName;
                        }
                    }
                    break;
                case Keys.D1: w_filename = g_file_name[0]; break;
                case Keys.D2: w_filename = g_file_name[1]; break;
                case Keys.D3: w_filename = g_file_name[2]; break;
                case Keys.D4: w_filename = g_file_name[3]; break;
                case Keys.D5: w_filename = g_file_name[4]; break;
                case Keys.D6: w_filename = g_file_name[5]; break;
                case Keys.D7: w_filename = g_file_name[6]; break;
                case Keys.D8: w_filename = g_file_name[7]; break;
                case Keys.D9: w_filename = g_file_name[8]; break;
            }
            if ((g_filelist_view == true) && (w_filename != null) && (w_filename != ""))
            {
                if (true == md_main.run(w_filename))
                {
                    file_list_update(w_filename);
                    this.MaximumSize = new Size(0, 0);
                    this.MinimumSize = new Size(0, 0);
                    this.Location = new System.Drawing.Point(g_screen_xpos, g_screen_ypos);
                    this.Width = g_screen_size_x + 16;
                    this.Height = g_screen_size_y + 85;
                    g_filelist_view = false;
                }
            }
        }

        //----------------------------------------------------------------
        //Event Handling: menu
        //----------------------------------------------------------------
        private void SettingMenuItem1_Click(object sender, EventArgs e)
        {
            md_main.g_form_setting.Show();
        }
        private void hardResetMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to reset?", "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            md_main.request_hard_reset();
        }
        private void input_capture_ReplayMenuItem_Click(object sender, EventArgs e)
        {
            input_capture_ReplayLatest();
        }

        private void input_capture_RecodeMenuItem_Click(object sender, EventArgs e)
        {
            input_capture_Start();
        }

        private void input_capture_ListMenuItem_Click(object sender, EventArgs e)
        {
            input_capture_list();
        }
        private void state_capture_saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StateSave();
        }
        private void state_capture_loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StateRestoreLatest();
        }
        private void state_capture_state_capture_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_capture_list();
        }
        private void pauseResumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopGame();
        }
        private void frameAdvanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frameAdvance();
        }
        private void screenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            screenshot();
        }
        private void videoRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoRecordingToggle();
        }
        private void openVideoFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openSaveFolder("Video");
        }
        private void openCaptureFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openSaveFolder("Screenshot");
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newForm = new Form_About();
            newForm.ShowDialog();
        }
        private static void openSaveFolder(string in_folderName)
        {
            string w_directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MDTracer", in_folderName);
            Directory.CreateDirectory(w_directoryPath);
            Process.Start(new ProcessStartInfo
            {
                FileName = w_directoryPath,
                UseShellExecute = true,
            });
        }

        //----------------------------------------------------------------
        //Event Handling: Painting
        //----------------------------------------------------------------
        private readonly object g_bitmapLock = new object();
        public void picture_update(int in_cpu)
        {
            if (this.WindowState == FormWindowState.Minimized) return;
            g_filelist_view = false;
            toolStripStatusLabel1.Text = "task usage:" + in_cpu + "%" + (md_main.g_md_io.g_input_recording ? " REC" : "") + (md_main.g_md_io.g_input_replaying ? " PLAY" : "") + (g_videoRecorder != null ? " VIDEO" : "") + (md_main.g_state_capture_status == "" ? "" : " " + md_main.g_state_capture_status);
            lock (g_bitmapLock)
            {
                int w_bitmap_x = panel_game.ClientSize.Width;
                int w_bitmap_y = panel_game.ClientSize.Height;
                float w_cx = (float)md_main.g_md_vdp.g_display_xsize / (float)w_bitmap_x;
                float w_cy = (float)md_main.g_md_vdp.g_display_ysize / (float)w_bitmap_y;

                if (g_work_bitmap == null || g_work_bitmapW != w_bitmap_x || g_work_bitmapH != w_bitmap_y)
                {
                    g_work_bitmap?.Dispose();
                    g_work_bitmap = new Bitmap(w_bitmap_x, w_bitmap_y, PixelFormat.Format32bppArgb);
                    g_work_bitmapW = w_bitmap_x;
                    g_work_bitmapH = w_bitmap_y;
                }
                BitmapData game_bmpData = g_work_bitmap.LockBits(new Rectangle(0, 0, g_work_bitmap.Width, g_work_bitmap.Height),
                                            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr dest_ptr = game_bmpData.Scan0;
                int dest_stride = game_bmpData.Stride;
                const int bytesPerPixel = 4;
                unsafe
                {
                    float w_dy = 0;
                    for (int wy = 0; wy < w_bitmap_y; wy++)
                    {
                        uint* pixel = (uint*)dest_ptr;
                        float w_dx = 0;
                        int w_base = (int)w_dy * md_main.g_md_vdp.g_display_xsize;
                        for (int wx = 0; wx < w_bitmap_x; wx++)
                        {
                            *pixel = md_main.g_md_vdp.g_game_screen[w_base + (int)w_dx];
                            w_dx += w_cx;
                            pixel = (uint*)((IntPtr)pixel + bytesPerPixel);
                        }
                        dest_ptr += dest_stride;
                        w_dy += w_cy;
                    }
                }
                g_work_bitmap.UnlockBits(game_bmpData);
                videoRecordingAddFrame(g_work_bitmap);
                try
                {
                    this.Invoke((Action)(() =>
                    {
                        pictureBox_game.Image?.Dispose();
                        pictureBox_game.Image = new Bitmap(g_work_bitmap);
                        pictureBox_game.Width = w_bitmap_x;
                        pictureBox_game.Height = w_bitmap_y;
                    }));
                }
                catch { }
            }
        }
        //----------------------------------------------------------------
        //Sub
        //----------------------------------------------------------------
        private void stopGame()
        {
            bool w_stopped = md_main.request_stop();
            toolStripStatusLabel1.Text = w_stopped ? "Pause" : "";
        }
        private void frameAdvance()
        {
            bool w_requested = md_main.request_frame_advance();
        }

        private void screenshot()
        {
            try
            {
                Image? w_image = pictureBox_game.Image;
                if (w_image == null)
                {
                    toolStripStatusLabel1.Text = "screenshot image is empty";
                    return;
                }

                string w_basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string w_directoryPath = Path.Combine(w_basePath, "MDTracer", "Screenshot");
                Directory.CreateDirectory(w_directoryPath);

                string w_romName = md_main.g_state_capture_rom_file_name;
                if (string.IsNullOrEmpty(w_romName) == true) w_romName = "screenshot";
                foreach (char w_char in Path.GetInvalidFileNameChars())
                {
                    w_romName = w_romName.Replace(w_char, '_');
                }

                string w_timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string w_filePrefix = w_romName + "_screenshot_" + w_timeStamp + "_screen";
                string w_filePath = Path.Combine(w_directoryPath, w_filePrefix + ".png");
                int w_suffix = 1;
                while (File.Exists(w_filePath) == true)
                {
                    w_filePath = Path.Combine(w_directoryPath, w_filePrefix + "_" + w_suffix + ".png");
                    w_suffix++;
                }

                using Bitmap w_bitmap = new Bitmap(w_image);
                w_bitmap.Save(w_filePath, ImageFormat.Png);
                vdpScreensScreenshot(w_timeStamp);
                toolStripStatusLabel1.Text = "screenshot saved: " + Path.GetFileName(w_filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Screenshot");
            }
        }
        private void videoRecordingToggle()
        {
            lock (g_videoRecordLock)
            {
                if (g_videoRecorder != null)
                {
                    videoRecordingStop(true);
                    return;
                }
            }

            videoRecordingStart();
        }
        private void videoRecordingStart()
        {
            try
            {
                Image? w_image = pictureBox_game.Image;
                int w_width = g_work_bitmap?.Width ?? w_image?.Width ?? 0;
                int w_height = g_work_bitmap?.Height ?? w_image?.Height ?? 0;
                if (w_width <= 0 || w_height <= 0)
                {
                    toolStripStatusLabel1.Text = "video recording image is empty";
                    return;
                }

                string w_basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string w_directoryPath = Path.Combine(w_basePath, "MDTracer", "Video");
                Directory.CreateDirectory(w_directoryPath);

                string w_romName = md_main.g_state_capture_rom_file_name;
                if (string.IsNullOrEmpty(w_romName) == true) w_romName = "video";
                foreach (char w_char in Path.GetInvalidFileNameChars())
                {
                    w_romName = w_romName.Replace(w_char, '_');
                }

                string w_timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string w_filePrefix = w_romName + "_video_" + w_timeStamp + "_screen";
                string w_filePath = Path.Combine(w_directoryPath, w_filePrefix + ".avi");
                int w_suffix = 1;
                while (File.Exists(w_filePath) == true)
                {
                    w_filePath = Path.Combine(w_directoryPath, w_filePrefix + "_" + w_suffix + ".avi");
                    w_suffix++;
                }

                lock (g_videoRecordLock)
                {
                    g_videoRecorder = new Form_Main_AviRecorder(w_filePath, w_width, w_height, 60);
                    g_videoRecordPath = w_filePath;
                }
                videoRecordingToolStripMenuItem.Checked = true;
                vdpScreensVideoRecordingStart(w_timeStamp);
                toolStripStatusLabel1.Text = "video recording: " + Path.GetFileName(w_filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Video Recording");
            }
        }
        private void videoRecordingStop(bool in_updateStatus)
        {
            Form_Main_AviRecorder? w_recorder;
            string w_filePath;
            int w_frameCount;
            lock (g_videoRecordLock)
            {
                if (g_videoRecorder == null) return;

                w_recorder = g_videoRecorder;
                w_filePath = g_videoRecordPath;
                w_frameCount = w_recorder.FrameCount;
                g_videoRecorder = null;
                g_videoRecordPath = "";
            }

            w_recorder.Dispose();
            vdpScreensVideoRecordingStop();
            videoRecordingToolStripMenuItem.Checked = false;
            if (in_updateStatus == true)
            {
                toolStripStatusLabel1.Text = "video saved: " + Path.GetFileName(w_filePath) + " (" + w_frameCount + " frames)";
            }
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
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
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
                vdpScreensVideoRecordingStop();
                BeginInvoke((Action)(() =>
                {
                    videoRecordingToolStripMenuItem.Checked = false;
                    toolStripStatusLabel1.Text = "video recording error: " + ex.Message;
                }));
            }
        }
        public void videoRecordingAddAudioSamples(byte[] in_buffer, int in_offset, int in_count)
        {
            Form_Main_AviRecorder? w_recorder;
            lock (g_videoRecordLock)
            {
                w_recorder = g_videoRecorder;
            }
            if (w_recorder != null)
            {
                try
                {
                    w_recorder.AddAudioSamples(in_buffer, in_offset, in_count);
                }
                catch (ObjectDisposedException)
                {
                }
            }
            vdpScreensVideoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
        }

        private void vdpScreensScreenshot(string in_timeStamp)
        {
            md_main.g_form_screenA.SyncScreenshot(in_timeStamp);
            md_main.g_form_screenB.SyncScreenshot(in_timeStamp);
            md_main.g_form_screenW.SyncScreenshot(in_timeStamp);
            md_main.g_form_screenS.SyncScreenshot(in_timeStamp);
        }
        private void vdpScreensVideoRecordingStart(string in_timeStamp)
        {
            md_main.g_form_screenA.SyncVideoRecordingStart(in_timeStamp);
            md_main.g_form_screenB.SyncVideoRecordingStart(in_timeStamp);
            md_main.g_form_screenW.SyncVideoRecordingStart(in_timeStamp);
            md_main.g_form_screenS.SyncVideoRecordingStart(in_timeStamp);
        }
        private void vdpScreensVideoRecordingStop()
        {
            md_main.g_form_screenA.SyncVideoRecordingStop();
            md_main.g_form_screenB.SyncVideoRecordingStop();
            md_main.g_form_screenW.SyncVideoRecordingStop();
            md_main.g_form_screenS.SyncVideoRecordingStop();
        }
        private void vdpScreensVideoRecordingAddAudioSamples(byte[] in_buffer, int in_offset, int in_count)
        {
            md_main.g_form_screenA.SyncVideoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
            md_main.g_form_screenB.SyncVideoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
            md_main.g_form_screenW.SyncVideoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
            md_main.g_form_screenS.SyncVideoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
        }
        private void state_capture_list()
        {
            try
            {
                using Form_Main_StateCapture_list w_form = new Form_Main_StateCapture_list();
                if (w_form.ShowDialog(this) != DialogResult.OK || w_form.SelectedEntry == null) return;

                md_main.request_state_capture_restore_file(w_form.SelectedEntry.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "State Capture History");
            }
        }
        private void StateSave()
        {
            if (md_main.StateStore.IsAvailable() == false) return;
            md_main.request_state_capture_save();
        }
        private void StateRestoreLatest()
        {
            md_main.request_state_capture_restore_latest();
        }
        private void input_capture_list()
        {
            try
            {
                using Form_Main_InputCapture_list w_form = new Form_Main_InputCapture_list();
                if (w_form.ShowDialog(this) != DialogResult.OK || w_form.SelectedEntry == null) return;

                md_main.input_capture_replay_file(w_form.SelectedEntry.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Input Capture History");
            }
        }
        private void input_capture_Start()
        {
            md_main.input_capture_record_start();
        }
        private void input_capture_Stop()
        {
            md_main.input_capture_record_stop();
        }
        private void input_capture_ReplayLatest()
        {
            md_main.input_capture_restore_latest();
        }
        private void input_capture_ReplayStop()
        {
            md_main.input_capture_replay_stop();
        }
        private void file_list_update(string in_file)
        {
            for (int i = 0; i < 8; i++)
            {
                if (g_file_name[i] == in_file)
                {
                    for (int m = i; m < 8; m++)
                    {
                        g_file_name[m] = g_file_name[m + 1];
                    }
                    break;
                }
            }
            for (int i = 8; i >= 1; i--)
            {
                g_file_name[i] = g_file_name[i - 1];
            }
            g_file_name[0] = in_file;
            md_main.write_setting();
        }

    }
}
