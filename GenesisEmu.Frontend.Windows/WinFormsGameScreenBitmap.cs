using System.Drawing;
using System.Drawing.Imaging;

namespace GenesisEmu.Frontend.Windows
{
    //----------------------------------------------------------------
    // Scales the core game framebuffer (uint[] ARGB) for WinForms display.
    //----------------------------------------------------------------
    public static class WinFormsGameScreenBitmap
    {
        public static Bitmap CreateScaledBitmap(
            uint[] in_gameScreen,
            int in_sourceWidth,
            int in_sourceHeight,
            int in_destWidth,
            int in_destHeight,
            GameScreenScaleMode in_scaleMode = GameScreenScaleMode.Stretch)
        {
            var w_bitmap = new Bitmap(in_destWidth, in_destHeight, PixelFormat.Format32bppArgb);
            WriteScaledPixels(in_gameScreen, in_sourceWidth, in_sourceHeight, w_bitmap, in_scaleMode);
            return w_bitmap;
        }

        public static void WriteScaledPixels(
            uint[] in_gameScreen,
            int in_sourceWidth,
            int in_sourceHeight,
            Bitmap in_destination,
            GameScreenScaleMode in_scaleMode = GameScreenScaleMode.Stretch)
        {
            if (in_scaleMode == GameScreenScaleMode.IntegerFit)
            {
                WriteIntegerFitPixels(in_gameScreen, in_sourceWidth, in_sourceHeight, in_destination);
                return;
            }

            WriteStretchPixels(in_gameScreen, in_sourceWidth, in_sourceHeight, in_destination);
        }

        private static void WriteStretchPixels(
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

        private static void WriteIntegerFitPixels(
            uint[] in_gameScreen,
            int in_sourceWidth,
            int in_sourceHeight,
            Bitmap in_destination)
        {
            int w_destWidth = in_destination.Width;
            int w_destHeight = in_destination.Height;
            if (in_sourceWidth <= 0 || in_sourceHeight <= 0 || w_destWidth <= 0 || w_destHeight <= 0)
            {
                return;
            }

            int w_scale = Math.Min(w_destWidth / in_sourceWidth, w_destHeight / in_sourceHeight);
            if (w_scale < 1) w_scale = 1;

            int w_scaledWidth = in_sourceWidth * w_scale;
            int w_scaledHeight = in_sourceHeight * w_scale;
            int w_offsetX = (w_destWidth - w_scaledWidth) / 2;
            int w_offsetY = (w_destHeight - w_scaledHeight) / 2;

            BitmapData w_bitmapData = in_destination.LockBits(
                new Rectangle(0, 0, w_destWidth, w_destHeight),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                int w_strideBytes = w_bitmapData.Stride;
                unsafe
                {
                    byte* w_dest = (byte*)w_bitmapData.Scan0;
                    int w_stridePixels = w_strideBytes / 4;
                    for (int w_y = 0; w_y < w_destHeight; w_y++)
                    {
                        uint* w_row = (uint*)(w_dest + (w_y * w_strideBytes));
                        for (int w_x = 0; w_x < w_destWidth; w_x++)
                        {
                            w_row[w_x] = 0xFF000000;
                        }
                    }

                    for (int w_sy = 0; w_sy < in_sourceHeight; w_sy++)
                    {
                        int w_destY = w_offsetY + (w_sy * w_scale);
                        int w_sourceRow = w_sy * in_sourceWidth;
                        for (int w_sx = 0; w_sx < in_sourceWidth; w_sx++)
                        {
                            uint w_pixel = in_gameScreen[w_sourceRow + w_sx];
                            int w_destX = w_offsetX + (w_sx * w_scale);
                            for (int w_dy = 0; w_dy < w_scale; w_dy++)
                            {
                                uint* w_row = (uint*)(w_dest + ((w_destY + w_dy) * w_strideBytes));
                                for (int w_dx = 0; w_dx < w_scale; w_dx++)
                                {
                                    w_row[w_destX + w_dx] = w_pixel;
                                }
                            }
                        }
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
