namespace MDTracer
{
    internal partial class md_vdp
    {
        private uint[]? g_snapshot_render_vram;
        private VDP_LINE_SNAP[]? g_snapshot_render_line_snap;
        private uint[]? g_snapshot_render_color;
        private uint[]? g_snapshot_render_color_shadow;
        private uint[]? g_snapshot_render_color_highlight;

        private sealed class SnapshotRenderSavedState
        {
            public int scroll_xsize_mask;
            public int screenA_left_x;
            public int screenA_right_x;
            public int screenA_top_y;
            public int screenA_bottom_y;
        }

        private SnapshotRenderSavedState? g_snapshot_render_saved;

        internal void render_snapshot_frame_cpu()
        {
            g_snapshot_render_vram = g_snap_renderer_vram;
            g_snapshot_render_line_snap = g_snap_line_snap;
            g_snapshot_render_color = g_snap_color;
            g_snapshot_render_color_shadow = g_snap_color_shadow;
            g_snapshot_render_color_highlight = g_snap_color_highlight;

            g_snapshot_render_saved = new SnapshotRenderSavedState
            {
                scroll_xsize_mask = g_scroll_xsize_mask,
                screenA_left_x = g_screenA_left_x,
                screenA_right_x = g_screenA_right_x,
                screenA_top_y = g_screenA_top_y,
                screenA_bottom_y = g_screenA_bottom_y,
            };
            g_scroll_xsize_mask = g_snap_register.scroll_xsize - 1;
            g_screenA_left_x = (int)g_snap_register.screenA_left;
            g_screenA_right_x = (int)g_snap_register.screenA_right;
            g_screenA_top_y = (int)g_snap_register.screenA_top;
            g_screenA_bottom_y = (int)g_snap_register.screenA_bottom;

            int w_saved_scanline = g_scanline;
            int w_display_ysize = g_snap_register.display_ysize;
            for (int w_y = 0; w_y < w_display_ysize; w_y++)
            {
                g_scanline = w_y;
                rendering_line_cpu();
            }
            g_scanline = w_saved_scanline;

            if (g_snapshot_render_saved != null)
            {
                g_scroll_xsize_mask = g_snapshot_render_saved.scroll_xsize_mask;
                g_screenA_left_x = g_snapshot_render_saved.screenA_left_x;
                g_screenA_right_x = g_snapshot_render_saved.screenA_right_x;
                g_screenA_top_y = g_snapshot_render_saved.screenA_top_y;
                g_screenA_bottom_y = g_snapshot_render_saved.screenA_bottom_y;
                g_snapshot_render_saved = null;
            }

            g_snapshot_render_vram = null;
            g_snapshot_render_line_snap = null;
            g_snapshot_render_color = null;
            g_snapshot_render_color_shadow = null;
            g_snapshot_render_color_highlight = null;
        }

        private bool snapshot_cpu_render_active => g_snapshot_render_vram != null;

        private int active_vscroll_mask =>
            snapshot_cpu_render_active
                ? g_snap_register.scroll_mask
                : (g_vdp_reg_11_2_vscroll == 1 ? 0x000f : 0xffff);

        private bool active_shadow_enabled =>
            snapshot_cpu_render_active
                ? g_snap_register.vdp_reg_12_3_shadow != 0
                : g_vdp_reg_12_3_shadow != 0;

        private int active_scroll_b_addr =>
            snapshot_cpu_render_active
                ? g_snap_register.vdp_reg_4_scrollb
                : (int)(g_vdp_reg_4_scrollb >> 1);

        private int active_scroll_a_addr =>
            snapshot_cpu_render_active
                ? g_snap_register.vdp_reg_2_scrolla
                : (int)(g_vdp_reg_2_scrolla >> 1);

        private int active_windows_addr =>
            snapshot_cpu_render_active
                ? g_snap_register.vdp_reg_3_windows
                : (int)(g_vdp_reg_3_windows >> 1);

        private uint active_backcolor =>
            snapshot_cpu_render_active
                ? g_snap_register.vdp_reg_7_backcolor
                : g_vdp_reg_7_backcolor;
    }
}
