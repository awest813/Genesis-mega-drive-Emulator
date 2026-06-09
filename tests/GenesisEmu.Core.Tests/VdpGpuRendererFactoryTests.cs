using MDTracer;
using MDTracer.Platform.Portable;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    public class VdpGpuRendererFactoryTests
    {
        [Fact]
        public void Create_ReturnsUsableRenderer()
        {
            IVdpGpuRenderer w_previous = md_vdp.g_gpuRenderer;
            try
            {
                using IVdpGpuRenderer w_renderer = VdpGpuRendererFactory.Create();
                w_renderer.InitializeIfNeeded();
                Assert.NotNull(w_renderer);
            }
            finally
            {
                md_vdp.g_gpuRenderer = w_previous;
            }
        }

        [Fact]
        public void TryCreateVulkan_MaySucceedWhenLoaderPresent()
        {
            IVdpGpuRenderer w_previous = md_vdp.g_gpuRenderer;
            try
            {
                if (VdpGpuRendererFactory.TryCreateVulkan(out IVdpGpuRenderer? w_renderer) == false)
                {
                    return;
                }

                using (w_renderer)
                {
                    w_renderer!.InitializeIfNeeded();
                }
            }
            finally
            {
                md_vdp.g_gpuRenderer = w_previous;
            }
        }
    }
}
