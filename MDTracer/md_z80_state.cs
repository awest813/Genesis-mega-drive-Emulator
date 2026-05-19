using System;
using System.IO;

namespace MDTracer
{
    internal partial class md_z80
    {
        public void write_state(BinaryWriter in_writer)
        {
            in_writer.Write(g_active);
            in_writer.Write(g_reg_PC);
            in_writer.Write(g_reg_A);
            in_writer.Write(g_reg_B);
            in_writer.Write(g_reg_C);
            in_writer.Write(g_reg_D);
            in_writer.Write(g_reg_E);
            in_writer.Write(g_reg_H);
            in_writer.Write(g_reg_L);
            in_writer.Write(g_reg_Au);
            in_writer.Write(g_reg_Bu);
            in_writer.Write(g_reg_Cu);
            in_writer.Write(g_reg_Du);
            in_writer.Write(g_reg_Eu);
            in_writer.Write(g_reg_Fu);
            in_writer.Write(g_reg_Hu);
            in_writer.Write(g_reg_Lu);
            in_writer.Write(g_reg_SP);
            in_writer.Write(g_reg_IX);
            in_writer.Write(g_reg_IY);
            in_writer.Write(g_flag_S);
            in_writer.Write(g_flag_Z);
            in_writer.Write(g_flag_H);
            in_writer.Write(g_flag_PV);
            in_writer.Write(g_flag_N);
            in_writer.Write(g_flag_C);
            in_writer.Write(g_flag_Su);
            in_writer.Write(g_flag_Zu);
            in_writer.Write(g_flag_Hu);
            in_writer.Write(g_flag_PVu);
            in_writer.Write(g_flag_Nu);
            in_writer.Write(g_flag_Cu);
            in_writer.Write(g_reg_R);
            in_writer.Write(g_reg_I);
            in_writer.Write(g_IFF1);
            in_writer.Write(g_IFF2);
            in_writer.Write(g_interruptMode);
            in_writer.Write(g_interrupt_irq);
            in_writer.Write(g_interrupt_nmi);
            in_writer.Write(g_halt);
            in_writer.Write(g_halt_out);
            in_writer.Write(g_clock);
            in_writer.Write(g_clock_total);
            in_writer.Write(g_bank_register);
            write_array(in_writer, g_ram);
        }

        public void restore_state(BinaryReader in_reader)
        {
            g_active = in_reader.ReadBoolean();
            g_reg_PC = in_reader.ReadUInt16();
            g_reg_A = in_reader.ReadByte();
            g_reg_B = in_reader.ReadByte();
            g_reg_C = in_reader.ReadByte();
            g_reg_D = in_reader.ReadByte();
            g_reg_E = in_reader.ReadByte();
            g_reg_H = in_reader.ReadByte();
            g_reg_L = in_reader.ReadByte();
            g_reg_Au = in_reader.ReadByte();
            g_reg_Bu = in_reader.ReadByte();
            g_reg_Cu = in_reader.ReadByte();
            g_reg_Du = in_reader.ReadByte();
            g_reg_Eu = in_reader.ReadByte();
            g_reg_Fu = in_reader.ReadByte();
            g_reg_Hu = in_reader.ReadByte();
            g_reg_Lu = in_reader.ReadByte();
            g_reg_SP = in_reader.ReadUInt16();
            g_reg_IX = in_reader.ReadUInt16();
            g_reg_IY = in_reader.ReadUInt16();
            g_flag_S = in_reader.ReadInt32();
            g_flag_Z = in_reader.ReadInt32();
            g_flag_H = in_reader.ReadInt32();
            g_flag_PV = in_reader.ReadInt32();
            g_flag_N = in_reader.ReadInt32();
            g_flag_C = in_reader.ReadInt32();
            g_flag_Su = in_reader.ReadInt32();
            g_flag_Zu = in_reader.ReadInt32();
            g_flag_Hu = in_reader.ReadInt32();
            g_flag_PVu = in_reader.ReadInt32();
            g_flag_Nu = in_reader.ReadInt32();
            g_flag_Cu = in_reader.ReadInt32();
            g_reg_R = in_reader.ReadByte();
            g_reg_I = in_reader.ReadByte();
            g_IFF1 = in_reader.ReadBoolean();
            g_IFF2 = in_reader.ReadBoolean();
            g_interruptMode = in_reader.ReadInt32();
            g_interrupt_irq = in_reader.ReadBoolean();
            g_interrupt_nmi = in_reader.ReadBoolean();
            g_halt = in_reader.ReadBoolean();
            g_halt_out = in_reader.ReadBoolean();
            g_clock = in_reader.ReadInt32();
            g_clock_total = in_reader.ReadInt32();
            g_bank_register = in_reader.ReadUInt32();
            read_array(in_reader, g_ram);
        }

        private static void write_array(BinaryWriter in_writer, byte[] in_array)
        {
            in_writer.Write(in_array.Length);
            in_writer.Write(in_array);
        }

        private static void read_array(BinaryReader in_reader, byte[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("Z80 RAM size is invalid.");
            byte[] w_data = in_reader.ReadBytes(w_length);
            if (w_data.Length != w_length) throw new InvalidDataException("Z80 RAM data is invalid.");
            Buffer.BlockCopy(w_data, 0, in_array, 0, w_length);
        }
    }
}
