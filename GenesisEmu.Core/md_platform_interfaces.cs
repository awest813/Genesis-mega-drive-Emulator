namespace MDTracer
{
    //----------------------------------------------------------------
    // Platform service interfaces (Phase 4).
    //
    // Genesis hardware emulation stays in the core; OS-specific audio
    // output, input polling, and VDP GPU rendering live behind injectable
    // backends (see also IVdpGpuRenderer in md_vdp_gpu_interfaces.cs).
    //----------------------------------------------------------------

    internal interface IAudioOutputBackend
    {
        void Initialize(int in_sampleRate, int in_bitsPerSample, int in_channels, int in_bufferSizeBytes);
        void Play();
        void ClearBuffer();
        int BufferedBytes { get; }
        int BufferCapacity { get; }
        bool TryEnqueueSamples(byte[] in_buffer, int in_offset, int in_count);
    }

    internal sealed class NullAudioOutputBackend : IAudioOutputBackend
    {
        public int BufferedBytes => 0;
        public int BufferCapacity => int.MaxValue;
        public void Initialize(int in_sampleRate, int in_bitsPerSample, int in_channels, int in_bufferSizeBytes) { }
        public void Play() { }
        public void ClearBuffer() { }
        public bool TryEnqueueSamples(byte[] in_buffer, int in_offset, int in_count) => true;
    }

    internal interface IInputDeviceBackend
    {
        string JoyName { get; }
        IReadOnlyList<string> JoyNameList { get; }
        int JoyDeviceIndex { get; }

        void SelectJoyByName(string in_name);
        void SelectJoyDevice(int in_index);
        void Rescan(bool in_updateKeyboard);
        int ReadJoystick(byte[]? in_status);
        int ReadKeyboard(byte[]? in_status);
    }

    internal sealed class NullInputDeviceBackend : IInputDeviceBackend
    {
        public string JoyName => "";
        public IReadOnlyList<string> JoyNameList { get; } = Array.Empty<string>();
        public int JoyDeviceIndex => -1;

        public void SelectJoyByName(string in_name) { }
        public void SelectJoyDevice(int in_index) { }
        public void Rescan(bool in_updateKeyboard) { }
        public int ReadJoystick(byte[]? in_status) => -1;
        public int ReadKeyboard(byte[]? in_status) => -1;
    }
}
