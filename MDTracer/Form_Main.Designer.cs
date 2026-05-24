namespace MDTracer
{
    partial class Form_Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            SettingMenuItem1 = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            screenshotToolStripMenuItem = new ToolStripMenuItem();
            videoRecordingToolStripMenuItem = new ToolStripMenuItem();
            openCaptureFolderToolStripMenuItem = new ToolStripMenuItem();
            openVideoFolderToolStripMenuItem = new ToolStripMenuItem();
            emulationToolStripMenuItem = new ToolStripMenuItem();
            pauseResumeToolStripMenuItem = new ToolStripMenuItem();
            resetToolStripMenuItem = new ToolStripMenuItem();
            frameAdvanceToolStripMenuItem = new ToolStripMenuItem();
            stateCaptureToolStripMenuItem = new ToolStripMenuItem();
            state_capture_saveToolStripMenuItem = new ToolStripMenuItem();
            state_capture_loadToolStripMenuItem = new ToolStripMenuItem();
            state_capture_listToolStripMenuItem = new ToolStripMenuItem();
            inputCaptureToolStripMenuItem = new ToolStripMenuItem();
            input_capture_recode_MenuItem = new ToolStripMenuItem();
            input_capture_ReplayMenuItem = new ToolStripMenuItem();
            input_capture_ListMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            toolStripStatusLabel3 = new ToolStripStatusLabel();
            panel_game = new Panel();
            pictureBox_game = new PictureBox();
            openFileDialog1 = new OpenFileDialog();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            panel_game.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_game).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, toolStripMenuItem1, emulationToolStripMenuItem, aboutToolStripMenuItem, toolStripMenuItem2 });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(320, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { SettingMenuItem1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(58, 20);
            fileToolStripMenuItem.Text = "Control";
            // 
            // SettingMenuItem1
            // 
            SettingMenuItem1.Name = "SettingMenuItem1";
            SettingMenuItem1.ShortcutKeys = Keys.F9;
            SettingMenuItem1.Size = new Size(135, 22);
            SettingMenuItem1.Text = "Setting";
            SettingMenuItem1.Click += SettingMenuItem1_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Q;
            exitToolStripMenuItem.Size = new Size(135, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { screenshotToolStripMenuItem, videoRecordingToolStripMenuItem, openCaptureFolderToolStripMenuItem, openVideoFolderToolStripMenuItem });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(60, 20);
            toolStripMenuItem1.Text = "Capture";
            // 
            // screenshotToolStripMenuItem
            // 
            screenshotToolStripMenuItem.Name = "screenshotToolStripMenuItem";
            screenshotToolStripMenuItem.ShortcutKeys = Keys.F10;
            screenshotToolStripMenuItem.Size = new Size(200, 22);
            screenshotToolStripMenuItem.Text = "Screenshot";
            screenshotToolStripMenuItem.Click += screenshotToolStripMenuItem_Click;
            // 
            // videoRecordingToolStripMenuItem
            // 
            videoRecordingToolStripMenuItem.Name = "videoRecordingToolStripMenuItem";
            videoRecordingToolStripMenuItem.ShortcutKeyDisplayString = "F11";
            videoRecordingToolStripMenuItem.Size = new Size(200, 22);
            videoRecordingToolStripMenuItem.Text = "Video Recording";
            videoRecordingToolStripMenuItem.Click += videoRecordingToolStripMenuItem_Click;
            // 
            // openCaptureFolderToolStripMenuItem
            // 
            openCaptureFolderToolStripMenuItem.Name = "openCaptureFolderToolStripMenuItem";
            openCaptureFolderToolStripMenuItem.Size = new Size(200, 22);
            openCaptureFolderToolStripMenuItem.Text = "Open Screenshot Folder";
            openCaptureFolderToolStripMenuItem.Click += openCaptureFolderToolStripMenuItem_Click;
            // 
            // openVideoFolderToolStripMenuItem
            // 
            openVideoFolderToolStripMenuItem.Name = "openVideoFolderToolStripMenuItem";
            openVideoFolderToolStripMenuItem.Size = new Size(200, 22);
            openVideoFolderToolStripMenuItem.Text = "Open Video Folder";
            openVideoFolderToolStripMenuItem.Click += openVideoFolderToolStripMenuItem_Click;
            // 
            // emulationToolStripMenuItem
            // 
            emulationToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { pauseResumeToolStripMenuItem, resetToolStripMenuItem, frameAdvanceToolStripMenuItem, stateCaptureToolStripMenuItem, inputCaptureToolStripMenuItem });
            emulationToolStripMenuItem.Name = "emulationToolStripMenuItem";
            emulationToolStripMenuItem.Size = new Size(72, 20);
            emulationToolStripMenuItem.Text = "Emulation";
            // 
            // pauseResumeToolStripMenuItem
            // 
            pauseResumeToolStripMenuItem.Name = "pauseResumeToolStripMenuItem";
            pauseResumeToolStripMenuItem.ShortcutKeyDisplayString = "Esc";
            pauseResumeToolStripMenuItem.Size = new Size(181, 22);
            pauseResumeToolStripMenuItem.Text = "Pause / Resume";
            pauseResumeToolStripMenuItem.Click += pauseResumeToolStripMenuItem_Click;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.ShortcutKeyDisplayString = "F12";
            resetToolStripMenuItem.Size = new Size(181, 22);
            resetToolStripMenuItem.Text = "Reset";
            resetToolStripMenuItem.Click += hardResetMenuItem_Click;
            // 
            // frameAdvanceToolStripMenuItem
            // 
            frameAdvanceToolStripMenuItem.Name = "frameAdvanceToolStripMenuItem";
            frameAdvanceToolStripMenuItem.ShortcutKeyDisplayString = "F5";
            frameAdvanceToolStripMenuItem.Size = new Size(181, 22);
            frameAdvanceToolStripMenuItem.Text = "Frame Advance";
            frameAdvanceToolStripMenuItem.Click += frameAdvanceToolStripMenuItem_Click;
            // 
            // stateCaptureToolStripMenuItem
            // 
            stateCaptureToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { state_capture_saveToolStripMenuItem, state_capture_loadToolStripMenuItem, state_capture_listToolStripMenuItem });
            stateCaptureToolStripMenuItem.Name = "stateCaptureToolStripMenuItem";
            stateCaptureToolStripMenuItem.Size = new Size(181, 22);
            stateCaptureToolStripMenuItem.Text = "State Capture";
            // 
            // state_capture_saveToolStripMenuItem
            // 
            state_capture_saveToolStripMenuItem.Name = "state_capture_saveToolStripMenuItem";
            state_capture_saveToolStripMenuItem.ShortcutKeys = Keys.F1;
            state_capture_saveToolStripMenuItem.Size = new Size(180, 22);
            state_capture_saveToolStripMenuItem.Text = "Save";
            state_capture_saveToolStripMenuItem.Click += state_capture_saveToolStripMenuItem_Click;
            // 
            // state_capture_loadToolStripMenuItem
            // 
            state_capture_loadToolStripMenuItem.Name = "state_capture_loadToolStripMenuItem";
            state_capture_loadToolStripMenuItem.ShortcutKeys = Keys.F4;
            state_capture_loadToolStripMenuItem.Size = new Size(180, 22);
            state_capture_loadToolStripMenuItem.Text = "Load(Latest)";
            state_capture_loadToolStripMenuItem.Click += state_capture_loadToolStripMenuItem_Click;
            // 
            // state_capture_listToolStripMenuItem
            // 
            state_capture_listToolStripMenuItem.Name = "state_capture_listToolStripMenuItem";
            state_capture_listToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F4;
            state_capture_listToolStripMenuItem.Size = new Size(180, 22);
            state_capture_listToolStripMenuItem.Text = "List";
            state_capture_listToolStripMenuItem.Click += state_capture_state_capture_ToolStripMenuItem_Click;
            // 
            // inputCaptureToolStripMenuItem
            // 
            inputCaptureToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { input_capture_recode_MenuItem, input_capture_ReplayMenuItem, input_capture_ListMenuItem });
            inputCaptureToolStripMenuItem.Name = "inputCaptureToolStripMenuItem";
            inputCaptureToolStripMenuItem.Size = new Size(181, 22);
            inputCaptureToolStripMenuItem.Text = "Input Capture";
            // 
            // input_capture_recode_MenuItem
            // 
            input_capture_recode_MenuItem.Name = "input_capture_recode_MenuItem";
            input_capture_recode_MenuItem.ShortcutKeys = Keys.F2;
            input_capture_recode_MenuItem.Size = new Size(191, 22);
            input_capture_recode_MenuItem.Text = "Record(Start/Stop)";
            input_capture_recode_MenuItem.Click += input_capture_RecodeMenuItem_Click;
            // 
            // input_capture_ReplayMenuItem
            // 
            input_capture_ReplayMenuItem.Name = "input_capture_ReplayMenuItem";
            input_capture_ReplayMenuItem.ShortcutKeys = Keys.None;
            input_capture_ReplayMenuItem.Size = new Size(191, 22);
            input_capture_ReplayMenuItem.Text = "Replay(Latest)";
            input_capture_ReplayMenuItem.Click += input_capture_ReplayMenuItem_Click;
            // 
            // input_capture_ListMenuItem
            // 
            input_capture_ListMenuItem.Name = "input_capture_ListMenuItem";
            input_capture_ListMenuItem.ShortcutKeys = Keys.None;
            input_capture_ListMenuItem.Size = new Size(191, 22);
            input_capture_ListMenuItem.Text = "List";
            input_capture_ListMenuItem.Click += input_capture_ListMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(50, 20);
            aboutToolStripMenuItem.Text = "about";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(12, 20);
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolStripStatusLabel2, toolStripStatusLabel3 });
            statusStrip1.Location = new Point(0, 242);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(320, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(0, 17);
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 128);
            toolStripStatusLabel2.ForeColor = Color.Red;
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(31, 17);
            toolStripStatusLabel2.Text = "        ";
            // 
            // toolStripStatusLabel3
            // 
            toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            toolStripStatusLabel3.Size = new Size(25, 17);
            toolStripStatusLabel3.Text = "      ";
            // 
            // panel_game
            // 
            panel_game.Controls.Add(pictureBox_game);
            panel_game.Dock = DockStyle.Fill;
            panel_game.Location = new Point(0, 24);
            panel_game.Name = "panel_game";
            panel_game.Size = new Size(320, 218);
            panel_game.TabIndex = 3;
            // 
            // pictureBox_game
            // 
            pictureBox_game.Location = new Point(0, 0);
            pictureBox_game.Name = "pictureBox_game";
            pictureBox_game.Size = new Size(320, 218);
            pictureBox_game.TabIndex = 0;
            pictureBox_game.TabStop = false;
            pictureBox_game.Paint += pictureBox_game_Paint;
            // 
            // Form_Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(320, 264);
            Controls.Add(panel_game);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form_Main";
            Text = "MD Tracer";
            FormClosing += Form_Main_FormClosing;
            Load += Form_Main_Load;
            ResizeEnd += Form_Main_ResizeEnd;
            SizeChanged += Form_Main_SizeChanged;
            KeyDown += Form_Main_KeyDown;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            panel_game.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox_game).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private StatusStrip statusStrip1;
        private Panel panel_game;
        private PictureBox pictureBox_game;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem SettingMenuItem1;
        private OpenFileDialog openFileDialog1;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem input_capture_recode_MenuItem;
        private ToolStripMenuItem input_capture_ReplayMenuItem;
        private ToolStripMenuItem input_capture_ListMenuItem;
        private ToolStripMenuItem emulationToolStripMenuItem;
        private ToolStripMenuItem stateCaptureToolStripMenuItem;
        private ToolStripMenuItem state_capture_saveToolStripMenuItem;
        private ToolStripMenuItem state_capture_loadToolStripMenuItem;
        private ToolStripMenuItem state_capture_listToolStripMenuItem;
        private ToolStripMenuItem pauseResumeToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripMenuItem frameAdvanceToolStripMenuItem;
        private ToolStripMenuItem openVideoFolderToolStripMenuItem;
        private ToolStripMenuItem openCaptureFolderToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem inputCaptureToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem screenshotToolStripMenuItem;
        private ToolStripMenuItem videoRecordingToolStripMenuItem;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private ToolStripStatusLabel toolStripStatusLabel3;
    }
}
