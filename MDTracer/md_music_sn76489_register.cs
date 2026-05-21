namespace MDTracer
{
    internal partial class md_sn76489
    {
        public void write8(byte in_val)
        {
            if ((in_val & 0x80) == 0x80)
            {
                int w_num = (in_val >> 5) & 0x03;
                if ((in_val & 0x10) == 0)
                {
                    //toon
                    if (w_num <= 2)
                    {
                        g_freq[w_num] = (g_freq[w_num] & 0x03f0) | (in_val & 0x0f);
                        if (g_freq[w_num] == 0) g_freq[w_num] = 1;
                        g_write_num_bk = w_num;
                    }
                    else
                    {
                        g_shift_reg = NOISEINITIAL;
                        g_freq[3] = 0x10 << (in_val & 0x3);
                        g_noise_mode = ((in_val & 0x04) == 0) ? false : true;
                        g_write_num_bk = -1;
                    }
                    RefreshPsgDisplayFrequency(w_num);
                }
                else
                {
                    //vol
                    g_vol[w_num] = VOL_MAP[in_val & 0x0f];
                    g_write_num_bk = -1;
                    RefreshPsgDisplayFrequency(w_num);
                }
            }
            else
            {
                if (g_write_num_bk != -1)
                {
                    int w_num = g_write_num_bk;
                    g_freq[w_num] = (g_freq[w_num] & 0x000f) | ((in_val & 0x3f) << 4);
                    if (g_freq[w_num] == 0) g_freq[w_num] = 1;
                    g_write_num_bk = -1;
                    RefreshPsgDisplayFrequency(w_num);
                }
            }
        }

        private void RefreshPsgDisplayFrequency(int in_ch)
        {
            if ((in_ch < 0) || (3 < in_ch)) return;

            if (g_vol[in_ch] == 0)
            {
                g_display_freq[in_ch] = 0;
                md_main.g_md_music.ClearChannelDisplay(6 + in_ch);
                return;
            }

            int w_freq = g_freq[in_ch];
            if ((in_ch == NOISE_CHANNEL) && (w_freq == 0x80))
            {
                w_freq = g_freq[2];
            }
            if (w_freq <= 0) w_freq = 1;

            g_display_freq[in_ch] = (int)(PSG_CLOCK / ((w_freq + 1) << 4));
        }
    }
}
