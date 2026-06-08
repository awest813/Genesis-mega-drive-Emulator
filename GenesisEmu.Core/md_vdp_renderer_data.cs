namespace MDTracer
{
    internal partial class md_vdp
    {
        public uint[] g_scrollA_pixels;
        public uint[] g_scrollB_pixels;
        public uint[] g_scrollW_pixels;
        public uint[] g_scrollS_pixels;
        public uint[] g_pattern_pixels;
        private uint[] MONOCOLOR_TABLE;
        private bool[] g_sprite_enable;

        private void rendering_data()
        {
            DebugViewState w_debugView = md_main.g_debugView;
            bool w_pattern_enable = w_debugView.pattern_enable;
            bool w_screenA_enable = w_debugView.screenA_enable;
            bool w_screenB_enable = w_debugView.screenB_enable;
            bool w_screenW_enable = w_debugView.screenW_enable;
            bool w_screenS_enable = w_debugView.screenS_enable;
            if ((w_pattern_enable == false)
                && (w_screenA_enable == false)
                && (w_screenB_enable == false)
                && (w_screenW_enable == false)
                && (w_screenS_enable == false))
            {
                return;
            }

            if (w_pattern_enable == true)
            {
                int w_width = VdpDebugLayerConstants.PatternWidth;
                for (int w_char = 0; w_char < PATTERN_MAX; w_char++)
                {
                    if (g_pattern_chk[w_char] == true)
                    {
                        g_pattern_chk[w_char] = false;
                        int wx = (w_char & 0x0f) << 3;
                        int wy = (w_char & 0xfff0) >> 1;
                        int w_pic_addr = w_char << 4;
                        for (int dy = 0; dy < 8; dy++)
                        {
                            int w_row = (wy + dy) * w_width;
                            for (int dx = 0; dx < 8; dx++)
                            {
                                uint w_pic_w = g_snap_renderer_vram[w_pic_addr + (dy << 1) + (dx >> 2)];
                                uint w_pic = (w_pic_w >> ((3 - (dx & 3)) << 2)) & 0x0f;
                                g_pattern_pixels[w_row + wx + dx] = MONOCOLOR_TABLE[w_pic];
                            }
                        }
                    }
                }
            }

            if (w_screenA_enable == true)
            {
                render_scroll_layer_pixels(
                    g_scrollA_pixels,
                    VdpDebugLayerConstants.ScrollLayerWidth,
                    g_vdp_reg_2_scrolla >> 1);
            }

            if (w_screenB_enable == true)
            {
                render_scroll_layer_pixels(
                    g_scrollB_pixels,
                    VdpDebugLayerConstants.ScrollLayerWidth,
                    g_vdp_reg_4_scrollb >> 1);
            }

            if (w_screenW_enable == true)
            {
                int w_width = VdpDebugLayerConstants.ScrollLayerWidth;
                uint[] w_pixels = g_scrollW_pixels;
                int pixelOffset1 = 0;
                for (int wy = 0; wy < g_scroll_ycell; wy++)
                {
                    int w_num = (g_vdp_reg_3_windows >> 1) + (wy * g_scroll_xcell);
                    int pixelOffset2 = pixelOffset1;
                    for (int wx = 0; wx < g_scroll_xcell; wx++)
                    {
                        int pixelOffset3 = pixelOffset2;
                        uint w_val = g_snap_renderer_vram[w_num];
                        uint w_palette = (((w_val >> 13) & 0x0003) << 4);
                        uint w_reverse = ((w_val >> 11) & 0x0003);
                        uint w_char = (w_val & 0x07ff);
                        int w_pic_addr = (int)((w_reverse * VRAM_DATASIZE) + (w_char << 4));
                        w_num += 1;
                        for (int dy = 0; dy < 8; dy++)
                        {
                            int pixelOffset4 = pixelOffset3;
                            for (int dx = 0; dx < 8; dx++)
                            {
                                uint w_pic_w = g_snap_renderer_vram[w_pic_addr + (dy << 1) + (dx >> 2)];
                                uint w_pic = (w_pic_w >> ((3 - (dx & 3)) << 2)) & 0x0f;
                                w_pixels[pixelOffset4] = g_snap_color[w_palette + w_pic];
                                pixelOffset4 += 1;
                            }
                            pixelOffset3 += w_width;
                        }
                        pixelOffset2 += 8;
                    }
                    pixelOffset1 += w_width << 3;
                }
            }

            if (w_screenS_enable == true)
            {
                render_sprite_layer_pixels();
            }
        }

        private void render_scroll_layer_pixels(uint[] in_pixels, int in_width, int in_tableStart)
        {
            int w_num = in_tableStart;
            int pixelOffset1 = 0;
            for (int wy = 0; wy < g_scroll_ycell; wy++)
            {
                int pixelOffset2 = pixelOffset1;
                for (int wx = 0; wx < g_scroll_xcell; wx++)
                {
                    int pixelOffset3 = pixelOffset2;
                    uint w_val = g_snap_renderer_vram[w_num];
                    uint w_palette = (((w_val >> 13) & 0x0003) << 4);
                    uint w_reverse = ((w_val >> 11) & 0x0003);
                    uint w_char = (w_val & 0x07ff);
                    int w_pic_addr = (int)((w_reverse * VRAM_DATASIZE) + (w_char << 4));
                    w_num += 1;
                    for (int dy = 0; dy < 8; dy++)
                    {
                        int pixelOffset4 = pixelOffset3;
                        for (int dx = 0; dx < 8; dx++)
                        {
                            uint w_pic_w = g_snap_renderer_vram[w_pic_addr + (dy << 1) + (dx >> 2)];
                            uint w_pic = (w_pic_w >> ((3 - (dx & 3)) << 2)) & 0x0f;
                            in_pixels[pixelOffset4] = g_snap_color[w_palette + w_pic];
                            pixelOffset4 += 1;
                        }
                        pixelOffset3 += in_width;
                    }
                    pixelOffset2 += 8;
                }
                pixelOffset1 += in_width << 3;
            }
        }

        private void render_sprite_layer_pixels()
        {
            int w_width = VdpDebugLayerConstants.SpriteLayerWidth;
            uint[] w_pixels = g_scrollS_pixels;
            Array.Clear(w_pixels, 0, w_pixels.Length);

            for (int i = 0; i < g_max_sprite_num; i++)
            {
                g_sprite_enable[i] = false;
            }
            int w_link = 0;
            for (int i = 0; i < g_max_sprite_num; i++)
            {
                int w_addr = (g_vdp_reg_5_sprite >> 1) + (w_link << 2);
                ushort w_val2 = (ushort)g_snap_renderer_vram[w_addr + 1];
                w_link = w_val2 & 0x007f;
                if (w_link >= g_max_sprite_num) break;
                g_sprite_enable[w_link] = true;
            }

            for (int i = g_max_sprite_num - 1; i >= 0; i--)
            {
                int w_addr = (g_vdp_reg_5_sprite >> 1) + (i << 2);
                ushort w_val1 = (ushort)g_snap_renderer_vram[w_addr];
                ushort w_val2 = (ushort)g_snap_renderer_vram[w_addr + 1];
                ushort w_val3 = (ushort)g_snap_renderer_vram[w_addr + 2];
                ushort w_val4 = (ushort)g_snap_renderer_vram[w_addr + 3];
                int w_pos_x = w_val4 & 0x01ff;
                int w_pos_y = w_val1 & g_sprite_vmask;
                int w_xcell_size = ((w_val2 >> 10) & 0x0003) + 1;
                int w_ycell_size = ((w_val2 >> 8) & 0x0003) + 1;
                int w_pic_size_x = w_xcell_size << 3;
                int w_pic_size_y = w_ycell_size << 3;
                int w_palette = ((w_val3 >> 13) & 0x0003) << 4;
                int w_reverse = ((w_val3 >> 11) & 0x0003);
                int w_char = w_val3 & 0x07ff;

                if (g_sprite_enable[i] == true)
                {
                    for (int cy = 0; cy < w_ycell_size; cy++)
                    {
                        for (int cx = 0; cx < w_xcell_size; cx++)
                        {
                            int w_char_cur = 0;
                            switch (w_reverse)
                            {
                                case 0:
                                    w_char_cur = w_char + (w_ycell_size * cx) + cy;
                                    break;
                                case 1:
                                    w_char_cur = w_char + (w_ycell_size * (w_xcell_size - cx - 1)) + cy;
                                    break;
                                case 2:
                                    w_char_cur = w_char + (w_ycell_size * cx) + (w_ycell_size - cy - 1);
                                    break;
                                default:
                                    w_char_cur = w_char + (w_ycell_size * (w_xcell_size - cx - 1)) + (w_ycell_size - cy - 1);
                                    break;
                            }
                            if (w_char_cur <= 0x7ff)
                            {
                                int w_pic_addr = (int)((w_reverse * VRAM_DATASIZE) + (w_char_cur << 4));
                                int wx = w_pos_x + (cx * 8);
                                int wy = w_pos_y + (cy * 8);
                                int pixelOffset3 = (w_width * wy) + wx;
                                for (int dy = 0; dy < 8; dy++)
                                {
                                    int pixelOffset4 = pixelOffset3;
                                    for (int dx = 0; dx < 8; dx++)
                                    {
                                        uint w_pic_w = g_snap_renderer_vram[w_pic_addr + (dy << 1) + (dx >> 2)];
                                        uint w_pic = (w_pic_w >> ((3 - (dx & 3)) << 2)) & 0x0f;
                                        uint color = g_snap_color[w_palette + w_pic];

                                        if (((wy + dy) < 512) && ((wx + dx) < 512))
                                        {
                                            w_pixels[pixelOffset4] = color;
                                            pixelOffset4 += 1;
                                        }
                                    }
                                    pixelOffset3 += w_width;
                                }
                            }
                        }
                    }
                }

                uint w_color = g_sprite_enable[i] == true ? 0xffffff00u : 0xff0000ffu;
                if (w_pos_y < 512)
                {
                    for (int dx = 0; dx < w_pic_size_x; dx++)
                    {
                        int wx = w_pos_x + dx;
                        if (wx < 512)
                        {
                            w_pixels[(w_width * w_pos_y) + wx] = w_color;
                            if ((w_pos_y + w_pic_size_y - 1) < 512)
                            {
                                w_pixels[(w_width * (w_pos_y + w_pic_size_y - 1)) + wx] = w_color;
                            }
                        }
                    }
                }
                if (w_pos_x < 512)
                {
                    for (int dy = 0; dy < w_pic_size_y; dy++)
                    {
                        int wy = w_pos_y + dy;
                        if (wy < 512)
                        {
                            w_pixels[(w_width * wy) + w_pos_x] = w_color;
                            if ((w_pos_x + w_pic_size_x - 1) < 512)
                            {
                                w_pixels[(w_width * wy) + (w_pos_x + w_pic_size_x - 1)] = w_color;
                            }
                        }
                    }
                }
            }

            for (int dx = 0; dx < g_display_xsize; dx++)
            {
                int wx = dx + 128;
                w_pixels[(w_width * 128) + wx] = 0xff00ff00u;
                w_pixels[(w_width * (127 + g_display_ysize)) + wx] = 0xff00ff00u;
            }
            for (int dy = 0; dy < g_display_ysize; dy++)
            {
                int wy = dy + 128;
                w_pixels[(w_width * wy) + 128] = 0xff00ff00u;
                w_pixels[(w_width * wy) + (127 + g_display_xsize)] = 0xff00ff00u;
            }
        }
    }
}
