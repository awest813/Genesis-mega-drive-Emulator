using System.Diagnostics;

using System.Runtime.CompilerServices;

namespace MDTracer
{
    //----------------------------------------------------------------
    //Bus arbiter : chips:315-5308
    //----------------------------------------------------------------
    internal class md_bus : IM68kBus
    {
        private static void report_bus_warning(string in_message)
        {
            Debug.WriteLine("[Bus] " + in_message);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool is_vdp_range(uint in_address)
        {
            return (0xc00000 <= in_address) && (in_address <= 0xdfffff);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool is_psg_port(uint in_address)
        {
            uint w_port = in_address & 0x1f;
            return is_vdp_range(in_address) && (0x10 <= w_port) && (w_port <= 0x17);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool is_tmss_address(uint in_address)
        {
            return (0xa14000 <= in_address) && (in_address <= 0xa14003);
        }
        //----------------------------------------------------------------
        //read
        //----------------------------------------------------------------
        public byte read8(uint in_address)
        {
            byte w_out = 0;
            in_address &= 0xffffff;
            if (in_address <= 0x3fffff)
            {
                w_out = md_main.g_md_m68k.read8(in_address);
            }
            else 
            if (0xff0000 <= in_address)
            {
                w_out = md_main.g_md_m68k.read8(in_address);
            }
            else
            if (is_vdp_range(in_address))
            {
                w_out = md_main.g_md_vdp.read8(in_address);
            }
            else
            if ((0xa10000 <= in_address) && (in_address <= 0xa10fff))
            {
                w_out = md_main.g_md_io.read8(in_address);
            }
            else
            if ((0xa04000 <= in_address) && (in_address <= 0xa04003))
            {
                w_out = md_main.g_md_music.g_md_ym2612.read8(in_address);
            }
            else
            if (is_tmss_address(in_address))
            {
                w_out = 0;
            }
            else
            if ((0xa11000 <= in_address) && (in_address <= 0xa1ffff))
            {
                w_out = md_main.g_md_control.read8(in_address);
            }
            else
            if ((0xa00000 <= in_address) && (in_address <= 0xa0ffff))
            {
                w_out = md_main.g_md_z80.read8(in_address);
            }
            else
            {
                report_bus_warning("md_bus.read8");
            }
            var w_form_code = md_main.g_form_code;
            if (w_form_code.memory_monitor_active == true) w_form_code.memory_monitor_check(in_address, w_out, false, 1);
            return w_out;
        }
        public ushort read16(uint in_address)
        {
            ushort w_out = 0;
            in_address &= 0xffffff;
            if (in_address <= 0x3fffff)
            {
                w_out = md_main.g_md_m68k.read16(in_address);
            }
            else
            if (0xff0000 <= in_address)
            {
                w_out = md_main.g_md_m68k.read16(in_address);
            }
            else
            if (is_vdp_range(in_address))
            {
                w_out = md_main.g_md_vdp.read16(in_address);
            }
            else
            if ((0xa10000 <= in_address) && (in_address <= 0xa10fff))
            {
                w_out = md_main.g_md_io.read16(in_address);
            }
            else
            if (is_tmss_address(in_address))
            {
                w_out = 0;
            }
            else
            if ((0xa11000 <= in_address) && (in_address <= 0xa1ffff))
            {
                w_out = md_main.g_md_control.read16(in_address);
            }
            else
            if ((0xa00000 <= in_address) && (in_address <= 0xa0ffff))
            {
                w_out = md_main.g_md_z80.read16(in_address);
            }
            else
            {
                report_bus_warning("md_bus.read16");
            }
            var w_form_code = md_main.g_form_code;
            if (w_form_code.memory_monitor_active == true) w_form_code.memory_monitor_check(in_address, w_out, false, 2);
            return w_out;
        }
        public uint read32(uint in_address)
        {
            uint w_out = 0;
            in_address &= 0xffffff;
            if (in_address <= 0x3fffff)
            {
                w_out = md_main.g_md_m68k.read32(in_address);
            }
            else
            if (0xff0000 <= in_address)
            {
                w_out = md_main.g_md_m68k.read32(in_address);
            }
            else
            if (is_vdp_range(in_address))
            {
                w_out = md_main.g_md_vdp.read32(in_address);
            }
            else
            if ((0xa10000 <= in_address) && (in_address <= 0xa10fff))
            {
                w_out = md_main.g_md_io.read32(in_address);
            }
            else
            if (is_tmss_address(in_address))
            {
                w_out = 0;
            }
            else
            if ((0xa11000 <= in_address) && (in_address <= 0xa1ffff))
            {
                w_out = md_main.g_md_control.read32(in_address);
            }
            else
            if ((0xa00000 <= in_address) && (in_address <= 0xa0ffff))
            {
                w_out = md_main.g_md_z80.read32(in_address);
            }
            else
            {
                report_bus_warning("md_bus.read32");
            }
            var w_form_code = md_main.g_form_code;
            if (w_form_code.memory_monitor_active == true) w_form_code.memory_monitor_check(in_address, w_out, false, 4);
            return w_out;
        }
        //----------------------------------------------------------------
        //write
        //----------------------------------------------------------------
        public void write8(uint in_address, byte in_data)
        {
            in_address &= 0xffffff;
            var w_form_code = md_main.g_form_code;
            if (w_form_code.memory_monitor_active == true) w_form_code.memory_monitor_check(in_address, in_data, true, 1);
            if (0xff0000 <= in_address)
            {
                md_main.g_md_m68k.write8(in_address, in_data);
            }
            else
            if (is_psg_port(in_address))
            {
                md_main.g_md_music.g_md_sn76489.write8(in_data);
            }
            else
            if (is_vdp_range(in_address))
            {
                md_main.g_md_vdp.write8(in_address, in_data);
            }
            else
            if ((0xa10000 <= in_address) && (in_address <= 0xa10fff))
            {
                md_main.g_md_io.write8(in_address, in_data);
            }
            else
            if ((0xa04000 <= in_address) && (in_address <= 0xa04003))
            {
                md_main.g_md_music.g_md_ym2612.write8(in_address, in_data);
            }
            else
            if (is_tmss_address(in_address))
            {
                //TMSS
            }
            else
            if ((0xa11000 <= in_address) && (in_address <= 0xa1ffff))
            {
                md_main.g_md_control.write8(in_address, in_data);
            }
            else
            if ((0xa00000 <= in_address) && (in_address <= 0xa0ffff))
            {
                md_main.g_md_z80.write8(in_address, in_data);
            }
            else
            {
                report_bus_warning("md_bus.write8");
            }
        }
        public void write16(uint in_address, ushort in_data)
        {
            in_address &= 0xffffff;
            var w_form_code = md_main.g_form_code;
            if (w_form_code.memory_monitor_active == true) w_form_code.memory_monitor_check(in_address, in_data, true, 2);
            if (0xff0000 <= in_address)
            {
                md_main.g_md_m68k.write16(in_address, in_data);
            }
            else
            if (is_psg_port(in_address))
            {
                md_main.g_md_music.g_md_sn76489.write8((byte)(in_data >> 8));
            }
            else
            if (is_vdp_range(in_address))
            {
                md_main.g_md_vdp.write16(in_address, in_data);
            }
            else
            if ((0xa04000 <= in_address) && (in_address <= 0xa04003))
            {
                md_main.g_md_music.g_md_ym2612.write8(in_address, (byte)(in_data >> 8));
                md_main.g_md_music.g_md_ym2612.write8(in_address + 1, (byte)(in_data & 0xff));
            }
            else
            if ((0xa10000 <= in_address) && (in_address <= 0xa10fff))
            {
                md_main.g_md_io.write16(in_address, in_data);
            }
            else
            if (is_tmss_address(in_address))
            {
                //TMSS
            }
            else
            if ((0xa11000 <= in_address) && (in_address <= 0xa1ffff))
            {
                md_main.g_md_control.write16(in_address, in_data);
            }
            else
            if ((0xa00000 <= in_address) && (in_address <= 0xa0ffff))
            {
                md_main.g_md_z80.write16(in_address, in_data);
            }
            else
            {
                report_bus_warning("md_bus.write16");
            }
        }
        public void write32(uint in_address, uint in_data)
        {
            in_address &= 0xffffff;
            var w_form_code = md_main.g_form_code;
            if (w_form_code.memory_monitor_active == true) w_form_code.memory_monitor_check(in_address, in_data, true, 4);
            if (0xff0000 <= in_address)
            {
                md_main.g_md_m68k.write32(in_address, in_data);
            }
            else
            if (is_psg_port(in_address))
            {
                write16(in_address, (ushort)(in_data >> 16));
                write16(in_address + 2, (ushort)(in_data & 0xffff));
            }
            else
            if (is_vdp_range(in_address))
            {
                md_main.g_md_vdp.write32(in_address, in_data);
            }
            else
            if ((0xa04000 <= in_address) && (in_address <= 0xa04003))
            {
                write16(in_address, (ushort)(in_data >> 16));
                write16(in_address + 2, (ushort)(in_data & 0xffff));
            }
            else
            if ((0xa10000 <= in_address) && (in_address <= 0xa10fff))
            {
                write16(in_address, (ushort)(in_data >> 16));
                write16(in_address + 2, (ushort)(in_data & 0xffff));
            }
            else
            if (is_tmss_address(in_address))
            {
                //TMSS
            }
            else
            if ((0xa11000 <= in_address) && (in_address <= 0xa1ffff))
            {
                md_main.g_md_control.write16(in_address, (ushort)(in_data >> 16));
            }
            else
            if ((0xa00000 <= in_address) && (in_address <= 0xa0ffff))
            {
                md_main.g_md_z80.write32(in_address, in_data);
            }
            else
            {
                report_bus_warning("md_bus.write32");
            }
        }
    }
}
