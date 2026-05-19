namespace MDTracer
{
    internal partial class md_main
    {
        private static volatile bool g_stop_req;
        private static volatile bool g_frame_advance_req;
        private static volatile bool g_frame_advance_update_req;
        private static readonly object g_state_capture_request_lock = new object();
        private static bool g_state_capture_save_req;
        private static bool g_state_capture_restore_latest_req;
        private static string g_state_capture_restore_file_req = "";
        public static string g_state_capture_rom_file_name = "";
        public static string g_state_capture_status = "";
        //----------------------------------------------------------------
        public static void request_state_capture_save()
        {
            lock (g_state_capture_request_lock)
            {
                g_state_capture_save_req = true;
            }
        }
        public static void request_state_capture_restore_latest()
        {
            lock (g_state_capture_request_lock)
            {
                g_state_capture_restore_latest_req = true;
            }
        }
        public static void request_state_capture_restore_file(string in_file)
        {
            lock (g_state_capture_request_lock)
            {
                g_state_capture_restore_file_req = in_file;
            }
        }
        public static bool request_stop()
        {
            g_stop_req = !g_stop_req;
            g_frame_advance_req = false;
            g_frame_advance_update_req = false;
            g_md_vdp?.g_waitHandle?.Set();
            return g_stop_req;
        }
        public static bool request_frame_advance()
        {
            g_stop_req = true;
            g_frame_advance_req = true;
            g_md_vdp?.g_waitHandle?.Set();
            return true;
        }
        public static void request_hard_reset()
        {
            g_hard_reset_req = true;
            g_frame_advance_update_req = false;
            if (g_stop_req == true)
            {
                g_frame_advance_req = true;
            }
            g_md_vdp?.g_waitHandle?.Set();
        }
        public static bool is_stop_requested()
        {
            return g_stop_req;
        }
        private static bool consume_frame_advance_request()
        {
            if (g_frame_advance_req == false) return false;

            g_frame_advance_req = false;
            return true;
        }
        private static void request_frame_advance_update()
        {
            g_frame_advance_update_req = true;
        }
        public static bool consume_frame_advance_update_request()
        {
            if (g_frame_advance_update_req == false) return false;

            g_frame_advance_update_req = false;
            return true;
        }

        private static void process_state_capture_request()
        {
            bool w_save_req;
            bool w_restore_latest_req;
            string w_restore_file_req;
            lock (g_state_capture_request_lock)
            {
                w_save_req = g_state_capture_save_req;
                w_restore_latest_req = g_state_capture_restore_latest_req;
                w_restore_file_req = g_state_capture_restore_file_req;

                g_state_capture_save_req = false;
                g_state_capture_restore_latest_req = false;
                g_state_capture_restore_file_req = "";
            }

            if (w_save_req == true)
            {
                try
                {
                    StateListEntry w_entry = StateStore.Save();
                    g_state_capture_status = "state saved: " + Path.GetFileName(w_entry.FilePath);
                }
                catch (Exception ex)
                {
                    g_state_capture_status = "state save error: " + ex.Message;
                }
            }

            if (w_restore_latest_req == true)
            {
                try
                {
                    StateListEntry? w_entry = StateStore.GetLatestEntry();
                    if (w_entry == null)
                    {
                        g_state_capture_status = "state capture is empty";
                    }
                    else
                    {
                        StateStore.Restore(w_entry);
                        g_state_capture_status = "state restored: " + Path.GetFileName(w_entry.FilePath);
                    }
                }
                catch (Exception ex)
                {
                    g_state_capture_status = "state restore error: " + ex.Message;
                }
            }

            if (string.IsNullOrEmpty(w_restore_file_req) == false)
            {
                try
                {
                    StateListEntry w_entry = new StateListEntry(w_restore_file_req);
                    StateStore.Restore(w_entry);
                    g_state_capture_status = "state restored: " + Path.GetFileName(w_entry.FilePath);
                }
                catch (Exception ex)
                {
                    g_state_capture_status = "state restore error: " + ex.Message;
                }
            }
        }

    }
}
