using System.Diagnostics;
using static MDTracer.md_m68k;

namespace MDTracer
{
    internal partial class md_z80
    {
        private byte[] g_ram;
        private uint g_bank_register;

        //----------------------------------------------------------------
        //read
        //----------------------------------------------------------------
        public byte read8(uint in_address)
        {
            byte w_out = 0;
            in_address &= 0xffff;
            if (in_address < 0x4000)
            {
                in_address &= 0x1fff;
                w_out = g_ram[in_address];
            }
            else
            if (in_address <= 0x5fff)
            {
                w_out = md_main.g_md_music.g_md_ym2612.read8(in_address);
            }   
            else
            if ((in_address >= 0x6000) && (in_address <= 0x7eff))
            {
                w_out = 0xff;
            }
            else
            if (in_address >= 0x8000)
            {
                w_out = md_main.g_md_m68k.read8(g_bank_register + (in_address & 0x7fff));
            }
            else
            {
                MessageBox.Show("md_z80_memory.read8", "error");
            }
            return w_out;
        }
        public ushort read16(uint in_address)
        {
            return (ushort)((read8(in_address) << 8)
                          | read8(in_address + 1));
        }

        public uint read32(uint in_address)
        {
            return (uint)((read16(in_address) << 16)
                        | read16(in_address + 2));
        }

        //----------------------------------------------------------------
        //write
        //----------------------------------------------------------------
        public void write8(uint in_address, byte in_data)
        {
            in_address &= 0xffff;
            if (in_address < 0x4000)
            {
                in_address &= 0x1fff;
                g_ram[in_address] = in_data;
            }
            else
            if ((0x4000 <= in_address) && (in_address <= 0x5fff))
            {
                md_main.g_md_music.g_md_ym2612.write8(in_address, in_data);
            }
            else
            if ((in_address >= 0x6000) && (in_address <= 0x60ff))
            {
                g_bank_register >>= 1;

                if ((in_data & 0x01) == 1)
                {
                    g_bank_register = (g_bank_register | 0x00800000);
                }
                g_bank_register &= 0x00ff8000;
            }
            else
            if ((in_address >= 0x6100) && (in_address <= 0x7eff))
            {
                //nothing
            }
            else
            if (0x7f11 == in_address)
            {
                md_main.g_md_music.g_md_sn76489.write8(in_data);
            }
            else
            if (in_address >= 0x8000)
            {
                md_main.g_md_m68k.write8(g_bank_register + (in_address & 0x7fff), in_data);
            }
            else
            {
                MessageBox.Show("md_z80_memory.write8", "error");
            }
        }
        public void write16(uint in_address, ushort in_data)
        {
            write8(in_address, (byte)(in_data >> 8));
            write8(in_address + 1, (byte)(in_data & 0xff));
        }

        public void write32(uint in_address, uint in_data)
        {
            write16(in_address, (ushort)(in_data >> 16));
            write16(in_address + 2, (ushort)(in_data & 0xffff));
        }
    }
}
