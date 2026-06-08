namespace MDTracer
{
    //----------------------------------------------------------------
    // Debug-tool coordination seam (Phase 3).
    //
    // Form_Code_Trace previously reached directly into Form_Code,
    // Form_Registry, and Form_Flow when a trace break fired. This
    // interface routes those notifications through one frontend hook.
    //----------------------------------------------------------------
    internal interface IDebugToolsCoordinator
    {
        void OnTraceBreak(int in_line, uint in_funcAddress, uint in_callerAddress);
    }

    internal sealed class NullDebugToolsCoordinator : IDebugToolsCoordinator
    {
        public void OnTraceBreak(int in_line, uint in_funcAddress, uint in_callerAddress) { }
    }
}
