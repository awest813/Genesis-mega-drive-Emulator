namespace MDTracer.Platform.Windows
{
    public static class WindowsPlatformServices
    {
        public static void Register()
        {
            md_main.g_audioBackend = new NaudioAudioOutputBackend();
            md_io.g_inputBackend = new DirectInputDeviceBackend();
        }
    }
}
