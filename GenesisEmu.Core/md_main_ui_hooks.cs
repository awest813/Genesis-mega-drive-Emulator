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
}
