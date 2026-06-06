namespace MDTracer
{
    internal interface IMainLoopUiHooks
    {
        void SaveCurrentGameCodeSettings();
        void LoadCurrentGameCodeSettings();
        void UpdateCodeTrace();
        void PushTopTraceEntry(uint in_pc, uint in_stackAddress);
        void TraceFirstStepBreak();
        void ShowSettingsWindow();
        void UpdateSettingsWindow();
        void UpdateGameScreen(int in_taskUsage);
        void UpdateDebugWindows();
        void FlashTraceBreakAtAddress(uint in_pc);
        void ResetCodeTraceAnalysis();
    }

    internal sealed class NullMainLoopUiHooks : IMainLoopUiHooks
    {
        public void SaveCurrentGameCodeSettings() { }
        public void LoadCurrentGameCodeSettings() { }
        public void UpdateCodeTrace() { }
        public void PushTopTraceEntry(uint in_pc, uint in_stackAddress) { }
        public void TraceFirstStepBreak() { }
        public void ShowSettingsWindow() { }
        public void UpdateSettingsWindow() { }
        public void UpdateGameScreen(int in_taskUsage) { }
        public void UpdateDebugWindows() { }
        public void FlashTraceBreakAtAddress(uint in_pc) { }
        public void ResetCodeTraceAnalysis() { }
    }

    internal sealed class WinFormsMainLoopUiHooks : IMainLoopUiHooks
    {
        public void SaveCurrentGameCodeSettings()
        {
            WinFormsDebugTools.g_form_code.SaveCurrentGameCodeSettings();
        }

        public void LoadCurrentGameCodeSettings()
        {
            WinFormsDebugTools.g_form_code.LoadCurrentGameCodeSettings();
        }

        public void UpdateCodeTrace()
        {
            WinFormsDebugTools.g_form_code_trace.update();
        }

        public void PushTopTraceEntry(uint in_pc, uint in_stackAddress)
        {
            WinFormsDebugTools.g_form_code_trace.CPU_Trace_push(M68kStackEntryType.TOP, 0x0004, in_pc, 0, in_stackAddress);
        }

        public void TraceFirstStepBreak()
        {
            WinFormsDebugTools.g_form_code_trace.Trace_FirstStepBreak();
        }

        public void ShowSettingsWindow()
        {
            WinFormsDebugTools.g_form_setting.show_window();
        }

        public void UpdateSettingsWindow()
        {
            WinFormsDebugTools.g_form_setting.update();
        }

        public void UpdateGameScreen(int in_taskUsage)
        {
            Form_Main.Instance.picture_update(in_taskUsage);
        }

        public void UpdateDebugWindows()
        {
            if (md_main.g_screenA_enable == true)
            {
                WinFormsDebugTools.g_form_screenA.picture_update(md_main.g_md_vdp.g_scrollA_bitmap
                                                        , md_main.g_md_vdp.g_scroll_xsize
                                                        , md_main.g_md_vdp.g_scroll_ysize);
            }
            if (md_main.g_screenB_enable == true)
            {
                WinFormsDebugTools.g_form_screenB.picture_update(md_main.g_md_vdp.g_scrollB_bitmap
                                                        , md_main.g_md_vdp.g_scroll_xsize
                                                        , md_main.g_md_vdp.g_scroll_ysize);
            }
            if (md_main.g_screenW_enable == true)
            {
                WinFormsDebugTools.g_form_screenW.picture_update(md_main.g_md_vdp.g_scrollW_bitmap
                                                        , md_main.g_md_vdp.g_scroll_xsize
                                                        , md_main.g_md_vdp.g_scroll_ysize);
            }
            if (md_main.g_screenS_enable == true)
            {
                WinFormsDebugTools.g_form_screenS.picture_update(md_main.g_md_vdp.g_scrollS_bitmap
                                                        , md_main.g_md_vdp.SPRITE_XSIZE
                                                        , md_main.g_md_vdp.SPRITE_YSIZE);
            }
            if (md_main.g_pattern_enable == true)
            {
                WinFormsDebugTools.g_form_pattern.picture_update(md_main.g_md_vdp.g_pattern_table);
            }
            if (md_main.g_pallete_enable == true)
            {
                WinFormsDebugTools.g_form_pallete.Invalidate();
            }
            if (md_main.g_music_enable == true)
            {
                for (int i = 0; i < 10; i++)
                {
                    WinFormsDebugTools.g_form_music.g_freq_out[i] = md_main.g_md_music.g_freq_out[i];
                }
                WinFormsDebugTools.g_form_music.Invalidate();
            }
            if (md_main.g_code_enable == true)
            {
                WinFormsDebugTools.g_form_code.Invalidate();
            }
        }

        public void FlashTraceBreakAtAddress(uint in_pc)
        {
            int w_line = WinFormsDebugTools.g_form_code_trace.get_code_from_addr(in_pc);
            if (w_line >= 0)
            {
                WinFormsDebugTools.g_form_code_trace.g_analyse_code[w_line].break_flash = true;
            }
        }

        public void ResetCodeTraceAnalysis()
        {
            WinFormsDebugTools.g_form_code_trace.analyses_reset();
        }
    }
}
