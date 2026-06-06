namespace MDTracer
{
    //----------------------------------------------------------------
    // WinForms debug-tool window registry.
    //
    // md_main previously owned every Form_* instance as static state.
    // That kept the emulation coordinator coupled to the WinForms frontend
    // even after the main-loop UI calls were moved behind IMainLoopUiHooks.
    //
    // This class is the single owner of debug-tool windows. Production
    // wires collaborators (tracer, bus monitor, settings hooks) here;
    // headless/tests never call Initialize().
    //----------------------------------------------------------------
    internal static class WinFormsDebugTools
    {
        public static Form_Setting g_form_setting;
        public static Form_VDP_Screen g_form_screenA;
        public static Form_VDP_Screen g_form_screenB;
        public static Form_VDP_Screen g_form_screenW;
        public static Form_VDP_Screen g_form_screenS;
        public static Form_Pattern g_form_pattern;
        public static Form_Pallete g_form_pallete;

        public static Form_Code g_form_code;
        public static Form_Code_Trace g_form_code_trace;
        public static Form_IO g_form_io;
        public static Form_MUSIC g_form_music;
        public static Form_Registry g_form_registry;
        public static Form_Flow g_form_flow;

        public static void Initialize()
        {
            g_form_setting = new Form_Setting();
            g_form_screenA = new Form_VDP_Screen();
            g_form_screenB = new Form_VDP_Screen();
            g_form_screenW = new Form_VDP_Screen();
            g_form_screenS = new Form_VDP_Screen();
            g_form_pattern = new Form_Pattern();
            g_form_pallete = new Form_Pallete();
            g_form_code_trace = new Form_Code_Trace();
            g_form_code = new Form_Code();
            g_form_io = new Form_IO();
            g_form_music = new Form_MUSIC();
            g_form_registry = new Form_Registry();
            g_form_flow = new Form_Flow();

            md_main.g_mainLoopUI = new WinFormsMainLoopUiHooks();
            md_main.g_frontendSettings = new WinFormsFrontendSettingsHooks();
            md_io.g_frontendHooks = new WinFormsIoFrontendHooks();

            g_md_m68k.g_tracer = g_form_code_trace;
            g_md_bus.g_monitor = g_form_code;
        }

        private static md_m68k g_md_m68k => md_main.g_md_m68k;
        private static md_bus g_md_bus => md_main.g_md_bus;
    }
}
