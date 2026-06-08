namespace MDTracer
{
    //----------------------------------------------------------------
    // Shared code-analysis session seam (Phase 3).
    //
    // Form_Code, Form_Flow, Form_Registry, and the main-loop hooks
    // previously reached directly into Form_Code_Trace for disassembly
    // buffers, trace controls, and call-stack display. This interface
    // exposes that shared state through WinFormsDebugTools instead.
    //----------------------------------------------------------------
    internal interface ICodeAnalysisSession
    {
        int MemSize { get; }
        Form_Code_Trace.TRACECODE[] AnalyseCode { get; }
        int ArrowStartLine { get; set; }
        int ArrowEndLine { get; set; }
        bool CpuPause { get; }
        int StackCur { get; }
        Form_Code_Trace.STACK_LIST[] StackList { get; }
        string[] StackListTypeStr { get; }

        int GetCodeFromAddr(uint in_addr);
        void Update();
        void Analyses();
        void AnalysesReset();
        void TraceStart();
        void TraceStop();
        void TraceStepIn();
        void TraceStepOver();
        void TraceFirstStepBreak();
        void SetBreakFlash(int in_line, bool in_value);
    }

    internal sealed class NullCodeAnalysisSession : ICodeAnalysisSession
    {
        private static readonly Form_Code_Trace.TRACECODE[] EmptyAnalyseCode = Array.Empty<Form_Code_Trace.TRACECODE>();
        private static readonly Form_Code_Trace.STACK_LIST[] EmptyStackList = Array.Empty<Form_Code_Trace.STACK_LIST>();
        private static readonly string[] EmptyStackListTypeStr = Array.Empty<string>();

        public int MemSize => 0;
        public Form_Code_Trace.TRACECODE[] AnalyseCode => EmptyAnalyseCode;
        public int ArrowStartLine { get; set; }
        public int ArrowEndLine { get; set; }
        public bool CpuPause => false;
        public int StackCur => 0;
        public Form_Code_Trace.STACK_LIST[] StackList => EmptyStackList;
        public string[] StackListTypeStr => EmptyStackListTypeStr;

        public int GetCodeFromAddr(uint in_addr) => -1;
        public void Update() { }
        public void Analyses() { }
        public void AnalysesReset() { }
        public void TraceStart() { }
        public void TraceStop() { }
        public void TraceStepIn() { }
        public void TraceStepOver() { }
        public void TraceFirstStepBreak() { }
        public void SetBreakFlash(int in_line, bool in_value) { }
    }
}
