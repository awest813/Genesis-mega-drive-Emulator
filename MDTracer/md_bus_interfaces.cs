namespace MDTracer
{
    //----------------------------------------------------------------
    // Collaborator interface for the system bus memory monitor.
    //
    // md_bus previously called md_main.g_form_code directly on every read and
    // write path, making it impossible to exercise the bus without a WinForms
    // window. This interface breaks that hard wire:
    //
    //   - Production wires in Form_Code (which already has the right methods).
    //   - Tests (and any future headless frontend) use NullBusMonitor.
    //----------------------------------------------------------------

    /// <summary>
    /// Memory access monitor hook called by the bus on every read/write.
    /// Implemented by <see cref="Form_Code"/> in the WinForms frontend.
    /// </summary>
    internal interface IBusMonitor
    {
        /// <summary>True while at least one watch-point is registered.</summary>
        bool memory_monitor_active { get; }

        /// <summary>Called on every bus read or write when the monitor is active.</summary>
        void memory_monitor_check(uint in_addr, uint in_val, bool in_write_enable, int in_access_byte_count);
    }

    /// <summary>
    /// No-op monitor. Used as the default so the bus can run without a UI
    /// attached. The real frontend replaces it via <see cref="md_bus.g_monitor"/>.
    /// </summary>
    internal sealed class NullBusMonitor : IBusMonitor
    {
        public bool memory_monitor_active => false;
        public void memory_monitor_check(uint in_addr, uint in_val, bool in_write_enable, int in_access_byte_count) { }
    }
}
