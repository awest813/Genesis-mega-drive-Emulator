using System.Runtime.InteropServices;
using MDTracer;
using MDTracer.Platform.Portable;
using Silk.NET.SDL;

namespace GenesisEmu.Game.Portable
{
  internal sealed class SdlGameApp : IDisposable
  {
    private const int DefaultWidth = 640;
    private const int DefaultHeight = 480;

    public static int g_windowX;
    public static int g_windowY;
    public static int g_windowWidth = DefaultWidth;
    public static int g_windowHeight = DefaultHeight;
    public static bool g_integerFitScale = true;
    public static string[] g_file_name = new string[9];

    private readonly Sdl g_sdl;
    private unsafe Window* g_window;
    private unsafe Renderer* g_renderer;
    private unsafe Texture* g_texture;
    private uint[] g_scaledPixels = Array.Empty<uint>();
    private int g_textureWidth = -1;
    private int g_textureHeight = -1;
    private bool g_romLoaded;
    private bool g_running = true;
    private string g_statusText = "GenesisEmu";
    private int g_framePresentPending;
    private readonly object g_presentLock = new();
    private readonly SdlPortableOverlay g_overlay;

    public SdlGameApp()
    {
      g_sdl = Sdl.GetApi();
      PortablePlatformServices.RegisterForSdlGame(g_sdl);
      g_overlay = new SdlPortableOverlay(g_sdl);
      md_main.g_frontendSettings = new SdlGameFrontendSettingsHooks();
      md_main.initialize();
      md_main.read_setting();
      md_main.g_mainLoopUI = new SdlGameMainLoopUiHooks(this);
      md_main.g_md_music.setting();
    }

    public int Run(string[] in_args)
    {
      unsafe
      {
        g_window = g_sdl.CreateWindow(
            "GenesisEmu",
            g_windowX,
            g_windowY,
            g_windowWidth > 0 ? g_windowWidth : DefaultWidth,
            g_windowHeight > 0 ? g_windowHeight : DefaultHeight,
            (uint)WindowFlags.Resizable);

        if (g_window == null)
        {
          throw new InvalidOperationException($"SDL window create failed: {g_sdl.GetErrorS()}");
        }

        g_renderer = g_sdl.CreateRenderer(g_window, -1, (uint)RendererFlags.Accelerated | (uint)RendererFlags.Presentvsync);
        if (g_renderer == null)
        {
          throw new InvalidOperationException($"SDL renderer create failed: {g_sdl.GetErrorS()}");
        }
      }

      string? w_romPath = ResolveRomPath(in_args);
      if (string.IsNullOrEmpty(w_romPath) == false)
      {
        LoadRom(w_romPath);
      }
      else
      {
        g_statusText = "GenesisEmu — pass a ROM path or set a recent ROM in settings";
      }

      while (g_running)
      {
        PumpEvents();
        PresentIfNeeded();
        System.Threading.Thread.Sleep(1);
      }

      CaptureWindowGeometry();
      md_main.write_setting();
      md_main.g_md_sram.save();
      return 0;
    }

    public void RequestFramePresent(int in_cpuUsage)
    {
      g_statusText = "CPU load: " + in_cpuUsage + "%";
      Interlocked.Exchange(ref g_framePresentPending, 1);
    }

    public void Dispose()
    {
      unsafe
      {
        if (g_texture != null)
        {
          g_sdl.DestroyTexture(g_texture);
          g_texture = null;
        }

        if (g_renderer != null)
        {
          g_sdl.DestroyRenderer(g_renderer);
          g_renderer = null;
        }

        if (g_window != null)
        {
          g_sdl.DestroyWindow(g_window);
          g_window = null;
        }
      }

      g_overlay.Dispose();
      g_sdl.Quit();
      g_sdl.Dispose();
    }

    private string? ResolveRomPath(string[] in_args)
    {
      if (in_args.Length > 0 && string.IsNullOrWhiteSpace(in_args[0]) == false)
      {
        return in_args[0];
      }

      if (string.IsNullOrEmpty(g_file_name[0]) == false && File.Exists(g_file_name[0]))
      {
        return g_file_name[0];
      }

      return null;
    }

    private void LoadRom(string in_path)
    {
      if (File.Exists(in_path) == false)
      {
        g_statusText = "File not found: " + in_path;
        return;
      }

      if (md_main.run(in_path) == false)
      {
        g_statusText = "Failed to load ROM: " + in_path;
        return;
      }

      g_romLoaded = true;
      UpdateRecentRomList(in_path);
      unsafe
      {
        g_sdl.SetWindowTitle(g_window, "GenesisEmu — " + Path.GetFileName(in_path));
      }
      g_statusText = "Running";
    }

    private unsafe void PumpEvents()
    {
      Event w_event;
      while (g_sdl.PollEvent(&w_event) != 0)
      {
        switch ((EventType)w_event.Type)
        {
          case EventType.Quit:
            g_running = false;
            break;
          case EventType.Dropfile:
            unsafe
            {
              string? w_path = Marshal.PtrToStringUTF8((nint)w_event.Drop.File);
              if (string.IsNullOrEmpty(w_path) == false)
              {
                LoadRom(w_path);
              }
              g_sdl.Free(w_event.Drop.File);
            }
            break;
          case EventType.Keydown:
            HandleKeyDown(ref w_event);
            break;
          case EventType.Windowevent:
            if (w_event.Window.Event == (byte)WindowEventID.Resized)
            {
              Interlocked.Exchange(ref g_framePresentPending, 1);
            }
            break;
        }
      }
    }

    private void HandleKeyDown(ref Event in_event)
    {
      if (in_event.Key.Keysym.Scancode == Scancode.ScancodeQ
          && (in_event.Key.Keysym.Mod & (ushort)Keymod.Ctrl) != 0)
      {
        g_running = false;
        return;
      }

      if (g_overlay.HandleKey(in_event.Key.Keysym.Scancode, in_event.Key.Keysym.Mod, ref g_statusText))
      {
        Interlocked.Exchange(ref g_framePresentPending, 1);
        return;
      }

      if (g_romLoaded == false) return;

      switch (in_event.Key.Keysym.Scancode)
      {
        case Scancode.ScancodeEscape:
          bool w_stopped = md_main.request_stop();
          g_statusText = w_stopped ? "Paused" : "Running";
          break;
        case Scancode.ScancodeF1:
          if (md_main.StateStore.IsAvailable())
          {
            md_main.request_state_capture_save();
            g_statusText = "State saved";
          }
          break;
        case Scancode.ScancodeF4:
          if (md_main.StateStore.IsAvailable())
          {
            md_main.request_state_capture_restore_latest();
            g_statusText = "State loaded";
          }
          break;
        case Scancode.ScancodeF5:
          md_main.request_frame_advance();
          break;
        case Scancode.ScancodeF12:
          md_main.request_hard_reset();
          g_statusText = "Running";
          break;
        case Scancode.ScancodeI:
          if ((in_event.Key.Keysym.Mod & (ushort)Keymod.Ctrl) != 0)
          {
            g_integerFitScale = !g_integerFitScale;
            Interlocked.Exchange(ref g_framePresentPending, 1);
            g_statusText = g_integerFitScale ? "Integer scaling" : "Stretch scaling";
          }
          break;
      }
    }

    private void PresentIfNeeded()
    {
      if (Interlocked.Exchange(ref g_framePresentPending, 0) == 0 && g_romLoaded == false)
      {
        PresentStatusOnly();
        return;
      }

      if (g_romLoaded == false) return;

      unsafe
      {
        int w_width = 0;
        int w_height = 0;
        g_sdl.GetRendererOutputSize(g_renderer, ref w_width, ref w_height);
        if (w_width <= 0 || w_height <= 0) return;

        int w_sourceWidth = md_main.g_md_vdp.g_display_xsize;
        int w_sourceHeight = md_main.g_md_vdp.g_display_ysize;
        uint[] w_gameScreen = md_main.g_md_vdp.g_game_screen;

        lock (g_presentLock)
        {
          EnsureTexture(w_width, w_height);
          if (g_integerFitScale)
          {
            GameScreenPixels.WriteIntegerFit(w_gameScreen, w_sourceWidth, w_sourceHeight, g_scaledPixels, w_width, w_height);
          }
          else
          {
            GameScreenPixels.WriteStretch(w_gameScreen, w_sourceWidth, w_sourceHeight, g_scaledPixels, w_width, w_height);
          }

          fixed (uint* w_pixels = g_scaledPixels)
          {
            g_sdl.UpdateTexture(g_texture, null, (void*)w_pixels, w_width * 4);
          }
        }

        g_sdl.SetRenderDrawColor(g_renderer, 0, 0, 0, 255);
        g_sdl.RenderClear(g_renderer);
        g_sdl.RenderCopy(g_renderer, g_texture, null, null);
        g_overlay.Draw(g_renderer, w_width, w_height);
        g_sdl.RenderPresent(g_renderer);
        UpdateWindowTitle();
      }
    }

    private unsafe void PresentStatusOnly()
    {
      int w_width = 0;
      int w_height = 0;
      g_sdl.GetRendererOutputSize(g_renderer, ref w_width, ref w_height);
      g_sdl.SetRenderDrawColor(g_renderer, 0, 0, 0, 255);
      g_sdl.RenderClear(g_renderer);
      g_overlay.Draw(g_renderer, w_width, w_height);
      g_sdl.RenderPresent(g_renderer);
      UpdateWindowTitle();
    }

    private unsafe void UpdateWindowTitle()
    {
      if (g_window == null) return;
      g_sdl.SetWindowTitle(g_window, g_statusText);
    }

    private unsafe void EnsureTexture(int in_width, int in_height)
    {
      if (g_texture != null && g_textureWidth == in_width && g_textureHeight == in_height)
      {
        return;
      }

      if (g_texture != null)
      {
        g_sdl.DestroyTexture(g_texture);
      }

      g_textureWidth = in_width;
      g_textureHeight = in_height;
      g_scaledPixels = new uint[in_width * in_height];
      g_texture = g_sdl.CreateTexture(
          g_renderer,
          (uint)PixelFormatEnum.Argb8888,
          (int)TextureAccess.Streaming,
          in_width,
          in_height);
    }

    private unsafe void CaptureWindowGeometry()
    {
      if (g_window == null) return;
      int w_x = g_windowX;
      int w_y = g_windowY;
      int w_width = g_windowWidth;
      int w_height = g_windowHeight;
      g_sdl.GetWindowPosition(g_window, ref w_x, ref w_y);
      g_sdl.GetWindowSize(g_window, ref w_width, ref w_height);
      g_windowX = w_x;
      g_windowY = w_y;
      g_windowWidth = w_width;
      g_windowHeight = w_height;
    }

    private static void UpdateRecentRomList(string in_file)
    {
      for (int i = 0; i < 9; i++)
      {
        if (g_file_name[i] == in_file)
        {
          for (int m = i; m < 8; m++)
          {
            g_file_name[m] = g_file_name[m + 1];
          }
          break;
        }
      }

      for (int i = 8; i >= 1; i--)
      {
        g_file_name[i] = g_file_name[i - 1];
      }
      g_file_name[0] = in_file;
    }
  }
}
