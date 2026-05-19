using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace MDTracer
{
    internal sealed class Form_Main_InputCapture_list : Form
    {
        private readonly ListView g_listView;
        private readonly Button g_replayButton;
        private readonly Button g_deleteButton;
        private readonly Button g_deleteAllButton;
        private readonly Button g_closeButton;
        private IReadOnlyList<md_main.InputRecordEntry> g_entries = Array.Empty<md_main.InputRecordEntry>();
        private int g_sortColumn = 0;
        private bool g_sortAscending = false;

        public md_main.InputRecordEntry? SelectedEntry { get; private set; }

        public Form_Main_InputCapture_list()
        {
            Text = "Input Record History";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(720, 360);
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            g_listView = new ListView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(12, 12),
                Size = new Size(696, 270),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = false,
                UseCompatibleStateImageBehavior = false
            };
            g_listView.Columns.Add("Created", 170);
            g_listView.Columns.Add("File Name", 500);
            g_listView.ListViewItemSorter = new ListViewTextComparer(g_sortColumn, g_sortAscending);
            g_listView.ColumnClick += ListView_ColumnClick;
            g_listView.SelectedIndexChanged += ListView_SelectedIndexChanged;
            g_listView.DoubleClick += ReplayButton_Click;

            g_replayButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(12, 300),
                Size = new Size(90, 30),
                Text = "Replay"
            };
            g_replayButton.Click += ReplayButton_Click;

            g_deleteButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(112, 300),
                Size = new Size(90, 30),
                Text = "Delete"
            };
            g_deleteButton.Click += DeleteButton_Click;

            g_deleteAllButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(212, 300),
                Size = new Size(90, 30),
                Text = "Delete All"
            };
            g_deleteAllButton.Click += DeleteAllButton_Click;

            g_closeButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(618, 300),
                Size = new Size(90, 30),
                Text = "Close",
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[] { g_listView, g_replayButton, g_deleteButton, g_deleteAllButton, g_closeButton });
            AcceptButton = g_replayButton;
            CancelButton = g_closeButton;
            Load += Form_InputRecordHistory_Load;
        }

        private void Form_InputRecordHistory_Load(object? sender, EventArgs e)
        {
            RefreshList(null);
        }

        private void RefreshList(string? in_selectFilePath)
        {
            g_entries = md_main.InputRecordStore.GetEntries();
            g_listView.Items.Clear();

            foreach (md_main.InputRecordEntry w_entry in g_entries)
            {
                ListViewItem w_item = new ListViewItem(w_entry.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                w_item.SubItems.Add(Path.GetFileName(w_entry.FilePath));
                w_item.Tag = w_entry;
                g_listView.Items.Add(w_item);
                if (string.Equals(w_entry.FilePath, in_selectFilePath, StringComparison.OrdinalIgnoreCase) == true)
                {
                    w_item.Selected = true;
                }
            }
            g_listView.Sort();

            if (g_listView.SelectedItems.Count == 0 && g_listView.Items.Count > 0)
            {
                g_listView.Items[0].Selected = true;
            }

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool w_hasSelection = GetSelectedEntry() != null;
            g_replayButton.Enabled = w_hasSelection;
            g_deleteButton.Enabled = w_hasSelection;
            g_deleteAllButton.Enabled = g_entries.Count > 0;
        }

        private void ListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void ListView_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            if (g_sortColumn == e.Column)
            {
                g_sortAscending = !g_sortAscending;
            }
            else
            {
                g_sortColumn = e.Column;
                g_sortAscending = true;
            }
            g_listView.ListViewItemSorter = new ListViewTextComparer(g_sortColumn, g_sortAscending);
            g_listView.Sort();
        }

        private md_main.InputRecordEntry? GetSelectedEntry()
        {
            if (g_listView.SelectedItems.Count <= 0) return null;
            return g_listView.SelectedItems[0].Tag as md_main.InputRecordEntry;
        }

        private void ReplayButton_Click(object? sender, EventArgs e)
        {
            SelectedEntry = GetSelectedEntry();
            if (SelectedEntry == null) return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            md_main.InputRecordEntry? w_entry = GetSelectedEntry();
            if (w_entry == null) return;

            if (MessageBox.Show("Delete selected input record?", "Input Record History", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            md_main.InputRecordStore.Delete(w_entry);
            RefreshList(null);
        }

        private void DeleteAllButton_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Delete all input records?", "Input Record History", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            md_main.InputRecordStore.DeleteAll();
            RefreshList(null);
        }

    }

    internal sealed class ListViewTextComparer : IComparer
    {
        private readonly int g_column;
        private readonly bool g_ascending;

        public ListViewTextComparer(int in_column, bool in_ascending)
        {
            g_column = in_column;
            g_ascending = in_ascending;
        }

        public int Compare(object? x, object? y)
        {
            ListViewItem? w_itemX = x as ListViewItem;
            ListViewItem? w_itemY = y as ListViewItem;
            string w_textX = GetText(w_itemX, g_column);
            string w_textY = GetText(w_itemY, g_column);
            int w_result = string.Compare(w_textX, w_textY, StringComparison.CurrentCultureIgnoreCase);
            return g_ascending ? w_result : -w_result;
        }

        private static string GetText(ListViewItem? in_item, int in_column)
        {
            if (in_item == null) return "";
            if (in_column < 0 || in_column >= in_item.SubItems.Count) return "";
            return in_item.SubItems[in_column].Text;
        }
    }
}
