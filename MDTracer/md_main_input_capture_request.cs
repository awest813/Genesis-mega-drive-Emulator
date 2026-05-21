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
            g_state_capture_status = "input recording: " + Path.GetFileName(w_filePath);
        }

        public static void input_capture_record_stop()
        {
            string w_filePath = g_md_io.g_input_record_path;
            g_md_io.input_record_stop();
            InputRecordStore.DeleteOldEntriesOverLimit();
            g_state_capture_status = string.IsNullOrEmpty(w_filePath) == true
                ? "input recording stopped"
                : "input recorded: " + Path.GetFileName(w_filePath);
        }

        public static void input_capture_restore_latest()
        {
            InputRecordEntry? w_entry = InputRecordStore.GetLatestEntry();
            if (w_entry == null)
            {
                g_state_capture_status = "input record is empty";
                return;
            }

            g_md_io.input_replay_start(w_entry.FilePath);
            g_state_capture_status = "input replay: " + Path.GetFileName(w_entry.FilePath);
        }

        public static void input_capture_replay_file(string in_filePath)
        {
            g_md_io.input_replay_start(in_filePath);
            g_state_capture_status = "input replay: " + Path.GetFileName(in_filePath);
        }

        public static void input_capture_replay_stop()
        {
            string w_filePath = g_md_io.g_input_replay_path;
            g_md_io.input_replay_stop();
            g_state_capture_status = string.IsNullOrEmpty(w_filePath) == true
                ? "input replay stopped"
                : "input replay stopped: " + Path.GetFileName(w_filePath);
        }
    }
}
