namespace MDTracer
{
    internal partial class md_vdp
    {
        public bool IsPalMode()
        {
            return g_vdp_status_0_tvmode != 0;
        }

        public void ApplyTimingMode(bool in_pal_mode)
        {
            g_vdp_status_0_tvmode = (byte)(in_pal_mode ? 1 : 0);
            UpdateVerticalLineMax();
        }

        internal void UpdateVerticalLineMax()
        {
            g_vertical_line_max = md_rom_metadata.LinesPerFrame(
                IsPalMode(),
                g_vdp_reg_1_3_cellmode != 0);
        }
    }
}
