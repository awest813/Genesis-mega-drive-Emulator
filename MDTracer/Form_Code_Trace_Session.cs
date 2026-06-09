namespace MDTracer
{
    public partial class Form_Code_Trace : ICodeAnalysisSession
    {
        int ICodeAnalysisSession.MemSize => CodeAnalysisConstants.MemSize;
        CodeAnalysisTraceCode[] ICodeAnalysisSession.AnalyseCode => g_analyse_code;
        int ICodeAnalysisSession.ArrowStartLine
        {
            get => g_arrow_start_line;
            set => g_arrow_start_line = value;
        }
        int ICodeAnalysisSession.ArrowEndLine
        {
            get => g_arrow_end_line;
            set => g_arrow_end_line = value;
        }
        bool ICodeAnalysisSession.CpuPause => g_executionTracer!.CpuPause;
        int ICodeAnalysisSession.StackCur => g_executionTracer!.StackCur;
        CodeAnalysisStackEntry[] ICodeAnalysisSession.StackList => g_executionTracer!.StackList;
        string[] ICodeAnalysisSession.StackListTypeStr => STACK_LIST_TYPE_STR;

        int ICodeAnalysisSession.GetCodeFromAddr(uint in_addr) => get_code_from_addr(in_addr);
        void ICodeAnalysisSession.Update() => update();
        void ICodeAnalysisSession.Analyses() => analyses();
        void ICodeAnalysisSession.AnalysesReset() => analyses_reset();
        void ICodeAnalysisSession.TraceStart() => Trace_Start();
        void ICodeAnalysisSession.TraceStop() => Trace_Stop();
        void ICodeAnalysisSession.TraceStepIn() => Trace_StepIn();
        void ICodeAnalysisSession.TraceStepOver() => Trace_StepOver();
        void ICodeAnalysisSession.TraceFirstStepBreak() => Trace_FirstStepBreak();
        void ICodeAnalysisSession.SetBreakFlash(int in_line, bool in_value)
        {
            if ((0 <= in_line) && (in_line < g_analyse_code.Length))
            {
                g_analyse_code[in_line].break_flash = in_value;
            }
        }
    }
}
