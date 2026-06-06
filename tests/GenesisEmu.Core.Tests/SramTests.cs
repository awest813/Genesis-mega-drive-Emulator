using System.IO;
using System.Text;
using MDTracer;
using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// Tests for battery-backed cartridge SRAM (md_sram): header detection,
    /// the 0xA130F1 enable gate, big-endian access, and .srm persistence.
    /// </summary>
    public class SramTests
    {
        // Builds a 0x200-byte ROM image with a valid Mega Drive system type and,
        // optionally, an SRAM ("RA") header describing [start, end].
        private static byte[] BuildRom(bool withSram, uint start = 0x200000, uint end = 0x2003FF)
        {
            byte[] rom = new byte[0x200];
            byte[] sys = Encoding.ASCII.GetBytes("SEGA MEGA DRIVE ");
            Array.Copy(sys, 0, rom, 0x100, sys.Length);
            if (withSram)
            {
                rom[0x1B0] = (byte)'R';
                rom[0x1B1] = (byte)'A';
                rom[0x1B2] = 0xA0;          // RAM type
                PutUint(rom, 0x1B4, start);
                PutUint(rom, 0x1B8, end);
            }
            return rom;
        }

        private static void PutUint(byte[] buf, int offset, uint value)
        {
            buf[offset + 0] = (byte)(value >> 24);
            buf[offset + 1] = (byte)(value >> 16);
            buf[offset + 2] = (byte)(value >> 8);
            buf[offset + 3] = (byte)value;
        }

        // Loads a cartridge from a temp file so the header is parsed exactly as in
        // production, then configures an md_sram against it. The temp ROM path is
        // returned so persistence tests can reason about the sibling .srm file.
        private static (md_sram sram, string romPath) Configure(byte[] rom, string name)
        {
            string romPath = Path.Combine(Path.GetTempPath(), name);
            File.WriteAllBytes(romPath, rom);

            var cart = new md_cartridge();
            Assert.True(cart.load(romPath));

            var sram = new md_sram();
            sram.configure(cart, romPath);
            return (sram, romPath);
        }

        private static void Cleanup(string romPath)
        {
            if (File.Exists(romPath)) File.Delete(romPath);
            string srm = Path.ChangeExtension(romPath, ".srm");
            if (File.Exists(srm)) File.Delete(srm);
        }

        [Fact]
        public void Configure_NoSramHeader_NotPresent()
        {
            var (sram, romPath) = Configure(BuildRom(withSram: false), "sram_none.bin");
            try
            {
                Assert.False(sram.g_present);
                Assert.False(sram.contains(0x200000));
            }
            finally { Cleanup(romPath); }
        }

        [Fact]
        public void Configure_ValidHeader_PopulatesWindow()
        {
            var (sram, romPath) = Configure(BuildRom(withSram: true, 0x200000, 0x2003FF), "sram_ok.bin");
            try
            {
                Assert.True(sram.g_present);
                Assert.True(sram.g_enabled);
                Assert.Equal(0x200000u, sram.g_start);
                Assert.Equal(0x2003FFu, sram.g_end);
                Assert.Equal(0x400, sram.g_data.Length);
                Assert.True(sram.contains(0x200000));
                Assert.True(sram.contains(0x2003FF));
                Assert.False(sram.contains(0x200400));   // just past the window
                Assert.False(sram.contains(0x1FFFFF));   // just before
            }
            finally { Cleanup(romPath); }
        }

        [Fact]
        public void Configure_OutOfRangeWindow_Rejected()
        {
            // end < start and a window outside cartridge space must be rejected.
            var (s1, p1) = Configure(BuildRom(true, 0x200400, 0x200000), "sram_bad1.bin");
            var (s2, p2) = Configure(BuildRom(true, 0x400000, 0x40FFFF), "sram_bad2.bin");
            try
            {
                Assert.False(s1.g_present);
                Assert.False(s2.g_present);
            }
            finally { Cleanup(p1); Cleanup(p2); }
        }

        [Fact]
        public void EnableGate_TogglesMapping()
        {
            var (sram, romPath) = Configure(BuildRom(true), "sram_gate.bin");
            try
            {
                sram.write8(0x200010, 0x42);
                Assert.True(sram.contains(0x200010));

                sram.set_enabled(false);                 // 0xA130F1 bit 0 = 0 -> ROM
                Assert.False(sram.contains(0x200010));

                sram.set_enabled(true);                  // mapping back in
                Assert.True(sram.contains(0x200010));
                Assert.Equal(0x42, sram.read8(0x200010)); // data survived the toggle
            }
            finally { Cleanup(romPath); }
        }

        [Fact]
        public void Access_IsBigEndianAndBounded()
        {
            var (sram, romPath) = Configure(BuildRom(true), "sram_endian.bin");
            try
            {
                sram.write16(0x200000, 0x1234);
                Assert.Equal(0x12, sram.read8(0x200000));
                Assert.Equal(0x34, sram.read8(0x200001));
                Assert.Equal((ushort)0x1234, sram.read16(0x200000));

                sram.write32(0x200004, 0xDEADBEEF);
                Assert.Equal(0xDEADBEEFu, sram.read32(0x200004));

                // Out-of-window access is ignored rather than throwing.
                sram.write8(0x300000, 0x99);
                Assert.Equal(0, sram.read8(0x300000));
            }
            finally { Cleanup(romPath); }
        }

        [Fact]
        public void Persistence_SavesAndReloads()
        {
            byte[] rom = BuildRom(true);
            var (sram, romPath) = Configure(rom, "sram_persist.bin");
            try
            {
                sram.write8(0x200000, 0xAB);
                sram.write8(0x2003FF, 0xCD);
                Assert.True(sram.g_dirty);

                sram.save();
                Assert.False(sram.g_dirty);
                Assert.True(File.Exists(Path.ChangeExtension(romPath, ".srm")));

                // A fresh instance configured against the same ROM must reload it.
                var cart = new md_cartridge();
                Assert.True(cart.load(romPath));
                var reloaded = new md_sram();
                reloaded.configure(cart, romPath);
                reloaded.load();

                Assert.Equal(0xAB, reloaded.read8(0x200000));
                Assert.Equal(0xCD, reloaded.read8(0x2003FF));
            }
            finally { Cleanup(romPath); }
        }

        [Fact]
        public void Save_NoChanges_WritesNothing()
        {
            var (sram, romPath) = Configure(BuildRom(true), "sram_clean.bin");
            try
            {
                sram.save();   // nothing dirty
                Assert.False(File.Exists(Path.ChangeExtension(romPath, ".srm")));
            }
            finally { Cleanup(romPath); }
        }

        private static byte[] WriteState(md_sram sram)
        {
            using var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                sram.write_state(bw);
            }
            return ms.ToArray();
        }

        [Fact]
        public void StateRoundTrip_RestoresBufferAndEnable()
        {
            var (src, p1) = Configure(BuildRom(true), "sram_state_src.bin");
            try
            {
                src.write8(0x200000, 0x11);
                src.write8(0x2003FF, 0x22);
                src.set_enabled(false);
                byte[] blob = WriteState(src);

                var (dst, p2) = Configure(BuildRom(true), "sram_state_dst.bin");
                try
                {
                    using (var br = new BinaryReader(new MemoryStream(blob)))
                    {
                        dst.restore_state(br);
                    }
                    Assert.Equal(0x11, dst.read8(0x200000));
                    Assert.Equal(0x22, dst.read8(0x2003FF));
                    Assert.False(dst.g_enabled);   // enable flag travelled with the state
                }
                finally { Cleanup(p2); }
            }
            finally { Cleanup(p1); }
        }

        [Fact]
        public void StateRoundTrip_NoCurrentSram_ConsumesWithoutApplying()
        {
            var (src, p1) = Configure(BuildRom(true), "sram_state_has.bin");
            try
            {
                src.write8(0x200000, 0x77);
                byte[] blob = WriteState(src);

                // Restoring into a cart that has no SRAM must not throw and must
                // leave the (empty) target untouched.
                var (dst, p2) = Configure(BuildRom(false), "sram_state_nosram.bin");
                try
                {
                    Assert.False(dst.g_present);
                    using (var br = new BinaryReader(new MemoryStream(blob)))
                    {
                        dst.restore_state(br);
                    }
                    Assert.False(dst.contains(0x200000));
                }
                finally { Cleanup(p2); }
            }
            finally { Cleanup(p1); }
        }
    }
}
