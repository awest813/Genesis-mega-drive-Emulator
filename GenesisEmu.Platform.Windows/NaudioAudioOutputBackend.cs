using NAudio.Wave;

namespace MDTracer.Platform.Windows
{
    internal sealed class NaudioAudioOutputBackend : IAudioOutputBackend
    {
        private BufferedWaveProvider? g_bufferedwaveprovider;
        private WaveOutEvent? g_waveOut;
        private int g_bufferSizeBytes;

        public int BufferedBytes => g_bufferedwaveprovider?.BufferedBytes ?? 0;
        public int BufferCapacity => g_bufferedwaveprovider?.BufferLength ?? 0;

        public void Initialize(int in_sampleRate, int in_bitsPerSample, int in_channels, int in_bufferSizeBytes)
        {
            g_bufferSizeBytes = in_bufferSizeBytes;
            g_bufferedwaveprovider = new BufferedWaveProvider(new WaveFormat(in_sampleRate, in_bitsPerSample, in_channels));
            g_bufferedwaveprovider.BufferDuration = TimeSpan.FromMilliseconds(200);
            g_waveOut = new WaveOutEvent();
            g_waveOut.DesiredLatency = 100;
            g_waveOut.Init(g_bufferedwaveprovider);
        }

        public void Play()
        {
            g_waveOut?.Play();
        }

        public void ClearBuffer()
        {
            g_bufferedwaveprovider?.ClearBuffer();
        }

        public bool TryEnqueueSamples(byte[] in_buffer, int in_offset, int in_count)
        {
            if (g_bufferedwaveprovider == null) return true;

            while (g_bufferedwaveprovider.BufferedBytes + in_count > g_bufferedwaveprovider.BufferLength)
            {
                Thread.Sleep(1);
            }

            g_bufferedwaveprovider.AddSamples(in_buffer, in_offset, in_count);
            return true;
        }
    }
}
