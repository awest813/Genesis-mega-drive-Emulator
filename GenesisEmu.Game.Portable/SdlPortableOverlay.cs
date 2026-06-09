using MDTracer;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace GenesisEmu.Game.Portable
{
  internal enum SdlOverlayMode
  {
    None,
    Help,
    GamepadPicker,
    StateList,
  }

  internal sealed class SdlPortableOverlay
  {
    private readonly Sdl g_sdl;
    private readonly SdlTtfRenderer g_text;
    private SdlOverlayMode g_mode;
    private int g_selectedIndex;
    private string[] g_gamepadNames = Array.Empty<string>();

    public SdlPortableOverlay(Sdl in_sdl)
    {
      g_sdl = in_sdl;
      g_text = new SdlTtfRenderer(in_sdl);
    }

    public SdlOverlayMode Mode => g_mode;

    public void Close()
    {
      g_mode = SdlOverlayMode.None;
      g_selectedIndex = 0;
    }

    public void OpenHelp()
    {
      g_mode = SdlOverlayMode.Help;
      g_selectedIndex = 0;
    }

    public void OpenGamepadPicker()
    {
      md_main.g_md_io.rescan();
      g_gamepadNames = md_main.g_md_io.g_joy_name_list.ToArray();
      g_mode = SdlOverlayMode.GamepadPicker;
      g_selectedIndex = Math.Max(0, md_main.g_md_io.g_joy_device_cur);
      if (g_selectedIndex >= g_gamepadNames.Length) g_selectedIndex = 0;
    }

    public void OpenStateList()
    {
      if (md_main.StateStore.IsAvailable() == false)
      {
        return;
      }

      g_mode = SdlOverlayMode.StateList;
      g_selectedIndex = 0;
    }

    public bool HandleKey(Scancode in_scancode, ushort in_mod, ref string io_statusText)
    {
      if (g_mode == SdlOverlayMode.None)
      {
        if (in_scancode == Scancode.ScancodeH && (in_mod & (ushort)Keymod.Ctrl) != 0)
        {
          OpenHelp();
          io_statusText = "Help overlay";
          return true;
        }

        if (in_scancode == Scancode.ScancodeG && (in_mod & (ushort)Keymod.Ctrl) != 0)
        {
          OpenGamepadPicker();
          io_statusText = "Gamepad picker";
          return true;
        }

        if (in_scancode == Scancode.ScancodeF4 && (in_mod & (ushort)Keymod.Ctrl) != 0)
        {
          OpenStateList();
          io_statusText = "Save state list";
          return true;
        }

        return false;
      }

      switch (in_scancode)
      {
        case Scancode.ScancodeEscape:
          Close();
          io_statusText = "Running";
          return true;
        case Scancode.ScancodeUp:
          g_selectedIndex = Math.Max(0, g_selectedIndex - 1);
          return true;
        case Scancode.ScancodeDown:
          g_selectedIndex += 1;
          return true;
        case Scancode.ScancodeReturn:
          return ActivateSelection(ref io_statusText);
        default:
          if (g_mode == SdlOverlayMode.StateList
              && in_scancode >= Scancode.Scancode1
              && in_scancode <= Scancode.Scancode9)
          {
            g_selectedIndex = (int)in_scancode - (int)Scancode.Scancode1;
            return ActivateSelection(ref io_statusText);
          }
          return true;
      }
    }

    public unsafe void Draw(Renderer* in_renderer, int in_width, int in_height)
    {
      if (g_mode == SdlOverlayMode.None) return;

      var w_panel = new Rectangle<int>
      {
        Origin = new Vector2D<int>(16, 16),
        Size = new Vector2D<int>(in_width - 32, in_height - 32),
      };
      g_sdl.SetRenderDrawColor(in_renderer, 24, 24, 28, 255);
      g_sdl.RenderFillRect(in_renderer, in w_panel);

      string[] w_lines = BuildLines();
      if (g_selectedIndex >= w_lines.Length)
      {
        g_selectedIndex = Math.Max(0, w_lines.Length - 1);
      }

      int w_y = 28;
      for (int i = 0; i < w_lines.Length; i++)
      {
        string w_line = (i == g_selectedIndex ? "> " : "  ") + w_lines[i];
        if (g_text.IsAvailable)
        {
          g_text.DrawText(in_renderer, w_line, 28, w_y, 220, 220, 220);
        }
        w_y += 20;
      }

      if (g_text.IsAvailable == false)
      {
        g_text.DrawText(in_renderer, "(install SDL2_ttf for overlay text)", 28, w_y, 180, 180, 180);
      }
    }

    public void Dispose()
    {
      g_text.Dispose();
    }

    private bool ActivateSelection(ref string io_statusText)
    {
      switch (g_mode)
      {
        case SdlOverlayMode.GamepadPicker:
          if (g_selectedIndex >= 0 && g_selectedIndex < g_gamepadNames.Length)
          {
            md_main.g_md_io.select_joy_device(g_selectedIndex);
            md_main.write_setting();
            io_statusText = "Gamepad: " + g_gamepadNames[g_selectedIndex];
          }
          Close();
          return true;
        case SdlOverlayMode.StateList:
          IReadOnlyList<md_main.StateListEntry> w_entries = md_main.StateStore.GetEntries();
          if (g_selectedIndex >= 0 && g_selectedIndex < w_entries.Count)
          {
            md_main.request_state_capture_restore_file(w_entries[g_selectedIndex].FilePath);
            io_statusText = "State loaded";
          }
          Close();
          return true;
        case SdlOverlayMode.Help:
          Close();
          io_statusText = "Running";
          return true;
        default:
          Close();
          return true;
      }
    }

    private string[] BuildLines()
    {
      return g_mode switch
      {
        SdlOverlayMode.Help => new[]
        {
          "GenesisEmu — controls",
          "Esc        Pause / resume",
          "F1         Save state",
          "F4         Load latest state",
          "Ctrl+F4    Save state list",
          "F5         Frame advance",
          "F12        Hard reset",
          "Ctrl+I     Toggle integer scaling",
          "Ctrl+G     Gamepad picker",
          "Ctrl+H     This help screen",
          "Ctrl+Q     Quit",
          "Esc        Close overlay",
        },
        SdlOverlayMode.GamepadPicker => BuildGamepadLines(),
        SdlOverlayMode.StateList => BuildStateLines(),
        _ => Array.Empty<string>(),
      };
    }

    private string[] BuildGamepadLines()
    {
      if (g_gamepadNames.Length == 0)
      {
        return new[] { "No gamepads found", "Press Rescan in a future build or plug in a controller" };
      }

      string[] w_lines = new string[g_gamepadNames.Length + 1];
      w_lines[0] = "Select gamepad (Enter to apply):";
      for (int i = 0; i < g_gamepadNames.Length; i++)
      {
        w_lines[i + 1] = g_gamepadNames[i];
      }
      return w_lines;
    }

    private static string[] BuildStateLines()
    {
      IReadOnlyList<md_main.StateListEntry> w_entries = md_main.StateStore.GetEntries();
      if (w_entries.Count == 0)
      {
        return new[] { "No save states for this ROM" };
      }

      string[] w_lines = new string[w_entries.Count + 1];
      w_lines[0] = "Save states (Enter or 1-9 to load):";
      for (int i = 0; i < w_entries.Count; i++)
      {
        w_lines[i + 1] = w_entries[i].DisplayName;
      }
      return w_lines;
    }
  }
}
