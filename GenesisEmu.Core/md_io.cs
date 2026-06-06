using SharpDX.DirectInput;
using System.Text;
namespace MDTracer
{
    //----------------------------------------------------------------
    //I/O Controller  : chips:315-5309
    //----------------------------------------------------------------
    internal partial class md_io
    {
        internal static IIoFrontendHooks g_frontendHooks = new NullIoFrontendHooks();

        public byte g_io_a10001_7_mode;
        public byte g_io_a10001_6_vmod;
        public byte g_io_a10001_5_disk;
        public byte g_io_a10001_3_ver;

        public byte g_io_a10003_data1;
        public byte g_io_a10005_data2;
        public byte g_io_a10007_data3;
        public byte g_io_a10009_ctrl1;
        public byte g_io_a1000b_ctrl2;
        public byte g_io_a1000d_ctrl3;
        public byte g_io_a1000f_txdata1;
        public byte g_io_a10011_rxdata1;
        public byte g_io_a10013_s_ctrl1;
        public byte g_io_a10015_txdata2;
        public byte g_io_a10017_rxdata2;
        public byte g_io_a10019_s_ctrl1;
        public byte g_io_a1001b_txdata3;
        public byte g_io_a1001d_rxdata3;
        public byte g_io_a1001f_s_ctrl3;

        public const int JOY_STATUS_NUM = 50;
        public const int KEY_STATUS_NUM = 256;
        public int KEY_ALLCATION_NUM = 12;

        public byte[] g_joy_status;
        public byte[] g_key_status;

        private SharpDX.DirectInput.Keyboard g_keyboard;
        public List<SharpDX.DirectInput.Joystick> g_joy_device;
        public List<string> g_joy_name_list;
        public int g_joy_device_cur;
        public string g_joy_name;
        private long g_joy_last_rescan_time;
        private int g_joy_rescan_in_progress;
        private readonly object g_joy_device_lock = new object();
        private readonly object g_joy_rescan_lock = new object();
        private const int JOY_RESCAN_INTERVAL_MS = 4000;
        public int[] g_key_allocation;
        public int[] g_joy_allocation;

        public int[] g_key_allocation2;
        public int[] g_joy_allocation2;
        private bool g_block_input_until_release;

        private int g_io_port1_th;
        private int g_io_port2_th;
        private int g_io_port1_phase;
        private int g_io_port2_phase;
        private long g_io_port1_last_th_change;
        private long g_io_port2_last_th_change;
        private const int IO_6BUTTON_TIMEOUT_MS = 12;
        private static void report_io_warning(string in_message)
        {
            System.Diagnostics.Debug.WriteLine("[IO] " + in_message);
        }
        //----------------------------------------------------------------
        public md_io()
        {
            g_io_a10001_7_mode = 1;
            g_io_a10001_6_vmod = 0;
            g_io_a10001_5_disk = 1;
            g_io_a10001_3_ver = 0;

            g_io_a10003_data1 = 0xff;
            g_io_a10005_data2 = 0xff;
            g_io_a10007_data3 = 0xff;
            g_io_a10011_rxdata1 = 0;
            g_io_a10017_rxdata2 = 0;
            g_io_a1001d_rxdata3 = 0;

            g_joy_name_list = new List<string>();
            g_joy_device = new List<Joystick>();
            g_joy_status = new byte[JOY_STATUS_NUM];
            g_key_status = new byte[KEY_STATUS_NUM];
            g_joy_allocation = new int[KEY_ALLCATION_NUM];
            g_joy_allocation2 = new int[KEY_ALLCATION_NUM];
            g_key_allocation = new int[KEY_ALLCATION_NUM];
            g_key_allocation2 = new int[KEY_ALLCATION_NUM];
            Array.Fill(g_joy_allocation, -1);
            Array.Fill(g_joy_allocation2, -1);
            g_io_port1_th = 1;
            g_io_port2_th = 1;
        }

        private bool is_key_down(int[] in_key_allocation, int in_index)
        {
            if (in_index < 0 || in_index >= in_key_allocation.Length) return false;
            int w_key = in_key_allocation[in_index];
            return w_key >= 0 && w_key < g_key_status.Length && g_key_status[w_key] == 1;
        }

        private bool is_joy_down(int[] in_joy_allocation, int in_index)
        {
            if (in_index < 0 || in_index >= in_joy_allocation.Length) return false;
            int w_joy = in_joy_allocation[in_index];
            return w_joy >= 0 && w_joy < g_joy_status.Length && g_joy_status[w_joy] == 1;
        }

        private bool is_button_down(int[] in_key_allocation, int[] in_joy_allocation, int in_index)
        {
            return is_key_down(in_key_allocation, in_index) || is_joy_down(in_joy_allocation, in_index);
        }

        public void block_input_until_release()
        {
            g_block_input_until_release = true;
            Array.Clear(g_key_status, 0, g_key_status.Length);
            Array.Clear(g_joy_status, 0, g_joy_status.Length);
        }

        private byte read_pad(int[] in_key_allocation, int[] in_joy_allocation, byte in_data, byte in_ctrl, int in_phase)
        {
            byte w_out;
            bool w_th = (in_data & 0x40) != 0;

            if (!w_th)
            {
                w_out = (byte)((in_phase == 3) ? 0x00 : 0x33);
                if (in_phase != 3)
                {
                    if (is_button_down(in_key_allocation, in_joy_allocation, 3)) w_out &= 0xdf;  //START
                    if (is_button_down(in_key_allocation, in_joy_allocation, 0)) w_out &= 0xef;  //A
                    if (is_button_down(in_key_allocation, in_joy_allocation, 5)) w_out &= 0xfd;  //DOWN
                    if (is_button_down(in_key_allocation, in_joy_allocation, 4)) w_out &= 0xfe;  //UP
                }
            }
            else
            {
                w_out = 0x7f;
                if (in_phase < 3)
                {
                    if (is_button_down(in_key_allocation, in_joy_allocation, 2)) w_out &= 0xdf;  //C
                    if (is_button_down(in_key_allocation, in_joy_allocation, 1)) w_out &= 0xef;  //B
                    if (is_button_down(in_key_allocation, in_joy_allocation, 7)) w_out &= 0xf7;  //RIGHT
                    if (is_button_down(in_key_allocation, in_joy_allocation, 6)) w_out &= 0xfb;  //LEFT
                    if (is_button_down(in_key_allocation, in_joy_allocation, 5)) w_out &= 0xfd;  //DOWN
                    if (is_button_down(in_key_allocation, in_joy_allocation, 4)) w_out &= 0xfe;  //UP
                }
                else
                {
                    if (is_button_down(in_key_allocation, in_joy_allocation, 11)) w_out &= 0xf7;  //MODE
                    if (is_button_down(in_key_allocation, in_joy_allocation, 8)) w_out &= 0xfb;   //X
                    if (is_button_down(in_key_allocation, in_joy_allocation, 9)) w_out &= 0xfd;   //Y
                    if (is_button_down(in_key_allocation, in_joy_allocation, 10)) w_out &= 0xfe;  //Z
                }
            }

            return (byte)((w_out & ~in_ctrl) | (in_data & in_ctrl));
        }

        private void update_th_phase(byte in_data, ref int io_th, ref int io_phase, ref long io_last_th_change)
        {
            int w_th = ((in_data & 0x40) != 0) ? 1 : 0;
            if (w_th == io_th) return;

            long w_now = Environment.TickCount64;
            if (w_now - io_last_th_change > IO_6BUTTON_TIMEOUT_MS)
            {
                io_phase = 0;
            }
            io_last_th_change = w_now;

            if (io_th == 1 && w_th == 0)
            {
                io_phase++;
                if (io_phase > 3)
                {
                    io_phase = 0;
                }
            }
            io_th = w_th;
        }

        private void reset_th_phase_if_timeout(ref int io_phase, long io_last_th_change)
        {
            if (io_phase != 0 && Environment.TickCount64 - io_last_th_change > IO_6BUTTON_TIMEOUT_MS)
            {
                io_phase = 0;
            }
        }

        //----------------------------------------------------------------
        //read
        //----------------------------------------------------------------
        public byte read8(uint in_address)
        {
            byte w_out = 0; 
            if (in_address == 0xa10001)
            {
                if (md_main.g_md_cartridge.g_country.Contains('J'))
                {
                    g_io_a10001_7_mode = 0;
                }
                else
                {
                    g_io_a10001_7_mode = 1;
                }
                w_out  = (byte)(g_io_a10001_7_mode << 7);
                w_out |= (byte)(g_io_a10001_6_vmod << 6);
                w_out |= (byte)(g_io_a10001_5_disk << 5);
                w_out |= g_io_a10001_3_ver;
            }
            else if (in_address == 0xa10003)
            {
                reset_th_phase_if_timeout(ref g_io_port1_phase, g_io_port1_last_th_change);
                w_out = read_pad(g_key_allocation, g_joy_allocation, g_io_a10003_data1, g_io_a10009_ctrl1, g_io_port1_phase);
            }
            else if (in_address == 0xa10005)
            {
                reset_th_phase_if_timeout(ref g_io_port2_phase, g_io_port2_last_th_change);
                w_out = read_pad(g_key_allocation2, g_joy_allocation2, g_io_a10005_data2, g_io_a1000b_ctrl2, g_io_port2_phase);
            }
            //w_out = g_io_a10005_data2;
            else if (in_address == 0xa10007) w_out = g_io_a10007_data3;
            else if (in_address == 0xa10009) w_out = g_io_a10009_ctrl1;
            else if (in_address == 0xa1000b) w_out = g_io_a1000b_ctrl2;
            else if (in_address == 0xa1000d) w_out = g_io_a1000d_ctrl3;
            else if (in_address == 0xa1000f) w_out = g_io_a1000f_txdata1;
            else if (in_address == 0xa10011) w_out = g_io_a10011_rxdata1;
            else if (in_address == 0xa10013) w_out = g_io_a10013_s_ctrl1;
            else if (in_address == 0xa10015) w_out = g_io_a10015_txdata2;
            else if (in_address == 0xa10017) w_out = g_io_a10017_rxdata2;
            else if (in_address == 0xa10019) w_out = g_io_a10019_s_ctrl1;
            else if (in_address == 0xa1001b) w_out = g_io_a1001b_txdata3;
            else if (in_address == 0xa1001d) w_out = g_io_a1001d_rxdata3;
            else if (in_address == 0xa1001f) w_out = g_io_a1001f_s_ctrl3;
            else
            {
                report_io_warning("md_io.read8");
            }
            return w_out;
        }
        public ushort read16(uint in_address)
        {
            return read8(in_address + 1);
        }
        public uint read32(uint in_address)
        {
            ushort w_data = (ushort)((read8(in_address + 1) << 8)
                                    + read8(in_address + 3));
            return w_data;
        }
        //----------------------------------------------------------------
        //write
        //----------------------------------------------------------------
        public void write8(uint in_address, byte in_data)
        {
            if (in_address == 0xa10003)
            {
                update_th_phase(in_data, ref g_io_port1_th, ref g_io_port1_phase, ref g_io_port1_last_th_change);
                g_io_a10003_data1 = in_data;
            }
            else if (in_address == 0xa10005)
            {
                update_th_phase(in_data, ref g_io_port2_th, ref g_io_port2_phase, ref g_io_port2_last_th_change);
                g_io_a10005_data2 = in_data;
            }
            else if (in_address == 0xa10007) g_io_a10007_data3 = in_data;
            else if (in_address == 0xa10009) g_io_a10009_ctrl1 = in_data;
            else if (in_address == 0xa1000b) g_io_a1000b_ctrl2 = in_data;
            else if (in_address == 0xa1000d) g_io_a1000d_ctrl3 = in_data;
            else if (in_address == 0xa1000f) g_io_a1000f_txdata1 = in_data;
            else if (in_address == 0xa10013) g_io_a10013_s_ctrl1 = in_data;
            else if (in_address == 0xa10015) g_io_a10015_txdata2 = in_data;
            else if (in_address == 0xa10019) g_io_a10019_s_ctrl1 = in_data;
            else if (in_address == 0xa1001b) g_io_a1001b_txdata3 = in_data;
            else if (in_address == 0xa1001f) g_io_a1001f_s_ctrl3 = in_data;
            else
            {
                report_io_warning("md_io.write8");
            }
        }
        public void write16(uint in_address, ushort in_data)
        {
            write8(in_address + 1, (byte)(in_data & 0xff));
        }
        public void write32(uint in_address, uint in_data)
        {
            write16(in_address, (ushort)(in_data >> 16));
            write16(in_address + 2, (ushort)(in_data & 0xffff));
        }
    }
}
