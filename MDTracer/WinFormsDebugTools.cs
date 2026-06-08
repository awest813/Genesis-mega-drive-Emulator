using GenesisEmu.Frontend.Windows;

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
        public static IDebugToolsCoordinator g_coordinator = new NullDebugToolsCoordinator();
        public static ICodeAnalysisSession g_codeAnalysis = new NullCodeAnalysisSession();
        public static IM68kTracer g_cpuTracer = new NullM68kTracer();
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
            g_codeAnalysis = g_form_code_trace;
            g_cpuTracer = g_form_code_trace;
            g_form_code = new Form_Code();
            g_form_io = new Form_IO();
            g_form_music = new Form_MUSIC();
            g_form_registry = new Form_Registry();
            g_form_flow = new Form_Flow();

            ApplyDebugTheme(
                g_form_setting,
                g_form_screenA,
                g_form_screenB,
                g_form_screenW,
                g_form_screenS,
                g_form_pattern,
                g_form_pallete,
                g_form_io,
                g_form_registry,
                g_form_flow,
                g_form_code,
                g_form_music);

            g_coordinator = new WinFormsDebugToolsCoordinator();
            md_main.g_mainLoopUI = new WinFormsMainLoopUiHooks();
            md_main.g_frontendSettings = new WinFormsFrontendSettingsHooks();
            md_main.g_audioFrontendHooks = new WinFormsAudioFrontendHooks();
            md_io.g_frontendHooks = new WinFormsIoFrontendHooks();

            g_md_m68k.g_tracer = g_cpuTracer;
            g_md_bus.g_monitor = g_form_code;
        }

        public static void UpdateDebugWindowDisplays()
        {
            md_vdp w_vdp = md_main.g_md_vdp;
            DebugViewState w_debugView = md_main.g_debugView;
            if (w_vdp == null) return;

            if (w_debugView.screenA_enable == true)
            {
                g_form_screenA.picture_update(w_vdp.g_scrollA_pixels
                    , VdpDebugLayerConstants.ScrollLayerWidth
                    , VdpDebugLayerConstants.ScrollLayerHeight
                    , w_vdp.g_scroll_xsize
                    , w_vdp.g_scroll_ysize);
            }
            if (w_debugView.screenB_enable == true)
            {
                g_form_screenB.picture_update(w_vdp.g_scrollB_pixels
                    , VdpDebugLayerConstants.ScrollLayerWidth
                    , VdpDebugLayerConstants.ScrollLayerHeight
                    , w_vdp.g_scroll_xsize
                    , w_vdp.g_scroll_ysize);
            }
            if (w_debugView.screenW_enable == true)
            {
                g_form_screenW.picture_update(w_vdp.g_scrollW_pixels
                    , VdpDebugLayerConstants.ScrollLayerWidth
                    , VdpDebugLayerConstants.ScrollLayerHeight
                    , w_vdp.g_scroll_xsize
                    , w_vdp.g_scroll_ysize);
            }
            if (w_debugView.screenS_enable == true)
            {
                g_form_screenS.picture_update(w_vdp.g_scrollS_pixels
                    , VdpDebugLayerConstants.SpriteLayerWidth
                    , VdpDebugLayerConstants.SpriteLayerHeight
                    , w_vdp.SPRITE_XSIZE
                    , w_vdp.SPRITE_YSIZE);
            }
            if (w_debugView.pattern_enable == true)
            {
                g_form_pattern.picture_update(
                    w_vdp.g_pattern_pixels
                    , VdpDebugLayerConstants.PatternWidth
                    , VdpDebugLayerConstants.PatternHeight);
            }
            if (w_debugView.pallete_enable == true)
            {
                g_form_pallete.Invalidate();
            }
            if (w_debugView.music_enable == true)
            {
                for (int i = 0; i < 10; i++)
                {
                    g_form_music.g_freq_out[i] = md_main.g_md_music.g_freq_out[i];
                }
                g_form_music.Invalidate();
            }
            if (w_debugView.code_enable == true)
            {
                g_form_code.Invalidate();
            }
        }

        private static md_m68k g_md_m68k => md_main.g_md_m68k;
        private static md_bus g_md_bus => md_main.g_md_bus;

        private static void ApplyDebugTheme(params Form[] in_forms)
        {
            foreach (Form w_form in in_forms)
            {
                WinFormsDebugTheme.Apply(w_form);
            }
        }
    }
}
