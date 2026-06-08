using MDTracer;

namespace GenesisEmu.Game
{
  //----------------------------------------------------------------
  // Minimal main-loop hooks for the game-only frontend. Debug/trace
  // and settings UI calls are no-ops; only the game screen updates.
  //----------------------------------------------------------------
  internal sealed class GameMainLoopUiHooks : IMainLoopUiHooks
    {
        private readonly GameForm g_form;

        public GameMainLoopUiHooks(GameForm in_form)
        {
            g_form = in_form;
        }

        public void SaveCurrentGameCodeSettings() { }
        public void LoadCurrentGameCodeSettings() { }
        public void UpdateCodeTrace() { }
        public void PushTopTraceEntry(uint in_pc, uint in_stackAddress) { }
        public void TraceFirstStepBreak() { }
        public void ShowSettingsWindow() { }
        public void UpdateSettingsWindow() { }
        public void UpdateGameScreen(int in_taskUsage) => g_form.PictureUpdate(in_taskUsage);
        public void UpdateDebugWindows() { }
        public void FlashTraceBreakAtAddress(uint in_pc) { }
        public void ResetCodeTraceAnalysis() { }
    }
}
