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
    }
}
