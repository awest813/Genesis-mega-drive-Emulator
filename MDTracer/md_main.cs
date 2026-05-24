using System.Diagnostics;

namespace MDTracer
{
    internal partial class md_main
    {
        public const int VDL_LINE_RENDER_MC68_CLOCK = 488;  //7670453(cpu)  / 60(frame) / 262(line)
        public const int VDL_LINE_RENDER_Z80_CLOCK = 228;   //VDL_LINE_RENDER_MC68_CLOCK / (7.67 / 3.58)

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
        
        public static md_cartridge g_md_cartridge;
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

        private static int g_task_usage;
        //----------------------------------------------------------------
        public static bool run(string in_romname)
        {
            g_form_code.SaveCurrentGameCodeSettings();
            g_stop_req = false;
            g_state_capture_rom_file_name = "";
            if (false == g_md_cartridge.load(in_romname)) return false;
            g_state_capture_rom_file_name = Path.GetFileName(in_romname);
            g_state_capture_status = "";
            g_md_m68k.reset();
            g_form_code_trace.update();
            g_form_code_trace.CPU_Trace_push(Form_Code_Trace.STACK_LIST_TYPE.TOP, 0x0004, g_md_m68k.g_reg_PC, 0, g_md_m68k.g_reg_addr[7].l);
            g_form_code.LoadCurrentGameCodeSettings();
            if (g_trace_fsb == true)
            {
                g_form_code_trace.Trace_FirstStepBreak();
                g_code_enable = true;
                g_form_setting.show_window();
                g_form_setting.update();
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
            Form_Main.Instance.picture_update(g_task_usage);
        }
        public static void Screen_Update()
        {
            if (g_screenA_enable == true)
            {
                g_form_screenA.picture_update(g_md_vdp.g_scrollA_bitmap
                                                , g_md_vdp.g_scroll_xsize
                                                , g_md_vdp.g_scroll_ysize);
            }
            if (g_screenB_enable == true)
            {
                g_form_screenB.picture_update(g_md_vdp.g_scrollB_bitmap
                                                , g_md_vdp.g_scroll_xsize
                                                , g_md_vdp.g_scroll_ysize);
            }
            if (g_screenW_enable == true)
            {
                g_form_screenW.picture_update(g_md_vdp.g_scrollW_bitmap
                                                , g_md_vdp.g_scroll_xsize
                                                , g_md_vdp.g_scroll_ysize);
            }
            if (g_screenS_enable == true)
            {
                g_form_screenS.picture_update(g_md_vdp.g_scrollS_bitmap
                                                , g_md_vdp.SPRITE_XSIZE
                                                , g_md_vdp.SPRITE_YSIZE);
            }
            if (g_pattern_enable == true)
            {
                g_form_pattern.picture_update(g_md_vdp.g_pattern_table);
            }
            if (g_pallete_enable == true)
            {
                g_form_pallete.Invalidate();
            }
            if (g_music_enable == true)
            {
                for (int i = 0; i < 10; i++)
                {
                    g_form_music.g_freq_out[i] = g_md_music.g_freq_out[i];
                }
                g_form_music.Invalidate();
            }
            if (g_code_enable == true)
            {
                g_form_code.Invalidate();
            }
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
                    int w_line = g_form_code_trace.get_code_from_addr(g_md_m68k.g_reg_PC);
                    if (w_line >= 0)
                    {
                        g_form_code_trace.g_analyse_code[w_line].break_flash = true;
                    }
                    g_trace_nextframe = false;
                }
                g_md_io.input_update_frame();
                process_state_capture_request();
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
        private static void hard_reset()
        {
            g_trace_nextframe = false;
            g_state_capture_status = "";
            g_md_m68k.reset();
            g_md_z80.reset();
            g_md_vdp.reset();
            g_md_music.reset();
            g_md_control.reset();
            g_md_z80.g_active = false;
            read_setting();
            g_md_music.setting();
            g_form_code_trace.analyses_reset();
            g_form_code_trace.CPU_Trace_push(Form_Code_Trace.STACK_LIST_TYPE.TOP, 0x0004, g_md_m68k.g_reg_PC, 0, g_md_m68k.g_reg_addr[7].l);
            if (g_trace_fsb == true)
            {
                g_form_code_trace.Trace_FirstStepBreak();
            }
        }
    }
}
