using Silk.NET.SDL;

namespace MDTracer.Platform.Portable
{
    //----------------------------------------------------------------
    // Maps SDL scancodes to DirectInput keyboard indices (DIK codes).
    //
    // The core IO layer and saved key bindings use the same integer
    // key slots as the Windows DirectInput backend so portable
    // frontends can share settings files.
    //----------------------------------------------------------------
    internal static class SdlDirectInputKeyMap
    {
        private static readonly int[] g_scancodeToDik = new int[512];

        static SdlDirectInputKeyMap()
        {
            Array.Fill(g_scancodeToDik, -1);
            Map(Scancode.ScancodeEscape, 0x01);
            Map(Scancode.Scancode1, 0x02);
            Map(Scancode.Scancode2, 0x03);
            Map(Scancode.Scancode3, 0x04);
            Map(Scancode.Scancode4, 0x05);
            Map(Scancode.Scancode5, 0x06);
            Map(Scancode.Scancode6, 0x07);
            Map(Scancode.Scancode7, 0x08);
            Map(Scancode.Scancode8, 0x09);
            Map(Scancode.Scancode9, 0x0A);
            Map(Scancode.Scancode0, 0x0B);
            Map(Scancode.ScancodeMinus, 0x0C);
            Map(Scancode.ScancodeEquals, 0x0D);
            Map(Scancode.ScancodeBackspace, 0x0E);
            Map(Scancode.ScancodeTab, 0x0F);
            Map(Scancode.ScancodeQ, 0x10);
            Map(Scancode.ScancodeW, 0x11);
            Map(Scancode.ScancodeE, 0x12);
            Map(Scancode.ScancodeR, 0x13);
            Map(Scancode.ScancodeT, 0x14);
            Map(Scancode.ScancodeY, 0x15);
            Map(Scancode.ScancodeU, 0x16);
            Map(Scancode.ScancodeI, 0x17);
            Map(Scancode.ScancodeO, 0x18);
            Map(Scancode.ScancodeP, 0x19);
            Map(Scancode.ScancodeLeftbracket, 0x1A);
            Map(Scancode.ScancodeRightbracket, 0x1B);
            Map(Scancode.ScancodeReturn, 0x1C);
            Map(Scancode.ScancodeLctrl, 0x1D);
            Map(Scancode.ScancodeA, 0x1E);
            Map(Scancode.ScancodeS, 0x1F);
            Map(Scancode.ScancodeD, 0x20);
            Map(Scancode.ScancodeF, 0x21);
            Map(Scancode.ScancodeG, 0x22);
            Map(Scancode.ScancodeH, 0x23);
            Map(Scancode.ScancodeJ, 0x24);
            Map(Scancode.ScancodeK, 0x25);
            Map(Scancode.ScancodeL, 0x26);
            Map(Scancode.ScancodeSemicolon, 0x27);
            Map(Scancode.ScancodeApostrophe, 0x28);
            Map(Scancode.ScancodeGrave, 0x29);
            Map(Scancode.ScancodeLshift, 0x2A);
            Map(Scancode.ScancodeBackslash, 0x2B);
            Map(Scancode.ScancodeZ, 0x2C);
            Map(Scancode.ScancodeX, 0x2D);
            Map(Scancode.ScancodeC, 0x2E);
            Map(Scancode.ScancodeV, 0x2F);
            Map(Scancode.ScancodeB, 0x30);
            Map(Scancode.ScancodeN, 0x31);
            Map(Scancode.ScancodeM, 0x32);
            Map(Scancode.ScancodeComma, 0x33);
            Map(Scancode.ScancodePeriod, 0x34);
            Map(Scancode.ScancodeSlash, 0x35);
            Map(Scancode.ScancodeRshift, 0x36);
            Map(Scancode.ScancodePrintscreen, 0x37);
            Map(Scancode.ScancodeLalt, 0x38);
            Map(Scancode.ScancodeSpace, 0x39);
            Map(Scancode.ScancodeCapslock, 0x3A);
            Map(Scancode.ScancodeF1, 0x3B);
            Map(Scancode.ScancodeF2, 0x3C);
            Map(Scancode.ScancodeF3, 0x3D);
            Map(Scancode.ScancodeF4, 0x3E);
            Map(Scancode.ScancodeF5, 0x3F);
            Map(Scancode.ScancodeF6, 0x40);
            Map(Scancode.ScancodeF7, 0x41);
            Map(Scancode.ScancodeF8, 0x42);
            Map(Scancode.ScancodeF9, 0x43);
            Map(Scancode.ScancodeF10, 0x44);
            Map(Scancode.ScancodeNumlockclear, 0x45);
            Map(Scancode.ScancodeScrolllock, 0x46);
            Map(Scancode.ScancodeKP7, 0x47);
            Map(Scancode.ScancodeKP8, 0x48);
            Map(Scancode.ScancodeKP9, 0x49);
            Map(Scancode.ScancodeKPMinus, 0x4A);
            Map(Scancode.ScancodeKP4, 0x4B);
            Map(Scancode.ScancodeKP5, 0x4C);
            Map(Scancode.ScancodeKP6, 0x4D);
            Map(Scancode.ScancodeKPPlus, 0x4E);
            Map(Scancode.ScancodeKP1, 0x4F);
            Map(Scancode.ScancodeKP2, 0x50);
            Map(Scancode.ScancodeKP3, 0x51);
            Map(Scancode.ScancodeKP0, 0x52);
            Map(Scancode.ScancodeKPPeriod, 0x53);
            Map(Scancode.ScancodeF11, 0x57);
            Map(Scancode.ScancodeF12, 0x58);
            Map(Scancode.ScancodePause, 0xC5);
            Map(Scancode.ScancodeInsert, 0xD2);
            Map(Scancode.ScancodeHome, 0xC7);
            Map(Scancode.ScancodePageup, 0xC9);
            Map(Scancode.ScancodeDelete, 0xD3);
            Map(Scancode.ScancodeEnd, 0xCF);
            Map(Scancode.ScancodePagedown, 0xD1);
            Map(Scancode.ScancodeRight, 0xCD);
            Map(Scancode.ScancodeLeft, 0xCB);
            Map(Scancode.ScancodeDown, 0xD0);
            Map(Scancode.ScancodeUp, 0xC8);
            Map(Scancode.ScancodeRctrl, 0x9D);
            Map(Scancode.ScancodeRalt, 0xB8);
        }

        public static int ToDirectInputKey(Scancode in_scancode)
        {
            int w_index = (int)in_scancode;
            if (w_index < 0 || w_index >= g_scancodeToDik.Length) return -1;
            return g_scancodeToDik[w_index];
        }

        private static void Map(Scancode in_scancode, int in_dik)
        {
            g_scancodeToDik[(int)in_scancode] = in_dik;
        }
    }
}
