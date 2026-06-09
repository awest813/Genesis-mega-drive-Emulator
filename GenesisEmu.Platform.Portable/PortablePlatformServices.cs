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
      md_vdp.g_gpuRenderer = new CpuVdpGpuRenderer();
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
  }
}
