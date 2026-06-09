namespace MDTracer.Platform.Portable
{
    //----------------------------------------------------------------
    // Software VDP GPU path (Phase 4).
    //
    // Reuses the scanline CPU compositor against staged frame snapshots.
    // Used on non-Windows platforms and as a Windows fallback when
    // DirectX 12 is unavailable.
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
