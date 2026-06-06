using System.Runtime.InteropServices;

namespace MDTracer
{
    //----------------------------------------------------------------
    // VDP GPU renderer seam (Phase 4).
    //
    // Compute-shader frame compositing is platform-specific. The core
    // captures per-frame snapshots and delegates upload/render/download
    // to an injectable backend (DirectX 12 on Windows).
    //----------------------------------------------------------------

    internal static class VdpGpuConstants
    {
        public const int VramDataSize = 65536 / 2;
        public const int VramBufferElements = VramDataSize * 4;
        public const int VsramDataSize = 20;
        public const int ColorMax = 64;
        public const int MaxSprite = 20;
        public const int DisplayXSize = 320;
        public const int DisplayYSize = 240;
        public const int DisplayBufSize = DisplayXSize * DisplayYSize;
    }

    [StructLayout(LayoutKind.Sequential, Size = 256)]
    internal struct VDP_REGISTER
    {
        public int display_xsize;
        public int display_ysize;
        public int scroll_xsize;
        public int scroll_xcell;
        public int scroll_mask;
        public int scrollw_xcell;
        public int vdp_reg_1_6_display;
        public int vdp_reg_2_scrolla;
        public int vdp_reg_4_scrollb;
        public int vdp_reg_3_windows;
        public uint vdp_reg_7_backcolor;
        public uint vdp_reg_12_3_shadow;
        public uint screenA_left;
        public uint screenA_right;
        public uint screenA_top;
        public uint screenA_bottom;
    }

    [StructLayout(LayoutKind.Sequential, Size = 4)]
    internal struct VDP_LINE_SNAP
    {
        public int hscrollA;
        public int hscrollB;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.VsramDataSize)]
        public int[] vscrollA;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.VsramDataSize)]
        public int[] vscrollB;
        public int window_x_st;
        public int window_x_ed;
        public int sprite_rendrere_num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public int[] sprite_left;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public int[] sprite_right;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public int[] sprite_top;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public int[] sprite_bottom;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public int[] sprite_xcell_size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public int[] sprite_ycell_size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public uint[] sprite_priority;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public uint[] sprite_palette;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public uint[] sprite_reverse;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = VdpGpuConstants.MaxSprite)]
        public uint[] sprite_char;
    }

    internal interface IVdpGpuRenderer : IDisposable
    {
        void InitializeIfNeeded();
        void StageFrameData(
            uint[] in_rendererVram,
            uint[] in_colors,
            uint[] in_colorShadow,
            uint[] in_colorHighlight,
            VDP_LINE_SNAP[] in_lineSnap);
        void Render(in VDP_REGISTER in_register, int in_displayYSize);
        void DownloadScreen(uint[] in_destination);
    }

    internal sealed class NullVdpGpuRenderer : IVdpGpuRenderer
    {
        public void InitializeIfNeeded() { }
        public void StageFrameData(
            uint[] in_rendererVram,
            uint[] in_colors,
            uint[] in_colorShadow,
            uint[] in_colorHighlight,
            VDP_LINE_SNAP[] in_lineSnap) { }
        public void Render(in VDP_REGISTER in_register, int in_displayYSize) { }
        public void DownloadScreen(uint[] in_destination) { }
        public void Dispose() { }
    }

    //----------------------------------------------------------------
    // Software GPU path: reuses the scanline CPU compositor against
    // staged frame snapshots. Default backend for headless/tests;
    // Windows production replaces this with DirectX 12.
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
