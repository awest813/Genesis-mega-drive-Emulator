using MDTracer;

namespace GenesisEmu.Game.Portable
{
  internal sealed class SdlGameFrontendSettingsHooks : IFrontendSettingsHooks
  {
    public bool TryApplySetting(string in_name, string in_value)
    {
      switch (in_name)
      {
        case "screen_main":
          string[] w_screenVal = in_value.Split(':');
          if (w_screenVal.Length < 4) return false;
          SdlGameApp.g_windowX = int.Parse(w_screenVal[0]);
          SdlGameApp.g_windowY = int.Parse(w_screenVal[1]);
          SdlGameApp.g_windowWidth = int.Parse(w_screenVal[2]);
          SdlGameApp.g_windowHeight = int.Parse(w_screenVal[3]);
          return true;
        case "game_scale":
          SdlGameApp.g_integerFitScale = in_value == "1";
          return true;
        case "file0": SdlGameApp.g_file_name[0] = in_value; return true;
        case "file1": SdlGameApp.g_file_name[1] = in_value; return true;
        case "file2": SdlGameApp.g_file_name[2] = in_value; return true;
        case "file3": SdlGameApp.g_file_name[3] = in_value; return true;
        case "file4": SdlGameApp.g_file_name[4] = in_value; return true;
        case "file5": SdlGameApp.g_file_name[5] = in_value; return true;
        case "file6": SdlGameApp.g_file_name[6] = in_value; return true;
        case "file7": SdlGameApp.g_file_name[7] = in_value; return true;
        case "file8": SdlGameApp.g_file_name[8] = in_value; return true;
        default:
          return false;
      }
    }

    public void CaptureSettings(Action<string, string> settingAdd)
    {
      string w_val = SdlGameApp.g_windowX
          + ":" + SdlGameApp.g_windowY
          + ":" + SdlGameApp.g_windowWidth
          + ":" + SdlGameApp.g_windowHeight;
      settingAdd("screen_main", w_val);
      settingAdd("game_scale", SdlGameApp.g_integerFitScale ? "1" : "0");

      for (int i = 0; i < 9; i++)
      {
        settingAdd("file" + i, SdlGameApp.g_file_name[i]);
      }
    }

    public void EnsureCodeToolLayoutVisible() { }
    public void NotifyDebugWindowLayoutChanged() { }
  }
}
