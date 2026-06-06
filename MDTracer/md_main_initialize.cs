using System.Diagnostics;

namespace MDTracer
{
    internal partial class md_main
    {
        public static void initialize()
        {
            Process currentProcess = Process.GetCurrentProcess();
            int processId = currentProcess.Id;
            currentProcess.PriorityClass = ProcessPriorityClass.High;

            g_md_cartridge = new md_cartridge();
            g_md_sram = new md_sram();
            g_md_mapper = new md_mapper();
            g_md_bus = new md_bus();
            g_md_control = new md_control();
            g_md_io = new md_io();
            g_md_m68k = new md_m68k();
            g_md_z80 = new md_z80();
            g_md_vdp = new md_vdp();
            g_md_music = new md_music();

            // Wire the 68000 core's injected collaborators now that the bus exists.
            // The tracer and bus monitor are attached later by WinFormsDebugTools
            // when the frontend starts; tests use NullM68kTracer / NullBusMonitor.
            g_md_m68k.g_bus = g_md_bus;

            g_setting_name = new List<string>();
            g_setting_val = new List<string>();
            g_task_usage = 0;
        }
    }
}
