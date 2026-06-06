using NAudio.Gui;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static MDTracer.Form_Code_Trace;

namespace MDTracer
{
    public partial class Form_Code : Form, IBusMonitor
    {
        private const int CODE_HEADER_HEIGHT = 20;
        private const int CODE_LINE_HEIGHT = 16;
        private const int CODE_BREAK_COLUMN_WIDTH = 20;
        private const int CODE_ADDRESS_X = 20;
        private const int CODE_DUMP_X = 70;
        private const int CODE_MNEMONIC_HEADER_X = 240;
        private const int CODE_MNEMONIC_X = 270;
        private const int CODE_COMMENT_X = 750;
        private const int CODE_JUMP_X = 238;
        private const int CODE_HORIZONTAL_CONTENT_WIDTH = 1500;
        private const int RAM_CHANGE_BASE_OFFSET = 0xff0000;
        private const uint RAM_CHANGE_BASE_ADDRESS = 0xff0000;
        private const int RAM_CHANGE_SIZE = 0x10000;
        private const string GAME_BREAKPOINT_SETTING_SUFFIX = "breakpoints";
        private const string GAME_MEMORY_MONITOR_SETTING_SUFFIX = "memorymonitoring";

        private readonly Font g_code_font = new Font("Consolas", 10);
        private readonly Font g_code_font_data = new Font("Consolas", 9);

        public int g_top_line;
        public int g_stop_line;
        public int g_cursole_line;
        public int g_screen_xpos;
        public int g_screen_ypos;

        private sealed class MEMORY_MONITOR_ENTRY
        {
            public uint address;
            public int byte_count;
            public uint value;
            public int row_index;
            public readonly List<MEMORY_ACCESS_HISTORY> history = new List<MEMORY_ACCESS_HISTORY>();
        }

        private sealed class MEMORY_ACCESS_HISTORY
        {
            public bool write_enable;
            public uint before_value;
            public uint after_value;
            public uint pc;
        }

        private readonly List<MEMORY_MONITOR_ENTRY> g_memory_monitor = new List<MEMORY_MONITOR_ENTRY>();
        private readonly object g_memory_monitor_lock = new object();
        private readonly object g_memory_monitor_update_lock = new object();
        private readonly Dictionary<int, uint> g_memory_monitor_pending_values = new Dictionary<int, uint>();
        private readonly System.Windows.Forms.Timer g_memory_monitor_timer = new System.Windows.Forms.Timer();
        private volatile bool g_memory_monitor_active;
        private bool g_memory_monitor_update_queued;
        private volatile int g_selected_memory_monitor_row = -1;
        private bool g_memory_history_refresh_queued;
        private bool g_game_tool_setting_loading;
        private byte[] g_ram_change_start_snapshot = Array.Empty<byte>();
        private int g_ram_change_byte_count = 1;
        public bool memory_monitor_active => g_memory_monitor_active;
        //----------------------------------------------------------------
        //form
        //----------------------------------------------------------------
        public Form_Code()
        {
            InitializeComponent();
            InitializeDockPanelSuite();
            pictureBox_code.MouseWheel += PictureBox_code_MouseWheel;

            scrollbar_set();
            WinFormsDebugTools.g_form_code_trace.g_arrow_start_line = -1;
            WinFormsDebugTools.g_form_code_trace.g_arrow_end_line = -1;

            g_memory_monitor_timer.Interval = 100;
            g_memory_monitor_timer.Tick += memory_monitor_timer_Tick;
            g_memory_monitor_timer.Start();
        }

        private sealed class DoubleBufferedPictureBox : PictureBox
        {
            public DoubleBufferedPictureBox()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }

        //----------------------------------------------------------------
        //Event Handling: Painting
        //----------------------------------------------------------------
        private void pictureBox_code_paint(object sender, PaintEventArgs e)
        {
            SetScrollBarValue(vScrollBar_code, g_top_line);
            Code_Paint_Code(e, pictureBox_code.Width
                                            , pictureBox_code.Height
                                            , pictureBox_Code_line_num()
                                            , g_top_line
                                            , g_stop_line
                                            , g_cursole_line
                                            , hScrollBar_code.Value);
        }
        //----------------------------------------------------------------               
        //Event Handling: Screen Operations
        //----------------------------------------------------------------
        private void Form_Code_Resize(object sender, EventArgs e)
        {
            scrollbar_set();
            pictureBox_code.Invalidate();
        }
        private void Form_Code_FormClosing(object sender, FormClosingEventArgs e)
        {
            md_main.g_code_enable = false;
            WinFormsDebugTools.g_form_setting.update();
            SaveCurrentGameCodeSettings();
            FlushCodeToolLayoutSave();
            md_main.write_setting();
            e.Cancel = true;
        }
        private void Form_Code_ResizeEnd(object sender, EventArgs e)
        {
            var currentPosition = this.Location;
            g_screen_xpos = currentPosition.X;
            g_screen_ypos = currentPosition.Y;
            md_main.write_setting();
        }
        private void Form_Code_Shown(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(g_screen_xpos, g_screen_ypos);
            RestorePendingCodeToolLayout();
            EnsureCodeToolLayoutVisible();
            RefreshComment1List(true);
        }

        //----------------------------------------------------------------
        //Event Handling: mouse operations
        //----------------------------------------------------------------
        private void vScrollBar_code_Scroll(object sender, ScrollEventArgs e)
        {
            picturebox_scroll(e.NewValue, 0);
        }
        private void hScrollBar_code_Scroll(object sender, ScrollEventArgs e)
        {
            SetScrollBarValue(hScrollBar_code, e.NewValue);
            pictureBox_code.Invalidate();
        }

        private void PictureBox_code_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (e.Delta == 0) return;

            int w_notches = Math.Abs(e.Delta / SystemInformation.MouseWheelScrollDelta);
            if (w_notches == 0) w_notches = 1;

            int w_lines_per_notch = SystemInformation.MouseWheelScrollLines;
            if ((w_lines_per_notch <= 0) || (w_lines_per_notch > pictureBox_Code_line_num()))
            {
                w_lines_per_notch = Math.Max(1, pictureBox_Code_line_num() - 1);
            }

            int w_cur = -Math.Sign(e.Delta) * w_notches * w_lines_per_notch;
            picturebox_scroll(g_top_line, w_cur);
        }
        private void pictureBox_code_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int w_cur = GetCodeLineFromMouseY(e.Y);
                if (w_cur < 0) return;
                int w_content_x = GetCodeContentX(e.X);
                if (w_content_x < CODE_BREAK_COLUMN_WIDTH)
                {
                    ToggleBreakpoint(w_cur);
                }
                else
                {
                    g_cursole_line = w_cur;
                }
                int w_jmp = WinFormsDebugTools.g_form_code_trace.g_analyse_code[w_cur].jmp_address;
                int w_jmp_line = WinFormsDebugTools.g_form_code_trace.get_code_from_addr((uint)w_jmp);
                if ((w_jmp != 0) && (w_jmp_line >= 0))
                {
                    WinFormsDebugTools.g_form_code_trace.g_arrow_start_line = w_cur;
                    WinFormsDebugTools.g_form_code_trace.g_arrow_end_line = w_jmp_line;
                }
                else
                {
                    WinFormsDebugTools.g_form_code_trace.g_arrow_start_line = -1;
                    WinFormsDebugTools.g_form_code_trace.g_arrow_end_line = -1;
                }
                pictureBox_code.Invalidate();
            }
        }
        private void pictureBox_code_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int w_content_x = GetCodeContentX(e.X);
                if ((CODE_MNEMONIC_HEADER_X <= w_content_x) && (w_content_x < CODE_COMMENT_X))
                {
                    int w_cur = GetCodeLineFromMouseY(e.Y);
                    if (w_cur < 0) return;
                    int w_jmp = WinFormsDebugTools.g_form_code_trace.g_analyse_code[w_cur].jmp_address;
                    int w_jmp_line = WinFormsDebugTools.g_form_code_trace.get_code_from_addr((uint)w_jmp);
                    if ((w_jmp != 0) && (w_jmp_line >= 0))
                    {
                        g_cursole_line = w_jmp_line;
                        picturebox_scroll(g_cursole_line, -pictureBox_Code_line_num() / 2);
                    }
                }
            }
        }
        //----------------------------------------------------------------
        //Event Handling: key operations
        //----------------------------------------------------------------
        private void Form_Code_KeyDown(object sender, KeyEventArgs e)
        {
            int w_page = Math.Max(1, pictureBox_Code_line_num() - 1);
            switch (e.KeyCode)
            {
                case Keys.Down:
                    g_cursole_line = GetNextCodeLine(g_cursole_line);
                    EnsureCodeLineVisible(g_cursole_line);
                    break;
                case Keys.Up:
                    g_cursole_line = GetPreviousCodeLine(g_cursole_line);
                    EnsureCodeLineVisible(g_cursole_line);
                    break;
                case Keys.PageDown:
                    g_cursole_line = MoveCodeLine(g_cursole_line, w_page);
                    picturebox_scroll(g_top_line, w_page);
                    EnsureCodeLineVisible(g_cursole_line);
                    break;
                case Keys.PageUp:
                    g_cursole_line = MoveCodeLine(g_cursole_line, -w_page);
                    picturebox_scroll(g_top_line, -w_page);
                    EnsureCodeLineVisible(g_cursole_line);
                    break;
            }
        }
        //----------------------------------------------------------------
        //Event Handling: menu
        //----------------------------------------------------------------
        private void runMenuItem_Click(object sender, EventArgs e)
        {
            WinFormsDebugTools.g_form_code_trace.Trace_Start();
        }

        private void stopMenuItem_Click(object sender, EventArgs e)
        {
            WinFormsDebugTools.g_form_code_trace.Trace_Stop();
        }
        private void stepOverMenuItem_Click(object sender, EventArgs e)
        {
            WinFormsDebugTools.g_form_code_trace.Trace_StepOver();
        }
        private void stepInMenuItem_Click(object sender, EventArgs e)
        {
            WinFormsDebugTools.g_form_code_trace.Trace_StepIn();
        }
        private void breakPointMenuItem_Click(object sender, EventArgs e)
        {
            ToggleBreakpoint(g_cursole_line);
            pictureBox_code.Invalidate();
        }
        private void codeRefleshMenuItem_Click(object sender, EventArgs e)
        {
            RefreshComment1List(true);
            pictureBox_code.Invalidate();
        }

        private void skipnextframeMenuItem_Click(object sender, EventArgs e)
        {
            md_main.g_trace_nextframe = true;
            WinFormsDebugTools.g_form_code_trace.Trace_Start();
        }
        private void traceOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog w_dialog = new SaveFileDialog())
            {
                w_dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                w_dialog.FileName = "trace_output_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                w_dialog.RestoreDirectory = true;
                if (w_dialog.ShowDialog(this) != DialogResult.OK) return;

                WinFormsDebugTools.g_form_code_trace.analyses();
                WriteTraceCodeCsv(w_dialog.FileName);
                MessageBox.Show(this, "Trace output completed.", "trace output", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        //----------------------------------------------------------------
        //sub function
        //----------------------------------------------------------------
        private int GetCodeLineFromMouseY(int in_y)
        {
            if (in_y < CODE_HEADER_HEIGHT) return -1;

            int w_visibleLine = (in_y - CODE_HEADER_HEIGHT) / CODE_LINE_HEIGHT;
            if (w_visibleLine < 0 || pictureBox_Code_line_num() <= w_visibleLine) return -1;

            int w_line = g_top_line;
            for (int i = 0; i < w_visibleLine; i++)
            {
                if (IsCodeLineInRange(w_line) == false) return -1;
                w_line += GetTraceCodeDumpLength(w_line);
            }

            return IsCodeLineInRange(w_line) ? w_line : -1;
        }

        private int GetNextCodeLine(int in_line)
        {
            if (IsCodeLineInRange(in_line) == false) return 0;

            int w_line = in_line + Math.Max(1, WinFormsDebugTools.g_form_code_trace.g_analyse_code[in_line].leng2);
            return Math.Min(w_line, Form_Code_Trace.MEMSIZE - 1);
        }

        private int GetPreviousCodeLine(int in_line)
        {
            if (in_line <= 0) return 0;
            int w_line = Math.Min(in_line, Form_Code_Trace.MEMSIZE - 1) - 1;
            int w_front = Math.Max(0, WinFormsDebugTools.g_form_code_trace.g_analyse_code[w_line].front);
            return Math.Max(0, w_line - w_front);
        }

        private bool IsCodeLineInRange(int in_line)
        {
            return (0 <= in_line) && (in_line < Form_Code_Trace.MEMSIZE);
        }

        private int GetCodeContentX(int in_x)
        {
            return in_x + hScrollBar_code.Value;
        }

        private int NormalizeCodeLine(int in_line)
        {
            if (IsCodeLineInRange(in_line) == false) return 0;

            int w_front = Math.Max(0, WinFormsDebugTools.g_form_code_trace.g_analyse_code[in_line].front);
            return Math.Max(0, in_line - w_front);
        }

        private int MoveCodeLine(int in_line, int in_offset)
        {
            int w_line = NormalizeCodeLine(in_line);
            int w_step_count = Math.Abs(in_offset);
            for (int i = 0; i < w_step_count; i++)
            {
                int w_next_line = (in_offset > 0) ? GetNextCodeLine(w_line) : GetPreviousCodeLine(w_line);
                if (w_next_line == w_line) break;
                w_line = w_next_line;
            }
            return w_line;
        }

        private bool IsCodeLineVisible(int in_line)
        {
            if (IsCodeLineInRange(in_line) == false) return false;

            int w_line = g_top_line;
            int w_visible_line_count = pictureBox_Code_line_num();
            for (int i = 0; i < w_visible_line_count; i++)
            {
                if (IsCodeLineInRange(w_line) == false) return false;

                int w_dump_len = GetTraceCodeDumpLength(w_line);
                if ((w_line <= in_line) && (in_line < w_line + w_dump_len)) return true;
                if (w_line > in_line) return false;
                w_line += w_dump_len;
            }
            return false;
        }

        private void EnsureCodeLineVisible(int in_line)
        {
            if (IsCodeLineInRange(in_line) == false) return;
            if (IsCodeLineVisible(in_line) == true)
            {
                pictureBox_code.Invalidate();
                return;
            }

            if (in_line < g_top_line)
            {
                picturebox_scroll(in_line, 0);
                return;
            }

            picturebox_scroll(in_line, -(pictureBox_Code_line_num() - 1));
        }

        private void ToggleBreakpoint(int in_line)
        {
            if (IsCodeLineInRange(in_line) == false) return;

            TRACECODE w_code = WinFormsDebugTools.g_form_code_trace.g_analyse_code[in_line];
            w_code.break_static = w_code.break_static == false;
            WinFormsDebugTools.g_form_code_trace.g_analyse_code[in_line] = w_code;
            SaveCurrentGameBreakpointSettings();
        }

        public void SaveCurrentGameCodeSettings()
        {
            if (g_game_tool_setting_loading == true) return;

            if (dataGridView_memory.IsCurrentCellInEditMode == true)
            {
                dataGridView_memory.EndEdit();
            }
            SaveCurrentGameBreakpointSettings();
            SaveCurrentGameMemoryMonitorSettings();
        }

        public void LoadCurrentGameCodeSettings()
        {
            string w_file_name = GetCurrentGameFileName();
            if (string.IsNullOrWhiteSpace(w_file_name) == true) return;

            g_game_tool_setting_loading = true;
            try
            {
                SetBreakpointSettingText(md_main.get_game_setting(w_file_name, GAME_BREAKPOINT_SETTING_SUFFIX));
                SetMemoryMonitorSettingText(md_main.get_game_setting(w_file_name, GAME_MEMORY_MONITOR_SETTING_SUFFIX));
            }
            finally
            {
                g_game_tool_setting_loading = false;
            }

            RefreshMemoryAccessHistoryList();
            pictureBox_code.Invalidate();
        }

        private void SaveCurrentGameBreakpointSettings()
        {
            if (g_game_tool_setting_loading == true) return;

            string w_file_name = GetCurrentGameFileName();
            if (string.IsNullOrWhiteSpace(w_file_name) == true) return;

            md_main.set_game_setting(w_file_name, GAME_BREAKPOINT_SETTING_SUFFIX, GetBreakpointSettingText());
        }

        private void SaveCurrentGameMemoryMonitorSettings()
        {
            if (g_game_tool_setting_loading == true) return;

            string w_file_name = GetCurrentGameFileName();
            if (string.IsNullOrWhiteSpace(w_file_name) == true) return;

            md_main.set_game_setting(w_file_name, GAME_MEMORY_MONITOR_SETTING_SUFFIX, GetMemoryMonitorSettingText());
        }

        private string GetCurrentGameFileName()
        {
            return md_main.g_state_capture_rom_file_name ?? "";
        }

        private string GetBreakpointSettingText()
        {
            List<string> w_breakpoints = new List<string>();
            for (int i = 0; i < Form_Code_Trace.MEMSIZE; i++)
            {
                TRACECODE w_code = WinFormsDebugTools.g_form_code_trace.g_analyse_code[i];
                if (w_code.break_static == false) continue;

                w_breakpoints.Add((w_code.address & 0x00ffffff).ToString("X6", CultureInfo.InvariantCulture));
            }
            return string.Join(",", w_breakpoints);
        }

        private void SetBreakpointSettingText(string in_text)
        {
            for (int i = 0; i < Form_Code_Trace.MEMSIZE; i++)
            {
                TRACECODE w_code = WinFormsDebugTools.g_form_code_trace.g_analyse_code[i];
                if (w_code.break_static == false) continue;

                w_code.break_static = false;
                WinFormsDebugTools.g_form_code_trace.g_analyse_code[i] = w_code;
            }

            foreach (string w_address_text in SplitGameSettingEntries(in_text))
            {
                if (TryParseHexAddress(w_address_text, out uint w_addr) == false) continue;

                int w_line = WinFormsDebugTools.g_form_code_trace.get_code_from_addr(w_addr);
                if (IsCodeLineInRange(w_line) == false) continue;

                TRACECODE w_code = WinFormsDebugTools.g_form_code_trace.g_analyse_code[w_line];
                w_code.break_static = true;
                WinFormsDebugTools.g_form_code_trace.g_analyse_code[w_line] = w_code;
            }
        }

        private string GetMemoryMonitorSettingText()
        {
            List<string> w_items = new List<string>();
            lock (g_memory_monitor_lock)
            {
                foreach (MEMORY_MONITOR_ENTRY w_monitor in g_memory_monitor)
                {
                    string w_address = (w_monitor.address & 0x00ffffff).ToString("X6", CultureInfo.InvariantCulture);
                    string w_byte_count = NormalizeMemoryMonitorByteCount(w_monitor.byte_count).ToString(CultureInfo.InvariantCulture);
                    w_items.Add(w_address + ":" + w_byte_count);
                }
            }
            return string.Join(",", w_items);
        }

        private void SetMemoryMonitorSettingText(string in_text)
        {
            dataGridView_memory.SuspendLayout();
            try
            {
                dataGridView_memory.Rows.Clear();
                foreach (string w_entry in SplitGameSettingEntries(in_text))
                {
                    string[] w_pair = w_entry.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (w_pair.Length == 0) continue;
                    if (TryParseHexAddress(w_pair[0], out uint w_addr) == false) continue;

                    int w_byte_count = 1;
                    if ((w_pair.Length > 1) && (int.TryParse(w_pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int w_parsed_byte_count) == true))
                    {
                        w_byte_count = w_parsed_byte_count;
                    }
                    w_byte_count = NormalizeMemoryMonitorByteCount(w_byte_count);
                    dataGridView_memory.Rows.Add(
                        (w_addr & 0x00ffffff).ToString("X6", CultureInfo.InvariantCulture),
                        w_byte_count.ToString(CultureInfo.InvariantCulture),
                        "",
                        "");
                }
            }
            finally
            {
                dataGridView_memory.ResumeLayout();
            }

            RefreshMemoryMonitorEntries();
            int w_first_row = GetNextMemoryMonitorRowIndex(0);
            if (w_first_row >= 0)
            {
                SelectMemoryMonitorGridRow(w_first_row);
            }
            else
            {
                dataGridView_memory.ClearSelection();
                dataGridView_memory.CurrentCell = null;
                SelectMemoryMonitorRow(-1);
            }
        }

        private string[] SplitGameSettingEntries(string in_text)
        {
            if (string.IsNullOrWhiteSpace(in_text) == true) return Array.Empty<string>();

            return in_text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private void SetScrollBarValue(ScrollBar in_scroll_bar, int in_value)
        {
            int w_effective_max = Math.Max(in_scroll_bar.Minimum, in_scroll_bar.Maximum - in_scroll_bar.LargeChange + 1);
            in_scroll_bar.Value = Math.Clamp(in_value, in_scroll_bar.Minimum, w_effective_max);
        }

        public int pictureBox_Code_line_num()
        {
            int w_body_height = Math.Max(0, pictureBox_code.ClientSize.Height - CODE_HEADER_HEIGHT);
            return Math.Max(1, (w_body_height + CODE_LINE_HEIGHT - 1) / CODE_LINE_HEIGHT);
        }

        public void RequestTraceBreakView(int in_line)
        {
            if (IsDisposed == true || IsHandleCreated == false) return;
            if (InvokeRequired == true)
            {
                try
                {
                    BeginInvoke(new Action<int>(RequestTraceBreakView), in_line);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                return;
            }

            if (IsCodeLineInRange(in_line) == false) return;
            g_stop_line = in_line;
            int w_line_offset = pictureBox_Code_line_num() >> 1;
            picturebox_scroll(in_line, -w_line_offset);
            Invalidate();
        }

        private void scrollbar_set()
        {
            int w_leng = pictureBox_Code_line_num();
            vScrollBar_code.Minimum = 0;
            vScrollBar_code.SmallChange = 1;
            vScrollBar_code.LargeChange = w_leng;
            vScrollBar_code.Maximum = Form_Code_Trace.MEMSIZE - 1 + w_leng - 1;
            SetScrollBarValue(vScrollBar_code, g_top_line);

            int w_client_width = Math.Max(1, pictureBox_code.ClientSize.Width);
            int w_h_large_change = Math.Max(1, Math.Min(w_client_width, CODE_HORIZONTAL_CONTENT_WIDTH));
            int w_h_max_value = Math.Max(0, CODE_HORIZONTAL_CONTENT_WIDTH - w_client_width);
            hScrollBar_code.Minimum = 0;
            hScrollBar_code.SmallChange = CODE_LINE_HEIGHT;
            hScrollBar_code.LargeChange = w_h_large_change;
            hScrollBar_code.Maximum = w_h_max_value + w_h_large_change - 1;
            SetScrollBarValue(hScrollBar_code, hScrollBar_code.Value);
            hScrollBar_code.Enabled = w_h_max_value > 0;
        }
        public void picturebox_scroll(int in_line, int in_line_offset)
        {
            if (IsCodeLineInRange(in_line) == false) return;

            int w_line = NormalizeCodeLine(in_line);
            int w_step_count = Math.Abs(in_line_offset);
            for (int i = 0; i < w_step_count; i++)
            {
                int w_next_line = (in_line_offset > 0) ? GetNextCodeLine(w_line) : GetPreviousCodeLine(w_line);
                if (w_next_line == w_line)
                {
                    break;
                }
                w_line = w_next_line;
            }

            g_top_line = w_line;
            SetScrollBarValue(vScrollBar_code, g_top_line);
            pictureBox_code.Invalidate();
        }
        private void WriteTraceCodeCsv(string in_path)
        {
            using (StreamWriter w_writer = new StreamWriter(in_path, false, new UTF8Encoding(true)))
            {
                w_writer.WriteLine("addr,dump,mnemonic,comment1");
                int w_cur_line = 0;
                while (w_cur_line < Form_Code_Trace.MEMSIZE)
                {
                    TRACECODE w_code = WinFormsDebugTools.g_form_code_trace.g_analyse_code[w_cur_line];
                    int w_dump_len = GetTraceCodeDumpLength(w_cur_line);
                    if (w_dump_len <= 0) break;

                    string w_addr = w_code.address.ToString("X6");
                    string w_dump = GetTraceCodeDumpText(w_cur_line, w_dump_len);
                    string w_mnemonic = (w_code.type == TRACECODE.TYPE.OPC) ? (w_code.operand ?? "") : "";
                    string w_comment1 = w_code.comment1 ?? "";

                    w_writer.WriteLine(
                        CsvEscape(w_addr) + "," +
                        CsvEscape(w_dump) + "," +
                        CsvEscape(w_mnemonic) + "," +
                        CsvEscape(w_comment1));

                    w_cur_line += w_dump_len;
                }
            }
        }
        private int GetTraceCodeDumpLength(int in_line)
        {
            TRACECODE w_code = WinFormsDebugTools.g_form_code_trace.g_analyse_code[in_line];
            if (w_code.type != TRACECODE.TYPE.NON)
            {
                return Math.Max(1, w_code.leng2);
            }

            int w_dump_len = 0;
            for (int i = 0; i < 8; i++)
            {
                if (Form_Code_Trace.MEMSIZE <= in_line + i) break;
                if (WinFormsDebugTools.g_form_code_trace.g_analyse_code[in_line + i].type != TRACECODE.TYPE.NON) break;
                w_dump_len += 1;
            }
            return w_dump_len;
        }
        private string GetTraceCodeDumpText(int in_line, int in_dump_len)
        {
            StringBuilder w_builder = new StringBuilder();
            for (int i = 0; i < in_dump_len; i++)
            {
                if (i != 0) w_builder.Append(' ');
                w_builder.Append(WinFormsDebugTools.g_form_code_trace.g_analyse_code[in_line + i].val.ToString("X4"));
            }
            return w_builder.ToString();
        }
        private string CsvEscape(string in_value)
        {
            if (in_value.Contains('"') || in_value.Contains(',') || in_value.Contains('\r') || in_value.Contains('\n'))
            {
                return "\"" + in_value.Replace("\"", "\"\"") + "\"";
            }
            return in_value;
        }
        //----------------------------------------------------------------
        //Event Handling: search
        //----------------------------------------------------------------
        private void textBoxAddr_TextChanged_1(object sender, EventArgs e)
        {
            string w_text = textBoxAddr.Text.Trim();
            if (w_text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) == true)
            {
                w_text = w_text.Substring(2);
            }
            if (w_text.Length < 6) return;

            if (TryParseHexAddress(textBoxAddr.Text, out uint waddr))
            {
                int w_line = WinFormsDebugTools.g_form_code_trace.get_code_from_addr(waddr);
                if (w_line >= 0)
                {
                    picturebox_scroll(w_line, 0);
                    this.Invalidate();
                }
            }
        }

        private void dataGridView_comment1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (TryGetSelectedCommentSearchLine(out int w_line) == true)
            {
                g_cursole_line = w_line;
                picturebox_scroll(w_line, 0);
                this.Invalidate();
            }
        }
        private void dataGridView_comment1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (TryGetSelectedCommentSearchLine(out int w_line) == true)
            {
                ToggleBreakpoint(w_line);
                picturebox_scroll(w_line, 0);
                this.Invalidate();
            }
        }

        private void RefreshComment1List(bool in_run_analysis)
        {
            if (in_run_analysis == true)
            {
                WinFormsDebugTools.g_form_code_trace.analyses();
            }

            dataGridView_comment1.SuspendLayout();
            try
            {
                dataGridView_comment1.Rows.Clear();
                for (int w_line = 0; w_line < Form_Code_Trace.MEMSIZE; w_line++)
                {
                    Form_Code_Trace.TRACECODE w_code = WinFormsDebugTools.g_form_code_trace.g_analyse_code[w_line];
                    if (w_code.type != Form_Code_Trace.TRACECODE.TYPE.OPC) continue;

                    string w_comment = (w_code.comment1 ?? "").Trim();
                    if (w_comment.Length == 0) continue;

                    dataGridView_comment1.Rows.Add(w_comment, w_code.address.ToString("X6"), w_line);
                }
            }
            finally
            {
                dataGridView_comment1.ResumeLayout();
            }
        }

        private bool TryGetSelectedCommentSearchLine(out int out_line)
        {
            out_line = -1;
            DataGridViewRow? w_row = dataGridView_comment1.CurrentRow;
            if ((w_row == null) || (w_row.IsNewRow == true)) return false;

            object? w_line_value = w_row.Cells[commentLineColumn.Name].Value;
            if (w_line_value is int w_line)
            {
                if (IsCodeLineInRange(w_line) == true)
                {
                    out_line = w_line;
                    return true;
                }
            }

            if (int.TryParse(w_line_value?.ToString(), out w_line) == true)
            {
                if (IsCodeLineInRange(w_line) == true)
                {
                    out_line = w_line;
                    return true;
                }
            }

            object? w_address_value = w_row.Cells[commentAddressColumn.Name].Value;
            if (TryParseHexAddress(w_address_value?.ToString(), out uint w_addr) == true)
            {
                out_line = WinFormsDebugTools.g_form_code_trace.get_code_from_addr(w_addr);
                return out_line >= 0;
            }

            return false;
        }
        private void dataGridView_memory_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            RefreshMemoryMonitorEntries();
        }

        private void dataGridView_memory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            SelectMemoryMonitorRow(e.RowIndex);
        }

        private void dataGridView_memory_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;
            if (dataGridView_memory.IsCurrentCellInEditMode == true) return;

            if (DeleteSelectedMemoryMonitorRow() == false) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void button_memory_history_clear_Click(object sender, EventArgs e)
        {
            ClearSelectedMemoryAccessHistory();
        }

        private void dataGridView_memory_history_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow w_row = dataGridView_memory_history.Rows[e.RowIndex];
            if (w_row.IsNewRow == true) return;

            string? w_pc_text = w_row.Cells[historyPcColumn.Name].Value?.ToString();
            if (TryParseHexAddress(w_pc_text, out uint w_pc) == false) return;

            RefreshComment1List(true);
            int w_line = WinFormsDebugTools.g_form_code_trace.get_code_from_addr(w_pc);
            if (w_line < 0) return;

            CenterCodeViewOnLine(w_line);
        }

        private void button_ram_change_start_Click(object sender, EventArgs e)
        {
            g_ram_change_byte_count = GetRamChangeByteCountFromCombo();
            g_ram_change_start_snapshot = CaptureRamChangeSnapshot();
            dataGridView_ram_change.Rows.Clear();
            ApplyRamChangeSearchFilter();
        }

        private void button_ram_change_stop_Click(object sender, EventArgs e)
        {
            if (g_ram_change_start_snapshot.Length != RAM_CHANGE_SIZE) return;

            g_ram_change_byte_count = GetRamChangeByteCountFromCombo();
            byte[] w_stop_snapshot = CaptureRamChangeSnapshot();
            RefreshRamChangeList(g_ram_change_start_snapshot, w_stop_snapshot, g_ram_change_byte_count);
        }

        private void dataGridView_ram_change_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow w_row = dataGridView_ram_change.Rows[e.RowIndex];
            if (w_row.IsNewRow == true) return;

            if (TryParseHexAddress(w_row.Cells[ramChangeAddressColumn.Name].Value?.ToString(), out uint w_address) == false) return;
            if (int.TryParse(w_row.Cells[ramChangeBytesColumn.Name].Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int w_byte_count) == false)
            {
                w_byte_count = 1;
            }
            AddMemoryMonitorAddress(w_address, w_byte_count);
        }

        private void memory_monitor_timer_Tick(object? sender, EventArgs e)
        {
            RefreshMemoryMonitorCurrentValues();
        }

        public void memory_monitor_check(uint in_addr, uint in_val, bool in_write_enable, int in_access_byte_count)
        {
            if (g_memory_monitor_active == false) return;

            in_addr &= 0x00ffffff;
            in_access_byte_count = NormalizeMemoryMonitorByteCount(in_access_byte_count);
            bool w_refresh_history = false;
            lock (g_memory_monitor_lock)
            {
                int w_row = g_memory_monitor.Count;
                for (int i = 0; i < w_row; i++)
                {
                    MEMORY_MONITOR_ENTRY w_monitor = g_memory_monitor[i];
                    if (IsMemoryAccessOverlapped(w_monitor.address, w_monitor.byte_count, in_addr, in_access_byte_count) == false) continue;

                    uint w_before = TryReadMemoryMonitorValue(w_monitor.address, w_monitor.byte_count, out uint w_current_value)
                        ? w_current_value
                        : w_monitor.value;
                    uint w_after = in_write_enable
                        ? ApplyMemoryAccessValue(w_before, w_monitor.address, w_monitor.byte_count, in_addr, in_val, in_access_byte_count)
                        : w_before;

                    w_monitor.value = w_after;
                    w_monitor.history.Add(new MEMORY_ACCESS_HISTORY
                    {
                        write_enable = in_write_enable,
                        before_value = w_before,
                        after_value = w_after,
                        pc = md_main.g_md_m68k.g_reg_PC & 0x00ffffff
                    });
                    UpdateMemoryMonitorValue(w_monitor.row_index, w_after);

                    if (w_monitor.row_index == g_selected_memory_monitor_row)
                    {
                        w_refresh_history = true;
                    }
                }
            }

            if (w_refresh_history == true)
            {
                QueueMemoryAccessHistoryRefresh();
            }
        }

        //----------------------------------------------------------------
        //sub func
        //----------------------------------------------------------------
        private void RefreshMemoryMonitorEntries()
        {
            Dictionary<string, List<MEMORY_ACCESS_HISTORY>> w_history = new Dictionary<string, List<MEMORY_ACCESS_HISTORY>>();
            lock (g_memory_monitor_lock)
            {
                foreach (MEMORY_MONITOR_ENTRY w_monitor in g_memory_monitor)
                {
                    w_history[GetMemoryMonitorKey(w_monitor.address, w_monitor.byte_count)] = new List<MEMORY_ACCESS_HISTORY>(w_monitor.history);
                }
            }

            List<MEMORY_MONITOR_ENTRY> w_memory_monitor = new List<MEMORY_MONITOR_ENTRY>();
            for (int i = 0; i < dataGridView_memory.Rows.Count; i++)
            {
                DataGridViewRow w_row = dataGridView_memory.Rows[i];
                if (w_row.IsNewRow == true) continue;

                string w_address_text = w_row.Cells[address.Name].Value?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(w_address_text) == true)
                {
                    ClearMemoryMonitorValueCells(w_row);
                    continue;
                }

                if (TryParseHexAddress(w_address_text, out uint w_addr) == false)
                {
                    ClearMemoryMonitorValueCells(w_row);
                    continue;
                }

                int w_byte_count = GetMemoryMonitorByteCountFromRow(w_row);
                uint w_value = TryReadMemoryMonitorValue(w_addr, w_byte_count, out uint w_current_value) ? w_current_value : 0;
                MEMORY_MONITOR_ENTRY w_entry = new MEMORY_MONITOR_ENTRY
                {
                    address = w_addr & 0x00ffffff,
                    byte_count = w_byte_count,
                    value = w_value,
                    row_index = i
                };

                if (w_history.TryGetValue(GetMemoryMonitorKey(w_entry.address, w_entry.byte_count), out List<MEMORY_ACCESS_HISTORY>? w_saved_history) == true)
                {
                    w_entry.history.AddRange(w_saved_history);
                }

                w_row.Cells[address.Name].Value = w_entry.address.ToString("X6", CultureInfo.InvariantCulture);
                w_row.Cells[monitorBytes.Name].Value = w_entry.byte_count.ToString(CultureInfo.InvariantCulture);
                UpdateMemoryMonitorValueCells(w_row, w_entry.value, w_entry.byte_count);
                w_memory_monitor.Add(w_entry);
            }

            lock (g_memory_monitor_lock)
            {
                g_memory_monitor.Clear();
                g_memory_monitor.AddRange(w_memory_monitor);
                g_memory_monitor_active = g_memory_monitor.Count != 0;
            }
            ClearQueuedMemoryMonitorValueUpdates();
            SelectMemoryMonitorRow(dataGridView_memory.CurrentRow?.Index ?? -1);
            SaveCurrentGameMemoryMonitorSettings();
        }

        private void AddMemoryMonitorAddress(uint in_address, int in_byte_count)
        {
            uint w_address = in_address & 0x00ffffff;
            int w_byte_count = NormalizeMemoryMonitorByteCount(in_byte_count);

            for (int i = 0; i < dataGridView_memory.Rows.Count; i++)
            {
                DataGridViewRow w_row = dataGridView_memory.Rows[i];
                if (w_row.IsNewRow == true) continue;
                if (TryParseHexAddress(w_row.Cells[address.Name].Value?.ToString(), out uint w_row_address) == false) continue;
                if (((w_row_address & 0x00ffffff) == w_address) && (GetMemoryMonitorByteCountFromRow(w_row) == w_byte_count))
                {
                    SelectMemoryMonitorGridRow(i);
                    return;
                }
            }

            int w_row_index = dataGridView_memory.Rows.Add(
                w_address.ToString("X6", CultureInfo.InvariantCulture),
                w_byte_count.ToString(CultureInfo.InvariantCulture),
                "",
                "");
            RefreshMemoryMonitorEntries();
            SelectMemoryMonitorGridRow(w_row_index);
        }

        private bool DeleteSelectedMemoryMonitorRow()
        {
            int w_row_index = GetSelectedMemoryMonitorRowIndex();
            if ((w_row_index < 0) || (dataGridView_memory.Rows.Count <= w_row_index)) return false;
            if (dataGridView_memory.Rows[w_row_index].IsNewRow == true) return false;

            dataGridView_memory.Rows.RemoveAt(w_row_index);
            RefreshMemoryMonitorEntries();

            int w_next_row = GetNextMemoryMonitorRowIndex(w_row_index);
            if (w_next_row >= 0)
            {
                SelectMemoryMonitorGridRow(w_next_row);
            }
            else
            {
                dataGridView_memory.ClearSelection();
                dataGridView_memory.CurrentCell = null;
                SelectMemoryMonitorRow(-1);
            }
            return true;
        }

        private int GetSelectedMemoryMonitorRowIndex()
        {
            if (dataGridView_memory.SelectedRows.Count > 0)
            {
                return dataGridView_memory.SelectedRows[0].Index;
            }
            if (dataGridView_memory.CurrentCell != null)
            {
                return dataGridView_memory.CurrentCell.RowIndex;
            }
            return dataGridView_memory.CurrentRow?.Index ?? -1;
        }

        private int GetNextMemoryMonitorRowIndex(int in_deleted_row)
        {
            int w_row_index = Math.Min(in_deleted_row, dataGridView_memory.Rows.Count - 1);
            while (0 <= w_row_index)
            {
                if (dataGridView_memory.Rows[w_row_index].IsNewRow == false) return w_row_index;
                w_row_index--;
            }
            return -1;
        }

        private void UpdateMemoryMonitorValue(int in_row, uint in_val)
        {
            if ((dataGridView_memory.IsDisposed == true) || (dataGridView_memory.IsHandleCreated == false)) return;

            bool w_queue_update = false;
            lock (g_memory_monitor_update_lock)
            {
                g_memory_monitor_pending_values[in_row] = in_val;
                if (g_memory_monitor_update_queued == false)
                {
                    g_memory_monitor_update_queued = true;
                    w_queue_update = true;
                }
            }

            if (w_queue_update == false) return;

            if (dataGridView_memory.InvokeRequired == false)
            {
                FlushMemoryMonitorValueUpdates();
                return;
            }

            try
            {
                dataGridView_memory.BeginInvoke(new Action(FlushMemoryMonitorValueUpdates));
            }
            catch (ObjectDisposedException)
            {
                ClearQueuedMemoryMonitorValueUpdates();
            }
            catch (InvalidOperationException)
            {
                ClearQueuedMemoryMonitorValueUpdates();
            }
        }

        private void FlushMemoryMonitorValueUpdates()
        {
            if ((dataGridView_memory.IsDisposed == true) || (dataGridView_memory.IsHandleCreated == false))
            {
                ClearQueuedMemoryMonitorValueUpdates();
                return;
            }

            List<KeyValuePair<int, uint>> w_updates;
            lock (g_memory_monitor_update_lock)
            {
                w_updates = new List<KeyValuePair<int, uint>>(g_memory_monitor_pending_values);
                g_memory_monitor_pending_values.Clear();
                g_memory_monitor_update_queued = false;
            }

            foreach (KeyValuePair<int, uint> w_update in w_updates)
            {
                if ((0 <= w_update.Key) && (w_update.Key < dataGridView_memory.Rows.Count))
                {
                    int w_byte_count = 1;
                    lock (g_memory_monitor_lock)
                    {
                        MEMORY_MONITOR_ENTRY? w_entry = FindMemoryMonitorEntryByRow(w_update.Key);
                        if (w_entry != null)
                        {
                            w_byte_count = w_entry.byte_count;
                        }
                    }
                    UpdateMemoryMonitorValueCells(dataGridView_memory.Rows[w_update.Key], w_update.Value, w_byte_count);
                }
            }
        }

        private void ClearQueuedMemoryMonitorValueUpdates()
        {
            lock (g_memory_monitor_update_lock)
            {
                g_memory_monitor_update_queued = false;
                g_memory_monitor_pending_values.Clear();
            }
        }

        private void RefreshMemoryMonitorCurrentValues()
        {
            if (g_memory_monitor_active == false) return;

            lock (g_memory_monitor_lock)
            {
                foreach (MEMORY_MONITOR_ENTRY w_monitor in g_memory_monitor)
                {
                    if (TryReadMemoryMonitorValue(w_monitor.address, w_monitor.byte_count, out uint w_value) == false) continue;
                    if (w_monitor.value == w_value) continue;

                    w_monitor.value = w_value;
                    UpdateMemoryMonitorValue(w_monitor.row_index, w_value);
                }
            }
        }

        private void SelectMemoryMonitorRow(int in_row)
        {
            g_selected_memory_monitor_row = in_row;
            RefreshMemoryAccessHistoryList();
        }

        private void SelectMemoryMonitorGridRow(int in_row)
        {
            if ((in_row < 0) || (dataGridView_memory.Rows.Count <= in_row)) return;
            DataGridViewRow w_row = dataGridView_memory.Rows[in_row];
            if (w_row.IsNewRow == true) return;

            dataGridView_memory.ClearSelection();
            w_row.Selected = true;
            dataGridView_memory.CurrentCell = w_row.Cells[address.Name];
            SelectMemoryMonitorRow(in_row);
        }

        private void CenterCodeViewOnLine(int in_line)
        {
            if (IsCodeLineInRange(in_line) == false) return;

            int w_line = NormalizeCodeLine(in_line);
            g_cursole_line = w_line;
            picturebox_scroll(w_line, -(pictureBox_Code_line_num() >> 1));
            if (g_code_tool_windows.TryGetValue(CODE_TOOL_KIND.Code, out CodeDockContent? w_code_window) == true)
            {
                if (w_code_window.DockPanel != null)
                {
                    w_code_window.Activate();
                }
            }
            pictureBox_code.Focus();
            pictureBox_code.Invalidate();
        }

        private void ClearSelectedMemoryAccessHistory()
        {
            lock (g_memory_monitor_lock)
            {
                MEMORY_MONITOR_ENTRY? w_entry = FindMemoryMonitorEntryByRow(g_selected_memory_monitor_row);
                if (w_entry != null)
                {
                    w_entry.history.Clear();
                }
            }
            RefreshMemoryAccessHistoryList();
        }

        private void RefreshMemoryAccessHistoryList()
        {
            if ((dataGridView_memory_history.IsDisposed == true) || (dataGridView_memory_history.IsHandleCreated == false)) return;
            if (dataGridView_memory_history.InvokeRequired == true)
            {
                try
                {
                    dataGridView_memory_history.BeginInvoke(new Action(RefreshMemoryAccessHistoryList));
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                return;
            }

            List<MEMORY_ACCESS_HISTORY> w_history = new List<MEMORY_ACCESS_HISTORY>();
            int w_byte_count = 1;
            lock (g_memory_monitor_lock)
            {
                MEMORY_MONITOR_ENTRY? w_entry = FindMemoryMonitorEntryByRow(g_selected_memory_monitor_row);
                if (w_entry != null)
                {
                    w_byte_count = w_entry.byte_count;
                    w_history.AddRange(w_entry.history);
                }
            }

            dataGridView_memory_history.SuspendLayout();
            try
            {
                dataGridView_memory_history.Rows.Clear();
                foreach (MEMORY_ACCESS_HISTORY w_access in w_history)
                {
                    dataGridView_memory_history.Rows.Add(
                        w_access.write_enable ? "変更" : "参照",
                        FormatMemoryMonitorHexValue(w_access.before_value, w_byte_count),
                        w_access.before_value.ToString(CultureInfo.InvariantCulture),
                        FormatMemoryMonitorHexValue(w_access.after_value, w_byte_count),
                        w_access.after_value.ToString(CultureInfo.InvariantCulture),
                        w_access.pc.ToString("X6", CultureInfo.InvariantCulture));
                }
            }
            finally
            {
                dataGridView_memory_history.ResumeLayout();
            }
        }

        private void QueueMemoryAccessHistoryRefresh()
        {
            if ((dataGridView_memory_history.IsDisposed == true) || (dataGridView_memory_history.IsHandleCreated == false)) return;
            lock (g_memory_monitor_update_lock)
            {
                if (g_memory_history_refresh_queued == true) return;
                g_memory_history_refresh_queued = true;
            }

            try
            {
                dataGridView_memory_history.BeginInvoke(new Action(FlushMemoryAccessHistoryRefresh));
            }
            catch (ObjectDisposedException)
            {
                ClearQueuedMemoryAccessHistoryRefresh();
            }
            catch (InvalidOperationException)
            {
                ClearQueuedMemoryAccessHistoryRefresh();
            }
        }

        private void FlushMemoryAccessHistoryRefresh()
        {
            ClearQueuedMemoryAccessHistoryRefresh();
            RefreshMemoryAccessHistoryList();
        }

        private void ClearQueuedMemoryAccessHistoryRefresh()
        {
            lock (g_memory_monitor_update_lock)
            {
                g_memory_history_refresh_queued = false;
            }
        }

        private MEMORY_MONITOR_ENTRY? FindMemoryMonitorEntryByRow(int in_row)
        {
            foreach (MEMORY_MONITOR_ENTRY w_monitor in g_memory_monitor)
            {
                if (w_monitor.row_index == in_row) return w_monitor;
            }
            return null;
        }

        private int GetMemoryMonitorByteCountFromRow(DataGridViewRow in_row)
        {
            object? w_value = in_row.Cells[monitorBytes.Name].Value;
            if (int.TryParse(w_value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int w_byte_count) == false)
            {
                w_byte_count = 1;
            }

            w_byte_count = NormalizeMemoryMonitorByteCount(w_byte_count);
            in_row.Cells[monitorBytes.Name].Value = w_byte_count.ToString(CultureInfo.InvariantCulture);
            return w_byte_count;
        }

        private int NormalizeMemoryMonitorByteCount(int in_byte_count)
        {
            return in_byte_count switch
            {
                2 => 2,
                4 => 4,
                _ => 1
            };
        }

        private bool TryReadMemoryMonitorValue(uint in_address, int in_byte_count, out uint out_value)
        {
            out_value = 0;
            try
            {
                in_address &= 0x00ffffff;
                in_byte_count = NormalizeMemoryMonitorByteCount(in_byte_count);
                for (int i = 0; i < in_byte_count; i++)
                {
                    out_value = (out_value << 8) | md_main.g_md_m68k.read8(in_address + (uint)i);
                }
                return true;
            }
            catch
            {
                out_value = 0;
                return false;
            }
        }

        private bool IsMemoryAccessOverlapped(uint in_monitor_address, int in_monitor_byte_count, uint in_access_address, int in_access_byte_count)
        {
            uint w_monitor_start = in_monitor_address & 0x00ffffff;
            uint w_monitor_end = w_monitor_start + (uint)NormalizeMemoryMonitorByteCount(in_monitor_byte_count);
            uint w_access_start = in_access_address & 0x00ffffff;
            uint w_access_end = w_access_start + (uint)NormalizeMemoryMonitorByteCount(in_access_byte_count);
            return (w_access_start < w_monitor_end) && (w_monitor_start < w_access_end);
        }

        private uint ApplyMemoryAccessValue(uint in_monitor_value, uint in_monitor_address, int in_monitor_byte_count, uint in_access_address, uint in_access_value, int in_access_byte_count)
        {
            uint w_value = in_monitor_value;
            in_monitor_address &= 0x00ffffff;
            in_access_address &= 0x00ffffff;
            in_monitor_byte_count = NormalizeMemoryMonitorByteCount(in_monitor_byte_count);
            in_access_byte_count = NormalizeMemoryMonitorByteCount(in_access_byte_count);

            for (int i = 0; i < in_access_byte_count; i++)
            {
                uint w_address = in_access_address + (uint)i;
                if ((w_address < in_monitor_address) || (in_monitor_address + (uint)in_monitor_byte_count <= w_address)) continue;

                int w_access_shift = (in_access_byte_count - 1 - i) * 8;
                uint w_access_byte = (in_access_value >> w_access_shift) & 0xff;
                int w_monitor_index = (int)(w_address - in_monitor_address);
                int w_monitor_shift = (in_monitor_byte_count - 1 - w_monitor_index) * 8;
                w_value &= ~(0xffu << w_monitor_shift);
                w_value |= w_access_byte << w_monitor_shift;
            }
            return w_value;
        }

        private void UpdateMemoryMonitorValueCells(DataGridViewRow in_row, uint in_value, int in_byte_count)
        {
            in_row.Cells[valHex.Name].Value = FormatMemoryMonitorHexValue(in_value, in_byte_count);
            in_row.Cells[valDec.Name].Value = in_value.ToString(CultureInfo.InvariantCulture);
        }

        private void ClearMemoryMonitorValueCells(DataGridViewRow in_row)
        {
            in_row.Cells[valHex.Name].Value = "";
            in_row.Cells[valDec.Name].Value = "";
        }

        private int GetRamChangeByteCountFromCombo()
        {
            if (int.TryParse(comboBox_ram_change_bytes.SelectedItem?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int w_byte_count) == false)
            {
                w_byte_count = 1;
            }
            w_byte_count = NormalizeMemoryMonitorByteCount(w_byte_count);
            comboBox_ram_change_bytes.SelectedItem = w_byte_count.ToString(CultureInfo.InvariantCulture);
            return w_byte_count;
        }

        private byte[] CaptureRamChangeSnapshot()
        {
            byte[] w_snapshot = new byte[RAM_CHANGE_SIZE];
            byte[] w_memory = md_main.g_md_m68k.g_memory;
            if ((w_memory != null) && (RAM_CHANGE_BASE_OFFSET + RAM_CHANGE_SIZE <= w_memory.Length))
            {
                Array.Copy(w_memory, RAM_CHANGE_BASE_OFFSET, w_snapshot, 0, RAM_CHANGE_SIZE);
            }
            return w_snapshot;
        }

        private void RefreshRamChangeList(byte[] in_start_snapshot, byte[] in_stop_snapshot, int in_byte_count)
        {
            int w_byte_count = NormalizeMemoryMonitorByteCount(in_byte_count);
            dataGridView_ram_change.SuspendLayout();
            try
            {
                dataGridView_ram_change.Rows.Clear();
                for (int w_offset = 0; w_offset <= RAM_CHANGE_SIZE - w_byte_count; w_offset += w_byte_count)
                {
                    uint w_start_value = GetRamChangeValue(in_start_snapshot, w_offset, w_byte_count);
                    uint w_stop_value = GetRamChangeValue(in_stop_snapshot, w_offset, w_byte_count);
                    if (w_start_value == w_stop_value) continue;

                    dataGridView_ram_change.Rows.Add(
                        (RAM_CHANGE_BASE_ADDRESS + (uint)w_offset).ToString("X6", CultureInfo.InvariantCulture),
                        FormatMemoryMonitorHexValue(w_start_value, w_byte_count),
                        w_start_value.ToString(CultureInfo.InvariantCulture),
                        FormatMemoryMonitorHexValue(w_stop_value, w_byte_count),
                        w_stop_value.ToString(CultureInfo.InvariantCulture),
                        w_byte_count.ToString(CultureInfo.InvariantCulture));
                }
                ApplyRamChangeSearchFilter();
            }
            finally
            {
                dataGridView_ram_change.ResumeLayout();
            }
        }

        private uint GetRamChangeValue(byte[] in_snapshot, int in_offset, int in_byte_count)
        {
            uint w_value = 0;
            for (int i = 0; i < in_byte_count; i++)
            {
                w_value = (w_value << 8) | in_snapshot[in_offset + i];
            }
            return w_value;
        }

        private string FormatMemoryMonitorHexValue(uint in_value, int in_byte_count)
        {
            return NormalizeMemoryMonitorByteCount(in_byte_count) switch
            {
                2 => in_value.ToString("X4", CultureInfo.InvariantCulture),
                4 => in_value.ToString("X8", CultureInfo.InvariantCulture),
                _ => in_value.ToString("X2", CultureInfo.InvariantCulture)
            };
        }

        private string GetMemoryMonitorKey(uint in_address, int in_byte_count)
        {
            return (in_address & 0x00ffffff).ToString("X6", CultureInfo.InvariantCulture) + ":" + NormalizeMemoryMonitorByteCount(in_byte_count).ToString(CultureInfo.InvariantCulture);
        }

        private bool TryParseHexAddress(string? in_text, out uint out_addr)
        {
            out_addr = 0;
            if (string.IsNullOrWhiteSpace(in_text) == true) return false;
            string w_text = in_text.Trim();
            if (w_text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) == true)
            {
                w_text = w_text.Substring(2);
            }
            return uint.TryParse(w_text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out out_addr);
        }

    }
}

