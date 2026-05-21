using System;
using System.IO;

namespace MDTracer
{
    internal partial class md_vdp
    {
        public void write_state(BinaryWriter in_writer)
        {
            write_array(in_writer, g_vram);
            write_array(in_writer, g_cram);
            write_array(in_writer, g_vsram);
            write_array(in_writer, g_color);
            write_array(in_writer, g_color_shadow);
            write_array(in_writer, g_color_highlight);
            write_array(in_writer, g_vdp_reg);
            write_array(in_writer, g_pattern_chk);
            write_array(in_writer, g_renderer_vram);

            in_writer.Write(g_scanline);
            in_writer.Write(g_hinterrupt_counter);
            in_writer.Write(g_vdp_reg_code);
            in_writer.Write(g_vdp_reg_dest_address);
            in_writer.Write(g_command_select);
            in_writer.Write(g_command_word);

            in_writer.Write(g_vdp_status_9_empl);
            in_writer.Write(g_vdp_status_8_full);
            in_writer.Write(g_vdp_status_7_vinterrupt);
            in_writer.Write(g_vdp_status_6_sprite);
            in_writer.Write(g_vdp_status_5_collision);
            in_writer.Write(g_vdp_status_4_frame);
            in_writer.Write(g_vdp_status_3_vbrank);
            in_writer.Write(g_vdp_status_2_hbrank);
            in_writer.Write(g_vdp_status_1_dma);
            in_writer.Write(g_vdp_status_0_tvmode);
            in_writer.Write(g_vdp_c00008_hvcounter);
            in_writer.Write(g_vdp_c00008_hvcounter_latched);

            in_writer.Write(g_vdp_reg_0_4_hinterrupt);
            in_writer.Write(g_vdp_reg_0_1_hvcounter);
            in_writer.Write(g_vdp_reg_1_6_display);
            in_writer.Write(g_vdp_reg_1_5_vinterrupt);
            in_writer.Write(g_vdp_reg_1_4_dma);
            in_writer.Write(g_vdp_reg_1_3_cellmode);
            in_writer.Write(g_vdp_reg_2_scrolla);
            in_writer.Write(g_vdp_reg_3_windows);
            in_writer.Write(g_vdp_reg_4_scrollb);
            in_writer.Write(g_vdp_reg_5_sprite);
            in_writer.Write(g_vdp_reg_7_backcolor);
            in_writer.Write(g_vdp_reg_10_hint);
            in_writer.Write(g_vdp_reg_11_3_ext);
            in_writer.Write(g_vdp_reg_11_2_vscroll);
            in_writer.Write(g_vdp_reg_11_1_hscroll);
            in_writer.Write(g_vdp_reg_12_7_cellmode1);
            in_writer.Write(g_vdp_reg_12_3_shadow);
            in_writer.Write(g_vdp_reg_12_2_interlacemode);
            in_writer.Write(g_vdp_reg_12_0_cellmode2);
            in_writer.Write(g_vdp_reg_13_hscroll);
            in_writer.Write(g_vdp_reg_15_autoinc);
            in_writer.Write(g_vdp_reg_16_5_scrollV);
            in_writer.Write(g_vdp_reg_16_1_scrollH);
            in_writer.Write(g_vdp_reg_17_7_windows);
            in_writer.Write(g_vdp_reg_17_4_basspointer);
            in_writer.Write(g_vdp_reg_18_7_windows);
            in_writer.Write(g_vdp_reg_18_4_basspointer);
            in_writer.Write(g_vdp_reg_19_dma_counter_low);
            in_writer.Write(g_vdp_reg_20_dma_counter_high);
            in_writer.Write(g_vdp_reg_21_dma_source_low);
            in_writer.Write(g_vdp_reg_22_dma_source_mid);
            in_writer.Write(g_vdp_reg_23_dma_mode);
            in_writer.Write(g_vdp_reg_23_5_dma_high);

            in_writer.Write(g_display_xsize);
            in_writer.Write(g_display_ysize);
            in_writer.Write(g_display_xcell);
            in_writer.Write(g_display_ycell);
            in_writer.Write(g_scroll_xcell);
            in_writer.Write(g_scroll_ycell);
            in_writer.Write(g_scroll_xsize);
            in_writer.Write(g_scroll_ysize);
            in_writer.Write(g_scroll_xsize_mask);
            in_writer.Write(g_scroll_ysize_mask);
            in_writer.Write(g_vertical_line_max);
            in_writer.Write(g_screenA_left_x);
            in_writer.Write(g_screenA_right_x);
            in_writer.Write(g_screenA_top_y);
            in_writer.Write(g_screenA_bottom_y);
            in_writer.Write(g_max_sprite_num);
            in_writer.Write(g_max_sprite_line);
            in_writer.Write(g_max_sprite_cell);
            in_writer.Write(g_sprite_vmask);

            in_writer.Write(g_dma_mode);
            in_writer.Write(g_dma_src_addr);
            in_writer.Write(g_dma_leng);
            in_writer.Write(g_dma_fill_req);
            in_writer.Write(g_dma_fill_data);
        }

        public void restore_state(BinaryReader in_reader)
        {
            read_array(in_reader, g_vram);
            read_array(in_reader, g_cram);
            read_array(in_reader, g_vsram);
            read_array(in_reader, g_color);
            read_array(in_reader, g_color_shadow);
            read_array(in_reader, g_color_highlight);
            read_array(in_reader, g_vdp_reg);
            read_array(in_reader, g_pattern_chk);
            read_array(in_reader, g_renderer_vram);

            g_scanline = in_reader.ReadInt32();
            g_hinterrupt_counter = in_reader.ReadInt32();
            g_vdp_reg_code = in_reader.ReadInt32();
            g_vdp_reg_dest_address = in_reader.ReadUInt16();
            g_command_select = in_reader.ReadBoolean();
            g_command_word = in_reader.ReadUInt16();

            g_vdp_status_9_empl = in_reader.ReadByte();
            g_vdp_status_8_full = in_reader.ReadByte();
            g_vdp_status_7_vinterrupt = in_reader.ReadByte();
            g_vdp_status_6_sprite = in_reader.ReadByte();
            g_vdp_status_5_collision = in_reader.ReadByte();
            g_vdp_status_4_frame = in_reader.ReadByte();
            g_vdp_status_3_vbrank = in_reader.ReadByte();
            g_vdp_status_2_hbrank = in_reader.ReadByte();
            g_vdp_status_1_dma = in_reader.ReadByte();
            g_vdp_status_0_tvmode = in_reader.ReadByte();
            g_vdp_c00008_hvcounter = in_reader.ReadUInt16();
            g_vdp_c00008_hvcounter_latched = in_reader.ReadBoolean();

            g_vdp_reg_0_4_hinterrupt = in_reader.ReadByte();
            g_vdp_reg_0_1_hvcounter = in_reader.ReadByte();
            g_vdp_reg_1_6_display = in_reader.ReadByte();
            g_vdp_reg_1_5_vinterrupt = in_reader.ReadByte();
            g_vdp_reg_1_4_dma = in_reader.ReadByte();
            g_vdp_reg_1_3_cellmode = in_reader.ReadByte();
            g_vdp_reg_2_scrolla = in_reader.ReadInt32();
            g_vdp_reg_3_windows = in_reader.ReadInt32();
            g_vdp_reg_4_scrollb = in_reader.ReadInt32();
            g_vdp_reg_5_sprite = in_reader.ReadInt32();
            g_vdp_reg_7_backcolor = in_reader.ReadByte();
            g_vdp_reg_10_hint = in_reader.ReadByte();
            g_vdp_reg_11_3_ext = in_reader.ReadByte();
            g_vdp_reg_11_2_vscroll = in_reader.ReadByte();
            g_vdp_reg_11_1_hscroll = in_reader.ReadByte();
            g_vdp_reg_12_7_cellmode1 = in_reader.ReadByte();
            g_vdp_reg_12_3_shadow = in_reader.ReadByte();
            g_vdp_reg_12_2_interlacemode = in_reader.ReadByte();
            g_vdp_reg_12_0_cellmode2 = in_reader.ReadByte();
            g_vdp_reg_13_hscroll = in_reader.ReadInt32();
            g_vdp_reg_15_autoinc = in_reader.ReadByte();
            g_vdp_reg_16_5_scrollV = in_reader.ReadInt32();
            g_vdp_reg_16_1_scrollH = in_reader.ReadInt32();
            g_vdp_reg_17_7_windows = in_reader.ReadByte();
            g_vdp_reg_17_4_basspointer = in_reader.ReadByte();
            g_vdp_reg_18_7_windows = in_reader.ReadByte();
            g_vdp_reg_18_4_basspointer = in_reader.ReadByte();
            g_vdp_reg_19_dma_counter_low = in_reader.ReadByte();
            g_vdp_reg_20_dma_counter_high = in_reader.ReadByte();
            g_vdp_reg_21_dma_source_low = in_reader.ReadByte();
            g_vdp_reg_22_dma_source_mid = in_reader.ReadByte();
            g_vdp_reg_23_dma_mode = in_reader.ReadByte();
            g_vdp_reg_23_5_dma_high = in_reader.ReadByte();

            g_display_xsize = in_reader.ReadInt32();
            g_display_ysize = in_reader.ReadInt32();
            g_display_xcell = in_reader.ReadInt32();
            g_display_ycell = in_reader.ReadInt32();
            g_scroll_xcell = in_reader.ReadInt32();
            g_scroll_ycell = in_reader.ReadInt32();
            g_scroll_xsize = in_reader.ReadInt32();
            g_scroll_ysize = in_reader.ReadInt32();
            g_scroll_xsize_mask = in_reader.ReadInt32();
            g_scroll_ysize_mask = in_reader.ReadInt32();
            g_vertical_line_max = in_reader.ReadInt32();
            g_screenA_left_x = in_reader.ReadInt32();
            g_screenA_right_x = in_reader.ReadInt32();
            g_screenA_top_y = in_reader.ReadInt32();
            g_screenA_bottom_y = in_reader.ReadInt32();
            g_max_sprite_num = in_reader.ReadInt32();
            g_max_sprite_line = in_reader.ReadInt32();
            g_max_sprite_cell = in_reader.ReadInt32();
            g_sprite_vmask = in_reader.ReadInt32();

            g_dma_mode = in_reader.ReadInt32();
            g_dma_src_addr = in_reader.ReadUInt32();
            g_dma_leng = in_reader.ReadInt32();
            g_dma_fill_req = in_reader.ReadBoolean();
            g_dma_fill_data = in_reader.ReadUInt16();
        }

        private static void write_array(BinaryWriter in_writer, byte[] in_array)
        {
            in_writer.Write(in_array.Length);
            in_writer.Write(in_array);
        }

        private static void write_array(BinaryWriter in_writer, ushort[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, uint[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, bool[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void read_array(BinaryReader in_reader, byte[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("VDP byte array size is invalid.");
            byte[] w_data = in_reader.ReadBytes(w_length);
            if (w_data.Length != w_length) throw new InvalidDataException("VDP byte array data is invalid.");
            Buffer.BlockCopy(w_data, 0, in_array, 0, w_length);
        }

        private static void read_array(BinaryReader in_reader, ushort[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("VDP ushort array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadUInt16();
        }

        private static void read_array(BinaryReader in_reader, uint[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("VDP uint array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadUInt32();
        }

        private static void read_array(BinaryReader in_reader, bool[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("VDP bool array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadBoolean();
        }
    }
}
