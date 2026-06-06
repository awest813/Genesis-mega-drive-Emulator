namespace MDTracer
{
    internal sealed class WinFormsDebugToolsCoordinator : IDebugToolsCoordinator
    {
        public void OnTraceBreak(int in_line, uint in_funcAddress, uint in_callerAddress)
        {
            WinFormsDebugTools.g_form_code.RequestTraceBreakView(in_line);
            WinFormsDebugTools.g_form_registry.RequestRegistryRefresh();
            WinFormsDebugTools.g_form_flow.RequestFlowUpdate(in_funcAddress, in_callerAddress);
        }
    }
}
