using MDTracer;
using MDTracer.Platform.Portable;
using Silk.NET.SDL;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    public class PortablePlatformTests
    {
        [Fact]
        public void SdlDirectInputKeyMap_MapsDefaultPlayerKeys()
        {
            Assert.Equal(0x11, SdlDirectInputKeyMap.ToDirectInputKey(Scancode.ScancodeW));
            Assert.Equal(0x1E, SdlDirectInputKeyMap.ToDirectInputKey(Scancode.ScancodeA));
            Assert.Equal(0x1F, SdlDirectInputKeyMap.ToDirectInputKey(Scancode.ScancodeS));
            Assert.Equal(0x20, SdlDirectInputKeyMap.ToDirectInputKey(Scancode.ScancodeD));
            Assert.Equal(0x39, SdlDirectInputKeyMap.ToDirectInputKey(Scancode.ScancodeSpace));
        }

        [Fact]
        public void CpuVdpGpuRenderer_CanBeConstructed()
        {
            using CpuVdpGpuRenderer w_renderer = new CpuVdpGpuRenderer();
            w_renderer.InitializeIfNeeded();
            w_renderer.Dispose();
        }

        [Fact]
        public void PortablePlatformServices_TryRegister_WhenNativeLibrariesAvailable()
        {
            IAudioOutputBackend w_previousAudio = md_main.g_audioBackend;
            IInputDeviceBackend w_previousInput = md_io.g_inputBackend;
            IVdpGpuRenderer w_previousGpu = md_vdp.g_gpuRenderer;

            try
            {
                if (PortablePlatformServices.TryRegister() == false)
                {
                    return;
                }

                Assert.IsType<OpenAlAudioOutputBackend>(md_main.g_audioBackend);
                Assert.IsType<SdlInputDeviceBackend>(md_io.g_inputBackend);
                Assert.True(
                    md_vdp.g_gpuRenderer is CpuVdpGpuRenderer or VulkanVdpGpuRenderer,
                    $"Unexpected GPU renderer: {md_vdp.g_gpuRenderer.GetType().Name}");
            }
            finally
            {
                md_main.g_audioBackend = w_previousAudio;
                md_io.g_inputBackend = w_previousInput;
                md_vdp.g_gpuRenderer = w_previousGpu;
            }
        }
    }
}
