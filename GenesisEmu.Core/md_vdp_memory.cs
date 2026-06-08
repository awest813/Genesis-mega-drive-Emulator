using System;
using System.Runtime.CompilerServices;

namespace MDTracer
{
    internal partial class md_vdp
    {
        private byte[] g_vram;
        private ushort[] g_cram;
        public ushort[] g_vsram;
        public uint[] g_color;
        public uint[] g_color_shadow;
        public uint[] g_color_highlight;
        private int g_vdp_reg_code;
        private ushort g_vdp_reg_dest_address;

        //work
        private bool g_command_select;
        private ushort g_command_word;

        private static readonly int[] COLOR_NORMAL = { 0, 52, 87, 116, 144, 172, 206, 255 };
        private static readonly int[] COLOR_SHADOW = { 0, 29, 52, 70, 87, 101, 116, 130 };
        private static readonly int[] COLOR_HIGHLIGHT = { 130, 144, 158, 172, 187, 206, 228, 255 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint get_vdp_port(uint in_address)
        {
            return in_address & 0x1e;
        }
        //----------------------------------------------------------------
        //read
        //----------------------------------------------------------------
        public byte read8(uint in_address)
        {
            byte w_out = 0;
            ushort w_data = read16(in_address);
            if ((in_address & 1) == 0)
            {
                w_out = (byte)(w_data >> 8);
            }
            else
            {
                w_out = (byte)(w_data & 0xff);
            }
            return w_out;
        }
        public ushort read16(uint in_address)
        {
            ushort w_out = 0;
            uint w_port = get_vdp_port(in_address);
            if (w_port <= 0x02)
            {
                g_command_select = false;
                switch (g_vdp_reg_code)
                {
                    case 0:
                        w_out = vram_read_w(g_vdp_reg_dest_address);
                        break;
                    case 8:
                        w_out = g_cram[(g_vdp_reg_dest_address >> 1) & 0x3f];
                        break;
                    case 4:
                        w_out = g_vsram[(g_vdp_reg_dest_address >> 1) % 40];
                        break;
                    default:
                        report_vdp_warning("read16_c0000");
                        break;
                }
                g_vdp_reg_dest_address = (ushort)(g_vdp_reg_dest_address + g_vdp_reg_15_autoinc);
            }
            else
            if (w_port <= 0x06)
            {
                g_command_select = false;
                w_out = get_vdp_status();
            }
            else
            if (w_port <= 0x0e)
            {
                w_out = get_vdp_hvcounter();
            }
            else
            {
                report_vdp_warning("md_vdp.read16");
            }
            return w_out;
        }
        public uint read32(uint in_address)
        {
            uint w_out = 0;
            uint w_port = get_vdp_port(in_address);
            if (w_port <= 0x0e)
            {
                w_out = (uint)((read16(in_address) << 16) + read16(in_address + 2));
            }
            else
            {
                report_vdp_warning("md_vdp.read32");
            }
            return w_out;
        }
        //----------------------------------------------------------------
        //write
        //----------------------------------------------------------------
        public void write8(uint in_address, byte in_data)
        {
            ushort w_data = (ushort)((in_data << 8) + in_data);
            write16(in_address, w_data);
        }
        public void write16(uint in_address, ushort in_data)
        {
            uint w_port = get_vdp_port(in_address);
            if (w_port <= 0x02)
            {
                g_command_select = false;
                if (g_dma_fill_req == true)
                {
                    g_dma_fill_req = false;
                    dma_run_fill_req(in_data);
                }
                else
                {
                    switch (g_vdp_reg_code & 0x0f)
                    {
                        case 1:
                            vram_write_w(g_vdp_reg_dest_address, in_data);
                            pattern_chk(g_vdp_reg_dest_address);
                            pattern_chk(g_vdp_reg_dest_address + 1);
                            g_vdp_reg_dest_address = (ushort)((g_vdp_reg_dest_address + g_vdp_reg_15_autoinc) & 0xffff);
                            break;
                        case 3:
                            int wcol_num = (int)((g_vdp_reg_dest_address >> 1) & 0x3f);
                            cram_set(wcol_num, in_data);
                            g_vdp_reg_dest_address = (ushort)((g_vdp_reg_dest_address + g_vdp_reg_15_autoinc) & 0xffff);
                            break;
                        case 5:
                            if (g_vdp_reg_dest_address < 80)
                            {
                                g_vsram[(g_vdp_reg_dest_address >> 1)] = in_data;
                                g_vdp_reg_dest_address = (ushort)((g_vdp_reg_dest_address + g_vdp_reg_15_autoinc) & 0xffff);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            if (w_port <= 0x06)
            {
                if (g_command_select == false)
                {
                    if ((in_data & 0xc000) == 0x8000)
                    {
                        byte w_rs = (byte)((in_data >> 8) & 0x001f);
                        byte w_data = (byte)(in_data & 0x00ff);
                        set_vdp_register(w_rs, w_data);
                    }
                    else
                    {
                        //address set 1st
                        g_command_select = true;
                        g_command_word = in_data;
                    }
                }
                else
                {
                    //address set 2nd
                    g_command_select = false;
                    g_vdp_reg_code = (int)((g_command_word >> 14) | ((in_data >> 2) & 0x3c));
                    g_vdp_reg_dest_address = (ushort)((g_command_word & 0x3fff) | ((in_data & 0x0003) << 14));
                    if ((g_vdp_reg_code & 0x20) == 0x20)
                    {
                        if (g_vdp_reg_1_4_dma == 1)
                        {
                            switch (g_vdp_reg_23_dma_mode)
                            {
                                case 0:
                                case 1:
                                    dma_run_memory_req();
                                    break;
                                case 2:
                                    g_dma_fill_req = true;
                                    break;
                                case 3:
                                    dma_run_copy_req();
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                report_vdp_warning("md_vdp.write16");
            }
        }
        public void write32(uint in_address, uint in_data)
        {
            uint w_port = get_vdp_port(in_address);
            if (w_port <= 0x06)
            {
                write16(in_address, (ushort)(in_data >> 16));
                write16(in_address + 2, (ushort)(in_data & 0xffff));
            }
            else
            {
                report_vdp_warning("md_vdp.write32");
            }
        }
        //----------------------------------------------------------------
        //sub
        //----------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort vram_read_w(int in_addr)
        {
            int w_addr = in_addr & 0xffff;
            byte[] w_vram = g_vram;
            return (ushort)((w_vram[w_addr] << 8)
                          | w_vram[w_addr ^ 1]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void vram_write_w(int in_addr, ushort in_data)
        {
            int w_addr = in_addr & 0xffff;
            byte[] w_vram = g_vram;
            w_vram[w_addr] = (byte)(in_data >> 8);
            w_vram[w_addr ^ 1] = (byte)in_data;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void cram_set(int in_num, ushort in_data)
        {
            g_cram[in_num] = in_data;
            int w_r = (in_data & 0x000e) >> 1;
            int w_g = (in_data & 0x00e0) >> 5;
            int w_b = (in_data & 0x0e00) >> 9;
            g_color[in_num] = (uint)(0xff000000
                                        | (uint)(COLOR_NORMAL[w_r] << 16)
                                        | (uint)(COLOR_NORMAL[w_g] << 8)
                                        | (uint)(COLOR_NORMAL[w_b]));
            g_color_shadow[in_num] = (uint)(0xff000000
                                        | (uint)(COLOR_SHADOW[w_r] << 16)
                                        | (uint)(COLOR_SHADOW[w_g] << 8)
                                        | (uint)(COLOR_SHADOW[w_b]));
            g_color_highlight[in_num] = (uint)(0xff000000
                                        | (uint)(COLOR_HIGHLIGHT[w_r] << 16)
                                        | (uint)(COLOR_HIGHLIGHT[w_g] << 8)
                                        | (uint)(COLOR_HIGHLIGHT[w_b]));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void pattern_chk(int in_address)
        {
            int w_address = in_address & 0xfffe;
            uint w_val = vram_read_w(w_address);
            {
                uint w_val_h = ((w_val >> 12) & 0x000f)
                                + ((w_val >> 4) & 0x00f0)
                                + ((w_val << 4) & 0x0f00)
                                + ((w_val << 12) & 0xf000);
                int w_char = (in_address & 0xffe0) >> 5;
                int w_addr = (in_address & 0xffe0) >> 1;
                int wx = (in_address & 0x0002) >> 1;
                int wy = (in_address & 0x001f) >> 2;
                uint[] w_renderer_vram = g_renderer_vram;
                w_renderer_vram[w_address >> 1] = w_val;
                if (wx == 0)
                {
                    w_renderer_vram[VRAM_DATASIZE + w_addr + (wy << 1) + 1] = w_val_h;
                    w_renderer_vram[(VRAM_DATASIZE * 2) + w_addr + ((7 - wy) << 1)] = w_val;
                    w_renderer_vram[(VRAM_DATASIZE * 3) + w_addr + ((7 - wy) << 1) + 1] = w_val_h;
                }
                else
                {
                    w_renderer_vram[VRAM_DATASIZE + w_addr + (wy << 1)] = w_val_h;
                    w_renderer_vram[(VRAM_DATASIZE * 2) + w_addr + ((7 - wy) << 1) + 1] = w_val;
                    w_renderer_vram[(VRAM_DATASIZE * 3) + w_addr + ((7 - wy) << 1)] = w_val_h;
                }
                bool[] w_pattern_chk = g_pattern_chk;
                w_pattern_chk[w_char] = true;
            }
        }
    }
}
