using MDTracer;

namespace GenesisEmu.Frontend.Windows
{
    public enum CaptureListMode
    {
        StateOnly,
        StateAndInput,
    }

    public sealed class CaptureListSelection
    {
        public string Timestamp { get; init; } = "";
        public string? StateFilePath { get; init; }
        public string? InputFilePath { get; init; }
        public string DisplayName { get; init; } = "";
        public string FileNameText { get; init; } = "";
        public bool HasState => string.IsNullOrEmpty(StateFilePath) == false;
        public bool HasInput => string.IsNullOrEmpty(InputFilePath) == false;
    }

    internal sealed class CaptureListEntry
    {
        public CaptureListEntry(string in_timestamp)
        {
            Timestamp = in_timestamp;
        }

        public string Timestamp { get; }
        public md_main.StateListEntry? StateEntry { get; set; }
        public md_main.InputRecordEntry? InputEntry { get; set; }

        public CaptureListSelection ToSelection()
        {
            return new CaptureListSelection
            {
                Timestamp = Timestamp,
                StateFilePath = StateEntry?.FilePath,
                InputFilePath = InputEntry?.FilePath,
                DisplayName = StateEntry?.DisplayName ?? Timestamp,
                FileNameText = BuildFileNameText(),
            };
        }

        private string BuildFileNameText()
        {
            string w_stateName = StateEntry == null ? "" : Path.GetFileName(StateEntry.FilePath);
            string w_inputName = InputEntry == null ? "" : Path.GetFileName(InputEntry.FilePath);
            if (string.IsNullOrEmpty(w_stateName) == true) return w_inputName;
            if (string.IsNullOrEmpty(w_inputName) == true) return w_stateName;
            return w_stateName + " / " + w_inputName;
        }
    }

    internal static class WinFormsCaptureList
    {
        public static IReadOnlyList<CaptureListEntry> BuildEntries(CaptureListMode in_mode)
        {
            Dictionary<string, CaptureListEntry> w_entries =
                new Dictionary<string, CaptureListEntry>(StringComparer.OrdinalIgnoreCase);

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

            if (in_mode == CaptureListMode.StateAndInput)
            {
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
            }

            return w_entries.Values
                .OrderByDescending(in_entry => in_entry.Timestamp, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static void DeleteEntry(CaptureListEntry in_entry)
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

        public static void DeleteAll(CaptureListMode in_mode)
        {
            md_main.StateStore.DeleteAll();
            if (in_mode == CaptureListMode.StateAndInput)
            {
                md_main.InputRecordStore.DeleteAll();
            }
        }
    }
}
