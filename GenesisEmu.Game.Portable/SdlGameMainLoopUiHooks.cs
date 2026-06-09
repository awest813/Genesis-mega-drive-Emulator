using MDTracer;

namespace GenesisEmu.Game.Portable
{
  internal sealed class SdlGameMainLoopUiHooks : IMainLoopUiHooks
  {
    private readonly SdlGameApp g_app;

    public SdlGameMainLoopUiHooks(SdlGameApp in_app)
    {
      g_app = in_app;
    }

    public void SaveCurrentGameCodeSettings() { }
    public void LoadCurrentGameCodeSettings() { }
    public void UpdateCodeTrace() { }
    public void PushTopTraceEntry(uint in_pc, uint in_stackAddress) { }
    public void TraceFirstStepBreak() { }
    public void ShowSettingsWindow() { }
    public void UpdateSettingsWindow() { }
    public void UpdateGameScreen(int in_taskUsage) => g_app.RequestFramePresent(in_taskUsage);
    public void UpdateDebugWindows() { }
    public void FlashTraceBreakAtAddress(uint in_pc) { }
    public void ResetCodeTraceAnalysis() { }
  }
}
