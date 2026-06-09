namespace MDTracer.Platform.Portable
{
  //----------------------------------------------------------------
  // Selects the best available IVdpGpuRenderer for portable hosts.
  // Vulkan (including MoltenVK on macOS) is preferred when usable;
  // software CPU compositing is the fallback.
  //----------------------------------------------------------------
  internal static class VdpGpuRendererFactory
  {
    public static IVdpGpuRenderer Create()
    {
      if (VulkanVdpGpuRenderer.TryCreate(out IVdpGpuRenderer? w_vulkan) && w_vulkan != null)
      {
        return w_vulkan;
      }

      return new CpuVdpGpuRenderer();
    }

    public static bool TryCreateVulkan(out IVdpGpuRenderer? out_renderer)
    {
      return VulkanVdpGpuRenderer.TryCreate(out out_renderer);
    }
  }
}
