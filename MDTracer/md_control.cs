namespace MDTracer
{
    internal class md_control
    {
        public byte g_io_a11200_z80reset;
        public byte g_io_a11100_z80active;

        private static void report_control_warning(string in_message)
        {
            System.Diagnostics.Debug.WriteLine("[Control] " + in_message);
        }
        public void reset()
        {
            g_io_a11200_z80reset = 0;
            g_io_a11100_z80active = 0;
        }
        //----------------------------------------------------------------
        //read
        //----------------------------------------------------------------
        public byte read8(uint in_address)
        {
            byte w_out = 0;
            if ((in_address & 0xfffffe) == 0xa11100)
            {
                w_out = (byte)((md_main.g_md_z80.g_active == true) ? 1 : 0);
            }
            else
            {
                report_control_warning("md_control.read8");
            }
            return w_out;
        }
        public ushort read16(uint in_address)
        {
            return read8(in_address + 1);
        }
        public uint read32(uint in_address)
        {
            uint w_out = 0;
            w_out = (uint)((read8(in_address + 1) << 8) + read8(in_address + 3));
            return w_out;
        }
        //----------------------------------------------------------------
        //write
        //----------------------------------------------------------------
        public void write8(uint in_address, byte in_data)
        {
            if ((in_address & 0xfffffe) == 0xa11100)
            {
                if (in_data == 1)
                {
                    md_main.g_md_z80.g_active = false;
                }
                else
                {
                    md_main.g_md_z80.g_active = true;
                }
            }
            else
            if (in_address == 0xa11200)
            {
                if (in_data == 0)
                {
                    md_main.g_md_z80.reset();
                }
            }
            else
            if (in_address == 0xa130f1)
            {
                // Bit 0 selects whether the cartridge SRAM window maps SRAM
                // (1) or ROM (0).
                md_main.g_md_sram.set_enabled((in_data & 0x01) != 0);
            }
            else
            if ((in_address >= 0xa130f3) && (in_address <= 0xa130ff))
            {
                // Sega mapper bank registers (banks 1-7).
                md_main.g_md_mapper.write_control(in_address, in_data);
            }
            else
            {
                report_control_warning("md_control.write8");
            }
        }
        public void write16(uint in_address, ushort in_data)
        {
            if (in_address == 0xa11100)
            {
                write8(in_address, (byte)(in_data >> 8));
            }
            else
            if (in_address == 0xa11200)
            {
                write8(in_address, (byte)(in_data >> 8));
            }
            else
            {
                report_control_warning("md_control.write16");
            }
        }
    }
}
