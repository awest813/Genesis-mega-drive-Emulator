namespace MDTracer.Platform.Windows
{
    //----------------------------------------------------------------
    // Software VDP GPU path (Phase 3/4).
    //
    // Reuses the scanline CPU compositor against staged frame snapshots.
    // Registered as a fallback when DirectX 12 is unavailable; headless
    // core/tests default to NullVdpGpuRenderer instead.
    //----------------------------------------------------------------
    internal sealed class CpuVdpGpuRenderer : IVdpGpuRenderer
    {
        public void InitializeIfNeeded() { }
        public void StageFrameData(
            uint[] in_rendererVram,
            uint[] in_colors,
            uint[] in_colorShadow,
            uint[] in_colorHighlight,
            VDP_LINE_SNAP[] in_lineSnap) { }
        public void Render(in VDP_REGISTER in_register, int in_displayYSize)
        {
            md_main.g_md_vdp?.render_snapshot_frame_cpu();
        }
        public void DownloadScreen(uint[] in_destination) { }
        public void Dispose() { }
    }
}
