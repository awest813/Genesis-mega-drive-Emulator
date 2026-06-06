using System.Drawing;
using System.Drawing.Imaging;

namespace MDTracer
{
    //----------------------------------------------------------------
    // Scales the core game framebuffer (uint[] ARGB) for WinForms display.
    //----------------------------------------------------------------
    internal static class WinFormsGameScreenBitmap
    {
        public static Bitmap CreateScaledBitmap(
            uint[] in_gameScreen,
            int in_sourceWidth,
            int in_sourceHeight,
            int in_destWidth,
            int in_destHeight)
        {
            var w_bitmap = new Bitmap(in_destWidth, in_destHeight, PixelFormat.Format32bppArgb);
            WriteScaledPixels(in_gameScreen, in_sourceWidth, in_sourceHeight, w_bitmap);
            return w_bitmap;
        }

        public static void WriteScaledPixels(
            uint[] in_gameScreen,
            int in_sourceWidth,
            int in_sourceHeight,
            Bitmap in_destination)
        {
            BitmapData w_bitmapData = in_destination.LockBits(
                new Rectangle(0, 0, in_destination.Width, in_destination.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                int w_destWidth = in_destination.Width;
                int w_destHeight = in_destination.Height;
                int w_strideBytes = w_bitmapData.Stride;
                int w_cx = (in_sourceWidth << 16) / w_destWidth;
                int w_cy = (in_sourceHeight << 16) / w_destHeight;
                unsafe
                {
                    byte* w_dest = (byte*)w_bitmapData.Scan0;
                    int w_dy = 0;
                    for (int w_y = 0; w_y < w_destHeight; w_y++)
                    {
                        uint* w_row = (uint*)(w_dest + (w_y * w_strideBytes));
                        int w_dx = 0;
                        int w_base = (w_dy >> 16) * in_sourceWidth;
                        for (int w_x = 0; w_x < w_destWidth; w_x++)
                        {
                            w_row[w_x] = in_gameScreen[w_base + (w_dx >> 16)];
                            w_dx += w_cx;
                        }
                        w_dy += w_cy;
                    }
                }
            }
            finally
            {
                in_destination.UnlockBits(w_bitmapData);
            }
        }
    }
}
