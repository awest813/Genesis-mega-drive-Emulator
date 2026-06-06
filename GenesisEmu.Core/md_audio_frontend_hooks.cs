namespace MDTracer
{
    //----------------------------------------------------------------
    // Optional audio output notification hooks.
    //
    // md_music previously forwarded mixed PCM samples directly to
    // Form_Main for AVI recording. This hook keeps synthesis in the
    // core while recording stays in the frontend.
    //----------------------------------------------------------------

    internal interface IAudioFrontendHooks
    {
        void OnAudioSamplesProduced(byte[] in_buffer, int in_offset, int in_count);
    }

    internal sealed class NullAudioFrontendHooks : IAudioFrontendHooks
    {
        public void OnAudioSamplesProduced(byte[] in_buffer, int in_offset, int in_count) { }
    }
}
