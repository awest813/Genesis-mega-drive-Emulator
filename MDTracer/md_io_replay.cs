using System.Text;

namespace MDTracer
{
    internal partial class md_io
    {
        private const string INPUT_RECORD_MAGIC = "MDTINPUT1";
        private FileStream g_input_record_stream;
        private FileStream g_input_replay_stream;

        public bool g_input_recording;
        public bool g_input_replaying;
        public string g_input_record_path;
        public string g_input_replay_path;

        public void input_record_start(string in_path)
        {
            input_record_stop();
            input_replay_stop();

            g_input_record_stream = new FileStream(in_path, FileMode.Create, FileAccess.Write, FileShare.Read);
            byte[] w_header = Encoding.ASCII.GetBytes(INPUT_RECORD_MAGIC + "\n");
            g_input_record_stream.Write(w_header, 0, w_header.Length);
            g_input_recording = true;
            g_input_record_path = in_path;
        }

        public void input_record_stop()
        {
            g_input_recording = false;
            if (g_input_record_stream != null)
            {
                g_input_record_stream.Flush();
                g_input_record_stream.Dispose();
                g_input_record_stream = null;
            }
        }

        public void input_replay_start(string in_path)
        {
            input_replay_stop();
            input_record_stop();

            g_input_replay_stream = new FileStream(in_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] w_header = new byte[INPUT_RECORD_MAGIC.Length + 1];
            int w_read = g_input_replay_stream.Read(w_header, 0, w_header.Length);
            string w_magic = Encoding.ASCII.GetString(w_header, 0, w_read).TrimEnd('\n', '\r');
            if (w_magic != INPUT_RECORD_MAGIC)
            {
                g_input_replay_stream.Dispose();
                g_input_replay_stream = null;
                throw new InvalidDataException("Input record file format is invalid.");
            }

            Array.Clear(g_key_status, 0, g_key_status.Length);
            Array.Clear(g_joy_status, 0, g_joy_status.Length);
            g_input_replaying = true;
            g_input_replay_path = in_path;
        }

        public void input_replay_stop()
        {
            g_input_replaying = false;
            if (g_input_replay_stream != null)
            {
                g_input_replay_stream.Dispose();
                g_input_replay_stream = null;
            }
            Array.Clear(g_key_status, 0, g_key_status.Length);
            Array.Clear(g_joy_status, 0, g_joy_status.Length);
        }

        public void input_update_frame()
        {
            if (g_input_replaying)
            {
                if (!input_replay_frame())
                {
                    input_replay_stop();
                }
                return;
            }

            if (md_main.is_clock_wait_skip() == true)
            {
                Array.Clear(g_key_status, 0, g_key_status.Length);
                Array.Clear(g_joy_status, 0, g_joy_status.Length);
                return;
            }

            if (g_block_input_until_release == true)
            {
                Array.Clear(g_key_status, 0, g_key_status.Length);
                Array.Clear(g_joy_status, 0, g_joy_status.Length);
                if (read_device_keyboard_for_setting() == -1 && read_device_joystick_for_setting() == -1)
                {
                    g_block_input_until_release = false;
                }
                return;
            }

            read_device_keyboard();
            read_device_joystick();

            if (g_input_recording)
            {
                input_record_frame();
            }
        }

        private void input_record_frame()
        {
            g_input_record_stream.Write(g_key_status, 0, g_key_status.Length);
            g_input_record_stream.Write(g_joy_status, 0, g_joy_status.Length);
        }

        private bool input_replay_frame()
        {
            return read_exact(g_input_replay_stream, g_key_status, g_key_status.Length)
                && read_exact(g_input_replay_stream, g_joy_status, g_joy_status.Length);
        }

        private bool read_exact(Stream in_stream, byte[] in_buffer, int in_size)
        {
            int w_offset = 0;
            while (w_offset < in_size)
            {
                int w_read = in_stream.Read(in_buffer, w_offset, in_size - w_offset);
                if (w_read <= 0)
                {
                    return false;
                }
                w_offset += w_read;
            }
            return true;
        }
    }
}
