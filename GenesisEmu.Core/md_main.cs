using System.Diagnostics;

namespace MDTracer
{
    internal partial class md_main
    {
        public const int VDL_LINE_RENDER_MC68_CLOCK = 488;  //7670453(cpu)  / 60(frame) / 262(line)
        public const int VDL_LINE_RENDER_Z80_CLOCK = 228;   //VDL_LINE_RENDER_MC68_CLOCK / (7.67 / 3.58)

        public static md_cartridge g_md_cartridge;
        public static md_sram g_md_sram;
        public static md_mapper g_md_mapper;
        public static md_bus g_md_bus;
        public static md_control g_md_control;
        public static md_io g_md_io;
        public static md_m68k g_md_m68k;
        public static md_z80 g_md_z80;
        public static md_vdp g_md_vdp;
        public static md_music g_md_music;

        public static bool g_screenA_enable;
        public static bool g_screenB_enable;
        public static bool g_screenW_enable;
        public static bool g_screenS_enable;
        public static bool g_pattern_enable;
        public static bool g_pallete_enable;
        public static bool g_code_enable;
        public static bool g_io_enable;
        public static bool g_music_enable;
        public static bool g_registry_enable;
        public static bool g_flow_enable;

        public static bool g_trace_fsb;
        public static bool g_trace_sip;
        public static bool g_hard_reset_req;
        public static bool g_trace_nextframe;
        internal static IMainLoopUiHooks g_mainLoopUI = new NullMainLoopUiHooks();
        internal static IFrontendSettingsHooks g_frontendSettings = new NullFrontendSettingsHooks();
        internal static IAudioFrontendHooks g_audioFrontendHooks = new NullAudioFrontendHooks();

        private static int g_task_usage;
        private const int SRAM_AUTOSAVE_FRAME_INTERVAL = 180;
        private static int g_sram_autosave_frame_counter;
        //----------------------------------------------------------------
        public static bool run(string in_romname)
        {
            g_mainLoopUI.SaveCurrentGameCodeSettings();
            g_stop_req = false;
            g_state_capture_rom_file_name = "";
            g_md_sram.save();   // flush the previously loaded game's battery RAM
            if (false == g_md_cartridge.load(in_romname)) return false;
            g_state_capture_rom_file_name = Path.GetFileName(in_romname);
            g_md_sram.configure(g_md_cartridge, in_romname);
            g_md_sram.load();
            g_md_mapper.configure(g_md_cartridge);
            g_state_capture_status = "";
            g_sram_autosave_frame_counter = 0;
            g_md_m68k.reset();
            g_mainLoopUI.UpdateCodeTrace();
            g_mainLoopUI.PushTopTraceEntry(g_md_m68k.g_reg_PC, g_md_m68k.g_reg_addr[7].l);
            g_mainLoopUI.LoadCurrentGameCodeSettings();
            if (g_trace_fsb == true)
            {
                g_mainLoopUI.TraceFirstStepBreak();
                g_code_enable = true;
                g_mainLoopUI.ShowSettingsWindow();
                g_mainLoopUI.UpdateSettingsWindow();
                write_setting();
            }

            g_md_vdp.g_waitHandle = new ManualResetEvent(false);
            Task<int> task = Task.Run<int>(() =>
            {
                md_run();
                return 0;
            });
            Task<int> task_ppu = Task.Run<int>(() =>
            {
                g_md_vdp.run_event();
                return 0;
            });
            return true;
        }
        public static void Screen_Game_Update()
        {
            g_mainLoopUI.UpdateGameScreen(g_task_usage);
        }
        public static void Screen_Update()
        {
            g_mainLoopUI.UpdateDebugWindows();
        }

        private static void md_run()
        {
            int w_log_pef_sum = 0;
            int w_log_pef_cnt = 0;
            Stopwatch w_stopwatch = new Stopwatch();
            w_stopwatch.Start();

            while (true)
            {
                if (g_hard_reset_req == true)
                {
                    hard_reset();
                    g_hard_reset_req = false;
                }
                bool w_frame_advance = false;
                if (g_stop_req == true)
                {
                    w_frame_advance = consume_frame_advance_request();
                }
                if ((g_stop_req == true) && (w_frame_advance == false))
                {
                    w_stopwatch.Restart();
                    Thread.Sleep(10);
                    continue;
                }
                if (w_frame_advance == true)
                {
                    request_frame_advance_update();
                }
                if(g_trace_nextframe == true)
                {
                    g_mainLoopUI.FlashTraceBreakAtAddress(g_md_m68k.g_reg_PC);
                    g_trace_nextframe = false;
                }
                g_md_io.input_update_frame();
                process_state_capture_request();
                maybe_autosave_sram();
                for (int w_vline = 0; w_vline < g_md_vdp.g_vertical_line_max; w_vline++)
                {
                    g_md_vdp.run(w_vline);
                    g_md_m68k.run(VDL_LINE_RENDER_MC68_CLOCK);
                    g_md_z80.run(VDL_LINE_RENDER_Z80_CLOCK);
                    g_md_music.run(VDL_LINE_RENDER_MC68_CLOCK);
                }

                //----------------------------------------------------------------
                //Clock Generator : chips:315-5345
                //----------------------------------------------------------------
                TimeSpan timeSpan2 = w_stopwatch.Elapsed;
                int wtime = 0;
                TimeSpan timeSpan;
                float w_wait = 16666.666f;

                timeSpan = w_stopwatch.Elapsed;
                wtime = (int)(timeSpan.TotalMilliseconds * 1000);
                int w_log_pef = (int)((wtime / w_wait) * 100);
                w_log_pef_sum += w_log_pef;
                w_log_pef_cnt += 1;
                if (w_log_pef_cnt % 60 == 0)
                {
                    g_task_usage = w_log_pef_sum / w_log_pef_cnt;
                    w_log_pef_cnt = 0;
                    w_log_pef_sum = 0;
                }
                if (is_clock_wait_skip() == false)
                {
                    do
                    {
                        timeSpan = w_stopwatch.Elapsed;
                        wtime = (int)(timeSpan.TotalMilliseconds * 1000);
                    } while ((w_wait > wtime) && (timeSpan.Seconds < 1));    //1,000,000 / 60
                }
                w_stopwatch.Restart();
            }
        }
        private static void maybe_autosave_sram()
        {
            if (g_md_sram.g_dirty == false) return;
            g_sram_autosave_frame_counter++;
            if (g_sram_autosave_frame_counter < SRAM_AUTOSAVE_FRAME_INTERVAL) return;
            g_sram_autosave_frame_counter = 0;
            g_md_sram.save();
        }

        private static void hard_reset()
        {
            g_trace_nextframe = false;
            g_state_capture_status = "";
            g_md_sram.save();
            g_md_m68k.reset();
            g_md_mapper.reset();
            g_md_z80.reset();
            g_md_vdp.reset();
            g_md_music.reset();
            g_md_control.reset();
            g_md_z80.g_active = false;
            read_setting();
            g_md_music.setting();
            g_mainLoopUI.ResetCodeTraceAnalysis();
            g_mainLoopUI.PushTopTraceEntry(g_md_m68k.g_reg_PC, g_md_m68k.g_reg_addr[7].l);
            if (g_trace_fsb == true)
            {
                g_mainLoopUI.TraceFirstStepBreak();
            }
        }
    }
}
