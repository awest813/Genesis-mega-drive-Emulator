namespace MDTracer
{
    internal partial class md_vdp
    {
        private const int PATTERN_MAX = 2048;
        private const int DISPLAY_XSIZE = 320;
        private const int DISPLAY_YSIZE = 240;
        private const int DISPLAY_BUFSIZE = DISPLAY_XSIZE * DISPLAY_YSIZE;
        public int SPRITE_XSIZE = 512;
        public int SPRITE_YSIZE = 512;
        private const int VRAM_DATASIZE = 65536 / 2;
        private const int VSRAM_DATASIZE = 20;
        private const int COLOR_MAX = 64;
        private const int MAX_SPRITE = 20;

        private bool[] g_pattern_chk;
        private uint[] g_renderer_vram;
        private VDP_LINE_SNAP[] g_line_snap;
        private uint[] g_game_cmap;
        private uint[] g_game_primap;
        private uint[] g_game_plain;      //0:back,1:A,2:B,3:W,4:S
        private uint[] g_game_shadowmap;

        private VDP_REGISTER g_snap_register;
        private uint[] g_snap_renderer_vram;
        private VDP_LINE_SNAP[] g_snap_line_snap;
        private uint[] g_snap_color;
        private uint[] g_snap_color_shadow;
        private uint[] g_snap_color_highlight;

        public uint[] g_game_screen;

        public int g_display_xsize;
        public int g_display_ysize;
        private int g_display_xcell;
        private int g_display_ycell;
        private int g_scroll_xcell;
        private int g_scroll_ycell;
        public int g_scroll_xsize;
        public int g_scroll_ysize;
        private int g_scroll_xsize_mask;
        private int g_scroll_ysize_mask;
        public int g_vertical_line_max;

        private int g_screenA_left_x;
        private int g_screenA_right_x;
        private int g_screenA_top_y;
        private int g_screenA_bottom_y;

        private int g_max_sprite_num;
        private int g_max_sprite_line;
        private int g_max_sprite_cell;
        private int g_sprite_vmask;

        public bool rendering_gpu;
        public ManualResetEvent g_waitHandle;

        private void rendering_line()
        {
            if (g_vdp_reg_1_6_display == 1)
            {
                rendering_line_snap();
                if (rendering_gpu == false)
                {
                    rendering_line_cpu();
                }
            }
            else
            {
                if (rendering_gpu == false)
                {
                    int w_pos = g_scanline * g_display_xsize;
                    for (int wx = 0; wx < g_display_xsize; wx++)
                    {
                        g_game_screen[w_pos] = 0;
                        w_pos += 1;
                    }
                }
            }
        }
        private void rendering_frame()
        {
            if (g_waitHandle.WaitOne(0) == false)
            {
                bool w_snapshot_required = is_rendering_snapshot_required();
                if ((rendering_gpu == true) && (w_snapshot_required == true))
                {
                    g_gpuRenderer.InitializeIfNeeded();
                    g_gpuRenderer.StageFrameData(
                        g_snap_renderer_vram,
                        g_snap_color,
                        g_snap_color_shadow,
                        g_snap_color_highlight,
                        g_snap_line_snap);
                }
                if (w_snapshot_required == true)
                {
                    rendering_frame_snap();
                }
                g_waitHandle.Set();
            }
        }
        private bool is_rendering_snapshot_required()
        {
            return rendering_gpu == true || is_rendering_data_required() == true;
        }
        private static bool is_rendering_data_required()
        {
            return md_main.g_debugView.IsAnyLayerViewerEnabled;
        }
        public void run_event()
        {
            while (true)
            {
                g_waitHandle.WaitOne(Timeout.Infinite);
                g_waitHandle.Reset();
                if ((md_main.is_stop_requested() == true) && (md_main.consume_frame_advance_update_request() == false)) continue;
                if (rendering_gpu == true)
                {
                    g_gpuRenderer.InitializeIfNeeded();
                    if (g_snap_register.vdp_reg_1_6_display == 1)
                    {
                        g_gpuRenderer.Render(in g_snap_register, g_display_ysize);
                        g_gpuRenderer.DownloadScreen(g_game_screen);
                    }
                    else
                    {
                        for (int w_pos = 0; w_pos < DISPLAY_BUFSIZE; w_pos++)
                        {
                            g_game_screen[w_pos] = 0;
                        }
                    }
                }
                md_main.Screen_Game_Update();
                if (is_rendering_data_required() == true)
                {
                    rendering_data();
                }
                md_main.Screen_Update();
            }
        }
    }
}
