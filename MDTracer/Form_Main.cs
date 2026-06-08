using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using GenesisEmu.Frontend.Windows;
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
        public static GameScreenScaleMode g_scale_mode = GameScreenScaleMode.IntegerFit;
        private static bool g_filelist_view;
        private Bitmap? g_work_bitmap;
        private Bitmap? g_display_bitmap;
        private int g_work_bitmapW = -1;
        private int g_work_bitmapH = -1;
        private int g_frame_update_pending;
        private readonly object g_videoRecordLock = new object();
        private readonly WinFormsFullscreenHelper g_fullscreen = new();
        private ToolStripMenuItem? g_scaleIntegerMenuItem;
        private ToolStripMenuItem? g_scaleStretchMenuItem;
        private ToolStripMenuItem? g_fullscreenMenuItem;
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
            InitializeViewMenu();
        }

        private void InitializeViewMenu()
        {
            var w_viewMenu = new ToolStripMenuItem("&View");
            g_scaleIntegerMenuItem = new ToolStripMenuItem(
                "&Integer Scale (letterbox)",
                null,
                (_, _) => SetScaleMode(GameScreenScaleMode.IntegerFit));
            g_scaleStretchMenuItem = new ToolStripMenuItem(
                "&Stretch to Window",
                null,
                (_, _) => SetScaleMode(GameScreenScaleMode.Stretch));
            g_fullscreenMenuItem = new ToolStripMenuItem(
                "&Fullscreen",
                null,
                (_, _) => ToggleFullscreen());
            g_fullscreenMenuItem.ShortcutKeys = Keys.Alt | Keys.Enter;
            w_viewMenu.DropDownItems.Add(g_scaleIntegerMenuItem);
            w_viewMenu.DropDownItems.Add(g_scaleStretchMenuItem);
            w_viewMenu.DropDownItems.Add(new ToolStripSeparator());
            w_viewMenu.DropDownItems.Add(g_fullscreenMenuItem);
            menuStrip1.Items.Insert(3, w_viewMenu);
            UpdateScaleMenuChecks();
        }
        //----------------------------------------------------------------
        //Event Handling: Screen Operations
        //----------------------------------------------------------------
        private void Form_Main_Load(object sender, EventArgs e)
        {
            g_file_name = new string[9];
            pictureBox_game.Image = new Bitmap(320, 240);
            pictureBox_game.BackColor = Color.Black;
            MDTracer.Platform.Windows.WindowsPlatformServices.Register();
            md_main.initialize();
            WinFormsDebugTools.Initialize();
            md_main.read_setting();
            if (g_screen_size_x == 0 && g_screen_size_y == 0)
            {
                g_screen_size_x = 640;
                g_screen_size_y = 448;
            }
            md_main.g_frontendSettings.NotifyDebugWindowLayoutChanged();
            WinFormsDebugTools.g_form_screenA.initialize("A", 256, 256, "screen A");
            WinFormsDebugTools.g_form_screenB.initialize("B", 256, 256, "screen B");
            WinFormsDebugTools.g_form_screenW.initialize("W", 256, 256, "screen W");
            WinFormsDebugTools.g_form_screenS.initialize("S", 512, 512, "screen S");
            WinFormsDebugTools.g_form_io.initialize();
            WinFormsDebugTools.g_form_music.initialize();
            md_main.g_md_music.setting();
            WinFormsDebugTools.g_form_io.rescan();
            g_filelist_view = true;
            this.Location = new System.Drawing.Point(g_screen_xpos, g_screen_ypos);
            UpdateScaleMenuChecks();
            BringToFront();
        }

        private void Form_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            videoRecordingStop(false);
            vdpScreensVideoRecordingStop();
            md_main.g_md_io.input_record_stop();
            md_main.g_md_io.input_replay_stop();
            md_main.g_md_sram.save();
            WinFormsDebugTools.g_form_code.SaveCurrentGameCodeSettings();
            WinFormsDebugTools.g_form_code.FlushCodeToolLayoutSave();
            md_main.write_setting();
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

        private void Form_Main_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form_Main_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is not string[] w_files || w_files.Length == 0) return;
            StartGameWithRom(w_files[0]);
        }

        private void StartGameWithRom(string in_path)
        {
            if (string.IsNullOrEmpty(in_path) == true) return;
            if (md_main.run(in_path) == false) return;

            file_list_update(in_path);
            this.MaximumSize = new Size(0, 0);
            this.MinimumSize = new Size(0, 0);
            this.Location = new System.Drawing.Point(g_screen_xpos, g_screen_ypos);
            this.Width = g_screen_size_x + 16;
            this.Height = g_screen_size_y + 85;
            g_filelist_view = false;
            WinFormsDebugTools.g_form_setting.show_window();
        }

        private void pictureBox_game_Paint(object sender, PaintEventArgs e)
        {
            if (g_filelist_view == true)
            {
                using Font wfont = new Font(SystemFonts.MessageBoxFont.FontFamily, 10);
                Brush wbrush = Brushes.White;
                e.Graphics.DrawString("ROM Select", wfont, wbrush, new PointF(20, 20));
                e.Graphics.DrawString("(F: browse, 1-9: recent, or drag a ROM file here)", wfont, wbrush, new PointF(30, 40));
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
            if (keyData == (Keys.Alt | Keys.Enter))
            {
                ToggleFullscreen();
                return true;
            }

            if (g_fullscreen.IsFullscreen == true && keyData == Keys.Escape)
            {
                ToggleFullscreen();
                return true;
            }

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
                        InputCaptureRecordToggle();
                        return true;
                    case Keys.F3:
                        StateSaveWithInputCaptureRecordToggle();
                        return true;
                    case Keys.F4:
                        StateRestoreLatestWithMatchingInputReplay();
                        return true;
                    case Keys.Control | Keys.F4:
                        capture_list();
                        return true;
                    case Keys.F5:
                        frameAdvance();
                        return true;
                    case Keys.F9:
                        WinFormsDebugTools.g_form_setting.Show();
                        return true;
                    case Keys.F10:
                        screenshot();
                        return true;
                    case Keys.F11:
                        videoRecordingToggle();
                        return true;
                    case Keys.F12:
                        hardReset();
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
                StartGameWithRom(w_filename);
            }
        }

        //----------------------------------------------------------------
        //Event Handling: menu
        //----------------------------------------------------------------
        private void SettingMenuItem1_Click(object sender, EventArgs e)
        {
            WinFormsDebugTools.g_form_setting.Show();
        }
        private void hardResetMenuItem_Click(object sender, EventArgs e)
        {
            hardReset();
        }

        private void hardReset()
        {
            if (MessageBox.Show("Are you sure you want to reset?", "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            md_main.request_hard_reset();
        }
        private void input_capture_ReplayMenuItem_Click(object sender, EventArgs e)
        {
            input_capture_ReplayLatest();
        }

        private void input_capture_RecodeMenuItem_Click(object sender, EventArgs e)
        {
            InputCaptureRecordToggle();
        }

        private void input_capture_ListMenuItem_Click(object sender, EventArgs e)
        {
            capture_list();
        }
        private void state_capture_saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StateSave();
        }
        private void state_capture_loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StateRestoreLatestWithMatchingInputReplay();
        }
        private void state_capture_state_capture_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            capture_list();
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
            if (IsDisposed == true || IsHandleCreated == false) return;
            if (Interlocked.Exchange(ref g_frame_update_pending, 1) == 1) return;

            g_filelist_view = false;
            string w_statusText1 = "task usage:" + in_cpu + "%";
            string w_statusText2 = (md_main.g_md_io.g_input_recording ? "INPUT REC" : "")
                                + (md_main.g_md_io.g_input_replaying ? "INPUT PLAY" : "")
                                + (g_videoRecorder != null ? "  VIDEO REC" : "");
            string w_statusText3 = (md_main.g_state_capture_status == "" ? "" : " " + md_main.g_state_capture_status);

            try
            {
                BeginInvoke(new Action<string, string, string>(ApplyPictureUpdate), w_statusText1, w_statusText2, w_statusText3);
            }
            catch
            {
                Interlocked.Exchange(ref g_frame_update_pending, 0);
            }
        }

        private void ApplyPictureUpdate(string in_statusText1, string in_statusText2, string in_statusText3)
        {
            Interlocked.Exchange(ref g_frame_update_pending, 0);
            if (IsDisposed == true || IsHandleCreated == false) return;
            if (WindowState == FormWindowState.Minimized) return;

            Size w_clientSize = panel_game.ClientSize;
            if (w_clientSize.Width <= 0 || w_clientSize.Height <= 0) return;

            int w_bitmap_x = w_clientSize.Width;
            int w_bitmap_y = w_clientSize.Height;
            int w_sourceWidth = md_main.g_md_vdp.g_display_xsize;
            int w_sourceHeight = md_main.g_md_vdp.g_display_ysize;
            uint[] w_gameScreen = md_main.g_md_vdp.g_game_screen;

            lock (g_bitmapLock)
            {
                if (g_work_bitmap == null || g_work_bitmapW != w_bitmap_x || g_work_bitmapH != w_bitmap_y)
                {
                    g_work_bitmap?.Dispose();
                    g_display_bitmap?.Dispose();
                    g_work_bitmap = new Bitmap(w_bitmap_x, w_bitmap_y, PixelFormat.Format32bppArgb);
                    g_display_bitmap = new Bitmap(w_bitmap_x, w_bitmap_y, PixelFormat.Format32bppArgb);
                    g_work_bitmapW = w_bitmap_x;
                    g_work_bitmapH = w_bitmap_y;
                }

                WinFormsGameScreenBitmap.WriteScaledPixels(
                    w_gameScreen, w_sourceWidth, w_sourceHeight, g_work_bitmap, g_scale_mode);
                videoRecordingAddFrame(g_work_bitmap);
                (g_work_bitmap, g_display_bitmap) = (g_display_bitmap, g_work_bitmap);
            }

            UpdateGamePicture(g_display_bitmap, w_bitmap_x, w_bitmap_y, in_statusText1, in_statusText2, in_statusText3);
        }

        private Size GetGamePanelClientSize()
        {
            if (IsDisposed == true || IsHandleCreated == false) return Size.Empty;
            if (InvokeRequired == true)
            {
                try
                {
                    return (Size)Invoke(new Func<Size>(GetGamePanelClientSize));
                }
                catch
                {
                    return Size.Empty;
                }
            }

            if (WindowState == FormWindowState.Minimized) return Size.Empty;
            return panel_game.ClientSize;
        }

        private void UpdateGamePicture(Bitmap in_bitmap, int in_width, int in_height, string in_statusText1, string in_statusText2, string in_statusText3)
        {
            if (IsDisposed == true || IsHandleCreated == false) return;
            if (InvokeRequired == true)
            {
                try
                {
                    BeginInvoke(new Action<Bitmap, int, int, string, string, string>(UpdateGamePicture), in_bitmap, in_width, in_height, in_statusText1, in_statusText2, in_statusText3);
                }
                catch
                {
                }
                return;
            }

            if (WindowState == FormWindowState.Minimized) return;

            toolStripStatusLabel1.Text = in_statusText1;
            toolStripStatusLabel2.Text = in_statusText2;
            toolStripStatusLabel3.Text = in_statusText3;
            pictureBox_game.Image = in_bitmap;
            pictureBox_game.Width = in_width;
            pictureBox_game.Height = in_height;
        }

        private void SetScaleMode(GameScreenScaleMode in_mode)
        {
            g_scale_mode = in_mode;
            UpdateScaleMenuChecks();
            md_main.write_setting();
        }

        private void ToggleFullscreen()
        {
            g_fullscreen.Toggle(this, menuStrip1, statusStrip1);
            if (g_fullscreenMenuItem != null) g_fullscreenMenuItem.Checked = g_fullscreen.IsFullscreen;
        }

        public void UpdateScaleMenuChecks()
        {
            if (g_scaleIntegerMenuItem == null || g_scaleStretchMenuItem == null) return;
            g_scaleIntegerMenuItem.Checked = g_scale_mode == GameScreenScaleMode.IntegerFit;
            g_scaleStretchMenuItem.Checked = g_scale_mode == GameScreenScaleMode.Stretch;
            if (g_fullscreenMenuItem != null) g_fullscreenMenuItem.Checked = g_fullscreen.IsFullscreen;
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

                string w_timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string w_filePath = GenesisEmu.Frontend.Windows.WinFormsGameScreenshot.SavePng(
                    w_image,
                    "MDTracer",
                    md_main.g_state_capture_rom_file_name);
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
            WinFormsDebugTools.g_form_screenA.SyncScreenshot(in_timeStamp);
            WinFormsDebugTools.g_form_screenB.SyncScreenshot(in_timeStamp);
            WinFormsDebugTools.g_form_screenW.SyncScreenshot(in_timeStamp);
            WinFormsDebugTools.g_form_screenS.SyncScreenshot(in_timeStamp);
        }
        private void vdpScreensVideoRecordingStart(string in_timeStamp)
        {
            WinFormsDebugTools.g_form_screenA.SyncVideoRecordingStart(in_timeStamp);
            WinFormsDebugTools.g_form_screenB.SyncVideoRecordingStart(in_timeStamp);
            WinFormsDebugTools.g_form_screenW.SyncVideoRecordingStart(in_timeStamp);
            WinFormsDebugTools.g_form_screenS.SyncVideoRecordingStart(in_timeStamp);
        }
        private void vdpScreensVideoRecordingStop()
        {
            WinFormsDebugTools.g_form_screenA.SyncVideoRecordingStop();
            WinFormsDebugTools.g_form_screenB.SyncVideoRecordingStop();
            WinFormsDebugTools.g_form_screenW.SyncVideoRecordingStop();
            WinFormsDebugTools.g_form_screenS.SyncVideoRecordingStop();
        }
        private void vdpScreensVideoRecordingAddAudioSamples(byte[] in_buffer, int in_offset, int in_count)
        {
            WinFormsDebugTools.g_form_screenA.SyncVideoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
            WinFormsDebugTools.g_form_screenB.SyncVideoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
            WinFormsDebugTools.g_form_screenW.SyncVideoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
            WinFormsDebugTools.g_form_screenS.SyncVideoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
        }
        private void StateSave()
        {
            if (md_main.StateStore.IsAvailable() == false) return;
            md_main.request_state_capture_save();
        }
        private void InputCaptureRecordToggle()
        {
            if (md_main.g_md_io.g_input_recording == true)
            {
                input_capture_Stop();
                return;
            }
            if (md_main.g_md_io.g_input_replaying == true)
            {
                input_capture_ReplayStop();
                return;
            }
            input_capture_Start();
        }
        private void StateSaveWithInputCaptureRecordToggle()
        {
            if (md_main.g_md_io.g_input_recording == true)
            {
                input_capture_Stop();
                return;
            }
            if (md_main.g_md_io.g_input_replaying == true)
            {
                input_capture_ReplayStop();
                return;
            }

            string w_filePrefix = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
            if (md_main.StateStore.IsAvailable() == true)
            {
                md_main.request_state_capture_save(w_filePrefix);
            }
            md_main.input_capture_record_start(w_filePrefix);
        }
        private void StateRestoreLatestWithMatchingInputReplay()
        {
            CaptureListSelection? w_entry = GetLatestCaptureEntry();
            if (w_entry == null)
            {
                StateRestoreLatest();
                return;
            }

            ExecuteCaptureEntry(w_entry.StateFilePath, w_entry.InputFilePath);
        }
        private void StateRestoreLatest()
        {
            md_main.request_state_capture_restore_latest();
        }
        private CaptureListSelection? GetLatestCaptureEntry()
        {
            CaptureListEntry? w_entry = WinFormsCaptureList.BuildEntries(CaptureListMode.StateAndInput).FirstOrDefault();
            return w_entry?.ToSelection();
        }
        private void capture_list()
        {
            try
            {
                using WinFormsCaptureListDialog w_form = new WinFormsCaptureListDialog(CaptureListMode.StateAndInput);
                w_form.EntrySelected += in_entry =>
                {
                    try
                    {
                        ExecuteCaptureEntry(in_entry.StateFilePath, in_entry.InputFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Capture History");
                    }
                };
                w_form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Capture History");
            }
        }
        private void ExecuteCaptureEntry(string? in_stateFilePath, string? in_inputFilePath)
        {
            string? w_inputFilePath = in_inputFilePath;
            if (string.IsNullOrEmpty(in_stateFilePath) == false)
            {
                md_main.request_state_capture_restore_file(in_stateFilePath);
                if (string.IsNullOrEmpty(w_inputFilePath) == true)
                {
                    string w_filePrefix = Path.GetFileNameWithoutExtension(in_stateFilePath);
                    md_main.InputRecordEntry? w_inputEntry =
                        md_main.InputRecordStore.GetEntryByFileNameWithoutExtension(w_filePrefix);
                    w_inputFilePath = w_inputEntry?.FilePath;
                }
            }

            if (string.IsNullOrEmpty(w_inputFilePath) == false)
            {
                md_main.input_capture_replay_file(w_inputFilePath);
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
