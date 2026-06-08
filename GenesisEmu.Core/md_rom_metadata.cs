namespace MDTracer
{
    // ROM header helpers adapted from Sandopolis (MIT License, pixel-clover/sandopolis).
    internal static class md_rom_metadata
    {
        public const int NTSC_LINES_PER_FRAME = 262;
        public const int NTSC_LINES_PER_FRAME_240 = 312;
        public const int PAL_LINES_PER_FRAME = 313;

        // Returns true for PAL-only, false for NTSC-only, or null when ambiguous.
        public static bool? InferPalModeFromCountryCodes(string in_country)
        {
            if (string.IsNullOrEmpty(in_country)) return null;

            bool w_uses_letter_codes = false;
            foreach (char w_raw in in_country)
            {
                char w_ch = char.ToUpperInvariant(w_raw);
                if (w_ch == 'E' || w_ch == 'U' || w_ch == 'J')
                {
                    w_uses_letter_codes = true;
                    break;
                }
            }

            bool w_pal_compatible = false;
            bool w_ntsc_compatible = false;
            foreach (char w_raw in in_country)
            {
                char w_ch = char.ToUpperInvariant(w_raw);
                if (w_uses_letter_codes)
                {
                    if (w_ch == '\0' || w_ch == ' ') continue;
                    if (w_ch == 'E') w_pal_compatible = true;
                    else if (w_ch == 'U' || w_ch == 'J') w_ntsc_compatible = true;
                    continue;
                }

                if (w_ch == '\0' || w_ch == ' ') continue;
                if (w_ch >= '0' && w_ch <= '9')
                {
                    int w_nibble = w_ch - '0';
                    if ((w_nibble & 0x8) != 0) w_pal_compatible = true;
                    if ((w_nibble & 0x5) != 0) w_ntsc_compatible = true;
                }
                else if (w_ch >= 'A' && w_ch <= 'F')
                {
                    int w_nibble = 10 + (w_ch - 'A');
                    if ((w_nibble & 0x8) != 0) w_pal_compatible = true;
                    if ((w_nibble & 0x5) != 0) w_ntsc_compatible = true;
                }
            }

            if (w_pal_compatible && !w_ntsc_compatible) return true;
            if (w_ntsc_compatible && !w_pal_compatible) return false;
            return null;
        }

        public static int LinesPerFrame(bool in_pal_mode, bool in_240_line_mode)
        {
            if (in_pal_mode) return PAL_LINES_PER_FRAME;
            return in_240_line_mode ? NTSC_LINES_PER_FRAME_240 : NTSC_LINES_PER_FRAME;
        }
    }
}
