using System.IO;

namespace MDTracer
{
    internal partial class md_main
    {
        public static void input_capture_record_start()
        {
            if (InputRecordStore.IsAvailable() == false) return;

            string w_filePath = InputRecordStore.CreateNewFilePath();
            g_md_io.input_record_start(w_filePath);
        }

        public static void input_capture_record_stop()
        {
            g_md_io.input_record_stop();
            InputRecordStore.DeleteOldEntriesOverLimit();
        }

        public static void input_capture_restore_latest()
        {
            InputRecordEntry? w_entry = InputRecordStore.GetLatestEntry();
        }

        public static void input_capture_replay_file(string in_filePath)
        {
            g_md_io.input_replay_start(in_filePath);
        }

        public static void input_capture_replay_stop()
        {
            g_md_io.input_replay_stop();
        }
    }
}
