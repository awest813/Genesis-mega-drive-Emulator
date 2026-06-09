using MDTracer.Platform.Portable;

namespace MDTracer.Platform.Windows
{
    public static class WindowsPlatformServices
    {
        public static void Register()
        {
            md_main.g_audioBackend = new NaudioAudioOutputBackend();
            md_io.g_inputBackend = new DirectInputDeviceBackend();
            md_vdp.g_gpuRenderer = CreateGpuRenderer();
        }

        private static IVdpGpuRenderer CreateGpuRenderer()
        {
            try
            {
                return new DirectX12VdpGpuRenderer();
            }
            catch
            {
                return new CpuVdpGpuRenderer();
            }
        }
    }
}
