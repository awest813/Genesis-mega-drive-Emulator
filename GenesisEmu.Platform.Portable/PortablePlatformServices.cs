using Silk.NET.SDL;

namespace MDTracer.Platform.Portable
{
  //----------------------------------------------------------------
  // Cross-platform backend registration (Phase 4).
  //
  // Registers OpenAL audio, SDL input, and the software VDP GPU
  // compositor for Linux/macOS frontends and headless tooling.
  //----------------------------------------------------------------
  public static class PortablePlatformServices
  {
    public static void Register()
    {
      md_main.g_audioBackend = CreateAudioBackend();
      md_io.g_inputBackend = CreateInputBackend();
      md_vdp.g_gpuRenderer = VdpGpuRendererFactory.Create();
    }

    public static void RegisterForSdlGame(Sdl in_sdl)
    {
      EnsureSdlSubsystems(in_sdl, Sdl.InitVideo | Sdl.InitJoystick | Sdl.InitGamecontroller | Sdl.InitEvents);
      md_main.g_audioBackend = CreateAudioBackend();
      md_io.g_inputBackend = new SdlInputDeviceBackend(in_sdl, in_ownSdlLifetime: false);
      md_vdp.g_gpuRenderer = CreateGpuRendererForHost();
    }

    private static IVdpGpuRenderer CreateGpuRendererForHost()
    {
      if (OperatingSystem.IsMacOS()
          && MetalVdpGpuRenderer.TryCreate(out IVdpGpuRenderer? w_metal)
          && w_metal != null)
      {
        return w_metal;
      }

      return VdpGpuRendererFactory.Create();
    }

    public static bool TryRegister()
    {
      try
      {
        Register();
        return true;
      }
      catch
      {
        return false;
      }
    }

    private static IAudioOutputBackend CreateAudioBackend()
    {
      return new OpenAlAudioOutputBackend();
    }

    private static IInputDeviceBackend CreateInputBackend()
    {
      return new SdlInputDeviceBackend();
    }

    private static void EnsureSdlSubsystems(Sdl in_sdl, uint in_flags)
    {
      if ((in_sdl.WasInit(in_flags) & in_flags) != in_flags)
      {
        if (in_sdl.Init(in_flags) != 0)
        {
          throw new InvalidOperationException($"SDL init failed: {in_sdl.GetErrorS()}");
        }
      }
    }
  }
}
