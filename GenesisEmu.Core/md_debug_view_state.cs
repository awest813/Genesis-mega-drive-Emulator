namespace MDTracer
{
    //----------------------------------------------------------------
    // Debug-window visibility and trace preferences.
    //
    // These flags used to live as loose static fields on md_main. They
    // are frontend/debug state consumed by the VDP data path and trace
    // tooling, not core hardware emulation.
    //----------------------------------------------------------------
    internal sealed class DebugViewState
    {
        public bool screenA_enable;
        public bool screenB_enable;
        public bool screenW_enable;
        public bool screenS_enable;
        public bool pattern_enable;
        public bool pallete_enable;
        public bool code_enable;
        public bool io_enable;
        public bool music_enable;
        public bool registry_enable;
        public bool flow_enable;
        public bool trace_fsb;
        public bool trace_sip;

        public bool IsAnyLayerViewerEnabled =>
            screenA_enable
            || screenB_enable
            || screenW_enable
            || screenS_enable
            || pattern_enable;
    }
}
