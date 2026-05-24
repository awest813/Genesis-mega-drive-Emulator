using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MDTracer
{
    internal sealed class Form_Main_Capture_list : Form
    {
        private readonly ListView g_listView;
        private readonly Button g_executeButton;
        private readonly Button g_deleteButton;
        private readonly Button g_deleteAllButton;
        private readonly Button g_closeButton;
        private IReadOnlyList<CaptureListEntry> g_entries = Array.Empty<CaptureListEntry>();

        public CaptureListEntry? SelectedEntry { get; private set; }
        public event Action<CaptureListEntry>? EntrySelected;

        public Form_Main_Capture_list()
        {
            Text = "Capture History";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(860, 360);
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            g_listView = new ListView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(12, 12),
                Size = new Size(836, 270),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = false,
                UseCompatibleStateImageBehavior = false
            };
            g_listView.Columns.Add("Timestamp", 170);
            g_listView.Columns.Add("State Capture", 100, HorizontalAlignment.Center);
            g_listView.Columns.Add("Input Capture", 100, HorizontalAlignment.Center);
            g_listView.Columns.Add("File Name", 360);
            g_listView.SelectedIndexChanged += ListView_SelectedIndexChanged;
            g_listView.DoubleClick += ExecuteButton_Click;

            g_executeButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(12, 300),
                Size = new Size(90, 30),
                Text = "Select"
            };
            g_executeButton.Click += ExecuteButton_Click;

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
                Location = new Point(758, 300),
                Size = new Size(90, 30),
                Text = "Close",
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[] { g_listView, g_executeButton, g_deleteButton, g_deleteAllButton, g_closeButton });
            AcceptButton = g_executeButton;
            CancelButton = g_closeButton;
            Load += Form_CaptureHistory_Load;
        }

        private void Form_CaptureHistory_Load(object? sender, EventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            Dictionary<string, CaptureListEntry> w_entries = new Dictionary<string, CaptureListEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (md_main.StateListEntry w_stateEntry in md_main.StateStore.GetEntries())
            {
                string w_timestamp = Path.GetFileNameWithoutExtension(w_stateEntry.FilePath);
                if (w_entries.TryGetValue(w_timestamp, out CaptureListEntry? w_entry) == false)
                {
                    w_entry = new CaptureListEntry(w_timestamp);
                    w_entries.Add(w_timestamp, w_entry);
                }
                w_entry.StateEntry = w_stateEntry;
            }

            foreach (md_main.InputRecordEntry w_inputEntry in md_main.InputRecordStore.GetEntries())
            {
                string w_timestamp = Path.GetFileNameWithoutExtension(w_inputEntry.FilePath);
                if (w_entries.TryGetValue(w_timestamp, out CaptureListEntry? w_entry) == false)
                {
                    w_entry = new CaptureListEntry(w_timestamp);
                    w_entries.Add(w_timestamp, w_entry);
                }
                w_entry.InputEntry = w_inputEntry;
            }

            g_entries = w_entries.Values
                .OrderByDescending(in_entry => in_entry.Timestamp, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            g_listView.Items.Clear();
            foreach (CaptureListEntry w_entry in g_entries)
            {
                ListViewItem w_item = new ListViewItem(w_entry.Timestamp);
                w_item.SubItems.Add(w_entry.StateEntry == null ? "" : "〇");
                w_item.SubItems.Add(w_entry.InputEntry == null ? "" : "〇");
                w_item.SubItems.Add(w_entry.FileNameText);
                w_item.Tag = w_entry;
                g_listView.Items.Add(w_item);
            }

            if (g_listView.Items.Count > 0)
            {
                g_listView.Items[0].Selected = true;
            }

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool w_hasSelection = GetSelectedEntry() != null;
            g_executeButton.Enabled = w_hasSelection;
            g_deleteButton.Enabled = w_hasSelection;
            g_deleteAllButton.Enabled = g_entries.Count > 0;
        }

        private void ListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
        }

        private CaptureListEntry? GetSelectedEntry()
        {
            if (g_listView.SelectedItems.Count <= 0) return null;
            return g_listView.SelectedItems[0].Tag as CaptureListEntry;
        }

        private void ExecuteButton_Click(object? sender, EventArgs e)
        {
            SelectedEntry = GetSelectedEntry();
            if (SelectedEntry == null) return;

            EntrySelected?.Invoke(SelectedEntry);
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            CaptureListEntry? w_entry = GetSelectedEntry();
            if (w_entry == null) return;

            if (MessageBox.Show("Delete selected capture?", "Capture History", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            DeleteEntry(w_entry);
            RefreshList();
        }

        private void DeleteAllButton_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Delete all captures?", "Capture History", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            md_main.StateStore.DeleteAll();
            md_main.InputRecordStore.DeleteAll();
            RefreshList();
        }

        private static void DeleteEntry(CaptureListEntry in_entry)
        {
            if (in_entry.StateEntry != null)
            {
                md_main.StateStore.Delete(in_entry.StateEntry);
            }
            if (in_entry.InputEntry != null)
            {
                md_main.InputRecordStore.Delete(in_entry.InputEntry);
            }
        }
    }

    internal sealed class CaptureListEntry
    {
        public CaptureListEntry(string timestamp)
        {
            Timestamp = timestamp;
        }

        public string Timestamp { get; }
        public md_main.StateListEntry? StateEntry { get; set; }
        public md_main.InputRecordEntry? InputEntry { get; set; }
        public string FileNameText
        {
            get
            {
                string w_stateName = StateEntry == null ? "" : Path.GetFileName(StateEntry.FilePath);
                string w_inputName = InputEntry == null ? "" : Path.GetFileName(InputEntry.FilePath);
                if (string.IsNullOrEmpty(w_stateName) == true) return w_inputName;
                if (string.IsNullOrEmpty(w_inputName) == true) return w_stateName;
                return w_stateName + " / " + w_inputName;
            }
        }
    }
}
