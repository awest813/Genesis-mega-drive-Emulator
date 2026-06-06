namespace MDTracer
{
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
            WinFormsDebugTools.g_codeAnalysis.Update();
        }

        public void PushTopTraceEntry(uint in_pc, uint in_stackAddress)
        {
            WinFormsDebugTools.g_cpuTracer.CPU_Trace_push(M68kStackEntryType.TOP, 0x0004, in_pc, 0, in_stackAddress);
        }

        public void TraceFirstStepBreak()
        {
            WinFormsDebugTools.g_codeAnalysis.TraceFirstStepBreak();
        }

        public void ShowSettingsWindow()
        {
            WinFormsDebugTools.g_form_setting.show_window();
        }

        public void UpdateSettingsWindow()
        {
            md_main.g_frontendSettings.NotifyDebugWindowLayoutChanged();
        }

        public void UpdateGameScreen(int in_taskUsage)
        {
            Form_Main.Instance.picture_update(in_taskUsage);
        }

        public void UpdateDebugWindows()
        {
            if (md_main.g_debugView.screenA_enable == true)
            {
                WinFormsDebugTools.g_form_screenA.picture_update(md_main.g_md_vdp.g_scrollA_bitmap
                                                        , md_main.g_md_vdp.g_scroll_xsize
                                                        , md_main.g_md_vdp.g_scroll_ysize);
            }
            if (md_main.g_debugView.screenB_enable == true)
            {
                WinFormsDebugTools.g_form_screenB.picture_update(md_main.g_md_vdp.g_scrollB_bitmap
                                                        , md_main.g_md_vdp.g_scroll_xsize
                                                        , md_main.g_md_vdp.g_scroll_ysize);
            }
            if (md_main.g_debugView.screenW_enable == true)
            {
                WinFormsDebugTools.g_form_screenW.picture_update(md_main.g_md_vdp.g_scrollW_bitmap
                                                        , md_main.g_md_vdp.g_scroll_xsize
                                                        , md_main.g_md_vdp.g_scroll_ysize);
            }
            if (md_main.g_debugView.screenS_enable == true)
            {
                WinFormsDebugTools.g_form_screenS.picture_update(md_main.g_md_vdp.g_scrollS_bitmap
                                                        , md_main.g_md_vdp.SPRITE_XSIZE
                                                        , md_main.g_md_vdp.SPRITE_YSIZE);
            }
            if (md_main.g_debugView.pattern_enable == true)
            {
                WinFormsDebugTools.g_form_pattern.picture_update(md_main.g_md_vdp.g_pattern_table);
            }
            if (md_main.g_debugView.pallete_enable == true)
            {
                WinFormsDebugTools.g_form_pallete.Invalidate();
            }
            if (md_main.g_debugView.music_enable == true)
            {
                for (int i = 0; i < 10; i++)
                {
                    WinFormsDebugTools.g_form_music.g_freq_out[i] = md_main.g_md_music.g_freq_out[i];
                }
                WinFormsDebugTools.g_form_music.Invalidate();
            }
            if (md_main.g_debugView.code_enable == true)
            {
                WinFormsDebugTools.g_form_code.Invalidate();
            }
        }

        public void FlashTraceBreakAtAddress(uint in_pc)
        {
            int w_line = WinFormsDebugTools.g_codeAnalysis.GetCodeFromAddr(in_pc);
            if (w_line >= 0)
            {
                WinFormsDebugTools.g_codeAnalysis.SetBreakFlash(w_line, true);
            }
        }

        public void ResetCodeTraceAnalysis()
        {
            WinFormsDebugTools.g_codeAnalysis.AnalysesReset();
        }
    }
}
