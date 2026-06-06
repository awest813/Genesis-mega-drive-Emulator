using NAudio.Wave;
using System;
using System.Runtime.CompilerServices;


namespace MDTracer
{
    internal partial class md_music
    {
        //const
        const int SAMPLING = 44100;
        const float CPU_CLOCK = 7670453.0f;
        const int BIT = 16;
        const int CHANNELS = 2;
        const int BUFSIZE = 1024;
        const short SILENCE_THRESHOLD = 16;
        const int DISPLAY_CHANNELS = 10;
        const int DISPLAY_HOLD_SAMPLES = SAMPLING / 45;
        const int DISPLAY_SIGNAL_THRESHOLD = 1;
        const float CLOCK_PER_SAMPLE = CPU_CLOCK / SAMPLING;
        public bool[] g_master_chk;
        public int[] g_master_vol;
        public float[] g_out_vol;
        public int[] g_freq_out;
        private int[] g_display_hold = Array.Empty<int>();

        private BufferedWaveProvider g_bufferedwaveprovider;
        private WaveOutEvent g_waveOut;

        private byte[] g_buffer;
        private int g_buffer_cur = 0;
        private float g_clock_total;
        public md_sn76489 g_md_sn76489;
        public md_ym2612 g_md_ym2612;
        //----------------------------------------------------------------
        public md_music()
        {
            initialize();
        }
        public void initialize()
        {
            g_master_chk = new bool[11];
            g_master_vol = new int[11];
            g_out_vol = new float[11];
            g_freq_out = new int[11];
            g_display_hold = new int[DISPLAY_CHANNELS];

            g_bufferedwaveprovider = new BufferedWaveProvider(new WaveFormat(SAMPLING, BIT, CHANNELS));
            g_bufferedwaveprovider.BufferDuration = TimeSpan.FromMilliseconds(200);
            g_waveOut = new WaveOutEvent();
            g_waveOut.DesiredLatency = 100;
            g_waveOut.Init(g_bufferedwaveprovider);
            g_buffer = new byte[BUFSIZE];

            g_md_sn76489 = new md_sn76489();
            g_md_ym2612 = new md_ym2612();
            g_md_sn76489.SN76489_Start();
            g_md_ym2612.YM2612_Start();
            g_waveOut.Play();
        }
        public void reset()
        {
            g_bufferedwaveprovider.ClearBuffer();
            Array.Clear(g_buffer, 0, g_buffer.Length);
            Array.Clear(g_freq_out, 0, g_freq_out.Length);
            Array.Clear(g_display_hold, 0, g_display_hold.Length);
            g_buffer_cur = 0;
            g_clock_total = 0;
            g_md_sn76489.SN76489_Start();
            g_md_ym2612.YM2612_Start();
        }
        public void setting()
        {
            for(int i = 0; i <= 9; i++)
            {
                g_out_vol[i] = 0;
            }
            if (g_master_chk[10] == true)
            {
                float w_master = g_master_vol[10] / 100.0f;
                for (int i = 0; i <= 9; i++)
                {
                    if (g_master_chk[i] == true) g_out_vol[i] = (g_master_vol[i] / 100.0f) * w_master;
                }
            }
            for (int i = 0; i < DISPLAY_CHANNELS; i++)
            {
                if (g_out_vol[i] <= 0.0f) ClearChannelDisplay(i);
            }
        }

        public void UpdateChannelDisplayFromSignal(int in_channel, int in_freq, int in_output, bool in_audible)
        {
            if (IsDisplayChannel(in_channel) == false) return;
            if ((in_audible == false) || (in_freq <= 0) || (g_out_vol[in_channel] <= 0.0f))
            {
                ClearChannelDisplay(in_channel);
                return;
            }

            double w_level = Math.Abs(in_output * (double)g_out_vol[in_channel]);
            if (w_level <= DISPLAY_SIGNAL_THRESHOLD)
            {
                DecayChannelDisplay(in_channel);
                return;
            }

            g_display_hold[in_channel] = DISPLAY_HOLD_SAMPLES;
            g_freq_out[in_channel] = in_freq;
        }

        public void ClearChannelDisplay(int in_channel)
        {
            if (IsDisplayChannel(in_channel) == false) return;
            g_display_hold[in_channel] = 0;
            g_freq_out[in_channel] = 0;
        }

        private bool IsDisplayChannel(int in_channel)
        {
            return (0 <= in_channel) && (in_channel < DISPLAY_CHANNELS);
        }

        private void DecayChannelDisplay(int in_channel)
        {
            if (0 < g_display_hold[in_channel])
            {
                g_display_hold[in_channel]--;
                return;
            }

            g_freq_out[in_channel] = 0;
        }

        public void run(int in_clock)
        {
            g_clock_total += in_clock;
            byte[] w_buffer = g_buffer;
            int w_buffer_cur = g_buffer_cur;
            md_ym2612 w_ym2612 = g_md_ym2612;
            md_sn76489 w_sn76489 = g_md_sn76489;
            BufferedWaveProvider w_bufferedwaveprovider = g_bufferedwaveprovider;
            while (CLOCK_PER_SAMPLE <= g_clock_total)
            {
                g_clock_total -= CLOCK_PER_SAMPLE;

                var result = w_ym2612.YM2612_Update();
                int result2 = w_sn76489.SN76489_Update();
                short w_mix_total1 = Clip16(result.out1 + result2);
                short w_mix_total2 = Clip16(result.out2 + result2);
                SilenceGate16(ref w_mix_total1, ref w_mix_total2);

                w_buffer[w_buffer_cur + 1] = (byte)(w_mix_total1 >> 8);
                w_buffer[w_buffer_cur] = (byte)w_mix_total1;
                w_buffer[w_buffer_cur + 3] = (byte)(w_mix_total2 >> 8);
                w_buffer[w_buffer_cur + 2] = (byte)w_mix_total2;
                w_buffer_cur += 4;
                if (BUFSIZE <= w_buffer_cur)
                {
                    WaitForBufferSpace(w_bufferedwaveprovider);
                    w_bufferedwaveprovider.AddSamples(w_buffer, 0, BUFSIZE);
                    md_main.g_audioFrontendHooks.OnAudioSamplesProduced(w_buffer, 0, BUFSIZE);
                    w_buffer_cur = 0;
                }
            }
            g_buffer_cur = w_buffer_cur;
        }

        private void WaitForBufferSpace(BufferedWaveProvider in_bufferedwaveprovider)
        {
            while (in_bufferedwaveprovider.BufferedBytes + BUFSIZE > in_bufferedwaveprovider.BufferLength)
            {
                System.Threading.Thread.Sleep(1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short Clip16(int in_val)
        {
            if (in_val > short.MaxValue) return short.MaxValue;
            if (in_val < short.MinValue) return short.MinValue;
            return (short)in_val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SilenceGate16(ref short in_left, ref short in_right)
        {
            if ((in_left > -SILENCE_THRESHOLD) && (in_left < SILENCE_THRESHOLD)
                && (in_right > -SILENCE_THRESHOLD) && (in_right < SILENCE_THRESHOLD))
            {
                in_left = 0;
                in_right = 0;
            }
        }

    }
}
