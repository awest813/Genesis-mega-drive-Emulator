using System.Runtime.CompilerServices;

namespace MDTracer
{
    internal partial class md_m68k
    {
        public byte[] g_memory;
        //----------------------------------------------------------------
        //read
        //----------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte read8(uint in_address)
        {
            in_address &= 0xffffff;
            if (0xe00000 <= in_address) in_address = (in_address & 0xffff) | 0xff0000;
            byte[] w_memory = g_memory;
            return w_memory[in_address];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort read16(uint in_address)
        {
            in_address &= 0xffffff;
            if (0xe00000 <= in_address) in_address = (in_address & 0xffff) | 0xff0000;
            byte[] w_memory = g_memory;
            return (ushort)((w_memory[in_address] << 8)
                          | w_memory[in_address + 1]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint read32(uint in_address)
        {
            in_address &= 0xffffff;
            if (0xe00000 <= in_address) in_address = (in_address & 0xffff) | 0xff0000;
            byte[] w_memory = g_memory;
            return (uint)((w_memory[in_address] << 24)
                        | (w_memory[in_address + 1] << 16)
                        | (w_memory[in_address + 2] << 8)
                        | w_memory[in_address + 3]);
        }
        //----------------------------------------------------------------
        //write
        //----------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void write8(uint in_address, byte in_data)
        {
            in_address &= 0xffffff;
            if (0xe00000 <= in_address) in_address = (in_address & 0xffff) | 0xff0000;
            byte[] w_memory = g_memory;
            w_memory[in_address] = in_data;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void write16(uint in_address, ushort in_data)
        {
            in_address &= 0xffffff;
            if (0xe00000 <= in_address) in_address = (in_address & 0xffff) | 0xff0000;
            byte[] w_memory = g_memory;
            w_memory[in_address] = (byte)(in_data >> 8);
            w_memory[in_address + 1] = (byte)in_data;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void write32(uint in_address, uint in_data)
        {
            in_address &= 0xffffff;
            if (0xe00000 <= in_address) in_address = (in_address & 0xffff) | 0xff0000;
            byte[] w_memory = g_memory;
            w_memory[in_address] = (byte)(in_data >> 24);
            w_memory[in_address + 1] = (byte)(in_data >> 16);
            w_memory[in_address + 2] = (byte)(in_data >> 8);
            w_memory[in_address + 3] = (byte)in_data;
        }
    }
}

