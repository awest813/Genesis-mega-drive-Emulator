namespace MDTracer
{
    partial class Form_Code
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
            if (disposing)
            {
                components?.Dispose();
                g_code_font.Dispose();
                g_code_font_data.Dispose();
                g_memory_monitor_timer.Dispose();
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
            menuStrip1 = new MenuStrip();
            menuToolStripMenuItem = new ToolStripMenuItem();
            traceOutputToolStripMenuItem = new ToolStripMenuItem();
            traceToolStripMenuItem = new ToolStripMenuItem();
            runMenuItem = new ToolStripMenuItem();
            stopMenuItem = new ToolStripMenuItem();
            stepOverMenuItem = new ToolStripMenuItem();
            stepInMenuItem1 = new ToolStripMenuItem();
            breakPointMenuItem = new ToolStripMenuItem();
            codeRefleshMenuItem = new ToolStripMenuItem();
            skipnextframeMenuItem = new ToolStripMenuItem();
            splitContainer1 = new SplitContainer();
            pictureBox_code = new DoubleBufferedPictureBox();
            hScrollBar_code = new HScrollBar();
            vScrollBar_code = new VScrollBar();
            dataGridView_memory_history = new DataGridView();
            historyTypeColumn = new DataGridViewTextBoxColumn();
            historyBeforeHexColumn = new DataGridViewTextBoxColumn();
            historyBeforeDecColumn = new DataGridViewTextBoxColumn();
            historyAfterHexColumn = new DataGridViewTextBoxColumn();
            historyAfterDecColumn = new DataGridViewTextBoxColumn();
            historyPcColumn = new DataGridViewTextBoxColumn();
            button_memory_history_clear = new Button();
            label_memory_history = new Label();
            dataGridView_ram_change = new DataGridView();
            ramChangeAddressColumn = new DataGridViewTextBoxColumn();
            ramChangeStartHexColumn = new DataGridViewTextBoxColumn();
            ramChangeStartDecColumn = new DataGridViewTextBoxColumn();
            ramChangeStopHexColumn = new DataGridViewTextBoxColumn();
            ramChangeStopDecColumn = new DataGridViewTextBoxColumn();
            ramChangeBytesColumn = new DataGridViewTextBoxColumn();
            button_ram_change_stop = new Button();
            button_ram_change_start = new Button();
            comboBox_ram_change_bytes = new ComboBox();
            label_ram_change = new Label();
            dataGridView_memory = new DataGridView();
            address = new DataGridViewTextBoxColumn();
            monitorBytes = new DataGridViewComboBoxColumn();
            valHex = new DataGridViewTextBoxColumn();
            valDec = new DataGridViewTextBoxColumn();
            label1 = new Label();
            label_event_wait = new Label();
            label3 = new Label();
            dataGridView_comment1 = new DataGridView();
            comment1Column = new DataGridViewTextBoxColumn();
            commentAddressColumn = new DataGridViewTextBoxColumn();
            commentLineColumn = new DataGridViewTextBoxColumn();
            textBoxAddr = new TextBox();
            label2 = new Label();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_code).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_memory_history).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_ram_change).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_memory).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_comment1).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { menuToolStripMenuItem, traceToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1184, 24);
            menuStrip1.TabIndex = 26;
            menuStrip1.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            menuToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { traceOutputToolStripMenuItem });
            menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            menuToolStripMenuItem.Size = new Size(37, 20);
            menuToolStripMenuItem.Text = "File";
            // 
            // traceOutputToolStripMenuItem
            // 
            traceOutputToolStripMenuItem.Name = "traceOutputToolStripMenuItem";
            traceOutputToolStripMenuItem.Size = new Size(139, 22);
            traceOutputToolStripMenuItem.Text = "trace output";
            traceOutputToolStripMenuItem.Click += traceOutputToolStripMenuItem_Click;
            // 
            // traceToolStripMenuItem
            // 
            traceToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { runMenuItem, stopMenuItem, stepOverMenuItem, stepInMenuItem1, breakPointMenuItem, codeRefleshMenuItem, skipnextframeMenuItem });
            traceToolStripMenuItem.Name = "traceToolStripMenuItem";
            traceToolStripMenuItem.Size = new Size(46, 20);
            traceToolStripMenuItem.Text = "Trace";
            // 
            // runMenuItem
            // 
            runMenuItem.Name = "runMenuItem";
            runMenuItem.ShortcutKeys = Keys.F5;
            runMenuItem.Size = new Size(295, 22);
            runMenuItem.Text = "run";
            runMenuItem.Click += runMenuItem_Click;
            // 
            // stopMenuItem
            // 
            stopMenuItem.Name = "stopMenuItem";
            stopMenuItem.ShortcutKeys = Keys.F6;
            stopMenuItem.Size = new Size(295, 22);
            stopMenuItem.Text = "stop";
            stopMenuItem.Click += stopMenuItem_Click;
            // 
            // stepOverMenuItem
            // 
            stepOverMenuItem.Name = "stepOverMenuItem";
            stepOverMenuItem.ShortcutKeys = Keys.F7;
            stepOverMenuItem.Size = new Size(295, 22);
            stepOverMenuItem.Text = "step over";
            stepOverMenuItem.Click += stepOverMenuItem_Click;
            // 
            // stepInMenuItem1
            // 
            stepInMenuItem1.Name = "stepInMenuItem1";
            stepInMenuItem1.ShortcutKeys = Keys.F8;
            stepInMenuItem1.Size = new Size(295, 22);
            stepInMenuItem1.Text = "step in";
            stepInMenuItem1.Click += stepInMenuItem_Click;
            // 
            // breakPointMenuItem
            // 
            breakPointMenuItem.Name = "breakPointMenuItem";
            breakPointMenuItem.ShortcutKeys = Keys.F9;
            breakPointMenuItem.Size = new Size(295, 22);
            breakPointMenuItem.Text = "break point";
            breakPointMenuItem.Click += breakPointMenuItem_Click;
            // 
            // codeRefleshMenuItem
            // 
            codeRefleshMenuItem.Name = "codeRefleshMenuItem";
            codeRefleshMenuItem.ShortcutKeys = Keys.F10;
            codeRefleshMenuItem.Size = new Size(295, 22);
            codeRefleshMenuItem.Text = "code refresh";
            codeRefleshMenuItem.Click += codeRefleshMenuItem_Click;
            // 
            // skipnextframeMenuItem
            // 
            skipnextframeMenuItem.Name = "skipnextframeMenuItem";
            skipnextframeMenuItem.ShortcutKeys = Keys.Control | Keys.F5;
            skipnextframeMenuItem.Size = new Size(295, 22);
            skipnextframeMenuItem.Text = "skip(next beginning of the frame)";
            skipnextframeMenuItem.Click += skipnextframeMenuItem_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(pictureBox_code);
            splitContainer1.Panel1.Controls.Add(hScrollBar_code);
            splitContainer1.Panel1.Controls.Add(vScrollBar_code);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.AutoScroll = true;
            splitContainer1.Panel2.Controls.Add(dataGridView_memory_history);
            splitContainer1.Panel2.Controls.Add(button_memory_history_clear);
            splitContainer1.Panel2.Controls.Add(label_memory_history);
            splitContainer1.Panel2.Controls.Add(dataGridView_ram_change);
            splitContainer1.Panel2.Controls.Add(button_ram_change_stop);
            splitContainer1.Panel2.Controls.Add(button_ram_change_start);
            splitContainer1.Panel2.Controls.Add(comboBox_ram_change_bytes);
            splitContainer1.Panel2.Controls.Add(label_ram_change);
            splitContainer1.Panel2.Controls.Add(dataGridView_memory);
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.Controls.Add(label_event_wait);
            splitContainer1.Panel2.Controls.Add(label3);
            splitContainer1.Panel2.Controls.Add(dataGridView_comment1);
            splitContainer1.Panel2.Controls.Add(textBoxAddr);
            splitContainer1.Panel2.Controls.Add(label2);
            splitContainer1.Size = new Size(1184, 725);
            splitContainer1.SplitterDistance = 940;
            splitContainer1.TabIndex = 27;
            // 
            // pictureBox_code
            // 
            pictureBox_code.BorderStyle = BorderStyle.FixedSingle;
            pictureBox_code.Dock = DockStyle.Fill;
            pictureBox_code.Location = new Point(0, 0);
            pictureBox_code.Name = "pictureBox_code";
            pictureBox_code.Size = new Size(923, 708);
            pictureBox_code.TabIndex = 10;
            pictureBox_code.TabStop = false;
            pictureBox_code.Paint += pictureBox_code_paint;
            pictureBox_code.MouseClick += pictureBox_code_MouseClick;
            pictureBox_code.MouseDoubleClick += pictureBox_code_MouseDoubleClick;
            // 
            // hScrollBar_code
            // 
            hScrollBar_code.Dock = DockStyle.Bottom;
            hScrollBar_code.Location = new Point(0, 708);
            hScrollBar_code.Maximum = 1000;
            hScrollBar_code.Name = "hScrollBar_code";
            hScrollBar_code.Size = new Size(923, 17);
            hScrollBar_code.TabIndex = 9;
            hScrollBar_code.Scroll += hScrollBar_code_Scroll;
            // 
            // vScrollBar_code
            // 
            vScrollBar_code.Dock = DockStyle.Right;
            vScrollBar_code.Location = new Point(923, 0);
            vScrollBar_code.Maximum = 32767;
            vScrollBar_code.Name = "vScrollBar_code";
            vScrollBar_code.Size = new Size(17, 725);
            vScrollBar_code.TabIndex = 7;
            vScrollBar_code.Scroll += vScrollBar_code_Scroll;
            // 
            // dataGridView_memory_history
            // 
            dataGridView_memory_history.AllowUserToAddRows = false;
            dataGridView_memory_history.AllowUserToDeleteRows = false;
            dataGridView_memory_history.AllowUserToResizeRows = false;
            dataGridView_memory_history.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_memory_history.Columns.AddRange(new DataGridViewColumn[] { historyTypeColumn, historyBeforeHexColumn, historyBeforeDecColumn, historyAfterHexColumn, historyAfterDecColumn, historyPcColumn });
            dataGridView_memory_history.Location = new Point(7, 524);
            dataGridView_memory_history.MultiSelect = false;
            dataGridView_memory_history.Name = "dataGridView_memory_history";
            dataGridView_memory_history.ReadOnly = true;
            dataGridView_memory_history.RowHeadersVisible = false;
            dataGridView_memory_history.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView_memory_history.Size = new Size(227, 170);
            dataGridView_memory_history.TabIndex = 36;
            // 
            // historyTypeColumn
            // 
            historyTypeColumn.HeaderText = "種別";
            historyTypeColumn.Name = "historyTypeColumn";
            historyTypeColumn.ReadOnly = true;
            historyTypeColumn.Width = 50;
            // 
            // historyBeforeHexColumn
            // 
            historyBeforeHexColumn.HeaderText = "前hex";
            historyBeforeHexColumn.Name = "historyBeforeHexColumn";
            historyBeforeHexColumn.ReadOnly = true;
            historyBeforeHexColumn.Width = 70;
            // 
            // historyBeforeDecColumn
            // 
            historyBeforeDecColumn.HeaderText = "前dec";
            historyBeforeDecColumn.Name = "historyBeforeDecColumn";
            historyBeforeDecColumn.ReadOnly = true;
            historyBeforeDecColumn.Width = 70;
            // 
            // historyAfterHexColumn
            // 
            historyAfterHexColumn.HeaderText = "後hex";
            historyAfterHexColumn.Name = "historyAfterHexColumn";
            historyAfterHexColumn.ReadOnly = true;
            historyAfterHexColumn.Width = 70;
            // 
            // historyAfterDecColumn
            // 
            historyAfterDecColumn.HeaderText = "後dec";
            historyAfterDecColumn.Name = "historyAfterDecColumn";
            historyAfterDecColumn.ReadOnly = true;
            historyAfterDecColumn.Width = 70;
            // 
            // historyPcColumn
            // 
            historyPcColumn.HeaderText = "PC";
            historyPcColumn.Name = "historyPcColumn";
            historyPcColumn.ReadOnly = true;
            historyPcColumn.Width = 70;
            // 
            // button_memory_history_clear
            // 
            button_memory_history_clear.Location = new Point(184, 498);
            button_memory_history_clear.Name = "button_memory_history_clear";
            button_memory_history_clear.Size = new Size(50, 23);
            button_memory_history_clear.TabIndex = 35;
            button_memory_history_clear.Text = "clear";
            button_memory_history_clear.UseVisualStyleBackColor = true;
            button_memory_history_clear.Click += button_memory_history_clear_Click;
            // 
            // label_memory_history
            // 
            label_memory_history.AutoSize = true;
            label_memory_history.Location = new Point(7, 502);
            label_memory_history.Name = "label_memory_history";
            label_memory_history.Size = new Size(92, 15);
            label_memory_history.TabIndex = 34;
            label_memory_history.Text = "■access history";
            // 
            // dataGridView_ram_change
            // 
            dataGridView_ram_change.AllowUserToAddRows = false;
            dataGridView_ram_change.AllowUserToDeleteRows = false;
            dataGridView_ram_change.AllowUserToResizeRows = false;
            dataGridView_ram_change.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_ram_change.Columns.AddRange(new DataGridViewColumn[] { ramChangeAddressColumn, ramChangeStartHexColumn, ramChangeStartDecColumn, ramChangeStopHexColumn, ramChangeStopDecColumn, ramChangeBytesColumn });
            dataGridView_ram_change.Location = new Point(7, 374);
            dataGridView_ram_change.MultiSelect = false;
            dataGridView_ram_change.Name = "dataGridView_ram_change";
            dataGridView_ram_change.ReadOnly = true;
            dataGridView_ram_change.RowHeadersVisible = false;
            dataGridView_ram_change.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView_ram_change.Size = new Size(227, 118);
            dataGridView_ram_change.TabIndex = 41;
            dataGridView_ram_change.CellDoubleClick += dataGridView_ram_change_CellDoubleClick;
            // 
            // ramChangeAddressColumn
            // 
            ramChangeAddressColumn.HeaderText = "address";
            ramChangeAddressColumn.Name = "ramChangeAddressColumn";
            ramChangeAddressColumn.ReadOnly = true;
            ramChangeAddressColumn.Width = 70;
            // 
            // ramChangeStartHexColumn
            // 
            ramChangeStartHexColumn.HeaderText = "start hex";
            ramChangeStartHexColumn.Name = "ramChangeStartHexColumn";
            ramChangeStartHexColumn.ReadOnly = true;
            ramChangeStartHexColumn.Width = 70;
            // 
            // ramChangeStartDecColumn
            // 
            ramChangeStartDecColumn.HeaderText = "start dec";
            ramChangeStartDecColumn.Name = "ramChangeStartDecColumn";
            ramChangeStartDecColumn.ReadOnly = true;
            ramChangeStartDecColumn.Width = 70;
            // 
            // ramChangeStopHexColumn
            // 
            ramChangeStopHexColumn.HeaderText = "stop hex";
            ramChangeStopHexColumn.Name = "ramChangeStopHexColumn";
            ramChangeStopHexColumn.ReadOnly = true;
            ramChangeStopHexColumn.Width = 70;
            // 
            // ramChangeStopDecColumn
            // 
            ramChangeStopDecColumn.HeaderText = "stop dec";
            ramChangeStopDecColumn.Name = "ramChangeStopDecColumn";
            ramChangeStopDecColumn.ReadOnly = true;
            ramChangeStopDecColumn.Width = 70;
            // 
            // ramChangeBytesColumn
            // 
            ramChangeBytesColumn.HeaderText = "bytes";
            ramChangeBytesColumn.Name = "ramChangeBytesColumn";
            ramChangeBytesColumn.ReadOnly = true;
            ramChangeBytesColumn.Visible = false;
            // 
            // button_ram_change_stop
            // 
            button_ram_change_stop.Location = new Point(190, 348);
            button_ram_change_stop.Name = "button_ram_change_stop";
            button_ram_change_stop.Size = new Size(44, 23);
            button_ram_change_stop.TabIndex = 40;
            button_ram_change_stop.Text = "stop";
            button_ram_change_stop.UseVisualStyleBackColor = true;
            button_ram_change_stop.Click += button_ram_change_stop_Click;
            // 
            // button_ram_change_start
            // 
            button_ram_change_start.Location = new Point(142, 348);
            button_ram_change_start.Name = "button_ram_change_start";
            button_ram_change_start.Size = new Size(44, 23);
            button_ram_change_start.TabIndex = 39;
            button_ram_change_start.Text = "start";
            button_ram_change_start.UseVisualStyleBackColor = true;
            button_ram_change_start.Click += button_ram_change_start_Click;
            // 
            // comboBox_ram_change_bytes
            // 
            comboBox_ram_change_bytes.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_ram_change_bytes.FormattingEnabled = true;
            comboBox_ram_change_bytes.Items.AddRange(new object[] { "1", "2", "4" });
            comboBox_ram_change_bytes.Location = new Point(98, 348);
            comboBox_ram_change_bytes.Name = "comboBox_ram_change_bytes";
            comboBox_ram_change_bytes.Size = new Size(40, 23);
            comboBox_ram_change_bytes.TabIndex = 38;
            // 
            // label_ram_change
            // 
            label_ram_change.AutoSize = true;
            label_ram_change.Location = new Point(7, 352);
            label_ram_change.Name = "label_ram_change";
            label_ram_change.Size = new Size(92, 15);
            label_ram_change.TabIndex = 37;
            label_ram_change.Text = "■RAM changes";
            // 
            // dataGridView_memory
            // 
            dataGridView_memory.AllowUserToResizeRows = false;
            dataGridView_memory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_memory.Columns.AddRange(new DataGridViewColumn[] { address, monitorBytes, valHex, valDec });
            dataGridView_memory.Location = new Point(7, 234);
            dataGridView_memory.MultiSelect = false;
            dataGridView_memory.Name = "dataGridView_memory";
            dataGridView_memory.RowHeadersVisible = false;
            dataGridView_memory.ShowCellErrors = false;
            dataGridView_memory.ShowCellToolTips = false;
            dataGridView_memory.ShowEditingIcon = false;
            dataGridView_memory.ShowRowErrors = false;
            dataGridView_memory.Size = new Size(227, 110);
            dataGridView_memory.TabIndex = 33;
            dataGridView_memory.CellClick += dataGridView_memory_CellClick;
            dataGridView_memory.CellEndEdit += dataGridView_memory_CellEndEdit;
            // 
            // address
            // 
            address.HeaderText = "address";
            address.Name = "address";
            address.Resizable = DataGridViewTriState.False;
            address.Width = 70;
            // 
            // monitorBytes
            // 
            monitorBytes.HeaderText = "bytes";
            monitorBytes.Items.AddRange(new object[] { "1", "2", "4" });
            monitorBytes.Name = "monitorBytes";
            monitorBytes.Resizable = DataGridViewTriState.False;
            monitorBytes.Width = 45;
            // 
            // valHex
            // 
            valHex.HeaderText = "hex";
            valHex.Name = "valHex";
            valHex.ReadOnly = true;
            valHex.Resizable = DataGridViewTriState.False;
            valHex.Width = 75;
            // 
            // valDec
            // 
            valDec.HeaderText = "dec";
            valDec.Name = "valDec";
            valDec.ReadOnly = true;
            valDec.Resizable = DataGridViewTriState.False;
            valDec.Width = 85;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 216);
            label1.Name = "label1";
            label1.Size = new Size(130, 15);
            label1.TabIndex = 32;
            label1.Text = "■memory  monitoring ";
            // 
            // label_event_wait
            // 
            label_event_wait.AutoSize = true;
            label_event_wait.Font = new Font("Yu Gothic UI", 9.75F, FontStyle.Bold);
            label_event_wait.ForeColor = Color.Red;
            label_event_wait.Location = new Point(5, 244);
            label_event_wait.Name = "label_event_wait";
            label_event_wait.Size = new Size(28, 17);
            label_event_wait.TabIndex = 31;
            label_event_wait.Text = "     ";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(5, 33);
            label3.Name = "label3";
            label3.Size = new Size(107, 15);
            label3.TabIndex = 24;
            label3.Text = "■comment1,result";
            // 
            // dataGridView_comment1
            // 
            dataGridView_comment1.AllowUserToAddRows = false;
            dataGridView_comment1.AllowUserToDeleteRows = false;
            dataGridView_comment1.AllowUserToResizeRows = false;
            dataGridView_comment1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_comment1.Columns.AddRange(new DataGridViewColumn[] { comment1Column, commentAddressColumn, commentLineColumn });
            dataGridView_comment1.Location = new Point(7, 51);
            dataGridView_comment1.MultiSelect = false;
            dataGridView_comment1.Name = "dataGridView_comment1";
            dataGridView_comment1.ReadOnly = true;
            dataGridView_comment1.RowHeadersVisible = false;
            dataGridView_comment1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView_comment1.Size = new Size(227, 153);
            dataGridView_comment1.TabIndex = 23;
            dataGridView_comment1.CellClick += dataGridView_comment1_CellClick;
            dataGridView_comment1.CellDoubleClick += dataGridView_comment1_CellDoubleClick;
            // 
            // comment1Column
            // 
            comment1Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            comment1Column.HeaderText = "comment1";
            comment1Column.Name = "comment1Column";
            comment1Column.ReadOnly = true;
            // 
            // commentAddressColumn
            // 
            commentAddressColumn.HeaderText = "address";
            commentAddressColumn.Name = "commentAddressColumn";
            commentAddressColumn.ReadOnly = true;
            commentAddressColumn.Width = 70;
            // 
            // commentLineColumn
            // 
            commentLineColumn.HeaderText = "line";
            commentLineColumn.Name = "commentLineColumn";
            commentLineColumn.ReadOnly = true;
            commentLineColumn.Visible = false;
            // 
            // textBoxAddr
            // 
            textBoxAddr.Location = new Point(72, 3);
            textBoxAddr.MaxLength = 8;
            textBoxAddr.Name = "textBoxAddr";
            textBoxAddr.Size = new Size(60, 23);
            textBoxAddr.TabIndex = 20;
            textBoxAddr.TextChanged += textBoxAddr_TextChanged_1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(5, 6);
            label2.Name = "label2";
            label2.Size = new Size(59, 15);
            label2.TabIndex = 21;
            label2.Text = "■address";
            // 
            // Form_Code
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 749);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            KeyPreview = true;
            Name = "Form_Code";
            Text = "Code View";
            FormClosing += Form_Code_FormClosing;
            Shown += Form_Code_Shown;
            ResizeEnd += Form_Code_ResizeEnd;
            KeyDown += Form_Code_KeyDown;
            Resize += Form_Code_Resize;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox_code).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_memory_history).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_ram_change).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_memory).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView_comment1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuToolStripMenuItem;
        private ToolStripMenuItem traceOutputToolStripMenuItem;
        private ToolStripMenuItem traceToolStripMenuItem;
        private ToolStripMenuItem runMenuItem;
        private ToolStripMenuItem stopMenuItem;
        private ToolStripMenuItem stepOverMenuItem;
        private SplitContainer splitContainer1;
        private VScrollBar vScrollBar_code;
        private Label label_event_wait;
        private Label label3;
        private DataGridView dataGridView_comment1;
        private TextBox textBoxAddr;
        private Label label2;
        private ToolStripMenuItem codeRefleshMenuItem;
        private HScrollBar hScrollBar_code;
        private ToolStripMenuItem breakPointMenuItem;
        private ToolStripMenuItem stepInMenuItem1;
        private ToolStripMenuItem skipnextframeMenuItem;
        private DataGridView dataGridView_memory;
        private Label label1;
        private DataGridViewTextBoxColumn address;
        private DataGridViewComboBoxColumn monitorBytes;
        private DataGridViewTextBoxColumn valHex;
        private DataGridViewTextBoxColumn valDec;
        private DataGridViewTextBoxColumn comment1Column;
        private DataGridViewTextBoxColumn commentAddressColumn;
        private DataGridViewTextBoxColumn commentLineColumn;
        private Label label_memory_history;
        private Button button_memory_history_clear;
        private DataGridView dataGridView_memory_history;
        private DataGridViewTextBoxColumn historyTypeColumn;
        private DataGridViewTextBoxColumn historyBeforeHexColumn;
        private DataGridViewTextBoxColumn historyBeforeDecColumn;
        private DataGridViewTextBoxColumn historyAfterHexColumn;
        private DataGridViewTextBoxColumn historyAfterDecColumn;
        private DataGridViewTextBoxColumn historyPcColumn;
        private Label label_ram_change;
        private ComboBox comboBox_ram_change_bytes;
        private Button button_ram_change_start;
        private Button button_ram_change_stop;
        private DataGridView dataGridView_ram_change;
        private DataGridViewTextBoxColumn ramChangeAddressColumn;
        private DataGridViewTextBoxColumn ramChangeStartHexColumn;
        private DataGridViewTextBoxColumn ramChangeStartDecColumn;
        private DataGridViewTextBoxColumn ramChangeStopHexColumn;
        private DataGridViewTextBoxColumn ramChangeStopDecColumn;
        private DataGridViewTextBoxColumn ramChangeBytesColumn;
        private DoubleBufferedPictureBox pictureBox_code;
    }
}
