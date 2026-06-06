using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// Tests for the VDP memory ports (md_vdp_memory): the control-port address/
    /// code latch and VRAM/CRAM/VSRAM access through the data port, plus the
    /// CRAM-to-RGB palette decode and address auto-increment.
    ///
    /// new md_vdp() only allocates buffers (no DirectX), so the chip can be
    /// driven headless. Access uses the real two-write control-port protocol via
    /// write16/read16, exactly as the 68000 would.
    /// </summary>
    public class VdpMemoryTests
    {
        private const uint DATA = 0xC00000;   // VDP data port  (port 0x00)
        private const uint CTRL = 0xC00004;   // VDP control port (port 0x04)

        // VDP transfer codes.
        private const uint CODE_VRAM_READ = 0;
        private const uint CODE_VRAM_WRITE = 1;
        private const uint CODE_CRAM_WRITE = 3;
        private const uint CODE_VSRAM_READ = 4;
        private const uint CODE_VSRAM_WRITE = 5;
        private const uint CODE_CRAM_READ = 8;

        // Issues the two-word control-port command that latches a transfer code
        // and destination address, matching md_vdp_memory's decode.
        private static void SetAddress(md_vdp vdp, uint code, uint addr)
        {
            ushort w1 = (ushort)(((code & 0x03) << 14) | (addr & 0x3FFF));
            ushort w2 = (ushort)((((code >> 2) & 0x0F) << 4) | ((addr >> 14) & 0x03));
            vdp.write16(CTRL, w1);
            vdp.write16(CTRL, w2);
        }

        [Fact]
        public void Vram_WriteThenRead_RoundTrips()
        {
            var vdp = new md_vdp();
            SetAddress(vdp, CODE_VRAM_WRITE, 0x1234);
            vdp.write16(DATA, 0xABCD);

            SetAddress(vdp, CODE_VRAM_READ, 0x1234);
            Assert.Equal((ushort)0xABCD, vdp.read16(DATA));
        }

        [Fact]
        public void Vsram_WriteThenRead_RoundTrips()
        {
            var vdp = new md_vdp();
            SetAddress(vdp, CODE_VSRAM_WRITE, 0x0000);
            vdp.write16(DATA, 0x01FF);

            Assert.Equal((ushort)0x01FF, vdp.g_vsram[0]);
            SetAddress(vdp, CODE_VSRAM_READ, 0x0000);
            Assert.Equal((ushort)0x01FF, vdp.read16(DATA));
        }

        [Fact]
        public void Cram_WriteThenRead_RoundTrips()
        {
            var vdp = new md_vdp();
            SetAddress(vdp, CODE_CRAM_WRITE, 0x0000);
            vdp.write16(DATA, 0x000E);

            SetAddress(vdp, CODE_CRAM_READ, 0x0000);
            Assert.Equal((ushort)0x000E, vdp.read16(DATA));
        }

        [Fact]
        public void Cram_Write_DecodesToOpaqueRgb()
        {
            var vdp = new md_vdp();

            // 0x000E sets the red component to its maximum (7) and leaves G/B at 0.
            SetAddress(vdp, CODE_CRAM_WRITE, 0x0000);
            vdp.write16(DATA, 0x000E);
            Assert.Equal(0xFFFF0000u, vdp.g_color[0]);   // opaque, full red

            // 0x0E00 sets blue to maximum.
            SetAddress(vdp, CODE_CRAM_WRITE, 0x0002);     // colour index 1
            vdp.write16(DATA, 0x0E00);
            Assert.Equal(0xFF0000FFu, vdp.g_color[1]);   // opaque, full blue
        }

        [Fact]
        public void Vram_AutoIncrement_AdvancesAddress()
        {
            var vdp = new md_vdp();
            vdp.g_vdp_reg_15_autoinc = 2;                 // advance 2 bytes per access

            SetAddress(vdp, CODE_VRAM_WRITE, 0x0000);
            vdp.write16(DATA, 0x1111);                    // -> 0x0000
            vdp.write16(DATA, 0x2222);                    // -> 0x0002

            SetAddress(vdp, CODE_VRAM_READ, 0x0000);
            Assert.Equal((ushort)0x1111, vdp.read16(DATA));
            Assert.Equal((ushort)0x2222, vdp.read16(DATA));
        }
    }
}
