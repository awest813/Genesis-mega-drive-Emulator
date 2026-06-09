using System.Runtime.InteropServices;
using Silk.NET.SDL;

namespace MDTracer.Platform.Portable
{
  internal sealed class SdlInputDeviceBackend : IInputDeviceBackend, IDisposable
  {
    private enum JoystickDeviceKind
    {
      Joystick,
      GameController
    }

    private readonly struct OpenJoystickDevice
    {
      public readonly nint Handle;
      public readonly JoystickDeviceKind Kind;

      public OpenJoystickDevice(nint in_handle, JoystickDeviceKind in_kind)
      {
        Handle = in_handle;
        Kind = in_kind;
      }
    }

    private const int JoyStatusNum = 50;
    private const int KeyStatusNum = 256;

    private readonly Sdl g_sdl;
    private readonly bool g_ownSdlLifetime;
    private readonly object g_deviceLock = new();
    private readonly List<string> g_joyNameList = new();
    private readonly List<OpenJoystickDevice> g_joyDevices = new();
    private int g_joyDeviceCur = -1;
    private string g_joyName = "";
    private bool g_initialized;

    public SdlInputDeviceBackend()
      : this(null, true)
    {
    }

    public SdlInputDeviceBackend(Sdl? in_sharedSdl, bool in_ownSdlLifetime = false)
    {
      g_sdl = in_sharedSdl ?? Sdl.GetApi();
      g_ownSdlLifetime = in_ownSdlLifetime;
      EnsureInitialized();
    }

    public string JoyName
    {
      get
      {
        lock (g_deviceLock) return g_joyName;
      }
    }

    public IReadOnlyList<string> JoyNameList
    {
      get
      {
        lock (g_deviceLock) return g_joyNameList.ToArray();
      }
    }

    public int JoyDeviceIndex
    {
      get
      {
        lock (g_deviceLock) return g_joyDeviceCur;
      }
    }

    public void SelectJoyByName(string in_name)
    {
      lock (g_deviceLock)
      {
        g_joyName = in_name ?? "";
        g_joyDeviceCur = g_joyNameList.IndexOf(g_joyName);
      }
    }

    public void SelectJoyDevice(int in_index)
    {
      lock (g_deviceLock)
      {
        if (in_index < 0 || in_index >= g_joyNameList.Count)
        {
          g_joyDeviceCur = -1;
          return;
        }

        g_joyDeviceCur = in_index;
        g_joyName = g_joyNameList[in_index];
      }
    }

    public void Rescan(bool in_updateKeyboard)
    {
      EnsureInitialized();
      g_sdl.PumpEvents();

      string w_targetName;
      string w_oldName;
      string[] w_oldNameList;
      int w_oldDeviceCur;
      lock (g_deviceLock)
      {
        w_targetName = g_joyName;
        w_oldName = g_joyName;
        w_oldNameList = g_joyNameList.ToArray();
        w_oldDeviceCur = g_joyDeviceCur;
      }

      List<string> w_newNameList = new();
      List<OpenJoystickDevice> w_newDevices = new();
      int w_newDeviceCur = -1;
      string w_newJoyName = w_targetName;

      int w_count = g_sdl.NumJoysticks();
      for (int i = 0; i < w_count; i++)
      {
        if (g_sdl.IsGameController(i) == SdlBool.True)
        {
          unsafe
          {
            GameController* w_controller = g_sdl.GameControllerOpen(i);
            if (w_controller == null) continue;

            string w_name = ReadJoystickName(i);
            if (!w_newNameList.Contains(w_name))
            {
              w_newDevices.Add(new OpenJoystickDevice((nint)w_controller, JoystickDeviceKind.GameController));
              w_newNameList.Add(w_name);
              if (w_name == w_targetName)
              {
                w_newDeviceCur = w_newNameList.Count - 1;
              }
            }
            else
            {
              g_sdl.GameControllerClose(w_controller);
            }
          }
        }
        else
        {
          unsafe
          {
            Joystick* w_joystick = g_sdl.JoystickOpen(i);
            if (w_joystick == null) continue;

            string w_name = ReadJoystickName(i);
            if (!w_newNameList.Contains(w_name))
            {
              w_newDevices.Add(new OpenJoystickDevice((nint)w_joystick, JoystickDeviceKind.Joystick));
              w_newNameList.Add(w_name);
              if (w_name == w_targetName)
              {
                w_newDeviceCur = w_newNameList.Count - 1;
              }
            }
            else
            {
              g_sdl.JoystickClose(w_joystick);
            }
          }
        }
      }

      if (w_newDeviceCur < 0 && w_newDevices.Count > 0)
      {
        w_newDeviceCur = 0;
        w_newJoyName = w_newNameList[0];
      }

      List<OpenJoystickDevice> w_oldDevices;
      lock (g_deviceLock)
      {
        w_oldDevices = new List<OpenJoystickDevice>(g_joyDevices);
        g_joyDevices.Clear();
        g_joyDevices.AddRange(w_newDevices);
        g_joyNameList.Clear();
        g_joyNameList.AddRange(w_newNameList);
        g_joyDeviceCur = w_newDeviceCur;
        g_joyName = w_newJoyName;
      }

      foreach (OpenJoystickDevice w_device in w_oldDevices)
      {
        CloseDevice(w_device);
      }

      bool w_changed = w_oldDeviceCur != w_newDeviceCur
          || w_oldName != w_newJoyName
          || !w_oldNameList.SequenceEqual(w_newNameList);
      if (w_changed && w_newDevices.Count > 0)
      {
        md_io.g_frontendHooks.NotifyJoystickDevicesChanged();
      }
    }

    public int ReadJoystick(byte[]? in_status)
    {
      EnsureInitialized();
      g_sdl.PumpEvents();
      return read_joystick_core(in_status);
    }

    public int ReadKeyboard(byte[]? in_status)
    {
      EnsureInitialized();
      g_sdl.PumpEvents();
      return read_keyboard_core(in_status);
    }

    public void Dispose()
    {
      lock (g_deviceLock)
      {
        foreach (OpenJoystickDevice w_device in g_joyDevices)
        {
          CloseDevice(w_device);
        }
        g_joyDevices.Clear();
        g_joyNameList.Clear();
        g_joyDeviceCur = -1;
        g_joyName = "";
      }

      if (g_initialized && g_ownSdlLifetime)
      {
        g_sdl.Quit();
        g_initialized = false;
      }

      if (g_ownSdlLifetime)
      {
        g_sdl.Dispose();
      }
    }

    private void EnsureInitialized()
    {
      if (g_initialized) return;

      uint w_required = Sdl.InitJoystick | Sdl.InitGamecontroller | Sdl.InitEvents;
      if ((g_sdl.WasInit(w_required) & w_required) == w_required)
      {
        g_initialized = true;
        return;
      }

      if (g_sdl.Init(w_required) != 0)
      {
        throw new InvalidOperationException($"SDL init failed: {g_sdl.GetErrorS()}");
      }

      g_sdl.SetHint(Sdl.HintJoystickAllowBackgroundEvents, "1");
      g_initialized = true;
    }

    private string ReadJoystickName(int in_index)
    {
      unsafe
      {
        byte* w_name = g_sdl.JoystickNameForIndex(in_index);
        if (w_name != null)
        {
          return Marshal.PtrToStringUTF8((nint)w_name) ?? $"Joystick {in_index}";
        }
      }

      return $"Joystick {in_index}";
    }

    private void CloseDevice(OpenJoystickDevice in_device)
    {
      if (in_device.Handle == nint.Zero) return;

      unsafe
      {
        if (in_device.Kind == JoystickDeviceKind.GameController)
        {
          g_sdl.GameControllerClose((GameController*)in_device.Handle);
        }
        else
        {
          g_sdl.JoystickClose((Joystick*)in_device.Handle);
        }
      }
    }

    private int read_joystick_core(byte[]? in_status)
    {
      int w_out = -1;
      if (in_status != null)
      {
        Array.Clear(in_status, 0, in_status.Length);
      }

      lock (g_deviceLock)
      {
        if (g_joyDevices.Count <= 0 || g_joyDeviceCur < 0 || g_joyDeviceCur >= g_joyDevices.Count)
        {
          return -1;
        }

        OpenJoystickDevice w_device = g_joyDevices[g_joyDeviceCur];
        unsafe
        {
          if (w_device.Kind == JoystickDeviceKind.GameController)
          {
            read_game_controller_state((GameController*)w_device.Handle, in_status, ref w_out);
          }
          else
          {
            read_joystick_state((Joystick*)w_device.Handle, in_status, ref w_out);
          }
        }
      }

      return w_out;
    }

    private unsafe void read_game_controller_state(GameController* in_controller, byte[]? in_status, ref int out_pressedIndex)
    {
      for (int i = 0; i < 32; i++)
      {
        if (g_sdl.GameControllerGetButton(in_controller, (GameControllerButton)i) == 1)
        {
          set_joystick_status(in_status, i, ref out_pressedIndex);
        }
      }

      update_joystick_axis_status(in_status, g_sdl.GameControllerGetAxis(in_controller, GameControllerAxis.Leftx), 40, 41, ref out_pressedIndex);
      update_joystick_axis_status(in_status, g_sdl.GameControllerGetAxis(in_controller, GameControllerAxis.Lefty), 42, 43, ref out_pressedIndex);
      update_joystick_axis_status(in_status, g_sdl.GameControllerGetAxis(in_controller, GameControllerAxis.Rightx), 44, 45, ref out_pressedIndex);
      update_joystick_axis_status(in_status, g_sdl.GameControllerGetAxis(in_controller, GameControllerAxis.Righty), 46, 47, ref out_pressedIndex);
      update_joystick_axis_status(in_status, g_sdl.GameControllerGetAxis(in_controller, GameControllerAxis.Triggerleft), 48, 49, ref out_pressedIndex);
    }

    private unsafe void read_joystick_state(Joystick* in_joystick, byte[]? in_status, ref int out_pressedIndex)
    {
      int w_buttonCount = g_sdl.JoystickNumButtons(in_joystick);
      for (int i = 0; i < 32 && i < w_buttonCount; i++)
      {
        if (g_sdl.JoystickGetButton(in_joystick, i) == 1)
        {
          set_joystick_status(in_status, i, ref out_pressedIndex);
        }
      }

      int w_hatCount = g_sdl.JoystickNumHats(in_joystick);
      for (int i = 0; i < 2 && i < w_hatCount; i++)
      {
        update_joystick_hat_status(in_status, 32 + (i * 4), g_sdl.JoystickGetHat(in_joystick, i), ref out_pressedIndex);
      }

      update_joystick_axis_status(in_status, g_sdl.JoystickGetAxis(in_joystick, 0), 40, 41, ref out_pressedIndex);
      update_joystick_axis_status(in_status, g_sdl.JoystickGetAxis(in_joystick, 1), 42, 43, ref out_pressedIndex);
      update_joystick_axis_status(in_status, g_sdl.JoystickGetAxis(in_joystick, 2), 44, 45, ref out_pressedIndex);
      update_joystick_axis_status(in_status, g_sdl.JoystickGetAxis(in_joystick, 3), 46, 47, ref out_pressedIndex);
      update_joystick_axis_status(in_status, g_sdl.JoystickGetAxis(in_joystick, 4), 48, 49, ref out_pressedIndex);
    }

    private int read_keyboard_core(byte[]? in_status)
    {
      int w_out = -1;
      if (in_status != null)
      {
        Array.Clear(in_status, 0, in_status.Length);
      }

      unsafe
      {
        byte* w_state = g_sdl.GetKeyboardState(null);
        if (w_state == null) return -1;

        int w_count = 512;
        for (int i = 0; i < w_count; i++)
        {
          if (w_state[i] == 0) continue;

          int w_dik = SdlDirectInputKeyMap.ToDirectInputKey((Scancode)i);
          if (w_dik < 0 || w_dik >= KeyStatusNum) continue;
          if (IsMainShortcutKey(w_dik)) continue;

          if (in_status != null && w_dik < in_status.Length)
          {
            in_status[w_dik] = 1;
          }

          if (w_out == -1)
          {
            w_out = w_dik;
          }
        }
      }

      return w_out;
    }

    private static void update_joystick_hat_status(byte[]? in_status, int in_baseIndex, byte in_hat, ref int out_pressedIndex)
    {
      if ((in_hat & Sdl.HatUp) != 0)
      {
        set_joystick_status(in_status, in_baseIndex, ref out_pressedIndex);
      }
      if ((in_hat & Sdl.HatRight) != 0)
      {
        set_joystick_status(in_status, in_baseIndex + 1, ref out_pressedIndex);
      }
      if ((in_hat & Sdl.HatDown) != 0)
      {
        set_joystick_status(in_status, in_baseIndex + 2, ref out_pressedIndex);
      }
      if ((in_hat & Sdl.HatLeft) != 0)
      {
        set_joystick_status(in_status, in_baseIndex + 3, ref out_pressedIndex);
      }
    }

    private static void update_joystick_axis_status(byte[]? in_status, short in_value, int in_lowIndex, int in_highIndex, ref int out_pressedIndex)
    {
      if (in_value < -16384)
      {
        set_joystick_status(in_status, in_lowIndex, ref out_pressedIndex);
      }
      else if (in_value > 16384)
      {
        set_joystick_status(in_status, in_highIndex, ref out_pressedIndex);
      }
    }

    private static void set_joystick_status(byte[]? in_status, int in_index, ref int out_pressedIndex)
    {
      if (in_index < 0 || in_index >= JoyStatusNum) return;
      if (in_status != null && in_index < in_status.Length)
      {
        in_status[in_index] = 1;
      }
      out_pressedIndex = in_index;
    }

    private static bool IsMainShortcutKey(int in_dik)
    {
      return in_dik == 0x3F || in_dik == 0x40;
    }
  }
}
