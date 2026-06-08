using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace MDTracer
{
    internal partial class md_main
    {
        public static List<string> g_setting_name;
        public static List<string> g_setting_val;

        public static Configuration g_config;
        public static int g_tvmode_req;
        public static int g_gpu_req;
        private const string GAME_SETTING_PREFIX = "game_";

        public static string get_game_setting(string in_rom_file_name, string in_suffix)
        {
            string w_key = get_game_setting_key(in_rom_file_name, in_suffix);
            if (string.IsNullOrEmpty(w_key) == true) return "";

            return ConfigurationManager.AppSettings[w_key] ?? "";
        }

        public static void set_game_setting(string in_rom_file_name, string in_suffix, string in_val)
        {
            string w_key = get_game_setting_key(in_rom_file_name, in_suffix);
            if (string.IsNullOrEmpty(w_key) == true) return;

            Configuration w_config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection w_settings = w_config.AppSettings.Settings;
            if (string.IsNullOrWhiteSpace(in_val) == true)
            {
                if (w_settings.AllKeys.Contains(w_key) == true)
                {
                    w_settings.Remove(w_key);
                }
            }
            else if (w_settings.AllKeys.Contains(w_key) == true)
            {
                w_settings[w_key].Value = in_val;
            }
            else
            {
                w_settings.Add(w_key, in_val);
            }
            w_config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static string get_game_setting_key(string in_rom_file_name, string in_suffix)
        {
            string w_file_name = Path.GetFileName(in_rom_file_name ?? "").Trim();
            string w_suffix = (in_suffix ?? "").Trim();
            if ((string.IsNullOrEmpty(w_file_name) == true) || (string.IsNullOrEmpty(w_suffix) == true)) return "";

            string w_token = Convert.ToBase64String(Encoding.UTF8.GetBytes(w_file_name))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return GAME_SETTING_PREFIX + w_token + "_" + w_suffix;
        }

        public static void read_setting()
        {
            g_setting_name.Clear();
            g_setting_val.Clear();
            foreach (string key in System.Configuration.ConfigurationSettings.AppSettings.AllKeys)
            {
                g_setting_name.Add(key);
                g_setting_val.Add(System.Configuration.ConfigurationSettings.AppSettings[key]);
            }
            if(g_setting_name.Count == 0)
            {
                read_init();
                g_frontendSettings.EnsureCodeToolLayoutVisible();
                return;
            }
            for (int i = 0; i < g_setting_name.Count; i++)
            {
                string w_name = g_setting_name[i];
                string[] w_val = g_setting_val[i].Split(':');
                if (g_frontendSettings.TryApplySetting(w_name, g_setting_val[i]) == true)
                {
                    continue;
                }
                switch(w_name)
                {
                    case "key":
                        for (int j = 0; j < g_md_io.KEY_ALLCATION_NUM && j < w_val.Length; j++)
                        {
                            g_md_io.g_key_allocation[j] = byte.Parse(w_val[j]);
                        }
                        break;
                    case "joyname":
                        g_md_io.g_joy_name = w_val[0];
                        break;
                    case "joy":
                        for (int j = 0; j < g_md_io.KEY_ALLCATION_NUM && j < w_val.Length; j++)
                        {
                            g_md_io.g_joy_allocation[j] = int.Parse(w_val[j]);
                        }
                        break;
                    case "joy2":
                        for (int j = 0; j < g_md_io.KEY_ALLCATION_NUM && j < w_val.Length; j++)
                        {
                            g_md_io.g_joy_allocation2[j] = int.Parse(w_val[j]);
                        }
                        break;
                    case "key2":
                        for (int j = 0; j < g_md_io.KEY_ALLCATION_NUM && j < w_val.Length; j++)
                        {
                            g_md_io.g_key_allocation2[j] = byte.Parse(w_val[j]);
                        }
                        break;
                    case "trace_fsb":
                        g_debugView.trace_fsb = (w_val[0] == "1") ? true : false;
                        break;
                    case "trace_sip":
                        g_debugView.trace_sip = (w_val[0] == "1") ? true : false;
                        break;
                    case "music_chk":
                        for (int j = 0; j <= 10; j++)
                        {
                            g_md_music.g_master_chk[j] = (w_val[j] == "1") ? true : false;
                        }
                        break;
                    case "music_vol":
                        for (int j = 0; j <= 10; j++)
                        {
                            g_md_music.g_master_vol[j] = int.Parse(w_val[j]);
                        }
                        break;
                    case "vdp_tvmode":
                        g_tvmode_req = int.Parse(w_val[0]);
                        g_md_vdp.ApplyTimingMode(g_tvmode_req != 0);
                        break;
                    case "vdp_gpu":
                        g_gpu_req = int.Parse(w_val[0]);
                        g_md_vdp.rendering_gpu = (w_val[0] == "1") ? true : false;
                        break;
                }
            }
            g_frontendSettings.EnsureCodeToolLayoutVisible();
        }
        public static void read_init()
        {
            g_md_io.g_key_allocation[0] = 49;
            g_md_io.g_key_allocation[1] = 50;
            g_md_io.g_key_allocation[2] = 51;
            g_md_io.g_key_allocation[3] = 57;
            g_md_io.g_key_allocation[4] = 17;
            g_md_io.g_key_allocation[5] = 31;
            g_md_io.g_key_allocation[6] = 30;
            g_md_io.g_key_allocation[7] = 32;
            g_md_io.g_key_allocation[8] = 35;
            g_md_io.g_key_allocation[9] = 36;
            g_md_io.g_key_allocation[10] = 37;
            g_md_io.g_key_allocation[11] = 28;

            g_md_io.g_key_allocation2[0] = 0;
            g_md_io.g_key_allocation2[1] = 0;
            g_md_io.g_key_allocation2[2] = 0;
            g_md_io.g_key_allocation2[3] = 0;
            g_md_io.g_key_allocation2[4] = 0;
            g_md_io.g_key_allocation2[5] = 0;
            g_md_io.g_key_allocation2[6] = 0;
            g_md_io.g_key_allocation2[7] = 0;
            g_md_io.g_key_allocation2[8] = 0;
            g_md_io.g_key_allocation2[9] = 0;
            g_md_io.g_key_allocation2[10] = 0;
            g_md_io.g_key_allocation2[11] = 0;
            for (int j = 0; j <= 10; j++)
            {
                md_main.g_md_music.g_master_chk[j] = true;
            }
            for (int j = 0; j <= 10; j++)
            {
                md_main.g_md_music.g_master_vol[j] = 100;
            }
        }
        public static void write_setting()
        {
            g_config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string w_val = "";
            for (int i = 0; i < g_md_io.KEY_ALLCATION_NUM; i++)
            {
                w_val += g_md_io.g_key_allocation[i].ToString();
                if (i < g_md_io.KEY_ALLCATION_NUM - 1)
                {
                    w_val += ":";
                }
            }
            setting_add("key", w_val);
            w_val = "";
            for (int i = 0; i < g_md_io.KEY_ALLCATION_NUM; i++)
            {
                w_val += g_md_io.g_key_allocation2[i].ToString();
                if (i < g_md_io.KEY_ALLCATION_NUM - 1)
                {
                    w_val += ":";
                }
            }
            setting_add("key2", w_val);

            setting_add("joyname", g_md_io.g_joy_name);
            w_val = "";
            for (int i = 0; i < g_md_io.KEY_ALLCATION_NUM; i++)
            {
                w_val += g_md_io.g_joy_allocation[i].ToString();
                if (i < g_md_io.KEY_ALLCATION_NUM - 1)
                {
                    w_val += ":";
                }
            }
            setting_add("joy", w_val);
            w_val = "";
            for (int i = 0; i < g_md_io.KEY_ALLCATION_NUM; i++)
            {
                w_val += g_md_io.g_joy_allocation2[i].ToString();
                if (i < g_md_io.KEY_ALLCATION_NUM - 1)
                {
                    w_val += ":";
                }
            }
            setting_add("joy2", w_val);

            g_frontendSettings.CaptureSettings(setting_add);

            w_val = ((md_main.g_debugView.trace_fsb == true) ? "1" : "0");
            setting_add("trace_fsb", w_val);

            w_val = ((md_main.g_debugView.trace_sip == true) ? "1" : "0");
            setting_add("trace_sip", w_val);

            w_val = "";
            for (int j = 0; j < 11; j++)
            {
                w_val += ((md_main.g_md_music.g_master_chk[j] == true) ? "1" : "0");
                if (j < 10)
                {
                    w_val += ":";
                }
            }
            setting_add("music_chk", w_val);
            w_val = "";
            for (int j = 0; j <= 10; j++)
            {
                w_val += md_main.g_md_music.g_master_vol[j].ToString();
                if (j < 10)
                {
                    w_val += ":";
                }
            }
            setting_add("music_vol", w_val);
            setting_add("vdp_tvmode", g_tvmode_req.ToString());
            setting_add("vdp_gpu", g_gpu_req.ToString());
            g_config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }
        public static void setting_add(string in_name, string in_val)
        {
            if (g_config.AppSettings.Settings.AllKeys.Contains(in_name))
            {
                g_config.AppSettings.Settings[in_name].Value = in_val;
            }
            else
            {
                g_config.AppSettings.Settings.Add(in_name, in_val);
            }
        }
    }
}
