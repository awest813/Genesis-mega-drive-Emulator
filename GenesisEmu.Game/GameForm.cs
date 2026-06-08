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

        private readonly Panel g_gamePanel;
        private readonly PictureBox g_pictureBox;
        private readonly StatusStrip g_statusStrip;
        private readonly ToolStripStatusLabel g_statusCpu;
        private readonly ToolStripStatusLabel g_statusRom;
        private readonly OpenFileDialog g_openFileDialog;
        private readonly object g_bitmapLock = new object();
        private Bitmap? g_workBitmap;
        private int g_workBitmapWidth = -1;
        private int g_workBitmapHeight = -1;
        private bool g_romLoaded;

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
            var w_exitItem = new ToolStripMenuItem("E&xit", null, (_, _) => Close());
            w_fileMenu.DropDownItems.Add(w_openItem);
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
            w_emulationMenu.DropDownItems.Add(w_pauseItem);
            w_emulationMenu.DropDownItems.Add(w_frameItem);
            w_emulationMenu.DropDownItems.Add(w_resetItem);
            w_emulationMenu.DropDownItems.Add(new ToolStripSeparator());
            w_emulationMenu.DropDownItems.Add(w_saveItem);

            w_menu.Items.Add(w_fileMenu);
            w_menu.Items.Add(w_emulationMenu);
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
                SizeMode = PictureBoxSizeMode.StretchImage,
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
            DragEnter += GameForm_DragEnter;
            DragDrop += GameForm_DragDrop;
            AllowDrop = true;

            if (in_args.Length > 0)
            {
                g_pendingRomPath = in_args[0];
            }
        }

        private string? g_pendingRomPath;

        private void GameForm_Load(object? sender, EventArgs e)
        {
            g_pictureBox.Image = new Bitmap(320, 240);
            WindowsPlatformServices.Register();
            md_main.initialize();
            md_main.g_mainLoopUI = new GameMainLoopUiHooks(this);
            md_main.g_md_music.setting();

            if (string.IsNullOrEmpty(g_pendingRomPath) == false)
            {
                LoadRom(g_pendingRomPath);
            }
            else
            {
                g_statusCpu.Text = "Open a ROM (Ctrl+O) or drag a file onto the window";
            }
        }

        private void GameForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            md_main.g_md_sram.save();
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
            if (g_romLoaded == false) return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Escape:
                    TogglePause();
                    return true;
                case Keys.F1:
                    SaveState();
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
            g_statusRom.Text = Path.GetFileName(in_path);
            g_statusCpu.Text = "Running";
            Text = "GenesisEmu — " + Path.GetFileName(in_path);
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
        }

        public void PictureUpdate(int in_cpuUsage)
        {
            if (IsDisposed == true || IsHandleCreated == false) return;
            if (g_romLoaded == false) return;

            Size w_clientSize = g_gamePanel.ClientSize;
            if (w_clientSize.Width <= 0 || w_clientSize.Height <= 0) return;

            string w_statusText = "CPU load: " + in_cpuUsage + "%";
            Bitmap w_displayBitmap;
            int w_bitmapWidth = w_clientSize.Width;
            int w_bitmapHeight = w_clientSize.Height;

            lock (g_bitmapLock)
            {
                int w_sourceWidth = md_main.g_md_vdp.g_display_xsize;
                int w_sourceHeight = md_main.g_md_vdp.g_display_ysize;
                uint[] w_gameScreen = md_main.g_md_vdp.g_game_screen;

                if (g_workBitmap == null || g_workBitmapWidth != w_bitmapWidth || g_workBitmapHeight != w_bitmapHeight)
                {
                    g_workBitmap?.Dispose();
                    g_workBitmap = new Bitmap(w_bitmapWidth, w_bitmapHeight, PixelFormat.Format32bppArgb);
                    g_workBitmapWidth = w_bitmapWidth;
                    g_workBitmapHeight = w_bitmapHeight;
                }

                WinFormsGameScreenBitmap.WriteScaledPixels(
                    w_gameScreen, w_sourceWidth, w_sourceHeight, g_workBitmap);
                w_displayBitmap = new Bitmap(g_workBitmap);
            }

            UpdateGamePicture(w_displayBitmap, w_statusText);
        }

        private void UpdateGamePicture(Bitmap in_bitmap, string in_statusText)
        {
            if (IsDisposed == true || IsHandleCreated == false)
            {
                in_bitmap.Dispose();
                return;
            }
            if (InvokeRequired == true)
            {
                try
                {
                    BeginInvoke(new Action<Bitmap, string>(UpdateGamePicture), in_bitmap, in_statusText);
                }
                catch
                {
                    in_bitmap.Dispose();
                }
                return;
            }

            if (WindowState == FormWindowState.Minimized)
            {
                in_bitmap.Dispose();
                return;
            }

            g_statusCpu.Text = in_statusText;
            g_pictureBox.Image?.Dispose();
            g_pictureBox.Image = in_bitmap;
        }
    }
}
