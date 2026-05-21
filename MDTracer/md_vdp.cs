namespace MDTracer
{
    //----------------------------------------------------------------
    //VDP : chips:315-5313
    //----------------------------------------------------------------
    internal partial class md_vdp : System.IDisposable
    {
        public int g_scanline;
        private int g_hinterrupt_counter;
        private bool g_external_interrupt_req;
        private int g_external_interrupt_x;
        private int g_external_interrupt_y;
        //----------------------------------------------------------------
        public md_vdp()
        {
            initialize();
        }
        public void Dispose()
        {
            dx_rendering_dispose();
            g_waitHandle?.Dispose();
            g_scrollA_bitmap?.Dispose();
            g_scrollB_bitmap?.Dispose();
            g_scrollW_bitmap?.Dispose();
            g_scrollS_bitmap?.Dispose();
            g_pattern_table?.Dispose();
        }
        public void request_external_interrupt(int in_x, int in_y)
        {
            g_external_interrupt_x = in_x;
            g_external_interrupt_y = in_y;
            g_external_interrupt_req = true;
            g_vdp_c00008_hvcounter_latched = false;
        }
        private static void report_vdp_warning(string in_message)
        {
            System.Diagnostics.Debug.WriteLine("[VDP] " + in_message);
        }
        public void run(int in_vline)
        {
            g_scanline = in_vline;
            if (g_scanline == 0)
            {
                rendering_line();
                set_hinterrupt();
                interrupt_check();
            }
            else
            if (g_scanline < g_display_ysize)
            {
                rendering_line();
                interrupt_check();
            }
            else
            if (g_scanline == g_display_ysize)
            {
                rendering_frame();
                interrupt_check();
                g_vdp_status_3_vbrank = 1;
                md_main.g_md_m68k.g_interrupt_V_req = true;
                md_main.g_md_vdp.g_vdp_status_7_vinterrupt = 1;
                md_main.g_md_z80.irq_request(true);
            }
            else
            if (g_scanline == g_vertical_line_max - 1)
            {
                g_vdp_status_3_vbrank = 0;
                g_vdp_status_7_vinterrupt = 0;
                md_main.g_md_z80.irq_request(false);

                g_vdp_status_4_frame = (byte)((g_vdp_status_4_frame == 0) ? 1 : 0);
                g_vdp_status_5_collision = 0;
                g_vdp_status_6_sprite = 0;
            }
        }
        private void set_hvcounter(int in_x, int in_y)
        {
            if (g_vdp_reg_12_2_interlacemode == 0)
            {
                g_vdp_c00008_hvcounter = (ushort)(((in_x >> 1) & 0x00ff)
                                            + (in_y << 8));
            }
            else
            {
                g_vdp_c00008_hvcounter = (ushort)(((in_x >> 1) & 0x00ff)
                                            + ((in_y << 8) & 0xfe00)
                                            + (in_y & 0x0100));
            }
        }
        private void set_hinterrupt()
        {
            g_hinterrupt_counter = g_vdp_reg_10_hint;
        }
        private void interrupt_check()
        {
            g_hinterrupt_counter -= 1;
            if (g_hinterrupt_counter < 0)
            {
                md_main.g_md_m68k.g_interrupt_H_req = true;
                set_hinterrupt();
            }

            if (g_external_interrupt_req == true)
            {
                g_external_interrupt_req = false;
                if (g_vdp_reg_11_3_ext == 1)
                {
                    md_main.g_md_m68k.g_interrupt_EXT_req = true;
                    if ((g_vdp_reg_0_1_hvcounter == 1) && (g_vdp_c00008_hvcounter_latched == false))
                    {
                        set_hvcounter(g_external_interrupt_x, g_external_interrupt_y);
                        g_vdp_c00008_hvcounter_latched = true;
                    }
                }
            }
        }
    }
}
