using System;
using static MDTracer.md_m68k;
namespace MDTracer
{
    internal partial class md_m68k
    {
        private void analyse_SUBA()
        {
            g_reg_PC += 2;
            int w_size = (g_op2 >> 2) + 1;
            g_clock = (w_size == 1) ? 8 : 6;
            adressing_func_address(g_op3, g_op4, w_size);
            uint src = adressing_func_read(g_op3, g_op4, w_size);
            if (w_size == 1)
            {
                src = (uint)(int)(short)(src & 0xffff);
            }
            else
            {
                src &= 0xffffffff;
            }
            g_reg_addr[g_op1].l = g_reg_addr[g_op1].l - src;
        }
    }
}
