using System.Diagnostics;

namespace MDTracer
{
    public partial class Form_Code_Trace
    {
        public string[] STACK_LIST_TYPE_STR = new string[] { "", "TOP", "JSR", "BSR", "TRAP", "HINT", "VINT", "EXT" };
        public int g_arrow_start_line;
        public int g_arrow_end_line;

        private M68kExecutionTracer? g_executionTracer;

        public M68kExecutionTracer ExecutionTracer => g_executionTracer!;

        public Form_Code_Trace()
        {
            initialize();
        }

        //----------------------------------------------------------------
        //initialize
        //----------------------------------------------------------------
        public void initialize()
        {
            g_analyse_code = new CodeAnalysisTraceCode[CodeAnalysisConstants.MemSize];
            g_executionTracer = new M68kExecutionTracer(this);
        }

        //----------------------------------------------------------------
        //trace event
        //----------------------------------------------------------------
        public void Trace_Start() => g_executionTracer!.Trace_Start();
        public void Trace_Stop() => g_executionTracer!.Trace_Stop();
        public void Trace_StepIn() => g_executionTracer!.Trace_StepIn();
        public void Trace_StepOver() => g_executionTracer!.Trace_StepOver();
        public void Trace_FirstStepBreak() => g_executionTracer!.Trace_FirstStepBreak();
    }
}
