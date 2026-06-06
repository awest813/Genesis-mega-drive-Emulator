namespace MDTracer
{
    internal partial class md_vdp
    {
        public void initialize()
        {
            g_vram = new byte[65536];
            g_cram = new ushort[64];
            g_vsram = new ushort[40];
            g_color = new uint[COLOR_MAX];
            g_color_shadow = new uint[COLOR_MAX];
            g_color_highlight = new uint[COLOR_MAX];

            g_pattern_chk = new bool[PATTERN_MAX];
            g_game_cmap = new uint[DISPLAY_XSIZE];
            g_game_primap = new uint[DISPLAY_XSIZE];
            g_game_plain = new uint[DISPLAY_XSIZE];
            g_game_shadowmap = new uint[DISPLAY_XSIZE];

            g_game_screen = new uint[DISPLAY_BUFSIZE];
            g_renderer_vram = new uint[VRAM_DATASIZE * 4];

            g_snap_register = new VDP_REGISTER();
            g_line_snap = new VDP_LINE_SNAP[DISPLAY_YSIZE];
            for(int i = 0; i < DISPLAY_YSIZE; i++)
            {
                g_line_snap[i].vscrollA = new int[VSRAM_DATASIZE];
                g_line_snap[i].vscrollB = new int[VSRAM_DATASIZE];
                g_line_snap[i].sprite_left = new int[MAX_SPRITE];
                g_line_snap[i].sprite_right = new int[MAX_SPRITE];
                g_line_snap[i].sprite_top = new int[MAX_SPRITE];
                g_line_snap[i].sprite_bottom = new int[MAX_SPRITE];
                g_line_snap[i].sprite_xcell_size = new int[MAX_SPRITE];
                g_line_snap[i].sprite_ycell_size = new int[MAX_SPRITE];
                g_line_snap[i].sprite_priority = new uint[MAX_SPRITE];
                g_line_snap[i].sprite_palette = new uint[MAX_SPRITE];
                g_line_snap[i].sprite_reverse = new uint[MAX_SPRITE];
                g_line_snap[i].sprite_char = new uint[MAX_SPRITE];
            }
            g_snap_line_snap = new VDP_LINE_SNAP[DISPLAY_YSIZE];
            for (int i = 0; i < DISPLAY_YSIZE; i++)
            {
                g_snap_line_snap[i].vscrollA = new int[VSRAM_DATASIZE];
                g_snap_line_snap[i].vscrollB = new int[VSRAM_DATASIZE];
                g_snap_line_snap[i].sprite_left = new int[MAX_SPRITE];
                g_snap_line_snap[i].sprite_right = new int[MAX_SPRITE];
                g_snap_line_snap[i].sprite_top = new int[MAX_SPRITE];
                g_snap_line_snap[i].sprite_bottom = new int[MAX_SPRITE];
                g_snap_line_snap[i].sprite_xcell_size = new int[MAX_SPRITE];
                g_snap_line_snap[i].sprite_ycell_size = new int[MAX_SPRITE];
                g_snap_line_snap[i].sprite_priority = new uint[MAX_SPRITE];
                g_snap_line_snap[i].sprite_palette = new uint[MAX_SPRITE];
                g_snap_line_snap[i].sprite_reverse = new uint[MAX_SPRITE];
                g_snap_line_snap[i].sprite_char = new uint[MAX_SPRITE];
            }

            g_snap_renderer_vram = new uint[VRAM_DATASIZE * 4];
            g_snap_color = new uint[64];
            g_snap_color_shadow = new uint[64];
            g_snap_color_highlight = new uint[64];

            g_scrollA_bitmap = new Bitmap(1024, 1024);
            g_scrollB_bitmap = new Bitmap(1024, 1024);
            g_scrollW_bitmap = new Bitmap(1024, 1024);
            g_scrollS_bitmap = new Bitmap(512, 512);
            g_pattern_table = new Bitmap(128, 1024);
            g_sprite_enable = new bool[80];
            MONOCOLOR_TABLE = new uint[16];
            for (uint i = 0; i <= 15; i++)
            {
                uint w_clor = i << 4;
                MONOCOLOR_TABLE[i] = (uint)(0xff000000
                                        | w_clor << 16
                                        | w_clor << 8
                                        | w_clor);
            }
            g_display_xsize = 320;
            g_display_ysize = 224;
            g_scroll_xcell = 32;
            g_scroll_ycell = 32;
            g_scroll_xsize = 256;
            g_scroll_ysize = 256;
            g_scroll_xsize_mask = 0x00ff;
            g_scroll_ysize_mask = 0x00ff;
            g_vertical_line_max = 262;

            //VDP control port
            g_vdp_status_9_empl = 1;            //const
            g_vdp_status_8_full = 0;            //const
            g_vdp_status_7_vinterrupt = 0;
            g_vdp_status_6_sprite = 0;
            g_vdp_status_5_collision = 0;
            g_vdp_status_4_frame = 0;
            g_vdp_status_3_vbrank = 0;
            g_vdp_status_2_hbrank = 0;      //const
            g_vdp_status_1_dma = 0;             //const
            g_vdp_status_0_tvmode = 0;

            g_vdp_reg = new byte[24];
            g_vdp_reg_2_scrolla = 0xffff;
            g_vdp_reg_3_windows = 0xffff;
            g_vdp_reg_4_scrollb = 0xffff;

            g_scanline = 0;
            g_hinterrupt_counter = -1;
        }
        public void reset()
        {
            Array.Clear(g_vram, 0, g_vram.Length);
            Array.Clear(g_cram, 0, g_cram.Length);
            Array.Clear(g_vsram, 0, g_vsram.Length);
            Array.Clear(g_color, 0, g_color.Length);
            Array.Clear(g_color_shadow, 0, g_color_shadow.Length);
            Array.Clear(g_color_highlight, 0, g_color_highlight.Length);
            Array.Clear(g_pattern_chk, 0, g_pattern_chk.Length);
            Array.Clear(g_game_cmap, 0, g_game_cmap.Length);
            Array.Clear(g_game_primap, 0, g_game_primap.Length);
            Array.Clear(g_game_plain, 0, g_game_plain.Length);
            Array.Clear(g_game_shadowmap, 0, g_game_shadowmap.Length);
            Array.Clear(g_game_screen, 0, g_game_screen.Length);
            Array.Clear(g_renderer_vram, 0, g_renderer_vram.Length);
            Array.Clear(g_snap_renderer_vram, 0, g_snap_renderer_vram.Length);
            Array.Clear(g_snap_color, 0, g_snap_color.Length);
            Array.Clear(g_snap_color_shadow, 0, g_snap_color_shadow.Length);
            Array.Clear(g_snap_color_highlight, 0, g_snap_color_highlight.Length);
            Array.Clear(g_sprite_enable, 0, g_sprite_enable.Length);
            ClearLineSnap(g_line_snap);
            ClearLineSnap(g_snap_line_snap);

            g_display_xsize = 320;
            g_display_ysize = 224;
            g_display_xcell = 40;
            g_display_ycell = 28;
            g_scroll_xcell = 32;
            g_scroll_ycell = 32;
            g_scroll_xsize = 256;
            g_scroll_ysize = 256;
            g_scroll_xsize_mask = 0x00ff;
            g_scroll_ysize_mask = 0x00ff;
            g_vertical_line_max = 262;
            g_screenA_left_x = 0;
            g_screenA_right_x = 0;
            g_screenA_top_y = 0;
            g_screenA_bottom_y = 0;
            g_max_sprite_num = 0;
            g_max_sprite_line = 0;
            g_max_sprite_cell = 0;
            g_sprite_vmask = 0;

            g_vdp_status_9_empl = 1;
            g_vdp_status_8_full = 0;
            g_vdp_status_7_vinterrupt = 0;
            g_vdp_status_6_sprite = 0;
            g_vdp_status_5_collision = 0;
            g_vdp_status_4_frame = 0;
            g_vdp_status_3_vbrank = 0;
            g_vdp_status_2_hbrank = 0;
            g_vdp_status_1_dma = 0;
            g_vdp_status_0_tvmode = 0;
            g_vdp_c00008_hvcounter = 0;
            g_vdp_c00008_hvcounter_latched = false;

            Array.Clear(g_vdp_reg, 0, g_vdp_reg.Length);
            g_vdp_reg_0_4_hinterrupt = 0;
            g_vdp_reg_0_1_hvcounter = 0;
            g_vdp_reg_1_6_display = 0;
            g_vdp_reg_1_5_vinterrupt = 0;
            g_vdp_reg_1_4_dma = 0;
            g_vdp_reg_1_3_cellmode = 0;
            g_vdp_reg_2_scrolla = 0xffff;
            g_vdp_reg_3_windows = 0xffff;
            g_vdp_reg_4_scrollb = 0xffff;
            g_vdp_reg_5_sprite = 0;
            g_vdp_reg_7_backcolor = 0;
            g_vdp_reg_10_hint = 0;
            g_vdp_reg_11_3_ext = 0;
            g_vdp_reg_11_2_vscroll = 0;
            g_vdp_reg_11_1_hscroll = 0;
            g_vdp_reg_12_7_cellmode1 = 0;
            g_vdp_reg_12_3_shadow = 0;
            g_vdp_reg_12_2_interlacemode = 0;
            g_vdp_reg_12_0_cellmode2 = 0;
            g_vdp_reg_13_hscroll = 0;
            g_vdp_reg_15_autoinc = 0;
            g_vdp_reg_16_5_scrollV = 0;
            g_vdp_reg_16_1_scrollH = 0;
            g_vdp_reg_17_7_windows = 0;
            g_vdp_reg_17_4_basspointer = 0;
            g_vdp_reg_18_7_windows = 0;
            g_vdp_reg_18_4_basspointer = 0;
            g_vdp_reg_19_dma_counter_low = 0;
            g_vdp_reg_20_dma_counter_high = 0;
            g_vdp_reg_21_dma_source_low = 0;
            g_vdp_reg_22_dma_source_mid = 0;
            g_vdp_reg_23_dma_mode = 0;
            g_vdp_reg_23_5_dma_high = 0;

            g_vdp_reg_code = 0;
            g_vdp_reg_dest_address = 0;
            g_command_select = false;
            g_command_word = 0;
            g_dma_mode = 0;
            g_dma_src_addr = 0;
            g_dma_leng = 0;
            g_dma_fill_req = false;
            g_dma_fill_data = 0;

            g_scanline = 0;
            g_hinterrupt_counter = -1;
        }
        private void ClearLineSnap(VDP_LINE_SNAP[] in_line_snap)
        {
            for (int i = 0; i < in_line_snap.Length; i++)
            {
                in_line_snap[i].hscrollA = 0;
                in_line_snap[i].hscrollB = 0;
                in_line_snap[i].window_x_st = 0;
                in_line_snap[i].window_x_ed = 0;
                in_line_snap[i].sprite_rendrere_num = 0;
                Array.Clear(in_line_snap[i].vscrollA, 0, in_line_snap[i].vscrollA.Length);
                Array.Clear(in_line_snap[i].vscrollB, 0, in_line_snap[i].vscrollB.Length);
                Array.Clear(in_line_snap[i].sprite_left, 0, in_line_snap[i].sprite_left.Length);
                Array.Clear(in_line_snap[i].sprite_right, 0, in_line_snap[i].sprite_right.Length);
                Array.Clear(in_line_snap[i].sprite_top, 0, in_line_snap[i].sprite_top.Length);
                Array.Clear(in_line_snap[i].sprite_bottom, 0, in_line_snap[i].sprite_bottom.Length);
                Array.Clear(in_line_snap[i].sprite_xcell_size, 0, in_line_snap[i].sprite_xcell_size.Length);
                Array.Clear(in_line_snap[i].sprite_ycell_size, 0, in_line_snap[i].sprite_ycell_size.Length);
                Array.Clear(in_line_snap[i].sprite_priority, 0, in_line_snap[i].sprite_priority.Length);
                Array.Clear(in_line_snap[i].sprite_palette, 0, in_line_snap[i].sprite_palette.Length);
                Array.Clear(in_line_snap[i].sprite_reverse, 0, in_line_snap[i].sprite_reverse.Length);
                Array.Clear(in_line_snap[i].sprite_char, 0, in_line_snap[i].sprite_char.Length);
            }
        }
    }
}
