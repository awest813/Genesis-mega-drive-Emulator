using Xunit;

namespace GenesisEmu.Core.Tests
{
    /// <summary>
    /// Tests for the md_cartridge ROM loader and header parser.
    /// </summary>
    public class CartridgeTests
    {
        [Fact]
        public void Load_FileTooSmall_ReturnsFalse()
        {
            var cart = new MDTracer.md_cartridge();

            // Create a temp file with less than the minimum required size
            string tmpFile = Path.Combine(Path.GetTempPath(), "test_small.bin");
            try
            {
                File.WriteAllBytes(tmpFile, new byte[4]);
                bool result = cart.load(tmpFile);
                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        [Fact]
        public void Load_InvalidSystemType_ReturnsFalse()
        {
            var cart = new MDTracer.md_cartridge();

            // Build a ROM-sized buffer with an invalid system type at 0x100
            byte[] rom = new byte[0x200];
            // Leave system type as zeros (not "SEGA MEGA DRIVE" or "SEGA GENESIS")

            string tmpFile = Path.Combine(Path.GetTempPath(), "test_invalid_system.bin");
            try
            {
                File.WriteAllBytes(tmpFile, rom);
                bool result = cart.load(tmpFile);
                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        [Fact]
        public void Load_ValidMegaDriveHeader_ReturnsTrue()
        {
            var cart = new MDTracer.md_cartridge();

            byte[] rom = new byte[0x200];
            // Write "SEGA MEGA DRIVE" at offset 0x100
            byte[] systemType = System.Text.Encoding.ASCII.GetBytes("SEGA MEGA DRIVE ");
            Array.Copy(systemType, 0, rom, 0x100, systemType.Length);

            string tmpFile = Path.Combine(Path.GetTempPath(), "test_valid_md.bin");
            try
            {
                File.WriteAllBytes(tmpFile, rom);
                bool result = cart.load(tmpFile);
                Assert.True(result);
                Assert.Equal("SEGA MEGA DRIVE", cart.g_system_type);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        [Fact]
        public void Load_ValidGenesisHeader_ReturnsTrue()
        {
            var cart = new MDTracer.md_cartridge();

            byte[] rom = new byte[0x200];
            // Write "SEGA GENESIS    " at offset 0x100
            byte[] systemType = System.Text.Encoding.ASCII.GetBytes("SEGA GENESIS    ");
            Array.Copy(systemType, 0, rom, 0x100, systemType.Length);

            string tmpFile = Path.Combine(Path.GetTempPath(), "test_valid_gen.bin");
            try
            {
                File.WriteAllBytes(tmpFile, rom);
                bool result = cart.load(tmpFile);
                Assert.True(result);
                Assert.Equal("SEGA GENESIS", cart.g_system_type);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        [Fact]
        public void Load_NonexistentFile_ReturnsFalse()
        {
            var cart = new MDTracer.md_cartridge();
            bool result = cart.load(Path.Combine(Path.GetTempPath(), "nonexistent_rom_file.bin"));
            Assert.False(result);
        }

        [Fact]
        public void GetString_ReadsCorrectRange()
        {
            var cart = new MDTracer.md_cartridge();
            cart.g_file = System.Text.Encoding.ASCII.GetBytes("ABCDEFGHIJ");
            cart.g_file_size = cart.g_file.Length;

            string result = cart.get_string(2, 5);
            Assert.Equal("CDEF", result);
        }

        [Fact]
        public void Load_DeinterleavesSmdPayloadWithHeader()
        {
            var cart = new MDTracer.md_cartridge();
            const int w_block = 16 * 1024;
            byte[] w_payload = new byte[w_block];
            byte[] w_system = System.Text.Encoding.ASCII.GetBytes("SEGA MEGA DRIVE ");
            for (int k = 0; k < w_system.Length; k++)
            {
                if ((k & 1) == 0) w_payload[128 + (k / 2)] = w_system[k];
                else w_payload[8320 + (k / 2)] = w_system[k];
            }

            byte[] w_smd = new byte[512 + w_block];
            Array.Copy(w_payload, 0, w_smd, 512, w_payload.Length);

            string tmpFile = Path.Combine(Path.GetTempPath(), "test_smd.bin");
            try
            {
                File.WriteAllBytes(tmpFile, w_smd);
                Assert.True(cart.load(tmpFile));
                Assert.Equal("SEGA", System.Text.Encoding.ASCII.GetString(cart.g_file, 0x100, 4));
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        [Fact]
        public void GetUint_ReadsBigEndian()
        {
            var cart = new MDTracer.md_cartridge();
            cart.g_file = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            cart.g_file_size = 4;

            uint result = cart.get_uint(0);
            Assert.Equal(0x00010203u, result);
        }
    }
}
