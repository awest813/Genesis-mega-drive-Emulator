using Microsoft.VisualBasic.Devices;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Xml.Linq;


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
        const float CLOCK_PER_SAMPLE = CPU_CLOCK / SAMPLING;
        public bool[] g_master_chk;
        public int[] g_master_vol;
        public float[] g_out_vol;
        public int[] g_freq_out;

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
        }

        public void run(int in_clock)
        {
            g_clock_total += in_clock;
            while (CLOCK_PER_SAMPLE <= g_clock_total)
            {
                g_clock_total -= CLOCK_PER_SAMPLE;

                var result = g_md_ym2612.YM2612_Update();
                int result2 = g_md_sn76489.SN76489_Update();
                short w_mix_total1 = Clip16(result.out1 + result2);
                short w_mix_total2 = Clip16(result.out2 + result2);
                SilenceGate16(ref w_mix_total1, ref w_mix_total2);

                g_buffer[g_buffer_cur + 1] = (byte)((short)w_mix_total1 >> 8);
                g_buffer[g_buffer_cur + 0] = (byte)((short)w_mix_total1 & 0xff);
                g_buffer[g_buffer_cur + 3] = (byte)((short)w_mix_total2 >> 8);
                g_buffer[g_buffer_cur + 2] = (byte)((short)w_mix_total2 & 0xff);
                g_buffer_cur += 4;
                if (BUFSIZE <= g_buffer_cur)
                {
                    WaitForBufferSpace();
                    g_bufferedwaveprovider.AddSamples(g_buffer, 0, BUFSIZE);
                    Form_Main.Instance.videoRecordingAddAudioSamples(g_buffer, 0, BUFSIZE);
                    g_buffer_cur = 0;
                }
            }
        }

        private void WaitForBufferSpace()
        {
            while (g_bufferedwaveprovider.BufferedBytes + BUFSIZE > g_bufferedwaveprovider.BufferLength)
            {
                System.Threading.Thread.Sleep(1);
            }
        }

        private short Clip16(int in_val)
        {
            if (in_val > short.MaxValue) return short.MaxValue;
            if (in_val < short.MinValue) return short.MinValue;
            return (short)in_val;
        }

        private void SilenceGate16(ref short in_left, ref short in_right)
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
