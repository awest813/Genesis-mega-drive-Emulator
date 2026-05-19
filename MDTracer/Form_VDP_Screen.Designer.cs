namespace MDTracer
{
    partial class Form_VDP_Screen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel_screen = new Panel();
            pictureBox_screen = new PictureBox();
            menuStrip1 = new MenuStrip();
            gridToolStripMenuItem = new ToolStripMenuItem();
            screenshotToolStripMenuItem = new ToolStripMenuItem();
            videoRecordingToolStripMenuItem = new ToolStripMenuItem();
            panel_screen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_screen).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // panel_screen
            // 
            panel_screen.BackColor = Color.Black;
            panel_screen.Controls.Add(pictureBox_screen);
            panel_screen.Dock = DockStyle.Fill;
            panel_screen.Location = new Point(0, 24);
            panel_screen.Name = "panel_screen";
            panel_screen.Size = new Size(264, 269);
            panel_screen.TabIndex = 2;
            // 
            // pictureBox_screen
            // 
            pictureBox_screen.BackColor = Color.Black;
            pictureBox_screen.Dock = DockStyle.Fill;
            pictureBox_screen.Location = new Point(0, 0);
            pictureBox_screen.Margin = new Padding(0);
            pictureBox_screen.Name = "pictureBox_screen";
            pictureBox_screen.Size = new Size(264, 269);
            pictureBox_screen.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_screen.TabIndex = 0;
            pictureBox_screen.TabStop = false;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { gridToolStripMenuItem, screenshotToolStripMenuItem, videoRecordingToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(264, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // gridToolStripMenuItem
            // 
            gridToolStripMenuItem.CheckOnClick = true;
            gridToolStripMenuItem.Name = "gridToolStripMenuItem";
            gridToolStripMenuItem.Size = new Size(41, 20);
            gridToolStripMenuItem.Text = "Grid";
            gridToolStripMenuItem.Click += gridToolStripMenuItem_Click;
            // 
            // screenshotToolStripMenuItem
            // 
            screenshotToolStripMenuItem.CheckOnClick = true;
            screenshotToolStripMenuItem.Name = "screenshotToolStripMenuItem";
            screenshotToolStripMenuItem.Size = new Size(77, 20);
            screenshotToolStripMenuItem.Text = "Screenshot";
            // 
            // videoRecordingToolStripMenuItem
            // 
            videoRecordingToolStripMenuItem.CheckOnClick = true;
            videoRecordingToolStripMenuItem.Name = "videoRecordingToolStripMenuItem";
            videoRecordingToolStripMenuItem.Size = new Size(106, 20);
            videoRecordingToolStripMenuItem.Text = "Video Recording";
            // 
            // Form_VDP_Screen
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(264, 293);
            Controls.Add(panel_screen);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(272, 318);
            Name = "Form_VDP_Screen";
            Text = "VDP Memory View";
            FormClosing += Form_VDP_Screen_FormClosing;
            Shown += Form_VDP_Screen_Shown;
            ResizeEnd += Form_VDP_Screen_ResizeEnd;
            panel_screen.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox_screen).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel_screen;
        private PictureBox pictureBox_screen;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem gridToolStripMenuItem;
        private ToolStripMenuItem screenshotToolStripMenuItem;
        private ToolStripMenuItem videoRecordingToolStripMenuItem;
    }
}
