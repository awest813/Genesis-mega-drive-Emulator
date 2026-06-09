namespace MDTracer
{
    //----------------------------------------------------------------
    // Shared code-analysis data types (Phase 3).
    //
    // TRACECODE and STACK_LIST previously lived inside Form_Code_Trace,
    // which forced ICodeAnalysisSession and NullCodeAnalysisSession to
    // reference the concrete WinForms tracer. These types now stand
    // alone so the session seam has no Form_* dependency.
    //----------------------------------------------------------------

    internal static class CodeAnalysisConstants
    {
        public const int StackListNum = 1024;
        public const int RomSize = 0x200000;
        public const int RamSize = 0x8000;
        public const int MemSize = RomSize + RamSize;
    }

    internal struct CodeAnalysisStackEntry
    {
        public M68kStackEntryType type;
        public uint func_address;
        public uint caller_address;
        public int caller_num;
        public uint ret_address;
        public uint start_address;
        public uint end_address;
        public uint stack_address;
    }

    internal struct CodeAnalysisTraceCode
    {
        public enum Type : int
        {
            Non,
            Opc,
            Opr,
            Unique,
            Chk
        }

        public Type type;
        public int address;
        public ushort val;
        public string? operand;
        public int leng2;
        public int front;
        public string? comment1;
        public bool break_static;
        public bool break_flash;
        public int jmp_address;
        public bool ret_line;
        public uint func_address;
        public bool operand_jsr;
        public List<CodeAnalysisStackEntry> stack;
    }
}
