using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using GenesisEmu.Frontend.Windows;
using MDTracer;
using MDTracer.Platform.Windows;

namespace GenesisEmu.Game
{
    //----------------------------------------------------------------
    // Minimal game-playing window: no debug tools, tracers, or VDP viewers.
    //----------------------------------------------------------------
    internal sealed class GameForm : Form
    {
        private const int DefaultClientWidth = 640;
        private const int DefaultClientHeight = 448;
        public static int g_screen_size_x = DefaultClientWidth;
        public static int g_screen_size_y = DefaultClientHeight;
        public static int g_screen_xpos;
        public static int g_screen_ypos;
        public static string[] g_file_name = new string[9];
        public static GameScreenScaleMode g_scale_mode = GameScreenScaleMode.IntegerFit;

        private readonly Panel g_gamePanel;
        private readonly PictureBox g_pictureBox;
        private readonly StatusStrip g_statusStrip;
        private readonly ToolStripStatusLabel g_statusCpu;
        private readonly ToolStripStatusLabel g_statusRom;
        private readonly OpenFileDialog g_openFileDialog;
        private readonly ToolStripMenuItem g_scaleStretchItem;
        private readonly ToolStripMenuItem g_scaleIntegerItem;
        private readonly ToolStripMenuItem g_fullscreenItem;
        private readonly MenuStrip g_menuStrip;
        private readonly WinFormsFullscreenHelper g_fullscreen = new();
        private readonly object g_bitmapLock = new object();
        private Bitmap? g_workBitmap;
        private Bitmap? g_displayBitmap;
        private int g_workBitmapWidth = -1;
        private int g_workBitmapHeight = -1;
        private bool g_romLoaded;
        private string? g_pendingRomPath;
        private int g_frameUpdatePending;

        public GameForm(string[] in_args)
        {
            Text = "GenesisEmu";
            ClientSize = new Size(DefaultClientWidth, DefaultClientHeight + 24);
            MinimumSize = new Size(336, 280);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            var w_menu = new MenuStrip();

            var w_fileMenu = new ToolStripMenuItem("&File");
            var w_openItem = new ToolStripMenuItem("&Open ROM...", null, (_, _) => OpenRomDialog());
            w_openItem.ShortcutKeys = Keys.Control | Keys.O;
            var w_screenshotItem = new ToolStripMenuItem("&Screenshot", null, (_, _) => Screenshot());
            w_screenshotItem.ShortcutKeys = Keys.F10;
            var w_openScreenshotFolderItem = new ToolStripMenuItem(
                "Open Screenshot &Folder",
                null,
                (_, _) => OpenScreenshotFolder());
            var w_exitItem = new ToolStripMenuItem("E&xit", null, (_, _) => Close());
            w_fileMenu.DropDownItems.Add(w_openItem);
            w_fileMenu.DropDownItems.Add(new ToolStripSeparator());
            w_fileMenu.DropDownItems.Add(w_screenshotItem);
            w_fileMenu.DropDownItems.Add(w_openScreenshotFolderItem);
            w_fileMenu.DropDownItems.Add(new ToolStripSeparator());
            w_fileMenu.DropDownItems.Add(w_exitItem);

            var w_emulationMenu = new ToolStripMenuItem("&Emulation");
            var w_pauseItem = new ToolStripMenuItem("&Pause / Resume", null, (_, _) => TogglePause());
            w_pauseItem.ShortcutKeys = Keys.Escape;
            var w_frameItem = new ToolStripMenuItem("&Frame Advance", null, (_, _) => FrameAdvance());
            w_frameItem.ShortcutKeys = Keys.F5;
            var w_resetItem = new ToolStripMenuItem("&Reset", null, (_, _) => RequestHardReset());
            w_resetItem.ShortcutKeys = Keys.F12;
            var w_saveItem = new ToolStripMenuItem("&Save State", null, (_, _) => SaveState());
            w_saveItem.ShortcutKeys = Keys.F1;
            var w_loadItem = new ToolStripMenuItem("&Load State", null, (_, _) => LoadState());
            w_loadItem.ShortcutKeys = Keys.F4;
            w_emulationMenu.DropDownItems.Add(w_pauseItem);
            w_emulationMenu.DropDownItems.Add(w_frameItem);
            w_emulationMenu.DropDownItems.Add(w_resetItem);
            w_emulationMenu.DropDownItems.Add(new ToolStripSeparator());
            w_emulationMenu.DropDownItems.Add(w_saveItem);
            w_emulationMenu.DropDownItems.Add(w_loadItem);

            var w_viewMenu = new ToolStripMenuItem("&View");
            g_scaleIntegerItem = new ToolStripMenuItem(
                "&Integer Scale (letterbox)",
                null,
                (_, _) => SetScaleMode(GameScreenScaleMode.IntegerFit));
            g_scaleStretchItem = new ToolStripMenuItem(
                "&Stretch to Window",
                null,
                (_, _) => SetScaleMode(GameScreenScaleMode.Stretch));
            g_fullscreenItem = new ToolStripMenuItem(
                "&Fullscreen",
                null,
                (_, _) => ToggleFullscreen());
            g_fullscreenItem.ShortcutKeys = Keys.Alt | Keys.Enter;
            w_viewMenu.DropDownItems.Add(g_scaleIntegerItem);
            w_viewMenu.DropDownItems.Add(g_scaleStretchItem);
            w_viewMenu.DropDownItems.Add(new ToolStripSeparator());
            w_viewMenu.DropDownItems.Add(g_fullscreenItem);

            var w_optionsMenu = new ToolStripMenuItem("&Options");
            var w_controlsItem = new ToolStripMenuItem("&Controller Settings...", null, (_, _) => OpenControlsDialog());
            w_optionsMenu.DropDownItems.Add(w_controlsItem);

            w_menu.Items.Add(w_fileMenu);
            w_menu.Items.Add(w_emulationMenu);
            w_menu.Items.Add(w_viewMenu);
            w_menu.Items.Add(w_optionsMenu);
            g_menuStrip = w_menu;
            MainMenuStrip = w_menu;
            Controls.Add(w_menu);

            g_gamePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
            };
            g_pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Normal,
                BackColor = Color.Black,
            };
            g_gamePanel.Controls.Add(g_pictureBox);
            Controls.Add(g_gamePanel);

            g_statusStrip = new StatusStrip();
            g_statusCpu = new ToolStripStatusLabel("Ready");
            g_statusRom = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleRight };
            g_statusStrip.Items.Add(g_statusCpu);
            g_statusStrip.Items.Add(g_statusRom);
            Controls.Add(g_statusStrip);

            g_openFileDialog = new OpenFileDialog
            {
                Filter = "Genesis ROM (*.bin;*.md;*.gen;*.smd)|*.bin;*.md;*.gen;*.smd|All files (*.*)|*.*",
                Title = "Open Genesis ROM",
            };

            Load += GameForm_Load;
            FormClosing += GameForm_FormClosing;
            ResizeEnd += GameForm_ResizeEnd;
            DragEnter += GameForm_DragEnter;
            DragDrop += GameForm_DragDrop;
            AllowDrop = true;

            if (in_args.Length > 0)
            {
                g_pendingRomPath = in_args[0];
            }
        }

        private void GameForm_Load(object? sender, EventArgs e)
        {
            g_pictureBox.Image = new Bitmap(320, 240);
            WindowsPlatformServices.Register();
            md_main.g_frontendSettings = new GameFrontendSettingsHooks();
            md_main.initialize();
            md_main.read_setting();
            md_main.g_mainLoopUI = new GameMainLoopUiHooks(this);
            md_main.g_md_music.setting();

            if (g_screen_size_x <= 0) g_screen_size_x = DefaultClientWidth;
            if (g_screen_size_y <= 0) g_screen_size_y = DefaultClientHeight;
            if (g_screen_xpos != 0 || g_screen_ypos != 0)
            {
                Location = new Point(g_screen_xpos, g_screen_ypos);
            }
            ClientSize = new Size(g_screen_size_x, g_screen_size_y + (ClientSize.Height - g_gamePanel.ClientSize.Height));

            UpdateScaleMenuChecks();

            if (string.IsNullOrEmpty(g_pendingRomPath) == false)
            {
                LoadRom(g_pendingRomPath);
            }
            else if (string.IsNullOrEmpty(g_file_name[0]) == false && File.Exists(g_file_name[0]))
            {
                g_statusCpu.Text = "Open a ROM (Ctrl+O), drag a file, or pick a recent ROM";
            }
            else
            {
                g_statusCpu.Text = "Open a ROM (Ctrl+O) or drag a file onto the window";
            }

            RebuildRecentRomMenu();
        }

        private void GameForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            g_screen_xpos = Location.X;
            g_screen_ypos = Location.Y;
            g_screen_size_x = ClientSize.Width;
            g_screen_size_y = g_gamePanel.ClientSize.Height;
            md_main.write_setting();
            md_main.g_md_sram.save();
        }

        private void GameForm_ResizeEnd(object? sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Normal) return;
            g_screen_xpos = Location.X;
            g_screen_ypos = Location.Y;
            g_screen_size_x = ClientSize.Width;
            g_screen_size_y = g_gamePanel.ClientSize.Height;
            md_main.write_setting();
        }

        private void GameForm_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void GameForm_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is not string[] w_files || w_files.Length == 0) return;
            LoadRom(w_files[0]);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F10:
                    Screenshot();
                    return true;
                case Keys.Alt | Keys.Enter:
                case Keys.F11:
                    ToggleFullscreen();
                    return true;
            }

            if (g_fullscreen.IsFullscreen == true && keyData == Keys.Escape)
            {
                ToggleFullscreen();
                return true;
            }

            if (g_romLoaded == false) return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Escape:
                    TogglePause();
                    return true;
                case Keys.F1:
                    SaveState();
                    return true;
                case Keys.F4:
                    LoadState();
                    return true;
                case Keys.F5:
                    FrameAdvance();
                    return true;
                case Keys.F12:
                    RequestHardReset();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OpenRomDialog()
        {
            if (g_openFileDialog.ShowDialog(this) != DialogResult.OK) return;
            LoadRom(g_openFileDialog.FileName);
        }

        private void LoadRom(string in_path)
        {
            if (File.Exists(in_path) == false)
            {
                MessageBox.Show(this, "File not found: " + in_path, "Open ROM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (md_main.run(in_path) == false)
            {
                MessageBox.Show(this, "Failed to load ROM: " + in_path, "Open ROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            g_romLoaded = true;
            UpdateRecentRomList(in_path);
            g_statusRom.Text = Path.GetFileName(in_path);
            g_statusCpu.Text = "Running";
            Text = "GenesisEmu — " + Path.GetFileName(in_path);
            RebuildRecentRomMenu();
        }

        private void TogglePause()
        {
            if (g_romLoaded == false) return;
            bool w_stopped = md_main.request_stop();
            g_statusCpu.Text = w_stopped ? "Paused" : "Running";
        }

        private void FrameAdvance()
        {
            if (g_romLoaded == false) return;
            md_main.request_frame_advance();
        }

        private void RequestHardReset()
        {
            if (g_romLoaded == false) return;
            if (MessageBox.Show(this, "Reset the emulated system?", "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            md_main.request_hard_reset();
            g_statusCpu.Text = "Running";
        }

        private void SaveState()
        {
            if (g_romLoaded == false) return;
            if (md_main.StateStore.IsAvailable() == false) return;
            md_main.request_state_capture_save();
            g_statusCpu.Text = "State saved";
        }

        private void LoadState()
        {
            if (g_romLoaded == false) return;
            if (md_main.StateStore.IsAvailable() == false) return;
            md_main.request_state_capture_restore_latest();
            g_statusCpu.Text = "State loaded";
        }

        private void Screenshot()
        {
            if (g_romLoaded == false)
            {
                MessageBox.Show(this, "Load a ROM before taking a screenshot.", "Screenshot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Image? w_image = g_pictureBox.Image;
                if (w_image == null)
                {
                    g_statusCpu.Text = "Screenshot image is empty";
                    return;
                }

                string w_filePath = WinFormsGameScreenshot.SavePng(
                    w_image,
                    "GenesisEmu",
                    md_main.g_state_capture_rom_file_name);
                g_statusCpu.Text = "Screenshot saved: " + Path.GetFileName(w_filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Screenshot", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenScreenshotFolder()
        {
            string w_folder = WinFormsGameScreenshot.GetScreenshotFolder("GenesisEmu");
            Directory.CreateDirectory(w_folder);
            Process.Start(new ProcessStartInfo
            {
                FileName = w_folder,
                UseShellExecute = true,
            });
        }

        private void SetScaleMode(GameScreenScaleMode in_mode)
        {
            g_scale_mode = in_mode;
            UpdateScaleMenuChecks();
            md_main.write_setting();
        }

        private void ToggleFullscreen()
        {
            g_fullscreen.Toggle(this, g_menuStrip, g_statusStrip);
            g_fullscreenItem.Checked = g_fullscreen.IsFullscreen;
        }

        private void OpenControlsDialog()
        {
            using var w_dialog = new GameControlsDialog();
            w_dialog.ShowDialog(this);
        }

        private void UpdateScaleMenuChecks()
        {
            g_scaleIntegerItem.Checked = g_scale_mode == GameScreenScaleMode.IntegerFit;
            g_scaleStretchItem.Checked = g_scale_mode == GameScreenScaleMode.Stretch;
            g_fullscreenItem.Checked = g_fullscreen.IsFullscreen;
        }

        private void RebuildRecentRomMenu()
        {
            if (MainMenuStrip?.Items[0] is not ToolStripMenuItem w_fileMenu) return;

            for (int i = w_fileMenu.DropDownItems.Count - 1; i >= 0; i--)
            {
                if (w_fileMenu.DropDownItems[i].Tag as string == "recent")
                {
                    w_fileMenu.DropDownItems.RemoveAt(i);
                }
            }

            bool w_hasRecent = false;
            for (int i = 0; i < 9; i++)
            {
                if (string.IsNullOrEmpty(g_file_name[i])) continue;
                w_hasRecent = true;
                break;
            }
            if (w_hasRecent == false) return;

            int w_insertIndex = 1;
            w_fileMenu.DropDownItems.Insert(w_insertIndex++, new ToolStripSeparator { Tag = "recent" });

            for (int i = 0; i < 9; i++)
            {
                string w_path = g_file_name[i];
                if (string.IsNullOrEmpty(w_path)) continue;

                string w_label = "&" + (i + 1) + " " + Path.GetFileName(w_path);
                var w_item = new ToolStripMenuItem(w_label, null, (_, _) => LoadRom(w_path))
                {
                    Tag = "recent",
                    Enabled = File.Exists(w_path),
                };
                w_fileMenu.DropDownItems.Insert(w_insertIndex++, w_item);
            }
        }

        private static void UpdateRecentRomList(string in_file)
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
        }

        public void PictureUpdate(int in_cpuUsage)
        {
            if (IsDisposed == true || IsHandleCreated == false) return;
            if (g_romLoaded == false) return;

            string w_statusText = "CPU load: " + in_cpuUsage + "%";
            if (Interlocked.Exchange(ref g_frameUpdatePending, 1) == 1) return;

            try
            {
                BeginInvoke(new Action<string>(ApplyPictureUpdate), w_statusText);
            }
            catch
            {
                Interlocked.Exchange(ref g_frameUpdatePending, 0);
            }
        }

        private void ApplyPictureUpdate(string in_statusText)
        {
            Interlocked.Exchange(ref g_frameUpdatePending, 0);
            if (IsDisposed == true || IsHandleCreated == false) return;
            if (WindowState == FormWindowState.Minimized) return;

            Size w_clientSize = g_gamePanel.ClientSize;
            if (w_clientSize.Width <= 0 || w_clientSize.Height <= 0) return;

            int w_bitmapWidth = w_clientSize.Width;
            int w_bitmapHeight = w_clientSize.Height;
            int w_sourceWidth = md_main.g_md_vdp.g_display_xsize;
            int w_sourceHeight = md_main.g_md_vdp.g_display_ysize;
            uint[] w_gameScreen = md_main.g_md_vdp.g_game_screen;

            lock (g_bitmapLock)
            {
                if (g_workBitmap == null || g_workBitmapWidth != w_bitmapWidth || g_workBitmapHeight != w_bitmapHeight)
                {
                    g_workBitmap?.Dispose();
                    g_displayBitmap?.Dispose();
                    g_workBitmap = new Bitmap(w_bitmapWidth, w_bitmapHeight, PixelFormat.Format32bppArgb);
                    g_displayBitmap = new Bitmap(w_bitmapWidth, w_bitmapHeight, PixelFormat.Format32bppArgb);
                    g_workBitmapWidth = w_bitmapWidth;
                    g_workBitmapHeight = w_bitmapHeight;
                }

                WinFormsGameScreenBitmap.WriteScaledPixels(
                    w_gameScreen,
                    w_sourceWidth,
                    w_sourceHeight,
                    g_workBitmap,
                    g_scale_mode);

                (g_workBitmap, g_displayBitmap) = (g_displayBitmap, g_workBitmap);
            }

            g_statusCpu.Text = in_statusText;
            g_pictureBox.Image = g_displayBitmap;
        }
    }
}
