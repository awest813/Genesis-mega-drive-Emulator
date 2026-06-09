using MDTracer.Platform.Portable;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    public class GameScreenPixelsTests
    {
        [Fact]
        public void WriteStretch_CopiesCornerPixels()
        {
            uint[] w_source = { 0xFF112233, 0xFF445566, 0xFF778899, 0xFFAABBCC };
            uint[] w_dest = new uint[16];

            GameScreenPixels.WriteStretch(w_source, 2, 2, w_dest, 4, 4);

            Assert.Equal(0xFF112233u, w_dest[0]);
            Assert.Equal(0xFF445566u, w_dest[3]);
            Assert.Equal(0xFF778899u, w_dest[12]);
            Assert.Equal(0xFFAABBCCu, w_dest[15]);
        }

        [Fact]
        public void WriteIntegerFit_LetterboxesAndCentersImage()
        {
            uint[] w_source = { 0xFFFFFFFF, 0xFF000000, 0xFF000000, 0xFFFFFFFF };
            uint[] w_dest = new uint[9];

            GameScreenPixels.WriteIntegerFit(w_source, 2, 2, w_dest, 3, 3);

            Assert.Equal(0xFFFFFFFFu, w_dest[0]);
            Assert.Equal(0xFF000000u, w_dest[1]);
            Assert.Equal(0xFF000000u, w_dest[3]);
            Assert.Equal(0xFFFFFFFFu, w_dest[4]);
            Assert.Equal(0xFF000000u, w_dest[8]);
        }
    }
}
