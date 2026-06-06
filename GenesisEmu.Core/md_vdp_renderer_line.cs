namespace MDTracer
{
    internal partial class md_vdp
    {
        private void rendering_line_cpu()
        {
            int w_scanline = g_scanline;
            int w_display_xsize = g_display_xsize;
            int w_scroll_xcell = g_scroll_xcell;
            int w_scroll_xsize_mask = g_scroll_xsize_mask;
            int w_vscroll_mask = g_vdp_reg_11_2_vscroll == 1 ? 0x000f : 0xffff;
            VDP_LINE_SNAP w_line_snap = g_line_snap[w_scanline];
            uint[] w_renderer_vram = g_renderer_vram;
            uint[] w_game_cmap = g_game_cmap;
            uint[] w_game_primap = g_game_primap;
            uint[] w_game_plain = g_game_plain;
            uint[] w_game_shadowmap = g_game_shadowmap;
            bool w_view_screenB = g_overlay_view_screenB;
            bool w_view_screenA = g_overlay_view_screenA;
            bool w_view_screenS = g_overlay_view_screenS;
            bool w_view_screenW = g_overlay_view_screenW;
            bool w_shadow_enabled = g_vdp_reg_12_3_shadow != 0;
            bool w_screenA_low = g_overlay_screenA_Low;
            bool w_screenA_high = g_overlay_screenA_High;
            bool w_screenB_low = g_overlay_screenB_Low;
            bool w_screenB_high = g_overlay_screenB_High;
            bool w_screenW_low = g_overlay_screenW_Low;
            bool w_screenW_high = g_overlay_screenW_High;
            bool w_screenS_low = g_overlay_screenS_Low;
            bool w_screenS_high = g_overlay_screenS_High;
            bool w_overlay_enabled = w_screenA_low == true
                || w_screenA_high == true
                || w_screenB_low == true
                || w_screenB_high == true
                || w_screenW_low == true
                || w_screenW_high == true
                || w_screenS_low == true
                || w_screenS_high == true;

            Array.Clear(w_game_cmap, 0, w_display_xsize);
            Array.Clear(w_game_primap, 0, w_display_xsize);
            if (w_overlay_enabled == true) Array.Clear(w_game_plain, 0, w_display_xsize);
            if (w_shadow_enabled == true) Array.Clear(w_game_shadowmap, 0, w_display_xsize);
            //rendering the scroll screenB
            if (w_view_screenB == true)
            {
                int w_view_x = w_line_snap.hscrollB;
                int[] w_vscroll = w_line_snap.vscrollB;
                uint w_priority = 0;
                uint w_palette = 0;
                uint w_reverse = 0;
                uint w_char = 0;
                int w_view_addr = 0;
                int w_view_dx = 8;
                int w_view_dy = 0;
                int w_screen_adrdr = g_vdp_reg_4_scrollb >> 1;
                int w_pic_addr = 0;
                uint w_pic_w = 0;
                for (int wx = 0; wx < w_display_xsize; wx++)
                {
                    if ((wx & w_vscroll_mask) == 0)
                    {
                        int w_view_y = w_vscroll[wx >> 4];
                        w_view_dy = w_view_y & 7;
                        w_view_addr = w_screen_adrdr + ((w_view_y >> 3) * w_scroll_xcell);
                        w_view_dx = 8;
                    }
                    if (w_view_dx == 8)
                    {
                        w_view_x &= w_scroll_xsize_mask;
                        w_view_dx = w_view_x & 7;
                        uint w_val = w_renderer_vram[w_view_addr + (w_view_x >> 3)];
                        w_priority = ((w_val >> 15) & 0x0001);
                        w_palette = (((w_val >> 13) & 0x0003) << 4);
                        w_reverse = ((w_val >> 11) & 0x0003);
                        w_char = (w_val & 0x07ff);
                        w_pic_addr = (int)((w_reverse * VRAM_DATASIZE) + (w_char << 4) + (w_view_dy << 1));
                        w_pic_w = w_renderer_vram[w_pic_addr + (w_view_dx >> 2)];
                    }
                    else if ((w_view_dx & 3) == 0)
                    {
                        w_pic_w = w_renderer_vram[w_pic_addr + (w_view_dx >> 2)];
                    }
                    uint w_pic = (w_pic_w >> ((3 - (w_view_dx & 3)) << 2)) & 0x0f;
                    if (w_pic != 0)
                    {
                        w_game_cmap[wx] = w_palette + w_pic;
                        w_game_primap[wx] = w_priority;
                        if (w_overlay_enabled == true) w_game_plain[wx] = 2;
                    }
                    if (w_shadow_enabled == true) w_game_shadowmap[wx] = w_priority;
                    w_view_x += 1;
                    w_view_dx += 1;
                }
            }
            //rendering the scroll screenA
            if (w_view_screenA == true
                && g_screenA_bottom_y != 0
                && g_screenA_top_y <= w_scanline
                && w_scanline <= g_screenA_bottom_y
                && g_screenA_right_x != 0)
            {
                int w_view_x = w_line_snap.hscrollA;
                int[] w_vscroll = w_line_snap.vscrollA;
                uint w_priority = 0;
                uint w_palette = 0;
                uint w_reverse = 0;
                uint w_char = 0;
                int w_view_addr = 0;
                int w_view_dx = 8;
                int w_view_dy = 0;
                int w_screen_adrdr = g_vdp_reg_2_scrolla >> 1;
                int w_pic_addr = 0;
                int w_left = g_screenA_left_x;
                int w_right = g_screenA_right_x;
                uint w_pic_w = 0;
                for (int wx = 0; wx < w_display_xsize; wx++)
                {
                    if ((wx & w_vscroll_mask) == 0)
                    {
                        int w_view_y = w_vscroll[wx >> 4];
                        w_view_dy = w_view_y & 7;
                        w_view_addr = w_screen_adrdr + ((w_view_y >> 3) * w_scroll_xcell);
                        w_view_dx = 8;
                    }
                    if (w_view_dx == 8)
                    {
                        w_view_x &= w_scroll_xsize_mask;
                        w_view_dx = w_view_x & 7;
                        uint w_val = w_renderer_vram[w_view_addr + (w_view_x >> 3)];
                        w_priority = ((w_val >> 15) & 0x0001);
                        w_palette = (((w_val >> 13) & 0x0003) << 4);
                        w_reverse = ((w_val >> 11) & 0x0003);
                        w_char = (w_val & 0x07ff);
                        w_pic_addr = (int)((w_reverse * VRAM_DATASIZE) + (w_char << 4) + (w_view_dy << 1));
                        w_pic_w = w_renderer_vram[w_pic_addr + (w_view_dx >> 2)];
                    }
                    else if ((w_view_dx & 3) == 0)
                    {
                        w_pic_w = w_renderer_vram[w_pic_addr + (w_view_dx >> 2)];
                    }
                    if (w_left <= wx && wx <= w_right && w_game_primap[wx] <= w_priority)
                    {
                        uint w_pic = (w_pic_w >> ((3 - (w_view_dx & 3)) << 2)) & 0x0f;
                        if (w_pic != 0)
                        {
                            w_game_cmap[wx] = w_palette + w_pic;
                            w_game_primap[wx] = w_priority;
                            if (w_overlay_enabled == true) w_game_plain[wx] = 1;
                        }
                        if (w_shadow_enabled == true) w_game_shadowmap[wx] |= w_priority;
                    }
                    w_view_x += 1;
                    w_view_dx += 1;
                }
            }

            //rendering the sprite screen
            if (w_view_screenS == true)
            {
                int w_sprite_num = w_line_snap.sprite_rendrere_num;
                int[] w_sprite_left = w_line_snap.sprite_left;
                int[] w_sprite_top = w_line_snap.sprite_top;
                int[] w_sprite_xcell_size = w_line_snap.sprite_xcell_size;
                int[] w_sprite_ycell_size = w_line_snap.sprite_ycell_size;
                uint[] w_sprite_priority = w_line_snap.sprite_priority;
                uint[] w_sprite_palette = w_line_snap.sprite_palette;
                uint[] w_sprite_reverse = w_line_snap.sprite_reverse;
                uint[] w_sprite_char = w_line_snap.sprite_char;
                for (int w_sp = w_sprite_num - 1; w_sp >= 0; w_sp--)
                {
                    int w_left = w_sprite_left[w_sp];
                    int w_top = w_sprite_top[w_sp];
                    int w_xcell_size = w_sprite_xcell_size[w_sp];
                    int w_ycell_size = w_sprite_ycell_size[w_sp];
                    uint w_priority = w_sprite_priority[w_sp];
                    uint w_palette = w_sprite_palette[w_sp];
                    uint w_reverse = w_sprite_reverse[w_sp];
                    uint w_reverse_addr = VRAM_DATASIZE * w_reverse;
                    int w_char = (int)w_sprite_char[w_sp];
                    int w_y = w_scanline - w_top;
                    int w_ycell = w_y >> 3;
                    int w_cy = w_y & 7;
                    int w_render_ycell = (w_reverse & 2) == 0 ? w_ycell : w_ycell_size - w_ycell - 1;
                    bool w_reverse_x = (w_reverse & 1) != 0;
                    for (int w_cur_xcell = 0; w_cur_xcell < w_xcell_size; w_cur_xcell++)
                    {
                        int w_render_xcell = w_reverse_x == false ? w_cur_xcell : w_xcell_size - w_cur_xcell - 1;
                        int w_char_cur = w_char + (w_ycell_size * w_render_xcell) + w_render_ycell;
                        int w_cell_left = w_left + (w_cur_xcell << 3);
                        int w_start_x = w_cell_left;
                        int w_end_x = w_cell_left + 8;
                        if (w_start_x < 0) w_start_x = 0;
                        if (w_end_x > w_display_xsize) w_end_x = w_display_xsize;
                        if (w_start_x >= w_end_x) continue;

                        int w_row_addr = (int)(w_reverse_addr + (w_char_cur << 4) + (w_cy << 1));
                        uint w_pic_w = 0;
                        for (int w_posx = w_start_x; w_posx < w_end_x; w_posx++)
                        {
                            int w_cx = w_posx - w_cell_left;
                            if ((w_cx & 3) == 0 || w_posx == w_start_x)
                            {
                                w_pic_w = w_renderer_vram[w_row_addr + (w_cx >> 2)];
                            }
                            if (w_game_primap[w_posx] <= w_priority)
                            {
                                uint w_pic = (w_pic_w >> ((3 - (w_cx & 3)) << 2)) & 0x0f;
                                if (w_pic != 0)
                                {
                                    uint w_color = w_palette + w_pic;
                                    if (w_shadow_enabled == false)
                                    {
                                        w_game_cmap[w_posx] = w_color;
                                        w_game_primap[w_posx] = w_priority;
                                        if (w_overlay_enabled == true) w_game_plain[w_posx] = 4;
                                        g_vdp_status_5_collision = 1;
                                    }
                                    else if (w_color == 0x3e)
                                    {
                                        uint w_map = w_game_shadowmap[w_posx];
                                        if (w_map < 2) w_game_shadowmap[w_posx] = w_map + 1;
                                    }
                                    else if (w_color == 0x3f)
                                    {
                                        uint w_map = w_game_shadowmap[w_posx];
                                        if (w_map > 0) w_game_shadowmap[w_posx] = w_map - 1;
                                    }
                                    else if ((w_color & 0x0f) == 0x0e)
                                    {
                                        w_game_cmap[w_posx] = w_color;
                                        w_game_primap[w_posx] = w_priority;
                                        if (w_overlay_enabled == true) w_game_plain[w_posx] = 4;
                                        w_game_shadowmap[w_posx] = 0x1000;
                                        g_vdp_status_5_collision = 1;
                                    }
                                    else
                                    {
                                        w_game_cmap[w_posx] = w_color;
                                        w_game_primap[w_posx] = w_priority;
                                        if (w_overlay_enabled == true) w_game_plain[w_posx] = 4;
                                        w_game_shadowmap[w_posx] |= w_priority;
                                        g_vdp_status_5_collision = 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //rendering the window screen
            if (w_view_screenW == true)
            {
                int w_xcell_st = w_line_snap.window_x_st;
                int w_xcell_ed = w_line_snap.window_x_ed;
                if (w_xcell_st != w_xcell_ed)
                {
                    int w_view_dy = w_scanline & 7;
                    int w_addr = (g_vdp_reg_3_windows >> 1) + ((w_scanline >> 3) * w_scroll_xcell) + w_xcell_st;
                    int w_posx = w_xcell_st << 3;
                    for (int w_cx = w_xcell_st; w_cx <= w_xcell_ed; w_cx++)
                    {
                        uint w_val = w_renderer_vram[w_addr];
                        w_addr += 1;
                        uint w_priority = ((w_val >> 15) & 0x0001);
                        uint w_palette = (((w_val >> 13) & 0x0003) << 4);
                        uint w_reverse = ((w_val >> 11) & 0x0003) * VRAM_DATASIZE;
                        uint w_char = (w_val & 0x07ff);
                        int w_pic_addr = (int)(w_reverse + (w_char << 4) + (w_view_dy << 1));
                        uint w_pic_w = 0;
                        for (int w_dx = 0; w_dx < 8; w_dx++)
                        {
                            if ((w_dx & 3) == 0)
                            {
                                w_pic_w = w_renderer_vram[w_pic_addr + (w_dx >> 2)];
                            }
                            if ((w_game_cmap[w_posx] == 0) || (w_game_primap[w_posx] <= w_priority))
                            {
                                uint w_pic = (w_pic_w >> ((3 - (w_dx & 3)) << 2)) & 0x0f;
                                if (w_pic != 0)
                                {
                                    w_game_cmap[w_posx] = w_palette + w_pic;
                                    w_game_primap[w_posx] = w_priority;
                                    if (w_overlay_enabled == true) w_game_plain[w_posx] = 3;
                                }
                                if (w_shadow_enabled == true) w_game_shadowmap[w_posx] |= w_priority;
                            }
                            w_posx += 1;
                        }
                    }
                }
            }

            //rendering the game screen
            {
                uint color = 0;
                int w_base = w_scanline * w_display_xsize;
                uint[] w_game_screen = g_game_screen;
                uint[] w_color = g_color;
                uint[] w_color_shadow = g_color_shadow;
                uint[] w_color_highlight = g_color_highlight;
                uint w_backcolor = g_vdp_reg_7_backcolor;
                if (w_overlay_enabled == false)
                {
                    if (w_shadow_enabled == false)
                    {
                        for (int wx = 0; wx < w_display_xsize; wx++)
                        {
                            uint w_colnum = w_game_cmap[wx];
                            if (w_colnum == 0) w_colnum = w_backcolor;
                            w_game_screen[w_base + wx] = w_color[w_colnum];
                        }
                    }
                    else
                    {
                        for (int wx = 0; wx < w_display_xsize; wx++)
                        {
                            uint w_colnum = w_game_cmap[wx];
                            if (w_colnum == 0) w_colnum = w_backcolor;
                            uint w_shadow = w_game_shadowmap[wx];
                            if (w_shadow == 0) color = w_color_shadow[w_colnum];
                            else if (w_shadow == 2) color = w_color_highlight[w_colnum];
                            else color = w_color[w_colnum];
                            w_game_screen[w_base + wx] = color;
                        }
                    }
                    return;
                }

                for (int wx = 0; wx < w_display_xsize; wx++)
                {
                    uint w_colnum = w_game_cmap[wx];
                    if (w_colnum == 0) w_colnum = w_backcolor;
                    if (w_shadow_enabled == false)
                    {
                        color = w_color[w_colnum];
                    }
                    else
                    {
                        uint w_shadow = w_game_shadowmap[wx];
                        if (w_shadow == 0) color = w_color_shadow[w_colnum];
                        else
                        if (w_shadow == 2) color = w_color_highlight[w_colnum];
                        else color = w_color[w_colnum];
                    }
                    switch (w_game_plain[wx])
                    {
                        case 1:
                            if (w_game_primap[wx] == 0)
                            {
                                if (w_screenA_low == true)
                                {
                                    color = 0xff800000;
                                }
                            }
                            else
                            {
                                if (w_screenA_high == true)
                                {
                                    color = 0xffff0000;
                                }
                            }
                            break;
                        case 2:
                            if (w_game_primap[wx] == 0)
                            {
                                if (w_screenB_low == true)
                                {
                                    color = 0xff008000;
                                }
                            }
                            else
                            {
                                if (w_screenB_high == true)
                                {
                                    color = 0xff00ff00;
                                }
                            }
                            break;
                        case 3:
                            if (w_game_primap[wx] == 0)
                            {
                                if (w_screenW_low == true)
                                {
                                    color = 0xff808000;
                                }
                            }
                            else
                            {
                                if (w_screenW_high == true)
                                {
                                    color = 0xffffff00;
                                }
                            }
                            break;
                        case 4:
                            if (w_game_primap[wx] == 0)
                            {
                                if (w_screenS_low == true)
                                {
                                    color = 0xff008080;
                                }
                            }
                            else
                            {
                                if (w_screenS_high == true)
                                {
                                    color = 0xff00ffff;
                                }
                            }
                            break;
                    }
                    w_game_screen[w_base + wx] = color;
                }
            }

        }
    }
}
