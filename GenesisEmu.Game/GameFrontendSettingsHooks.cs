using MDTracer;

namespace GenesisEmu.Game
{
    //----------------------------------------------------------------
    // Persists window layout, recent ROM list, and display scale mode
    // for the minimal game frontend.
    //----------------------------------------------------------------
    internal sealed class GameFrontendSettingsHooks : IFrontendSettingsHooks
    {
        public bool TryApplySetting(string in_name, string in_value)
        {
            switch (in_name)
            {
                case "screen_main":
                    string[] w_screenVal = in_value.Split(':');
                    if (w_screenVal.Length < 4) return false;
                    GameForm.g_screen_xpos = int.Parse(w_screenVal[0]);
                    GameForm.g_screen_ypos = int.Parse(w_screenVal[1]);
                    GameForm.g_screen_size_x = int.Parse(w_screenVal[2]);
                    GameForm.g_screen_size_y = int.Parse(w_screenVal[3]);
                    return true;
                case "game_scale":
                    GameForm.g_scale_mode = in_value == "1"
                        ? GenesisEmu.Frontend.Windows.GameScreenScaleMode.IntegerFit
                        : GenesisEmu.Frontend.Windows.GameScreenScaleMode.Stretch;
                    return true;
                case "file0": GameForm.g_file_name[0] = in_value; return true;
                case "file1": GameForm.g_file_name[1] = in_value; return true;
                case "file2": GameForm.g_file_name[2] = in_value; return true;
                case "file3": GameForm.g_file_name[3] = in_value; return true;
                case "file4": GameForm.g_file_name[4] = in_value; return true;
                case "file5": GameForm.g_file_name[5] = in_value; return true;
                case "file6": GameForm.g_file_name[6] = in_value; return true;
                case "file7": GameForm.g_file_name[7] = in_value; return true;
                case "file8": GameForm.g_file_name[8] = in_value; return true;
                default:
                    return false;
            }
        }

        public void CaptureSettings(Action<string, string> settingAdd)
        {
            string w_val = GameForm.g_screen_xpos
                + ":" + GameForm.g_screen_ypos
                + ":" + GameForm.g_screen_size_x
                + ":" + GameForm.g_screen_size_y;
            settingAdd("screen_main", w_val);
            settingAdd(
                "game_scale",
                GameForm.g_scale_mode == GenesisEmu.Frontend.Windows.GameScreenScaleMode.IntegerFit ? "1" : "0");

            for (int i = 0; i < 9; i++)
            {
                settingAdd("file" + i, GameForm.g_file_name[i]);
            }
        }

        public void EnsureCodeToolLayoutVisible() { }
        public void NotifyDebugWindowLayoutChanged() { }
    }
}
