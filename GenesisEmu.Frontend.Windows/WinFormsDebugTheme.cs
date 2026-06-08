namespace GenesisEmu.Frontend.Windows
{
    //----------------------------------------------------------------
    // Light blue debug-tool palette shared across MDTracer tool windows.
    //----------------------------------------------------------------
    public static class WinFormsDebugTheme
    {
        public static readonly Color WindowBackColor = Color.FromArgb(246, 250, 255);
        public static readonly Color PanelBackColor = Color.FromArgb(221, 235, 247);
        public static readonly Color ControlBackColor = Color.FromArgb(230, 241, 252);
        public static readonly Color TextColor = Color.FromArgb(25, 45, 65);
        public static readonly Color GridHeaderBackColor = Color.FromArgb(210, 228, 247);
        public static readonly Color GridLineColor = Color.FromArgb(190, 212, 236);
        public static readonly Color GridAltBackColor = Color.FromArgb(239, 247, 255);
        public static readonly Color GridSelectBackColor = Color.FromArgb(76, 143, 202);
        public static readonly Color GridSelectForeColor = Color.White;
        public static readonly Color BorderColor = Color.FromArgb(148, 180, 213);

        public static void Apply(Form in_form)
        {
            in_form.BackColor = WindowBackColor;
            in_form.ForeColor = TextColor;
            ApplyRecursive(in_form.Controls);
        }

        private static void ApplyRecursive(Control.ControlCollection in_controls)
        {
            foreach (Control w_control in in_controls)
            {
                switch (w_control)
                {
                    case PictureBox:
                        break;
                    case MenuStrip w_menu:
                        ApplyToolStrip(w_menu);
                        break;
                    case StatusStrip w_status:
                        ApplyToolStrip(w_status);
                        break;
                    case ToolStrip w_toolStrip:
                        ApplyToolStrip(w_toolStrip);
                        break;
                    case DataGridView w_grid:
                        ApplyDataGridView(w_grid);
                        break;
                    case ListView w_list:
                        ApplyListView(w_list);
                        break;
                    case GroupBox w_group:
                        w_group.BackColor = WindowBackColor;
                        w_group.ForeColor = TextColor;
                        break;
                    case Panel w_panel:
                        w_panel.BackColor = PanelBackColor;
                        w_panel.ForeColor = TextColor;
                        break;
                    case ComboBox w_combo:
                        w_combo.BackColor = ControlBackColor;
                        w_combo.ForeColor = TextColor;
                        w_combo.FlatStyle = FlatStyle.Flat;
                        break;
                    case CheckBox w_check:
                        w_check.BackColor = Color.Transparent;
                        w_check.ForeColor = TextColor;
                        break;
                    case Label w_label:
                        w_label.BackColor = Color.Transparent;
                        w_label.ForeColor = TextColor;
                        break;
                    case Button w_button:
                        w_button.BackColor = ControlBackColor;
                        w_button.ForeColor = TextColor;
                        w_button.FlatStyle = FlatStyle.Flat;
                        w_button.FlatAppearance.BorderColor = BorderColor;
                        break;
                    case TextBox w_text:
                        w_text.BackColor = ControlBackColor;
                        w_text.ForeColor = TextColor;
                        w_text.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    default:
                        if (w_control is ScrollBar == false)
                        {
                            w_control.BackColor = WindowBackColor;
                            w_control.ForeColor = TextColor;
                        }
                        break;
                }

                if (w_control.HasChildren == true)
                {
                    ApplyRecursive(w_control.Controls);
                }
            }
        }

        private static void ApplyDataGridView(DataGridView in_grid)
        {
            in_grid.BackgroundColor = WindowBackColor;
            in_grid.GridColor = GridLineColor;
            in_grid.BorderStyle = BorderStyle.FixedSingle;
            in_grid.EnableHeadersVisualStyles = false;
            in_grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            in_grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            in_grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBackColor;
            in_grid.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
            in_grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeaderBackColor;
            in_grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextColor;
            in_grid.RowHeadersDefaultCellStyle.BackColor = GridHeaderBackColor;
            in_grid.RowHeadersDefaultCellStyle.ForeColor = TextColor;
            in_grid.DefaultCellStyle.BackColor = WindowBackColor;
            in_grid.DefaultCellStyle.ForeColor = TextColor;
            in_grid.DefaultCellStyle.SelectionBackColor = GridSelectBackColor;
            in_grid.DefaultCellStyle.SelectionForeColor = GridSelectForeColor;
            in_grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltBackColor;
            in_grid.AlternatingRowsDefaultCellStyle.ForeColor = TextColor;
            in_grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = GridSelectBackColor;
            in_grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = GridSelectForeColor;
        }

        private static void ApplyListView(ListView in_list)
        {
            in_list.BackColor = WindowBackColor;
            in_list.ForeColor = TextColor;
            in_list.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void ApplyToolStrip(ToolStrip in_strip)
        {
            in_strip.BackColor = PanelBackColor;
            in_strip.ForeColor = TextColor;
            in_strip.RenderMode = ToolStripRenderMode.System;
            foreach (ToolStripItem w_item in in_strip.Items)
            {
                w_item.BackColor = PanelBackColor;
                w_item.ForeColor = TextColor;
            }
        }
    }
}
