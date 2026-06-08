using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    public class TimingBaselineTests
    {
        [Fact]
        public void PerLineClockConstants_MatchCurrentBaseline()
        {
            Assert.Equal(488, md_main.VDL_LINE_RENDER_MC68_CLOCK);
            Assert.Equal(228, md_main.VDL_LINE_RENDER_Z80_CLOCK);
        }

        [Fact]
        public void FrameWaitConstants_MatchNtscAndPalTargets()
        {
            Assert.Equal(16666.666f, md_main.FRAME_WAIT_NTSC_US, 3);
            Assert.Equal(20119.87f, md_main.FRAME_WAIT_PAL_US, 2);
        }
    }
}
