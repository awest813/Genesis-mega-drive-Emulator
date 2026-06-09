namespace MDTracer.Platform.Portable
{
  //----------------------------------------------------------------
  // Vulkan VDP GPU renderer (Phase 4).
  //
  // Initializes a Vulkan 1.1 instance/device when the loader and a
  // compute-capable queue are available, then delegates frame
  // compositing to the software snapshot path until the HLSL compute
  // pipeline is ported to SPIR-V. macOS uses the same class through
  // MoltenVK (MetalVdpGpuRenderer is an alias).
  //----------------------------------------------------------------
  internal sealed class VulkanVdpGpuRenderer : IVdpGpuRenderer
  {
    private readonly CpuVdpGpuRenderer g_cpuFallback = new();
    private bool g_initialized;

    public static bool TryCreate(out IVdpGpuRenderer? out_renderer)
    {
      try
      {
        VulkanVdpGpuRenderer w_renderer = new VulkanVdpGpuRenderer();
        w_renderer.InitializeIfNeeded();
        out_renderer = w_renderer;
        return true;
      }
      catch
      {
        out_renderer = null;
        return false;
      }
    }

    public void InitializeIfNeeded()
    {
      if (g_initialized) return;
      VulkanRuntime.EnsureInitialized();
      g_initialized = true;
    }

    public void StageFrameData(
      uint[] in_rendererVram,
      uint[] in_colors,
      uint[] in_colorShadow,
      uint[] in_colorHighlight,
      VDP_LINE_SNAP[] in_lineSnap)
    {
      g_cpuFallback.StageFrameData(
          in_rendererVram,
          in_colors,
          in_colorShadow,
          in_colorHighlight,
          in_lineSnap);
    }

    public void Render(in VDP_REGISTER in_register, int in_displayYSize)
    {
      g_cpuFallback.Render(in_register, in_displayYSize);
    }

    public void DownloadScreen(uint[] in_destination)
    {
      g_cpuFallback.DownloadScreen(in_destination);
    }

    public void Dispose()
    {
      g_cpuFallback.Dispose();
      VulkanRuntime.Shutdown();
      g_initialized = false;
    }
  }
}
