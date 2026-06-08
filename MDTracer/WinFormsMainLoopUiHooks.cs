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
            WinFormsDebugTools.UpdateDebugWindowDisplays();
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
