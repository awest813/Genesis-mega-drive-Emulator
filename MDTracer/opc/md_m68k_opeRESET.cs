using System;
using static MDTracer.md_m68k;
namespace MDTracer
{
    internal partial class md_m68k
    {
        private void analyse_RESET()
        {
            g_reg_addr[7].l = md_main.g_md_bus.read32(0);
            g_reg_PC = md_main.g_md_bus.read32(4);
        }
   }
}
