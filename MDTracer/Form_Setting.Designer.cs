namespace MDTracer
{
    partial class Form_Setting
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
            groupBox1 = new GroupBox();
            checkBox_io = new CheckBox();
            checkBox_flow = new CheckBox();
            checkBox_register = new CheckBox();
            checkBox_music = new CheckBox();
            checkBox_code = new CheckBox();
            checkBox_pallete = new CheckBox();
            checkBox_pattern = new CheckBox();
            checkBox_screenS = new CheckBox();
            checkBox_screenW = new CheckBox();
            checkBox_screenB = new CheckBox();
            checkBox_screenA = new CheckBox();
            groupBox2 = new GroupBox();
            checkBox_sip = new CheckBox();
            checkBox_fsb = new CheckBox();
            groupBox3 = new GroupBox();
            label3 = new Label();
            comboBox_rendering = new ComboBox();
            label2 = new Label();
            label1 = new Label();
            comboBox_videoformat = new ComboBox();
            groupBox4 = new GroupBox();
            label7 = new Label();
            checkBox_S_Low = new CheckBox();
            checkBox_S_High = new CheckBox();
            checkBox_W_Low = new CheckBox();
            checkBox_W_High = new CheckBox();
            checkBox_B_Low = new CheckBox();
            checkBox_B_High = new CheckBox();
            label6 = new Label();
            checkBox_A_Low = new CheckBox();
            label5 = new Label();
            label4 = new Label();
            checkBox_A_High = new CheckBox();
            checkBox_viewS = new CheckBox();
            checkBox_viewW = new CheckBox();
            checkBox_viewB = new CheckBox();
            checkBox_viewA = new CheckBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(checkBox_io);
            groupBox1.Controls.Add(checkBox_flow);
            groupBox1.Controls.Add(checkBox_register);
            groupBox1.Controls.Add(checkBox_music);
            groupBox1.Controls.Add(checkBox_code);
            groupBox1.Controls.Add(checkBox_pallete);
            groupBox1.Controls.Add(checkBox_pattern);
            groupBox1.Controls.Add(checkBox_screenS);
            groupBox1.Controls.Add(checkBox_screenW);
            groupBox1.Controls.Add(checkBox_screenB);
            groupBox1.Controls.Add(checkBox_screenA);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(176, 180);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Window";
            // 
            // checkBox_io
            // 
            checkBox_io.AutoSize = true;
            checkBox_io.Location = new Point(99, 55);
            checkBox_io.Name = "checkBox_io";
            checkBox_io.Size = new Size(43, 19);
            checkBox_io.TabIndex = 12;
            checkBox_io.Text = "I/O";
            checkBox_io.UseVisualStyleBackColor = true;
            checkBox_io.CheckedChanged += checkBox_io_CheckedChanged;
            // 
            // checkBox_flow
            // 
            checkBox_flow.AutoSize = true;
            checkBox_flow.Location = new Point(97, 130);
            checkBox_flow.Name = "checkBox_flow";
            checkBox_flow.Size = new Size(51, 19);
            checkBox_flow.TabIndex = 11;
            checkBox_flow.Text = "Flow";
            checkBox_flow.UseVisualStyleBackColor = true;
            checkBox_flow.CheckedChanged += checkBox_flow_CheckedChanged;
            // 
            // checkBox_register
            // 
            checkBox_register.AutoSize = true;
            checkBox_register.Location = new Point(97, 105);
            checkBox_register.Name = "checkBox_register";
            checkBox_register.Size = new Size(68, 19);
            checkBox_register.TabIndex = 9;
            checkBox_register.Text = "Register";
            checkBox_register.UseVisualStyleBackColor = true;
            checkBox_register.CheckedChanged += checkBox_register_CheckedChanged;
            // 
            // checkBox_music
            // 
            checkBox_music.AutoSize = true;
            checkBox_music.Location = new Point(98, 80);
            checkBox_music.Name = "checkBox_music";
            checkBox_music.Size = new Size(58, 19);
            checkBox_music.TabIndex = 8;
            checkBox_music.Text = "Music";
            checkBox_music.UseVisualStyleBackColor = true;
            checkBox_music.CheckedChanged += checkBox_music_CheckedChanged;
            // 
            // checkBox_code
            // 
            checkBox_code.AutoSize = true;
            checkBox_code.Location = new Point(99, 30);
            checkBox_code.Name = "checkBox_code";
            checkBox_code.Size = new Size(53, 19);
            checkBox_code.TabIndex = 6;
            checkBox_code.Text = "Code";
            checkBox_code.UseVisualStyleBackColor = true;
            checkBox_code.CheckedChanged += checkBox_code_CheckedChanged;
            // 
            // checkBox_pallete
            // 
            checkBox_pallete.AutoSize = true;
            checkBox_pallete.Location = new Point(24, 155);
            checkBox_pallete.Name = "checkBox_pallete";
            checkBox_pallete.Size = new Size(61, 19);
            checkBox_pallete.TabIndex = 10;
            checkBox_pallete.Text = "Pallete";
            checkBox_pallete.UseVisualStyleBackColor = true;
            checkBox_pallete.CheckedChanged += checkBox_pallete_CheckedChanged;
            // 
            // checkBox_pattern
            // 
            checkBox_pattern.AutoSize = true;
            checkBox_pattern.Location = new Point(24, 130);
            checkBox_pattern.Name = "checkBox_pattern";
            checkBox_pattern.Size = new Size(64, 19);
            checkBox_pattern.TabIndex = 4;
            checkBox_pattern.Text = "Pattern";
            checkBox_pattern.UseVisualStyleBackColor = true;
            checkBox_pattern.CheckedChanged += checkBox_pattern_CheckedChanged;
            // 
            // checkBox_screenS
            // 
            checkBox_screenS.AutoSize = true;
            checkBox_screenS.Location = new Point(24, 105);
            checkBox_screenS.Name = "checkBox_screenS";
            checkBox_screenS.Size = new Size(67, 19);
            checkBox_screenS.TabIndex = 3;
            checkBox_screenS.Text = "ScreenS";
            checkBox_screenS.UseVisualStyleBackColor = true;
            checkBox_screenS.CheckedChanged += checkBox_screenS_CheckedChanged;
            // 
            // checkBox_screenW
            // 
            checkBox_screenW.AutoSize = true;
            checkBox_screenW.Location = new Point(24, 80);
            checkBox_screenW.Name = "checkBox_screenW";
            checkBox_screenW.Size = new Size(72, 19);
            checkBox_screenW.TabIndex = 2;
            checkBox_screenW.Text = "ScreenW";
            checkBox_screenW.UseVisualStyleBackColor = true;
            checkBox_screenW.CheckedChanged += checkBox_screenW_CheckedChanged;
            // 
            // checkBox_screenB
            // 
            checkBox_screenB.AutoSize = true;
            checkBox_screenB.Location = new Point(24, 55);
            checkBox_screenB.Name = "checkBox_screenB";
            checkBox_screenB.Size = new Size(68, 19);
            checkBox_screenB.TabIndex = 1;
            checkBox_screenB.Text = "ScreenB";
            checkBox_screenB.UseVisualStyleBackColor = true;
            checkBox_screenB.CheckedChanged += checkBox_screenB_CheckedChanged;
            // 
            // checkBox_screenA
            // 
            checkBox_screenA.AutoSize = true;
            checkBox_screenA.Location = new Point(24, 30);
            checkBox_screenA.Name = "checkBox_screenA";
            checkBox_screenA.Size = new Size(69, 19);
            checkBox_screenA.TabIndex = 0;
            checkBox_screenA.Text = "ScreenA";
            checkBox_screenA.UseVisualStyleBackColor = true;
            checkBox_screenA.CheckedChanged += checkBox_screenA_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(checkBox_sip);
            groupBox2.Controls.Add(checkBox_fsb);
            groupBox2.Location = new Point(209, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(200, 74);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Trace";
            // 
            // checkBox_sip
            // 
            checkBox_sip.AutoSize = true;
            checkBox_sip.Location = new Point(6, 47);
            checkBox_sip.Name = "checkBox_sip";
            checkBox_sip.Size = new Size(157, 19);
            checkBox_sip.TabIndex = 2;
            checkBox_sip.Text = "Skip interrupt processing";
            checkBox_sip.UseVisualStyleBackColor = true;
            checkBox_sip.CheckedChanged += checkBox_sip_CheckedChanged;
            // 
            // checkBox_fsb
            // 
            checkBox_fsb.AutoSize = true;
            checkBox_fsb.Location = new Point(6, 22);
            checkBox_fsb.Name = "checkBox_fsb";
            checkBox_fsb.Size = new Size(106, 19);
            checkBox_fsb.TabIndex = 1;
            checkBox_fsb.Text = "First Step Break";
            checkBox_fsb.UseVisualStyleBackColor = true;
            checkBox_fsb.CheckedChanged += checkBox_fsb_CheckedChanged;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(comboBox_rendering);
            groupBox3.Controls.Add(label2);
            groupBox3.Controls.Add(label1);
            groupBox3.Controls.Add(comboBox_videoformat);
            groupBox3.Location = new Point(209, 92);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(200, 100);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "system";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(125, 76);
            label3.Name = "label3";
            label3.Size = new Size(69, 15);
            label3.TabIndex = 8;
            label3.Text = "(GPU ...test)";
            // 
            // comboBox_rendering
            // 
            comboBox_rendering.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_rendering.FormattingEnabled = true;
            comboBox_rendering.Items.AddRange(new object[] { "CPU", "GPU" });
            comboBox_rendering.Location = new Point(110, 44);
            comboBox_rendering.Name = "comboBox_rendering";
            comboBox_rendering.Size = new Size(85, 23);
            comboBox_rendering.TabIndex = 7;
            comboBox_rendering.SelectedIndexChanged += comboBox_rendering_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 47);
            label2.Name = "label2";
            label2.Size = new Size(94, 15);
            label2.TabIndex = 6;
            label2.Text = "Rendering mode";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 21);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 5;
            label1.Text = "Video Format";
            // 
            // comboBox_videoformat
            // 
            comboBox_videoformat.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_videoformat.FormattingEnabled = true;
            comboBox_videoformat.Items.AddRange(new object[] { "NTSC", "PAL" });
            comboBox_videoformat.Location = new Point(109, 18);
            comboBox_videoformat.Name = "comboBox_videoformat";
            comboBox_videoformat.Size = new Size(85, 23);
            comboBox_videoformat.TabIndex = 4;
            comboBox_videoformat.SelectedIndexChanged += comboBox_videoformat_SelectedIndexChanged;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(label7);
            groupBox4.Controls.Add(checkBox_S_Low);
            groupBox4.Controls.Add(checkBox_S_High);
            groupBox4.Controls.Add(checkBox_W_Low);
            groupBox4.Controls.Add(checkBox_W_High);
            groupBox4.Controls.Add(checkBox_B_Low);
            groupBox4.Controls.Add(checkBox_B_High);
            groupBox4.Controls.Add(label6);
            groupBox4.Controls.Add(checkBox_A_Low);
            groupBox4.Controls.Add(label5);
            groupBox4.Controls.Add(label4);
            groupBox4.Controls.Add(checkBox_A_High);
            groupBox4.Controls.Add(checkBox_viewS);
            groupBox4.Controls.Add(checkBox_viewW);
            groupBox4.Controls.Add(checkBox_viewB);
            groupBox4.Controls.Add(checkBox_viewA);
            groupBox4.Location = new Point(12, 202);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(176, 150);
            groupBox4.TabIndex = 4;
            groupBox4.TabStop = false;
            groupBox4.Text = "View";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(22, 31);
            label7.Name = "label7";
            label7.Size = new Size(32, 15);
            label7.TabIndex = 19;
            label7.Text = "View";
            // 
            // checkBox_S_Low
            // 
            checkBox_S_Low.AutoSize = true;
            checkBox_S_Low.Location = new Point(141, 126);
            checkBox_S_Low.Name = "checkBox_S_Low";
            checkBox_S_Low.Size = new Size(15, 14);
            checkBox_S_Low.TabIndex = 18;
            checkBox_S_Low.UseVisualStyleBackColor = true;
            checkBox_S_Low.CheckedChanged += checkBox_S_Low_CheckedChanged;
            // 
            // checkBox_S_High
            // 
            checkBox_S_High.AutoSize = true;
            checkBox_S_High.Location = new Point(107, 126);
            checkBox_S_High.Name = "checkBox_S_High";
            checkBox_S_High.Size = new Size(15, 14);
            checkBox_S_High.TabIndex = 17;
            checkBox_S_High.UseVisualStyleBackColor = true;
            checkBox_S_High.CheckedChanged += checkBox_S_High_CheckedChanged;
            // 
            // checkBox_W_Low
            // 
            checkBox_W_Low.AutoSize = true;
            checkBox_W_Low.Location = new Point(141, 101);
            checkBox_W_Low.Name = "checkBox_W_Low";
            checkBox_W_Low.Size = new Size(15, 14);
            checkBox_W_Low.TabIndex = 16;
            checkBox_W_Low.UseVisualStyleBackColor = true;
            checkBox_W_Low.CheckedChanged += checkBox_W_Low_CheckedChanged;
            // 
            // checkBox_W_High
            // 
            checkBox_W_High.AutoSize = true;
            checkBox_W_High.Location = new Point(107, 101);
            checkBox_W_High.Name = "checkBox_W_High";
            checkBox_W_High.Size = new Size(15, 14);
            checkBox_W_High.TabIndex = 15;
            checkBox_W_High.UseVisualStyleBackColor = true;
            checkBox_W_High.CheckedChanged += checkBox_W_High_CheckedChanged;
            // 
            // checkBox_B_Low
            // 
            checkBox_B_Low.AutoSize = true;
            checkBox_B_Low.Location = new Point(141, 76);
            checkBox_B_Low.Name = "checkBox_B_Low";
            checkBox_B_Low.Size = new Size(15, 14);
            checkBox_B_Low.TabIndex = 14;
            checkBox_B_Low.UseVisualStyleBackColor = true;
            checkBox_B_Low.CheckedChanged += checkBox_B_Low_CheckedChanged;
            // 
            // checkBox_B_High
            // 
            checkBox_B_High.AutoSize = true;
            checkBox_B_High.Location = new Point(107, 76);
            checkBox_B_High.Name = "checkBox_B_High";
            checkBox_B_High.Size = new Size(15, 14);
            checkBox_B_High.TabIndex = 13;
            checkBox_B_High.UseVisualStyleBackColor = true;
            checkBox_B_High.CheckedChanged += checkBox_B_High_CheckedChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(133, 31);
            label6.Name = "label6";
            label6.Size = new Size(29, 15);
            label6.TabIndex = 12;
            label6.Text = "Low";
            // 
            // checkBox_A_Low
            // 
            checkBox_A_Low.AutoSize = true;
            checkBox_A_Low.Location = new Point(141, 49);
            checkBox_A_Low.Name = "checkBox_A_Low";
            checkBox_A_Low.Size = new Size(15, 14);
            checkBox_A_Low.TabIndex = 11;
            checkBox_A_Low.UseVisualStyleBackColor = true;
            checkBox_A_Low.CheckedChanged += checkBox_A_Low_CheckedChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(99, 31);
            label5.Name = "label5";
            label5.Size = new Size(33, 15);
            label5.TabIndex = 10;
            label5.Text = "High";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(99, 16);
            label4.Name = "label4";
            label4.Size = new Size(45, 15);
            label4.TabIndex = 9;
            label4.Text = "Priority";
            // 
            // checkBox_A_High
            // 
            checkBox_A_High.AutoSize = true;
            checkBox_A_High.Location = new Point(107, 49);
            checkBox_A_High.Name = "checkBox_A_High";
            checkBox_A_High.Size = new Size(15, 14);
            checkBox_A_High.TabIndex = 8;
            checkBox_A_High.UseVisualStyleBackColor = true;
            checkBox_A_High.CheckedChanged += checkBox_A_High_CheckedChanged;
            // 
            // checkBox_viewS
            // 
            checkBox_viewS.AutoSize = true;
            checkBox_viewS.Checked = true;
            checkBox_viewS.CheckState = CheckState.Checked;
            checkBox_viewS.Location = new Point(24, 124);
            checkBox_viewS.Name = "checkBox_viewS";
            checkBox_viewS.Size = new Size(67, 19);
            checkBox_viewS.TabIndex = 7;
            checkBox_viewS.Text = "ScreenS";
            checkBox_viewS.UseVisualStyleBackColor = true;
            checkBox_viewS.CheckedChanged += checkBox_viewS_CheckedChanged;
            // 
            // checkBox_viewW
            // 
            checkBox_viewW.AutoSize = true;
            checkBox_viewW.Checked = true;
            checkBox_viewW.CheckState = CheckState.Checked;
            checkBox_viewW.Location = new Point(24, 99);
            checkBox_viewW.Name = "checkBox_viewW";
            checkBox_viewW.Size = new Size(72, 19);
            checkBox_viewW.TabIndex = 6;
            checkBox_viewW.Text = "ScreenW";
            checkBox_viewW.UseVisualStyleBackColor = true;
            checkBox_viewW.CheckedChanged += checkBox_viewW_CheckedChanged;
            // 
            // checkBox_viewB
            // 
            checkBox_viewB.AutoSize = true;
            checkBox_viewB.Checked = true;
            checkBox_viewB.CheckState = CheckState.Checked;
            checkBox_viewB.Location = new Point(24, 74);
            checkBox_viewB.Name = "checkBox_viewB";
            checkBox_viewB.Size = new Size(68, 19);
            checkBox_viewB.TabIndex = 5;
            checkBox_viewB.Text = "ScreenB";
            checkBox_viewB.UseVisualStyleBackColor = true;
            checkBox_viewB.CheckedChanged += checkBox_viewB_CheckedChanged;
            // 
            // checkBox_viewA
            // 
            checkBox_viewA.AutoSize = true;
            checkBox_viewA.Checked = true;
            checkBox_viewA.CheckState = CheckState.Checked;
            checkBox_viewA.Location = new Point(24, 49);
            checkBox_viewA.Name = "checkBox_viewA";
            checkBox_viewA.Size = new Size(69, 19);
            checkBox_viewA.TabIndex = 4;
            checkBox_viewA.Text = "ScreenA";
            checkBox_viewA.UseVisualStyleBackColor = true;
            checkBox_viewA.CheckedChanged += checkBox_viewA_CheckedChanged;
            // 
            // Form_Setting
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(418, 357);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "Form_Setting";
            Text = "Setting View";
            FormClosing += Form_Setting_FormClosing;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private CheckBox checkBox_pallete;
        private CheckBox checkBox_pattern;
        private CheckBox checkBox_screenS;
        private CheckBox checkBox_B_High;
        private CheckBox checkBox_screenB;
        private CheckBox checkBox_screenA;
        private CheckBox checkBox_register;
        private CheckBox checkBox_music;
        private CheckBox checkBox_code;
        private GroupBox groupBox2;
        private CheckBox checkBox_fsb;
        private GroupBox groupBox3;
        private ComboBox comboBox_rendering;
        private Label label2;
        private Label label1;
        private ComboBox comboBox_videoformat;
        private CheckBox checkBox_screenW;
        private CheckBox checkBox_flow;
        private CheckBox checkBox_sip;
        private Label label3;
        private CheckBox checkBox_io;
        private GroupBox groupBox4;
        private CheckBox checkBox_viewS;
        private CheckBox checkBox_viewW;
        private CheckBox checkBox_viewB;
        private CheckBox checkBox_viewA;
        private Label label7;
        private CheckBox checkBox_S_Low;
        private CheckBox checkBox_S_High;
        private CheckBox checkBox_W_Low;
        private CheckBox checkBox_W_High;
        private CheckBox checkBox_B_Low;
        private Label label6;
        private CheckBox checkBox_A_Low;
        private Label label5;
        private Label label4;
        private CheckBox checkBox_A_High;
    }
}
