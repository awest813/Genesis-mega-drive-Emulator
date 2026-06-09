namespace MDTracer.Platform.Portable
{
  //----------------------------------------------------------------
  // Metal VDP GPU renderer entry point (Phase 4).
  //
  // Native Metal compute shaders are not wired yet. On macOS this
  // type aliases the Vulkan/MoltenVK path so frontends can register a
  // Metal-named backend while sharing one portable implementation.
  //----------------------------------------------------------------
  internal sealed class MetalVdpGpuRenderer : IVdpGpuRenderer
  {
    private readonly IVdpGpuRenderer g_inner;

    public MetalVdpGpuRenderer()
    {
      g_inner = VulkanVdpGpuRenderer.TryCreate(out IVdpGpuRenderer? w_vulkan)
        ? w_vulkan!
        : new CpuVdpGpuRenderer();
    }

    public static bool TryCreate(out IVdpGpuRenderer? out_renderer)
    {
      try
      {
        out_renderer = new MetalVdpGpuRenderer();
        return true;
      }
      catch
      {
        out_renderer = null;
        return false;
      }
    }

    public void InitializeIfNeeded() => g_inner.InitializeIfNeeded();
    public void StageFrameData(
      uint[] in_rendererVram,
      uint[] in_colors,
      uint[] in_colorShadow,
      uint[] in_colorHighlight,
      VDP_LINE_SNAP[] in_lineSnap) =>
      g_inner.StageFrameData(in_rendererVram, in_colors, in_colorShadow, in_colorHighlight, in_lineSnap);
    public void Render(in VDP_REGISTER in_register, int in_displayYSize) =>
      g_inner.Render(in_register, in_displayYSize);
    public void DownloadScreen(uint[] in_destination) => g_inner.DownloadScreen(in_destination);
    public void Dispose() => g_inner.Dispose();
  }
}
