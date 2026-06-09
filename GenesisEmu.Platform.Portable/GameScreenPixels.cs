namespace MDTracer.Platform.Portable
{
  //----------------------------------------------------------------
  // Scales the core ARGB framebuffer into a destination buffer for
  // SDL (or other portable) presentation.
  //----------------------------------------------------------------
  internal static class GameScreenPixels
  {
    public static void WriteStretch(
      uint[] in_source,
      int in_sourceWidth,
      int in_sourceHeight,
      uint[] in_destination,
      int in_destWidth,
      int in_destHeight)
    {
      if (in_sourceWidth <= 0 || in_sourceHeight <= 0 || in_destWidth <= 0 || in_destHeight <= 0)
      {
        return;
      }

      int w_cx = (in_sourceWidth << 16) / in_destWidth;
      int w_cy = (in_sourceHeight << 16) / in_destHeight;
      int w_dy = 0;
      for (int w_y = 0; w_y < in_destHeight; w_y++)
      {
        int w_base = (w_dy >> 16) * in_sourceWidth;
        int w_dx = 0;
        int w_row = w_y * in_destWidth;
        for (int w_x = 0; w_x < in_destWidth; w_x++)
        {
          in_destination[w_row + w_x] = in_source[w_base + (w_dx >> 16)];
          w_dx += w_cx;
        }
        w_dy += w_cy;
      }
    }

    public static void WriteIntegerFit(
      uint[] in_source,
      int in_sourceWidth,
      int in_sourceHeight,
      uint[] in_destination,
      int in_destWidth,
      int in_destHeight)
    {
      if (in_sourceWidth <= 0 || in_sourceHeight <= 0 || in_destWidth <= 0 || in_destHeight <= 0)
      {
        return;
      }

      Array.Fill(in_destination, 0xFF000000u);

      int w_scale = Math.Min(in_destWidth / in_sourceWidth, in_destHeight / in_sourceHeight);
      if (w_scale < 1) w_scale = 1;

      int w_scaledWidth = in_sourceWidth * w_scale;
      int w_scaledHeight = in_sourceHeight * w_scale;
      int w_offsetX = (in_destWidth - w_scaledWidth) / 2;
      int w_offsetY = (in_destHeight - w_scaledHeight) / 2;

      for (int w_sy = 0; w_sy < in_sourceHeight; w_sy++)
      {
        int w_destY = w_offsetY + (w_sy * w_scale);
        int w_sourceRow = w_sy * in_sourceWidth;
        for (int w_sx = 0; w_sx < in_sourceWidth; w_sx++)
        {
          uint w_pixel = in_source[w_sourceRow + w_sx];
          int w_destX = w_offsetX + (w_sx * w_scale);
          for (int w_dy = 0; w_dy < w_scale; w_dy++)
          {
            int w_row = (w_destY + w_dy) * in_destWidth;
            for (int w_dx = 0; w_dx < w_scale; w_dx++)
            {
              in_destination[w_row + w_destX + w_dx] = w_pixel;
            }
          }
        }
      }
    }
  }
}
