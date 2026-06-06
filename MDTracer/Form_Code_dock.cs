using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using WeifenLuo.WinFormsUI.Docking;
using WeifenLuo.WinFormsUI.ThemeVS2015;

namespace MDTracer
{
    public partial class Form_Code
    {
        private static readonly Color CODE_DOCK_PANEL_BACK_COLOR = Color.FromArgb(221, 235, 247);
        private static readonly Color CODE_DOCK_WINDOW_BACK_COLOR = Color.FromArgb(246, 250, 255);
        private static readonly Color CODE_DOCK_COMMAND_BACK_COLOR = Color.FromArgb(230, 241, 252);
        private static readonly Color CODE_DOCK_MENU_BACK_COLOR = Color.FromArgb(216, 232, 248);
        private static readonly Color CODE_DOCK_MENU_TEXT_COLOR = Color.FromArgb(30, 55, 80);
        private static readonly Color CODE_DOCK_GRID_HEADER_BACK_COLOR = Color.FromArgb(210, 228, 247);
        private static readonly Color CODE_DOCK_GRID_LINE_COLOR = Color.FromArgb(190, 212, 236);
        private static readonly Color CODE_DOCK_GRID_TEXT_COLOR = Color.FromArgb(25, 45, 65);
        private static readonly Color CODE_DOCK_GRID_ALT_BACK_COLOR = Color.FromArgb(239, 247, 255);
        private static readonly Color CODE_DOCK_GRID_SELECT_BACK_COLOR = Color.FromArgb(76, 143, 202);
        private static readonly Color CODE_DOCK_BUTTON_BORDER_COLOR = Color.FromArgb(148, 180, 213);
        private const double CODE_DOCK_DEFAULT_RIGHT_PORTION = 0.40;
        private const double CODE_DOCK_DEFAULT_AFTER_ADDRESS_PORTION = 0.90;
        private const double CODE_DOCK_DEFAULT_AFTER_COMMENT_PORTION = 2.0 / 3.0;
        private const double CODE_DOCK_DEFAULT_MEMORY_PORTION = 0.50;

        private enum CODE_TOOL_KIND
        {
            Code,
            Address,
            Comment,
            MemoryMonitoring,
            AccessHistory,
            RamChanges
        }

        private sealed class CodeDockContent : DockContent
        {
            public CODE_TOOL_KIND Kind { get; }

            public CodeDockContent(CODE_TOOL_KIND in_kind, string in_title)
            {
                Kind = in_kind;
                Text = in_title;
                TabText = in_title;
                BackColor = CODE_DOCK_WINDOW_BACK_COLOR;
                DockAreas = DockAreas.Document
                    | DockAreas.Float
                    | DockAreas.DockLeft
                    | DockAreas.DockRight
                    | DockAreas.DockTop
                    | DockAreas.DockBottom;
                HideOnClose = true;
                CloseButton = false;
                CloseButtonVisible = false;
                Padding = new Padding(0);
            }

            protected override string GetPersistString()
            {
                return Kind.ToString();
            }
        }

        private sealed class CodeToolSavedPane
        {
            public int Id { get; set; }
            public DockState DockState { get; set; }
            public List<int> ContentIds { get; } = new List<int>();
        }

        private sealed class CodeToolSavedNestedPane
        {
            public int RefId { get; set; }
            public int PrevPane { get; set; } = -1;
            public DockAlignment Alignment { get; set; } = DockAlignment.Right;
            public double Proportion { get; set; } = 0.5;
        }

        private sealed class CodeToolSavedDockWindow
        {
            public DockState DockState { get; set; }
            public List<CodeToolSavedNestedPane> NestedPanes { get; } = new List<CodeToolSavedNestedPane>();
        }

        private sealed class CodeToolSavedLayout
        {
            public Dictionary<int, CODE_TOOL_KIND> ContentKinds { get; } = new Dictionary<int, CODE_TOOL_KIND>();
            public HashSet<CODE_TOOL_KIND> VisibleKinds { get; } = new HashSet<CODE_TOOL_KIND>();
            public Dictionary<int, CodeToolSavedPane> Panes { get; } = new Dictionary<int, CodeToolSavedPane>();
            public List<CodeToolSavedDockWindow> DockWindows { get; } = new List<CodeToolSavedDockWindow>();
        }

        private readonly DockPanel g_code_dock_panel = new DockPanel();
        private readonly Dictionary<CODE_TOOL_KIND, CodeDockContent> g_code_tool_windows = new Dictionary<CODE_TOOL_KIND, CodeDockContent>();
        private readonly Dictionary<CODE_TOOL_KIND, ToolStripMenuItem> g_code_tool_menu_items = new Dictionary<CODE_TOOL_KIND, ToolStripMenuItem>();
        private bool g_code_tool_layout_updating;
        private bool g_code_tool_layout_restored;
        private bool g_code_tool_windows_ready;
        private ToolStripMenuItem? g_viewToolStripMenuItem;
        private readonly System.Windows.Forms.Timer g_code_tool_layout_save_timer = new System.Windows.Forms.Timer();
        private bool g_code_tool_layout_save_pending;
        private string g_pending_code_tool_layout_text = "";
        private bool g_default_code_tool_layout_pending;
        private readonly Label g_ram_change_search_label = new Label();
        private readonly TextBox g_ram_change_search_textbox = new TextBox();
        private readonly Button g_ram_change_search_clear_button = new Button();

        private void InitializeDockPanelSuite()
        {
            g_code_tool_layout_updating = true;
            splitContainer1.Panel2Collapsed = true;
            splitContainer1.Panel1.BackColor = CODE_DOCK_PANEL_BACK_COLOR;

            g_code_tool_layout_save_timer.Interval = 500;
            g_code_tool_layout_save_timer.Tick += codeToolLayoutSaveTimer_Tick;

            CreateViewMenu();
            HideLegacyToolLabels();

            g_code_dock_panel.Dock = DockStyle.Fill;
            g_code_dock_panel.BackColor = CODE_DOCK_PANEL_BACK_COLOR;
            g_code_dock_panel.DocumentStyle = DocumentStyle.DockingWindow;
            g_code_dock_panel.Theme = new VS2015BlueTheme();
            splitContainer1.Panel1.Controls.Add(g_code_dock_panel);
            g_code_dock_panel.BringToFront();

            AddCodeToolWindow(CODE_TOOL_KIND.Code, "Code", ConfigureCodeToolWindow);
            AddCodeToolWindow(CODE_TOOL_KIND.Address, "Address", ConfigureAddressToolWindow);
            AddCodeToolWindow(CODE_TOOL_KIND.Comment, "Comment1 Result", ConfigureCommentToolWindow);
            AddCodeToolWindow(CODE_TOOL_KIND.MemoryMonitoring, "Memory Monitoring", ConfigureMemoryToolWindow);
            AddCodeToolWindow(CODE_TOOL_KIND.RamChanges, "RAM Changes", ConfigureRamChangesToolWindow);
            AddCodeToolWindow(CODE_TOOL_KIND.AccessHistory, "Access History", ConfigureAccessHistoryToolWindow);

            ApplyToolWindowTheme();
            g_code_tool_layout_updating = false;
            g_code_tool_windows_ready = true;
            UpdateCodeToolMenuChecks();
        }

        private void CreateViewMenu()
        {
            g_viewToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "viewToolStripMenuItem",
                Text = "View"
            };

            AddCodeToolMenuItem(CODE_TOOL_KIND.Code, "Code");
            AddCodeToolMenuItem(CODE_TOOL_KIND.Address, "Address");
            AddCodeToolMenuItem(CODE_TOOL_KIND.Comment, "Comment");
            AddCodeToolMenuItem(CODE_TOOL_KIND.MemoryMonitoring, "Memory monitoring");
            AddCodeToolMenuItem(CODE_TOOL_KIND.AccessHistory, "Access history");
            AddCodeToolMenuItem(CODE_TOOL_KIND.RamChanges, "RAM changes");
            menuStrip1.Items.Add(g_viewToolStripMenuItem);
        }

        private void AddCodeToolMenuItem(CODE_TOOL_KIND in_kind, string in_text)
        {
            if (g_viewToolStripMenuItem == null) return;
            ToolStripMenuItem w_item = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Checked = true,
                Text = in_text,
                Tag = in_kind
            };
            w_item.CheckedChanged += codeToolStripMenuItem_CheckedChanged;
            g_code_tool_menu_items[in_kind] = w_item;
            g_viewToolStripMenuItem.DropDownItems.Add(w_item);
        }

        private void AddCodeToolWindow(CODE_TOOL_KIND in_kind, string in_title, Action<CodeDockContent> in_configure)
        {
            CodeDockContent w_window = new CodeDockContent(in_kind, in_title);
            w_window.DockStateChanged += CodeToolContent_LayoutChanged;
            w_window.LocationChanged += CodeToolContent_LayoutChanged;
            w_window.SizeChanged += CodeToolContent_LayoutChanged;
            w_window.VisibleChanged += CodeToolContent_VisibleChanged;

            in_configure(w_window);
            g_code_tool_windows[in_kind] = w_window;
        }

        private void ShowDefaultCodeToolLayout()
        {
            g_code_dock_panel.DockRightPortion = CODE_DOCK_DEFAULT_RIGHT_PORTION;
            if (g_code_tool_windows.TryGetValue(CODE_TOOL_KIND.Code, out CodeDockContent? w_code) == true)
            {
                w_code.Show(g_code_dock_panel, DockState.Document);
            }

            if (g_code_tool_windows.TryGetValue(CODE_TOOL_KIND.Address, out CodeDockContent? w_address) == true)
            {
                w_address.Show(g_code_dock_panel, DockState.DockRight);
            }

            ShowCodeToolBelow(CODE_TOOL_KIND.Comment, w_address, CODE_DOCK_DEFAULT_AFTER_ADDRESS_PORTION);
            if (g_code_tool_windows.TryGetValue(CODE_TOOL_KIND.Comment, out CodeDockContent? w_comment) == true)
            {
                ShowCodeToolBelow(CODE_TOOL_KIND.AccessHistory, w_comment, CODE_DOCK_DEFAULT_AFTER_COMMENT_PORTION);
            }
            if (g_code_tool_windows.TryGetValue(CODE_TOOL_KIND.AccessHistory, out CodeDockContent? w_access) == true)
            {
                ShowCodeToolAsTab(CODE_TOOL_KIND.RamChanges, w_access);
            }
            if (g_code_tool_windows.TryGetValue(CODE_TOOL_KIND.AccessHistory, out w_access) == true)
            {
                ShowCodeToolBelow(CODE_TOOL_KIND.MemoryMonitoring, w_access, CODE_DOCK_DEFAULT_MEMORY_PORTION);
            }
        }

        private void ShowCodeToolBelow(CODE_TOOL_KIND in_kind, CodeDockContent? in_previous, double in_proportion)
        {
            if (g_code_tool_windows.TryGetValue(in_kind, out CodeDockContent? w_window) == false) return;
            if (in_previous?.DockHandler.Pane != null)
            {
                w_window.Show(in_previous.DockHandler.Pane, DockAlignment.Bottom, in_proportion);
                return;
            }
            w_window.Show(g_code_dock_panel, GetDefaultCodeToolDockState(in_kind));
        }

        private void ShowCodeToolAsTab(CODE_TOOL_KIND in_kind, CodeDockContent? in_previous)
        {
            if (g_code_tool_windows.TryGetValue(in_kind, out CodeDockContent? w_window) == false) return;
            if (in_previous?.DockHandler.Pane != null)
            {
                w_window.Show(in_previous.DockHandler.Pane, in_previous);
                return;
            }
            w_window.Show(g_code_dock_panel, GetDefaultCodeToolDockState(in_kind));
        }

        private DockState GetDefaultCodeToolDockState(CODE_TOOL_KIND in_kind)
        {
            return in_kind switch
            {
                CODE_TOOL_KIND.Code => DockState.Document,
                _ => DockState.DockRight
            };
        }

        private void ConfigureCodeToolWindow(CodeDockContent in_window)
        {
            MoveControlTo(pictureBox_code, in_window);
            MoveControlTo(hScrollBar_code, in_window);
            MoveControlTo(vScrollBar_code, in_window);
            pictureBox_code.Dock = DockStyle.Fill;
            hScrollBar_code.Dock = DockStyle.Bottom;
            vScrollBar_code.Dock = DockStyle.Right;
            in_window.Resize += (_, _) =>
            {
                scrollbar_set();
                pictureBox_code.Invalidate();
            };
        }

        private void ConfigureAddressToolWindow(CodeDockContent in_window)
        {
            MoveControlTo(label2, in_window);
            MoveControlTo(textBoxAddr, in_window);
            label2.Text = "address";
            label2.ForeColor = CODE_DOCK_GRID_TEXT_COLOR;
            label2.Location = new Point(8, 10);
            label2.AutoSize = true;
            textBoxAddr.Location = new Point(72, 7);
            textBoxAddr.Width = 90;
            textBoxAddr.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxAddr.BackColor = Color.White;
            textBoxAddr.ForeColor = CODE_DOCK_GRID_TEXT_COLOR;
            textBoxAddr.BorderStyle = BorderStyle.FixedSingle;
        }

        private void ConfigureCommentToolWindow(CodeDockContent in_window)
        {
            MoveControlTo(dataGridView_comment1, in_window);
            dataGridView_comment1.Dock = DockStyle.Fill;
        }

        private void ConfigureMemoryToolWindow(CodeDockContent in_window)
        {
            MoveControlTo(dataGridView_memory, in_window);
            dataGridView_memory.Dock = DockStyle.Fill;
            dataGridView_memory.AllowUserToDeleteRows = false;
            dataGridView_memory.KeyDown += dataGridView_memory_KeyDown;
        }

        private void ConfigureAccessHistoryToolWindow(CodeDockContent in_window)
        {
            TableLayoutPanel w_layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = CODE_DOCK_WINDOW_BACK_COLOR,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            w_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            w_layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            w_layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            in_window.Controls.Add(w_layout);

            Panel w_command_panel = CreateToolCommandPanel();
            w_command_panel.Dock = DockStyle.Fill;
            MoveControlTo(button_memory_history_clear, w_command_panel);
            button_memory_history_clear.Location = new Point(6, 3);
            button_memory_history_clear.Size = new Size(58, 23);
            MoveControlTo(dataGridView_memory_history, in_window);
            dataGridView_memory_history.Dock = DockStyle.Fill;
            dataGridView_memory_history.CellDoubleClick += dataGridView_memory_history_CellDoubleClick;
            w_layout.Controls.Add(w_command_panel, 0, 0);
            w_layout.Controls.Add(dataGridView_memory_history, 0, 1);
        }

        private void ConfigureRamChangesToolWindow(CodeDockContent in_window)
        {
            TableLayoutPanel w_layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = CODE_DOCK_WINDOW_BACK_COLOR,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            w_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            w_layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            w_layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            in_window.Controls.Add(w_layout);

            Panel w_command_panel = CreateToolCommandPanel(62);
            w_command_panel.Dock = DockStyle.Fill;
            MoveControlTo(comboBox_ram_change_bytes, w_command_panel);
            MoveControlTo(button_ram_change_start, w_command_panel);
            MoveControlTo(button_ram_change_stop, w_command_panel);
            MoveControlTo(g_ram_change_search_label, w_command_panel);
            MoveControlTo(g_ram_change_search_textbox, w_command_panel);
            MoveControlTo(g_ram_change_search_clear_button, w_command_panel);
            g_ram_change_search_label.Text = "search";
            g_ram_change_search_label.AutoSize = true;
            g_ram_change_search_label.TextAlign = ContentAlignment.MiddleLeft;
            g_ram_change_search_textbox.BorderStyle = BorderStyle.FixedSingle;
            g_ram_change_search_textbox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            g_ram_change_search_textbox.TextChanged += ramChangeSearchTextBox_TextChanged;
            g_ram_change_search_clear_button.Text = "clear";
            g_ram_change_search_clear_button.Click += ramChangeSearchClearButton_Click;
            w_command_panel.Resize += (_, _) => LayoutRamChangeCommandPanel(w_command_panel);
            LayoutRamChangeCommandPanel(w_command_panel);

            MoveControlTo(dataGridView_ram_change, in_window);
            dataGridView_ram_change.Dock = DockStyle.Fill;
            w_layout.Controls.Add(w_command_panel, 0, 0);
            w_layout.Controls.Add(dataGridView_ram_change, 0, 1);
        }

        private Panel CreateToolCommandPanel(int in_height = 30)
        {
            return new Panel
            {
                Height = in_height,
                BackColor = CODE_DOCK_COMMAND_BACK_COLOR
            };
        }

        private void LayoutRamChangeCommandPanel(Panel in_panel)
        {
            const int w_margin = 6;
            const int w_control_height = 23;
            const int w_clear_button_width = 52;

            comboBox_ram_change_bytes.Location = new Point(w_margin, 4);
            comboBox_ram_change_bytes.Size = new Size(46, w_control_height);
            button_ram_change_start.Location = new Point(58, 4);
            button_ram_change_start.Size = new Size(52, w_control_height);
            button_ram_change_stop.Location = new Point(116, 4);
            button_ram_change_stop.Size = new Size(52, w_control_height);

            g_ram_change_search_label.Location = new Point(w_margin, 35);
            g_ram_change_search_label.Size = new Size(46, w_control_height);
            g_ram_change_search_clear_button.Location = new Point(Math.Max(174, in_panel.ClientSize.Width - w_clear_button_width - w_margin), 32);
            g_ram_change_search_clear_button.Size = new Size(w_clear_button_width, w_control_height);
            g_ram_change_search_textbox.Location = new Point(58, 32);
            g_ram_change_search_textbox.Size = new Size(Math.Max(40, g_ram_change_search_clear_button.Left - 64), w_control_height);
        }

        private void MoveControlTo(Control in_control, Control in_parent)
        {
            in_control.Parent?.Controls.Remove(in_control);
            in_parent.Controls.Add(in_control);
        }

        private void HideLegacyToolLabels()
        {
            label1.Visible = false;
            label3.Visible = false;
            label_event_wait.Visible = false;
            label_memory_history.Visible = false;
            label_ram_change.Visible = false;
        }

        private void ApplyToolWindowTheme()
        {
            menuStrip1.BackColor = CODE_DOCK_MENU_BACK_COLOR;
            menuStrip1.ForeColor = CODE_DOCK_MENU_TEXT_COLOR;
            foreach (ToolStripMenuItem w_item in menuStrip1.Items.OfType<ToolStripMenuItem>())
            {
                ApplyMenuTheme(w_item);
            }
            ApplyGridTheme(dataGridView_comment1);
            ApplyGridTheme(dataGridView_memory);
            ApplyGridTheme(dataGridView_memory_history);
            ApplyGridTheme(dataGridView_ram_change);
            ConfigureAccessHistoryGridColumns();
            ConfigureRamChangeGridColumns();
            StyleToolButton(button_memory_history_clear);
            StyleToolButton(button_ram_change_start);
            StyleToolButton(button_ram_change_stop);
            StyleToolButton(g_ram_change_search_clear_button);
            comboBox_ram_change_bytes.BackColor = Color.White;
            comboBox_ram_change_bytes.ForeColor = CODE_DOCK_GRID_TEXT_COLOR;
            g_ram_change_search_label.ForeColor = CODE_DOCK_GRID_TEXT_COLOR;
            g_ram_change_search_textbox.BackColor = Color.White;
            g_ram_change_search_textbox.ForeColor = CODE_DOCK_GRID_TEXT_COLOR;
        }

        private void ApplyMenuTheme(ToolStripMenuItem in_item)
        {
            in_item.ForeColor = CODE_DOCK_MENU_TEXT_COLOR;
            foreach (ToolStripItem w_child in in_item.DropDownItems)
            {
                w_child.BackColor = CODE_DOCK_MENU_BACK_COLOR;
                w_child.ForeColor = CODE_DOCK_MENU_TEXT_COLOR;
                if (w_child is ToolStripMenuItem w_child_menu)
                {
                    ApplyMenuTheme(w_child_menu);
                }
            }
        }

        private void ApplyGridTheme(DataGridView in_grid)
        {
            in_grid.BackgroundColor = CODE_DOCK_WINDOW_BACK_COLOR;
            in_grid.BorderStyle = BorderStyle.None;
            in_grid.GridColor = CODE_DOCK_GRID_LINE_COLOR;
            in_grid.EnableHeadersVisualStyles = false;
            in_grid.ColumnHeadersDefaultCellStyle.BackColor = CODE_DOCK_GRID_HEADER_BACK_COLOR;
            in_grid.ColumnHeadersDefaultCellStyle.ForeColor = CODE_DOCK_GRID_TEXT_COLOR;
            in_grid.DefaultCellStyle.BackColor = Color.White;
            in_grid.DefaultCellStyle.ForeColor = CODE_DOCK_GRID_TEXT_COLOR;
            in_grid.DefaultCellStyle.SelectionBackColor = CODE_DOCK_GRID_SELECT_BACK_COLOR;
            in_grid.DefaultCellStyle.SelectionForeColor = Color.White;
            in_grid.AlternatingRowsDefaultCellStyle.BackColor = CODE_DOCK_GRID_ALT_BACK_COLOR;
            in_grid.RowTemplate.DefaultCellStyle.BackColor = Color.White;
        }

        private void ConfigureAccessHistoryGridColumns()
        {
            dataGridView_memory_history.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView_memory_history.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView_memory_history.ScrollBars = ScrollBars.Both;
            SetGridColumnWidth(historyTypeColumn, "種別", 64);
            SetGridColumnWidth(historyBeforeHexColumn, "変更前 HEX", 96);
            SetGridColumnWidth(historyBeforeDecColumn, "変更前 DEC", 104);
            SetGridColumnWidth(historyAfterHexColumn, "変更後 HEX", 96);
            SetGridColumnWidth(historyAfterDecColumn, "変更後 DEC", 104);
            SetGridColumnWidth(historyPcColumn, "PC", 88);
        }

        private void ConfigureRamChangeGridColumns()
        {
            dataGridView_ram_change.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView_ram_change.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView_ram_change.ScrollBars = ScrollBars.Both;
            SetGridColumnWidth(ramChangeAddressColumn, "address", 88);
            SetGridColumnWidth(ramChangeStartHexColumn, "開始 HEX", 96);
            SetGridColumnWidth(ramChangeStartDecColumn, "開始 DEC", 104);
            SetGridColumnWidth(ramChangeStopHexColumn, "停止 HEX", 96);
            SetGridColumnWidth(ramChangeStopDecColumn, "停止 DEC", 104);
            ramChangeBytesColumn.Visible = false;
        }

        private void SetGridColumnWidth(DataGridViewColumn in_column, string in_header_text, int in_width)
        {
            in_column.HeaderText = in_header_text;
            in_column.MinimumWidth = in_width;
            in_column.Width = in_width;
            in_column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        }

        private void ramChangeSearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            ApplyRamChangeSearchFilter();
        }

        private void ramChangeSearchClearButton_Click(object? sender, EventArgs e)
        {
            g_ram_change_search_textbox.Clear();
        }

        private void ApplyRamChangeSearchFilter()
        {
            if ((dataGridView_ram_change.IsDisposed == true) || (dataGridView_ram_change.IsHandleCreated == false)) return;

            string w_search_text = NormalizeRamChangeSearchText(g_ram_change_search_textbox.Text);
            dataGridView_ram_change.SuspendLayout();
            try
            {
                dataGridView_ram_change.ClearSelection();
                dataGridView_ram_change.CurrentCell = null;
                foreach (DataGridViewRow w_row in dataGridView_ram_change.Rows)
                {
                    if (w_row.IsNewRow == true) continue;
                    w_row.Visible = string.IsNullOrEmpty(w_search_text) == true
                        || IsRamChangeRowSearchMatched(w_row, w_search_text) == true;
                }
            }
            finally
            {
                dataGridView_ram_change.ResumeLayout();
            }
        }

        private bool IsRamChangeRowSearchMatched(DataGridViewRow in_row, string in_search_text)
        {
            foreach (DataGridViewCell w_cell in in_row.Cells)
            {
                string w_cell_text = w_cell.Value?.ToString() ?? "";
                if (ContainsRamChangeSearchText(w_cell_text, in_search_text) == true) return true;
            }
            return false;
        }

        private bool ContainsRamChangeSearchText(string in_cell_text, string in_search_text)
        {
            if (in_cell_text.IndexOf(in_search_text, StringComparison.OrdinalIgnoreCase) >= 0) return true;

            string w_normalized_cell_text = NormalizeRamChangeSearchText(in_cell_text);
            return w_normalized_cell_text.IndexOf(in_search_text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string NormalizeRamChangeSearchText(string in_text)
        {
            string w_text = in_text.Trim();
            if (w_text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) == true)
            {
                w_text = w_text.Substring(2);
            }
            if (w_text.StartsWith("$", StringComparison.Ordinal) == true)
            {
                w_text = w_text.Substring(1);
            }
            return w_text;
        }

        private void StyleToolButton(Button in_button)
        {
            in_button.FlatStyle = FlatStyle.Flat;
            in_button.FlatAppearance.BorderColor = CODE_DOCK_BUTTON_BORDER_COLOR;
            in_button.BackColor = Color.FromArgb(235, 245, 255);
            in_button.ForeColor = CODE_DOCK_GRID_TEXT_COLOR;
        }

        private void codeToolStripMenuItem_CheckedChanged(object? sender, EventArgs e)
        {
            if (g_code_tool_layout_updating == true) return;
            if (sender is not ToolStripMenuItem w_item) return;
            if (w_item.Tag is not CODE_TOOL_KIND w_kind) return;
            SetCodeToolVisible(w_kind, w_item.Checked, true);
        }

        private void SetCodeToolVisible(CODE_TOOL_KIND in_kind, bool in_visible, bool in_save)
        {
            if (g_code_tool_windows.TryGetValue(in_kind, out CodeDockContent? w_window) == false) return;

            g_code_tool_layout_updating = true;
            if (in_visible == true)
            {
                if (w_window.DockPanel == null)
                {
                    w_window.Show(g_code_dock_panel, GetDefaultCodeToolDockState(in_kind));
                }
                else
                {
                    w_window.Show(g_code_dock_panel);
                }
            }
            else
            {
                w_window.Hide();
            }
            if (g_code_tool_menu_items.TryGetValue(in_kind, out ToolStripMenuItem? w_item) == true)
            {
                w_item.Checked = in_visible;
            }
            g_code_tool_layout_updating = false;

            if (in_visible == true) w_window.Activate();
            if (in_save == true) SaveCodeToolLayout();
        }

        private void UpdateCodeToolMenuChecks()
        {
            g_code_tool_layout_updating = true;
            foreach (KeyValuePair<CODE_TOOL_KIND, CodeDockContent> w_pair in g_code_tool_windows)
            {
                if (g_code_tool_menu_items.TryGetValue(w_pair.Key, out ToolStripMenuItem? w_item) == true)
                {
                    w_item.Checked = (w_pair.Value.DockPanel != null) && (w_pair.Value.IsHidden == false);
                }
            }
            g_code_tool_layout_updating = false;
        }

        private void SaveCodeToolLayout()
        {
            if (g_code_tool_windows_ready == false) return;
            if (g_code_tool_layout_updating == true) return;
            if (ReferenceEquals(WinFormsDebugTools.g_form_code, this) == false) return;

            g_code_tool_layout_save_pending = true;
            g_code_tool_layout_save_timer.Stop();
            g_code_tool_layout_save_timer.Start();
        }

        public void FlushCodeToolLayoutSave()
        {
            if (g_code_tool_windows_ready == false) return;
            if (g_code_tool_layout_updating == true) return;
            if (ReferenceEquals(WinFormsDebugTools.g_form_code, this) == false) return;
            if (g_code_tool_layout_save_pending == false) return;

            g_code_tool_layout_save_timer.Stop();
            g_code_tool_layout_save_pending = false;
            md_main.write_setting();
        }

        private void codeToolLayoutSaveTimer_Tick(object? sender, EventArgs e)
        {
            FlushCodeToolLayoutSave();
        }

        public void EnsureCodeToolLayoutVisible()
        {
            if (g_code_tool_windows_ready == false) return;
            if (RestorePendingCodeToolLayout() == true)
            {
                return;
            }
            if (RestorePendingDefaultCodeToolLayout() == true)
            {
                return;
            }
            if (string.IsNullOrEmpty(g_pending_code_tool_layout_text) == false)
            {
                return;
            }
            if (g_default_code_tool_layout_pending == true)
            {
                return;
            }
            if (g_code_tool_layout_restored == true)
            {
                UpdateCodeToolMenuChecks();
                return;
            }
            if (g_code_tool_windows.Values.Any(in_window => in_window.DockPanel != null) == true)
            {
                UpdateCodeToolMenuChecks();
                return;
            }

            if (CanRestoreCodeToolLayout() == false)
            {
                g_default_code_tool_layout_pending = true;
                return;
            }

            g_code_tool_layout_updating = true;
            try
            {
                ShowDefaultCodeToolLayout();
                g_code_tool_layout_restored = true;
            }
            finally
            {
                g_code_tool_layout_updating = false;
            }
            UpdateCodeToolMenuChecks();
        }

        public string GetCodeToolLayoutText()
        {
            if (g_code_tool_windows_ready == false) return "";
            if ((g_code_tool_layout_restored == false) && (string.IsNullOrEmpty(g_pending_code_tool_layout_text) == false))
            {
                return g_pending_code_tool_layout_text;
            }

            string w_file_name = Path.Combine(Path.GetTempPath(), "MDTracer_CodeDock_" + Guid.NewGuid().ToString("N") + ".xml");
            try
            {
                g_code_dock_panel.SaveAsXml(w_file_name);
                string w_xml = File.ReadAllText(w_file_name, Encoding.UTF8);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(w_xml));
            }
            catch
            {
                return g_pending_code_tool_layout_text;
            }
            finally
            {
                try
                {
                    File.Delete(w_file_name);
                }
                catch
                {
                }
            }
        }

        public void SetCodeToolLayoutText(string in_text)
        {
            if (TryGetCodeToolLayoutXml(in_text, out string w_xml, out string w_setting_text) == false) return;

            if (IsBrokenInitialFloatLayoutXml(w_xml) == true)
            {
                g_pending_code_tool_layout_text = "";
                g_default_code_tool_layout_pending = true;
                RestorePendingDefaultCodeToolLayout();
                return;
            }

            g_pending_code_tool_layout_text = w_setting_text;
            RestorePendingCodeToolLayout();
        }

        private bool RestorePendingCodeToolLayout()
        {
            if (string.IsNullOrEmpty(g_pending_code_tool_layout_text) == true) return false;
            if (CanRestoreCodeToolLayout() == false) return false;
            if (TryGetCodeToolLayoutXml(g_pending_code_tool_layout_text, out string w_xml, out _) == false)
            {
                g_pending_code_tool_layout_text = "";
                return false;
            }

            if (LoadCodeToolLayoutXml(w_xml) == false)
            {
                g_pending_code_tool_layout_text = "";
                return false;
            }

            g_pending_code_tool_layout_text = "";
            return true;
        }

        private bool RestorePendingDefaultCodeToolLayout()
        {
            if (g_default_code_tool_layout_pending == false) return false;
            if (CanRestoreCodeToolLayout() == false) return false;

            g_default_code_tool_layout_pending = false;
            g_code_tool_layout_updating = true;
            try
            {
                ShowDefaultCodeToolLayout();
                g_code_tool_layout_restored = true;
                UpdateCodeToolMenuChecks();
            }
            finally
            {
                g_code_tool_layout_updating = false;
            }
            SaveCodeToolLayout();
            return true;
        }

        private bool CanRestoreCodeToolLayout()
        {
            if (g_code_tool_windows_ready == false) return false;
            if (g_code_dock_panel.IsHandleCreated == false) return false;
            if (g_code_dock_panel.ClientSize.Width <= 0) return false;
            if (g_code_dock_panel.ClientSize.Height <= 0) return false;
            return true;
        }

        private bool TryGetCodeToolLayoutXml(string in_text, out string out_xml, out string out_setting_text)
        {
            out_xml = "";
            out_setting_text = "";
            if (string.IsNullOrWhiteSpace(in_text) == true) return false;

            try
            {
                string w_trimmed_text = in_text.Trim();
                if (w_trimmed_text.StartsWith("<", StringComparison.Ordinal) == true)
                {
                    out_xml = w_trimmed_text;
                    out_setting_text = Convert.ToBase64String(Encoding.UTF8.GetBytes(out_xml));
                }
                else
                {
                    out_xml = Encoding.UTF8.GetString(Convert.FromBase64String(w_trimmed_text));
                    out_setting_text = w_trimmed_text;
                }
            }
            catch
            {
                ShowMissingCodeDockContents();
                UpdateCodeToolMenuChecks();
                return false;
            }
            return string.IsNullOrWhiteSpace(out_xml) == false;
        }

        private bool LoadCodeToolLayoutXml(string in_xml)
        {
            string w_file_name = Path.Combine(Path.GetTempPath(), "MDTracer_CodeDock_" + Guid.NewGuid().ToString("N") + ".xml");
            HashSet<CODE_TOOL_KIND> w_visible_kinds = GetVisibleCodeToolKinds(in_xml);
            bool w_restore_prev = g_code_tool_layout_updating;
            g_code_tool_layout_updating = true;
            try
            {
                File.WriteAllText(w_file_name, in_xml, Encoding.UTF8);
                g_code_dock_panel.LoadFromXml(w_file_name, DeserializeCodeDockContent);
                ApplyCodeToolLayoutXmlFallback(in_xml);
                g_code_tool_layout_restored = true;
                ShowMissingCodeDockContents(w_visible_kinds);
                UpdateCodeToolMenuChecks();
                return true;
            }
            catch
            {
                if (ApplyCodeToolLayoutXmlFallback(in_xml) == false)
                {
                    return false;
                }

                g_code_tool_layout_restored = true;
                ShowMissingCodeDockContents(w_visible_kinds);
                UpdateCodeToolMenuChecks();
                return true;
            }
            finally
            {
                try
                {
                    File.Delete(w_file_name);
                }
                catch
                {
                }
                g_code_tool_layout_updating = w_restore_prev;
            }
        }

        private bool IsBrokenInitialFloatLayoutXml(string in_xml)
        {
            if (TryReadCodeToolSavedLayout(in_xml, out CodeToolSavedLayout w_layout) == false) return false;

            HashSet<CODE_TOOL_KIND> w_float_kinds = GetFloatCodeToolKinds(in_xml);
            CODE_TOOL_KIND[] w_expected_docked_kinds =
            {
                CODE_TOOL_KIND.Comment,
                CODE_TOOL_KIND.AccessHistory,
                CODE_TOOL_KIND.RamChanges,
                CODE_TOOL_KIND.MemoryMonitoring
            };

            if (w_float_kinds.Contains(CODE_TOOL_KIND.Code) == true) return false;
            if (w_float_kinds.Contains(CODE_TOOL_KIND.Address) == true) return false;
            foreach (CODE_TOOL_KIND w_kind in w_expected_docked_kinds)
            {
                if (w_layout.VisibleKinds.Contains(w_kind) == false) return false;
                if (w_float_kinds.Contains(w_kind) == false) return false;
            }
            return true;
        }

        private HashSet<CODE_TOOL_KIND> GetFloatCodeToolKinds(string in_xml)
        {
            HashSet<CODE_TOOL_KIND> w_float_kinds = new HashSet<CODE_TOOL_KIND>();
            try
            {
                XDocument w_document = XDocument.Parse(in_xml);
                foreach (XElement w_content in w_document.Descendants("Content"))
                {
                    string w_persist_string = w_content.Attribute("PersistString")?.Value ?? "";
                    if (TryGetCodeToolKind(w_persist_string, out CODE_TOOL_KIND w_kind) == false) continue;

                    bool w_is_float = string.Equals(w_content.Attribute("IsFloat")?.Value, "True", StringComparison.OrdinalIgnoreCase);
                    if (w_is_float == true)
                    {
                        w_float_kinds.Add(w_kind);
                    }
                }
            }
            catch
            {
            }
            return w_float_kinds;
        }

        private bool ApplyCodeToolLayoutXmlFallback(string in_xml)
        {
            try
            {
                if (TryReadCodeToolSavedLayout(in_xml, out CodeToolSavedLayout w_layout) == false) return false;
                if (w_layout.ContentKinds.Count == 0) return false;

                foreach (CodeDockContent w_window in g_code_tool_windows.Values)
                {
                    if (w_window.DockPanel == null) continue;
                    w_window.Hide();
                }

                Dictionary<int, DockPane> w_restored_panes = new Dictionary<int, DockPane>();
                HashSet<CODE_TOOL_KIND> w_restored_kinds = new HashSet<CODE_TOOL_KIND>();
                foreach (CodeToolSavedDockWindow w_dock_window in w_layout.DockWindows)
                {
                    foreach (CodeToolSavedNestedPane w_nested_pane in w_dock_window.NestedPanes)
                    {
                        if (w_layout.Panes.TryGetValue(w_nested_pane.RefId, out CodeToolSavedPane? w_pane) == false) continue;

                        DockPane? w_pane_control = ShowSavedPane(w_layout, w_pane, w_nested_pane, w_dock_window.DockState, w_restored_panes);
                        if (w_pane_control == null) continue;

                        w_restored_panes[w_pane.Id] = w_pane_control;
                        foreach (int w_content_id in w_pane.ContentIds)
                        {
                            if (w_layout.ContentKinds.TryGetValue(w_content_id, out CODE_TOOL_KIND w_kind) == true)
                            {
                                w_restored_kinds.Add(w_kind);
                            }
                        }
                    }
                }

                foreach (CodeToolSavedPane w_pane in w_layout.Panes.Values)
                {
                    if (w_restored_panes.ContainsKey(w_pane.Id) == true) continue;

                    CodeToolSavedNestedPane w_nested_pane = new CodeToolSavedNestedPane
                    {
                        RefId = w_pane.Id
                    };
                    DockPane? w_pane_control = ShowSavedPane(w_layout, w_pane, w_nested_pane, w_pane.DockState, w_restored_panes);
                    if (w_pane_control == null) continue;

                    w_restored_panes[w_pane.Id] = w_pane_control;
                }

                foreach (CODE_TOOL_KIND w_kind in w_layout.VisibleKinds)
                {
                    if (w_restored_kinds.Contains(w_kind) == true) continue;
                    if (g_code_tool_windows.TryGetValue(w_kind, out CodeDockContent? w_window) == false) continue;

                    w_window.Show(g_code_dock_panel, GetDefaultCodeToolDockState(w_kind));
                }

                foreach (KeyValuePair<CODE_TOOL_KIND, CodeDockContent> w_pair in g_code_tool_windows)
                {
                    if (w_layout.VisibleKinds.Contains(w_pair.Key) == false && w_pair.Value.DockPanel != null)
                    {
                        w_pair.Value.Hide();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private DockPane? ShowSavedPane(
            CodeToolSavedLayout in_layout,
            CodeToolSavedPane in_pane,
            CodeToolSavedNestedPane in_nested_pane,
            DockState in_fallback_dock_state,
            Dictionary<int, DockPane> in_restored_panes)
        {
            CodeDockContent? w_first_window = null;
            foreach (int w_content_id in in_pane.ContentIds)
            {
                if (in_layout.ContentKinds.TryGetValue(w_content_id, out CODE_TOOL_KIND w_kind) == false) continue;
                if (in_layout.VisibleKinds.Contains(w_kind) == false) continue;
                if (g_code_tool_windows.TryGetValue(w_kind, out CodeDockContent? w_window) == false) continue;

                w_first_window = w_window;
                break;
            }
            if (w_first_window == null) return null;

            DockPane? w_prev_pane = null;
            if (in_nested_pane.PrevPane >= 0)
            {
                in_restored_panes.TryGetValue(in_nested_pane.PrevPane, out w_prev_pane);
            }

            if (w_prev_pane != null)
            {
                w_first_window.Show(w_prev_pane, in_nested_pane.Alignment, Math.Clamp(in_nested_pane.Proportion, 0.05, 0.95));
            }
            else
            {
                DockState w_dock_state = NormalizeSavedDockState(in_pane.DockState, in_fallback_dock_state, w_first_window.Kind);
                w_first_window.Show(g_code_dock_panel, w_dock_state);
            }

            DockPane? w_current_pane = w_first_window.DockHandler.Pane;
            if (w_current_pane == null) return null;

            foreach (int w_content_id in in_pane.ContentIds)
            {
                if (in_layout.ContentKinds.TryGetValue(w_content_id, out CODE_TOOL_KIND w_kind) == false) continue;
                if (w_kind == w_first_window.Kind) continue;
                if (in_layout.VisibleKinds.Contains(w_kind) == false) continue;
                if (g_code_tool_windows.TryGetValue(w_kind, out CodeDockContent? w_window) == false) continue;

                w_window.Show(w_current_pane, w_first_window);
            }
            return w_current_pane;
        }

        private DockState NormalizeSavedDockState(DockState in_pane_state, DockState in_fallback_state, CODE_TOOL_KIND in_kind)
        {
            DockState w_state = in_pane_state != DockState.Unknown ? in_pane_state : in_fallback_state;
            if (w_state == DockState.Unknown || w_state == DockState.Hidden)
            {
                return GetDefaultCodeToolDockState(in_kind);
            }
            return w_state;
        }

        private bool TryReadCodeToolSavedLayout(string in_xml, out CodeToolSavedLayout out_layout)
        {
            out_layout = new CodeToolSavedLayout();
            try
            {
                XDocument w_document = XDocument.Parse(in_xml);
                XElement? w_root = w_document.Root;
                if (w_root == null) return false;

                XElement? w_contents = w_root.Element("Contents");
                if (w_contents != null)
                {
                    foreach (XElement w_content in w_contents.Elements("Content"))
                    {
                        int w_id = GetIntAttribute(w_content, "ID", -1);
                        string w_persist_string = w_content.Attribute("PersistString")?.Value ?? "";
                        if (w_id < 0) continue;
                        if (TryGetCodeToolKind(w_persist_string, out CODE_TOOL_KIND w_kind) == false) continue;

                        out_layout.ContentKinds[w_id] = w_kind;
                        bool w_is_hidden = string.Equals(w_content.Attribute("IsHidden")?.Value, "True", StringComparison.OrdinalIgnoreCase);
                        if (w_is_hidden == false)
                        {
                            out_layout.VisibleKinds.Add(w_kind);
                        }
                    }
                }

                XElement? w_panes = w_root.Element("Panes");
                if (w_panes != null)
                {
                    foreach (XElement w_pane_element in w_panes.Elements("Pane"))
                    {
                        int w_id = GetIntAttribute(w_pane_element, "ID", -1);
                        if (w_id < 0) continue;

                        CodeToolSavedPane w_pane = new CodeToolSavedPane
                        {
                            Id = w_id,
                            DockState = GetDockStateAttribute(w_pane_element, "DockState", DockState.Unknown)
                        };
                        XElement? w_pane_contents = w_pane_element.Element("Contents");
                        if (w_pane_contents != null)
                        {
                            foreach (XElement w_content_ref in w_pane_contents.Elements("Content"))
                            {
                                int w_ref_id = GetIntAttribute(w_content_ref, "RefID", -1);
                                if (w_ref_id >= 0)
                                {
                                    w_pane.ContentIds.Add(w_ref_id);
                                }
                            }
                        }
                        out_layout.Panes[w_id] = w_pane;
                    }
                }

                XElement? w_dock_windows = w_root.Element("DockWindows");
                if (w_dock_windows != null)
                {
                    foreach (XElement w_dock_window_element in w_dock_windows.Elements("DockWindow"))
                    {
                        CodeToolSavedDockWindow w_dock_window = new CodeToolSavedDockWindow
                        {
                            DockState = GetDockStateAttribute(w_dock_window_element, "DockState", DockState.Unknown)
                        };
                        XElement? w_nested_panes = w_dock_window_element.Element("NestedPanes");
                        if (w_nested_panes != null)
                        {
                            foreach (XElement w_nested_pane_element in w_nested_panes.Elements("Pane"))
                            {
                                int w_ref_id = GetIntAttribute(w_nested_pane_element, "RefID", -1);
                                if (w_ref_id < 0) continue;

                                w_dock_window.NestedPanes.Add(new CodeToolSavedNestedPane
                                {
                                    RefId = w_ref_id,
                                    PrevPane = GetIntAttribute(w_nested_pane_element, "PrevPane", -1),
                                    Alignment = GetDockAlignmentAttribute(w_nested_pane_element, "Alignment", DockAlignment.Right),
                                    Proportion = GetDoubleAttribute(w_nested_pane_element, "Proportion", 0.5)
                                });
                            }
                        }
                        out_layout.DockWindows.Add(w_dock_window);
                    }
                }
                return out_layout.ContentKinds.Count != 0;
            }
            catch
            {
                out_layout = new CodeToolSavedLayout();
                return false;
            }
        }

        private int GetIntAttribute(XElement in_element, string in_name, int in_default)
        {
            return int.TryParse(in_element.Attribute(in_name)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int w_value)
                ? w_value
                : in_default;
        }

        private double GetDoubleAttribute(XElement in_element, string in_name, double in_default)
        {
            return double.TryParse(in_element.Attribute(in_name)?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double w_value)
                ? w_value
                : in_default;
        }

        private DockState GetDockStateAttribute(XElement in_element, string in_name, DockState in_default)
        {
            string w_value = in_element.Attribute(in_name)?.Value ?? "";
            return Enum.TryParse(w_value, out DockState w_state) == true ? w_state : in_default;
        }

        private DockAlignment GetDockAlignmentAttribute(XElement in_element, string in_name, DockAlignment in_default)
        {
            string w_value = in_element.Attribute(in_name)?.Value ?? "";
            return Enum.TryParse(w_value, out DockAlignment w_alignment) == true ? w_alignment : in_default;
        }

        private IDockContent? DeserializeCodeDockContent(string in_persist_string)
        {
            if (Enum.TryParse(in_persist_string, out CODE_TOOL_KIND w_kind) == true)
            {
                return g_code_tool_windows.TryGetValue(w_kind, out CodeDockContent? w_window) == true ? w_window : null;
            }

            if (TryGetCodeToolKind(in_persist_string, out w_kind) == true)
            {
                return g_code_tool_windows.TryGetValue(w_kind, out CodeDockContent? w_window) == true ? w_window : null;
            }

            return null;
        }

        private HashSet<CODE_TOOL_KIND> GetVisibleCodeToolKinds(string in_xml)
        {
            HashSet<CODE_TOOL_KIND> w_visible_kinds = new HashSet<CODE_TOOL_KIND>();
            try
            {
                XDocument w_document = XDocument.Parse(in_xml);
                foreach (XElement w_content in w_document.Descendants("Content"))
                {
                    string w_persist_string = w_content.Attribute("PersistString")?.Value ?? "";
                    if (TryGetCodeToolKind(w_persist_string, out CODE_TOOL_KIND w_kind) == false) continue;

                    bool w_is_hidden = string.Equals(w_content.Attribute("IsHidden")?.Value, "True", StringComparison.OrdinalIgnoreCase);
                    if (w_is_hidden == false)
                    {
                        w_visible_kinds.Add(w_kind);
                    }
                }
            }
            catch
            {
                foreach (CODE_TOOL_KIND w_kind in Enum.GetValues<CODE_TOOL_KIND>())
                {
                    w_visible_kinds.Add(w_kind);
                }
            }
            return w_visible_kinds;
        }

        private void ShowMissingCodeDockContents(HashSet<CODE_TOOL_KIND>? in_visible_kinds = null)
        {
            foreach (KeyValuePair<CODE_TOOL_KIND, CodeDockContent> w_pair in g_code_tool_windows)
            {
                if (w_pair.Value.DockPanel != null) continue;
                if ((in_visible_kinds != null) && (in_visible_kinds.Contains(w_pair.Key) == false)) continue;
                w_pair.Value.Show(g_code_dock_panel, GetDefaultCodeToolDockState(w_pair.Key));
            }
        }

        private void CodeToolContent_VisibleChanged(object? sender, EventArgs e)
        {
            if (g_code_tool_layout_updating == true) return;
            if (sender is not CodeDockContent w_window) return;
            if (g_code_tool_menu_items.TryGetValue(w_window.Kind, out ToolStripMenuItem? w_item) == true)
            {
                g_code_tool_layout_updating = true;
                w_item.Checked = (w_window.DockPanel != null) && (w_window.IsHidden == false);
                g_code_tool_layout_updating = false;
            }
            SaveCodeToolLayout();
        }

        private void CodeToolContent_LayoutChanged(object? sender, EventArgs e)
        {
            SaveCodeToolLayout();
        }

        private bool TryGetCodeToolKind(string in_text, out CODE_TOOL_KIND out_kind)
        {
            string w_text = in_text.Trim().ToLowerInvariant()
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "");
            switch (w_text)
            {
                case "code":
                    out_kind = CODE_TOOL_KIND.Code;
                    return true;
                case "address":
                    out_kind = CODE_TOOL_KIND.Address;
                    return true;
                case "comment":
                case "comment1":
                case "commentresult":
                case "comment1result":
                    out_kind = CODE_TOOL_KIND.Comment;
                    return true;
                case "memory":
                case "memorymonitor":
                case "memorymonitoring":
                    out_kind = CODE_TOOL_KIND.MemoryMonitoring;
                    return true;
                case "access":
                case "accesshistory":
                    out_kind = CODE_TOOL_KIND.AccessHistory;
                    return true;
                case "ram":
                case "ramchange":
                case "ramchanges":
                    out_kind = CODE_TOOL_KIND.RamChanges;
                    return true;
                default:
                    out_kind = CODE_TOOL_KIND.Code;
                    return false;
            }
        }
    }
}
