namespace MDTracer
{
    //----------------------------------------------------------------
    // Collaborator interfaces for the MC68000 core.
    //
    // Historically the CPU reached directly into static md_main fields for
    // memory-mapped operand access (md_main.g_md_bus) and execution tracing
    // (md_main.g_form_code_trace). That hard-wired the core to the concrete
    // bus and the WinForms tracer, so individual instructions could not be
    // exercised without standing up the whole machine and UI.
    //
    // These interfaces let the core depend on behaviour instead of concrete
    // singletons: production wires in md_bus / Form_Code_Trace, while tests
    // (or a future headless frontend) can supply lightweight implementations.
    //----------------------------------------------------------------

    /// <summary>
    /// Memory-mapped bus seen by the 68000 for operand and immediate access.
    /// Implemented by <see cref="md_bus"/>.
    /// </summary>
    internal interface IM68kBus
    {
        byte read8(uint in_address);
        ushort read16(uint in_address);
        uint read32(uint in_address);
        void write8(uint in_address, byte in_data);
        void write16(uint in_address, ushort in_data);
        void write32(uint in_address, uint in_data);
    }

    /// <summary>
    /// Call-stack entry type used by the 68000 tracer. Lives here (not in
    /// Form_Code_Trace) so the interface and null implementation have no UI
    /// dependency.
    /// </summary>
    public enum M68kStackEntryType
    {
        NON,
        TOP,
        JSR,
        BSR,
        TRAP,
        HINT,
        VINT,
        EXT
    }

    /// <summary>
    /// Execution/call-stack tracing hooks invoked by the 68000 core.
    /// Implemented by <see cref="Form_Code_Trace"/> in the WinForms frontend.
    /// </summary>
    internal interface IM68kTracer
    {
        void CPU_Trace(uint in_addr);
        void CPU_Trace_push(M68kStackEntryType in_type, uint in_caller_address, uint in_start_address, uint in_ret_address, uint in_stack_address);
        void CPU_Trace_pop(uint in_pc, uint in_end_addres, uint in_stack_address);
    }

    /// <summary>
    /// No-op tracer used as the default so the core can execute without a UI
    /// attached (e.g. in unit tests). The real frontend replaces it via
    /// <see cref="md_m68k.g_tracer"/>.
    /// </summary>
    internal sealed class NullM68kTracer : IM68kTracer
    {
        public void CPU_Trace(uint in_addr) { }
        public void CPU_Trace_push(M68kStackEntryType in_type, uint in_caller_address, uint in_start_address, uint in_ret_address, uint in_stack_address) { }
        public void CPU_Trace_pop(uint in_pc, uint in_end_addres, uint in_stack_address) { }
    }
}
