namespace MDTracer
{
    internal sealed class WinFormsAudioFrontendHooks : IAudioFrontendHooks
    {
        public void OnAudioSamplesProduced(byte[] in_buffer, int in_offset, int in_count)
        {
            Form_Main.Instance?.videoRecordingAddAudioSamples(in_buffer, in_offset, in_count);
        }
    }
}
