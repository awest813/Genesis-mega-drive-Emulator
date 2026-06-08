using System.Drawing;
using System.Drawing.Imaging;
namespace MDTracer
{
    //----------------------------------------------------------------
    // Converts core VDP debug-layer ARGB buffers into WinForms bitmaps.
    //----------------------------------------------------------------
    internal static class WinFormsVdpDebugBitmap
    {
        public static Bitmap CreateFromArgbBuffer(uint[] in_pixels, int in_width, int in_height)
        {
            var w_bitmap = new Bitmap(in_width, in_height, PixelFormat.Format32bppArgb);
            BitmapData w_bitmapData = w_bitmap.LockBits(
                new Rectangle(0, 0, in_width, in_height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                int w_stridePixels = w_bitmapData.Stride / 4;
                unsafe
                {
                    uint* w_dest = (uint*)w_bitmapData.Scan0;
                    for (int w_y = 0; w_y < in_height; w_y++)
                    {
                        int w_srcOffset = w_y * in_width;
                        uint* w_row = w_dest + (w_y * w_stridePixels);
                        for (int w_x = 0; w_x < in_width; w_x++)
                        {
                            w_row[w_x] = in_pixels[w_srcOffset + w_x];
                        }
                    }
                }
            }
            finally
            {
                w_bitmap.UnlockBits(w_bitmapData);
            }
            return w_bitmap;
        }

        public static Bitmap CreatePatternTileView(uint[] in_pixels, int in_width, int in_height, int in_topY)
        {
            int w_maxY = in_height - 128;
            if (in_topY < 0) in_topY = 0;
            else if (w_maxY < in_topY) in_topY = w_maxY;

            var w_bitmap = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
            BitmapData w_bitmapData = w_bitmap.LockBits(
                new Rectangle(0, 0, 128, 128),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                unsafe
                {
                    uint* w_dest = (uint*)w_bitmapData.Scan0;
                    int w_stridePixels = w_bitmapData.Stride / 4;
                    for (int w_y = 0; w_y < 128; w_y++)
                    {
                        int w_srcRow = in_topY + w_y;
                        int w_srcOffset = w_srcRow * in_width;
                        uint* w_row = w_dest + (w_y * w_stridePixels);
                        for (int w_x = 0; w_x < 128; w_x++)
                        {
                            w_row[w_x] = in_pixels[w_srcOffset + w_x];
                        }
                    }
                }
            }
            finally
            {
                w_bitmap.UnlockBits(w_bitmapData);
            }
            return w_bitmap;
        }
    }
}
