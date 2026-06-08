namespace MDTracer
{
    //----------------------------------------------------------------
    // Game-view layer overlay toggles (debug compositing).
    //
    // These flags used to live on Form_Setting, which forced the VDP
    // renderer to reach into WinForms during scanline rendering.
    // They are emulation-view state and belong on the VDP subsystem.
    //----------------------------------------------------------------
    internal partial class md_vdp
    {
        public bool g_overlay_view_screenA = true;
        public bool g_overlay_view_screenB = true;
        public bool g_overlay_view_screenW = true;
        public bool g_overlay_view_screenS = true;
        public bool g_overlay_screenA_High;
        public bool g_overlay_screenB_High;
        public bool g_overlay_screenW_High;
        public bool g_overlay_screenS_High;
        public bool g_overlay_screenA_Low;
        public bool g_overlay_screenB_Low;
        public bool g_overlay_screenW_Low;
        public bool g_overlay_screenS_Low;
    }
}
