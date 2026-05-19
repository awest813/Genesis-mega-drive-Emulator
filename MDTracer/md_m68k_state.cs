using System.IO;

namespace MDTracer
{
    internal partial class md_m68k
    {
        private const int StateWorkRamStart = 0xff0000;
        internal const int StateWorkRamSize = 0x10000;

        public void write_state(BinaryWriter in_writer)
        {
            in_writer.Write(g_reg_PC);
            in_writer.Write(g_stack_top);
            in_writer.Write(g_initial_PC);
            in_writer.Write(g_reg_addr_usp.l);
            in_writer.Write(g_reg_SR);

            for (int i = 0; i < 8; i++)
            {
                in_writer.Write(g_reg_data[i].l);
            }
            for (int i = 0; i < 8; i++)
            {
                in_writer.Write(g_reg_addr[i].l);
            }

            in_writer.Write(g_interrupt_V_req);
            in_writer.Write(g_interrupt_H_req);
            in_writer.Write(g_interrupt_EXT_req);
            in_writer.Write(g_interrupt_V_act);
            in_writer.Write(g_interrupt_H_act);
            in_writer.Write(g_interrupt_EXT_act);
            in_writer.Write(g_68k_stop);
            in_writer.Write(g_clock_total);
            in_writer.Write(g_clock_now);
            in_writer.Write(g_clock);

            in_writer.Write(StateWorkRamSize);
            in_writer.Write(g_memory, StateWorkRamStart, StateWorkRamSize);
        }

        public void restore_state(BinaryReader in_reader)
        {
            restore_state(in_reader, true);
        }

        public void restore_state(BinaryReader in_reader, bool in_restoreMemory)
        {
            g_reg_PC = in_reader.ReadUInt32();
            g_stack_top = in_reader.ReadUInt32();
            g_initial_PC = in_reader.ReadUInt32();
            g_reg_addr_usp.l = in_reader.ReadUInt32();
            g_reg_SR = in_reader.ReadUInt16();

            for (int i = 0; i < 8; i++)
            {
                g_reg_data[i].l = in_reader.ReadUInt32();
            }
            for (int i = 0; i < 8; i++)
            {
                g_reg_addr[i].l = in_reader.ReadUInt32();
            }

            g_interrupt_V_req = in_reader.ReadBoolean();
            g_interrupt_H_req = in_reader.ReadBoolean();
            g_interrupt_EXT_req = in_reader.ReadBoolean();
            g_interrupt_V_act = in_reader.ReadBoolean();
            g_interrupt_H_act = in_reader.ReadBoolean();
            g_interrupt_EXT_act = in_reader.ReadBoolean();
            g_68k_stop = in_reader.ReadBoolean();
            g_clock_total = in_reader.ReadInt64();
            g_clock_now = in_reader.ReadInt64();
            g_clock = in_reader.ReadInt32();

            if (in_restoreMemory == true)
            {
                restore_work_ram_state(in_reader);
            }
        }

        public void restore_work_ram_state(BinaryReader in_reader)
        {
            int w_ram_size = in_reader.ReadInt32();
            if (w_ram_size != StateWorkRamSize)
            {
                throw new InvalidDataException("MD state capture RAM size is invalid. Expected " + StateWorkRamSize + " bytes, got " + w_ram_size + " bytes.");
            }

            byte[] w_ram = in_reader.ReadBytes(w_ram_size);
            if (w_ram.Length != StateWorkRamSize)
            {
                throw new InvalidDataException("MD state capture RAM size is invalid. Expected " + StateWorkRamSize + " bytes, got " + w_ram.Length + " bytes.");
            }

            Buffer.BlockCopy(w_ram, 0, g_memory, StateWorkRamStart, StateWorkRamSize);
        }

        public void restore_work_ram_raw_state(Stream in_stream)
        {
            byte[] w_ram = new byte[StateWorkRamSize];
            int w_read = in_stream.Read(w_ram, 0, w_ram.Length);
            if (w_read != StateWorkRamSize)
            {
                throw new InvalidDataException("MD state capture RAM size is invalid. Expected " + StateWorkRamSize + " bytes, got " + w_read + " bytes.");
            }

            Buffer.BlockCopy(w_ram, 0, g_memory, StateWorkRamStart, StateWorkRamSize);
        }
    }
}
