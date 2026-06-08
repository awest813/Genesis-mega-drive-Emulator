using MDTracer;

namespace GenesisEmu.Frontend.Windows
{
    //----------------------------------------------------------------
    // Lists saved state captures (and optional input captures) for the
    // current ROM. Used by GenesisEmu.Game and MDTracer.
    //----------------------------------------------------------------
    public sealed class WinFormsCaptureListDialog : Form
    {
        private readonly CaptureListMode g_mode;
        private readonly ListView g_listView;
        private readonly Button g_loadButton;
        private readonly Button g_deleteButton;
        private readonly Button g_deleteAllButton;
        private readonly Button g_closeButton;
        private IReadOnlyList<CaptureListEntry> g_entries = Array.Empty<CaptureListEntry>();

        public CaptureListSelection? SelectedEntry { get; private set; }
        public event Action<CaptureListSelection>? EntrySelected;

        public WinFormsCaptureListDialog(CaptureListMode in_mode)
        {
            g_mode = in_mode;
            Text = in_mode == CaptureListMode.StateOnly ? "Save State History" : "Capture History";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(in_mode == CaptureListMode.StateOnly ? 720 : 860, 360);
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            g_listView = new ListView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(12, 12),
                Size = new Size(ClientSize.Width - 24, 270),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = false,
                UseCompatibleStateImageBehavior = false,
            };
            g_listView.Columns.Add("Saved", 170);
            g_listView.Columns.Add("State", 70, HorizontalAlignment.Center);
            if (in_mode == CaptureListMode.StateAndInput)
            {
                g_listView.Columns.Add("Input", 70, HorizontalAlignment.Center);
            }
            g_listView.Columns.Add("File", in_mode == CaptureListMode.StateOnly ? 430 : 360);
            g_listView.SelectedIndexChanged += (_, _) => UpdateButtons();
            g_listView.DoubleClick += LoadButton_Click;

            g_loadButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(12, 300),
                Size = new Size(90, 30),
                Text = in_mode == CaptureListMode.StateOnly ? "Load" : "Select",
            };
            g_loadButton.Click += LoadButton_Click;

            g_deleteButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(112, 300),
                Size = new Size(90, 30),
                Text = "Delete",
            };
            g_deleteButton.Click += DeleteButton_Click;

            g_deleteAllButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(212, 300),
                Size = new Size(90, 30),
                Text = "Delete All",
            };
            g_deleteAllButton.Click += DeleteAllButton_Click;

            g_closeButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 102, 300),
                Size = new Size(90, 30),
                Text = "Close",
                DialogResult = DialogResult.Cancel,
            };

            Controls.AddRange(new Control[]
            {
                g_listView, g_loadButton, g_deleteButton, g_deleteAllButton, g_closeButton,
            });
            AcceptButton = g_loadButton;
            CancelButton = g_closeButton;
            Load += (_, _) => RefreshList();
            WinFormsDebugTheme.Apply(this);
        }

        private void RefreshList()
        {
            g_entries = WinFormsCaptureList.BuildEntries(g_mode);
            g_listView.Items.Clear();

            foreach (CaptureListEntry w_entry in g_entries)
            {
                CaptureListSelection w_selection = w_entry.ToSelection();
                ListViewItem w_item = new ListViewItem(w_selection.DisplayName);
                w_item.SubItems.Add(w_selection.HasState ? "Yes" : "");
                if (g_mode == CaptureListMode.StateAndInput)
                {
                    w_item.SubItems.Add(w_selection.HasInput ? "Yes" : "");
                }
                w_item.SubItems.Add(w_selection.FileNameText);
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
            g_loadButton.Enabled = w_hasSelection;
            g_deleteButton.Enabled = w_hasSelection;
            g_deleteAllButton.Enabled = g_entries.Count > 0;
        }

        private CaptureListEntry? GetSelectedEntry()
        {
            if (g_listView.SelectedItems.Count <= 0) return null;
            return g_listView.SelectedItems[0].Tag as CaptureListEntry;
        }

        private void LoadButton_Click(object? sender, EventArgs e)
        {
            CaptureListEntry? w_entry = GetSelectedEntry();
            if (w_entry == null) return;
            SelectedEntry = w_entry.ToSelection();
            EntrySelected?.Invoke(SelectedEntry);
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            CaptureListEntry? w_entry = GetSelectedEntry();
            if (w_entry == null) return;

            string w_prompt = g_mode == CaptureListMode.StateOnly
                ? "Delete selected save state?"
                : "Delete selected capture?";
            if (MessageBox.Show(w_prompt, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            WinFormsCaptureList.DeleteEntry(w_entry);
            RefreshList();
        }

        private void DeleteAllButton_Click(object? sender, EventArgs e)
        {
            string w_prompt = g_mode == CaptureListMode.StateOnly
                ? "Delete all save states for this ROM?"
                : "Delete all captures for this ROM?";
            if (MessageBox.Show(w_prompt, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            WinFormsCaptureList.DeleteAll(g_mode);
            RefreshList();
        }
    }
}
