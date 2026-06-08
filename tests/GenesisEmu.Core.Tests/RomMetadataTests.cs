using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    public class RomMetadataTests
    {
        [Theory]
        [InlineData("E", true)]
        [InlineData("J", false)]
        [InlineData("U", false)]
        public void InferPalModeFromCountryCodes_ClassifiesKnownRegions(string in_country, bool in_expected)
        {
            Assert.Equal(in_expected, md_rom_metadata.InferPalModeFromCountryCodes(in_country));
        }

        [Theory]
        [InlineData("JUE")]
        public void InferPalModeFromCountryCodes_ReturnsNullWhenAmbiguous(string in_country)
        {
            Assert.Null(md_rom_metadata.InferPalModeFromCountryCodes(in_country));
        }

        [Theory]
        [InlineData(false, false, md_rom_metadata.NTSC_LINES_PER_FRAME)]
        [InlineData(false, true, md_rom_metadata.NTSC_LINES_PER_FRAME_240)]
        [InlineData(true, false, md_rom_metadata.PAL_LINES_PER_FRAME)]
        [InlineData(true, true, md_rom_metadata.PAL_LINES_PER_FRAME)]
        public void LinesPerFrame_MatchesVideoMode(bool in_pal, bool in_240_line, int in_expected)
        {
            Assert.Equal(in_expected, md_rom_metadata.LinesPerFrame(in_pal, in_240_line));
        }

        [Fact]
        public void ApplyTimingMode_SetsPalStatusAndLineCount()
        {
            var w_vdp = new md_vdp();
            w_vdp.ApplyTimingMode(true);

            Assert.Equal((byte)1, w_vdp.g_vdp_status_0_tvmode);
            Assert.Equal(md_rom_metadata.PAL_LINES_PER_FRAME, w_vdp.g_vertical_line_max);
        }
    }
}
