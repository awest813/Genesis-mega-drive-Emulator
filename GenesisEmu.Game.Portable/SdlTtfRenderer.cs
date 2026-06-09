using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace GenesisEmu.Game.Portable
{
  [StructLayout(LayoutKind.Sequential)]
  internal struct SdlColor
  {
    public byte R;
    public byte G;
    public byte B;
    public byte A;
  }

  internal sealed unsafe class SdlTtfRenderer : IDisposable
  {
    private const string SdlTtfLibLinux = "libSDL2_ttf-2.0.so.0";
    private const string SdlTtfLibMac = "libSDL2_ttf-2.0.0.dylib";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int TtfInitDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint TtfOpenFontDelegate(nint in_fontPath, int in_ptSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint TtfRenderUtf8BlendedDelegate(nint in_font, nint in_text, SdlColor in_color);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int TtfQuitDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void TtfCloseFontDelegate(nint in_font);

    private readonly Sdl g_sdl;
    private readonly nint g_library;
    private readonly nint g_font;
    private readonly TtfInitDelegate g_ttfInit;
    private readonly TtfOpenFontDelegate g_ttfOpenFont;
    private readonly TtfRenderUtf8BlendedDelegate g_ttfRenderUtf8Blended;
    private readonly TtfQuitDelegate g_ttfQuit;
    private readonly TtfCloseFontDelegate g_ttfCloseFont;
    private bool g_available;

    public SdlTtfRenderer(Sdl in_sdl)
    {
      g_sdl = in_sdl;
      try
      {
        g_library = NativeLibrary.Load(GetLibraryName());
      }
      catch
      {
        g_library = nint.Zero;
        g_ttfInit = () => -1;
        g_ttfOpenFont = (_, _) => nint.Zero;
        g_ttfRenderUtf8Blended = (_, _, _) => nint.Zero;
        g_ttfQuit = () => 0;
        g_ttfCloseFont = _ => { };
        g_font = nint.Zero;
        return;
      }

      g_ttfInit = GetExport<TtfInitDelegate>("TTF_Init");
      g_ttfOpenFont = GetExport<TtfOpenFontDelegate>("TTF_OpenFont");
      g_ttfRenderUtf8Blended = GetExport<TtfRenderUtf8BlendedDelegate>("TTF_RenderUTF8_Blended");
      g_ttfQuit = GetExport<TtfQuitDelegate>("TTF_Quit");
      g_ttfCloseFont = GetExport<TtfCloseFontDelegate>("TTF_CloseFont");

      if (g_ttfInit() != 0)
      {
        return;
      }

      string w_fontPath = ResolveFontPath();
      if (File.Exists(w_fontPath) == false)
      {
        return;
      }

      nint w_fontPathPtr = Marshal.StringToHGlobalAnsi(w_fontPath);
      try
      {
        g_font = g_ttfOpenFont(w_fontPathPtr, 16);
        g_available = g_font != nint.Zero;
      }
      finally
      {
        Marshal.FreeHGlobal(w_fontPathPtr);
      }
    }

    public bool IsAvailable => g_available;

    public void DrawText(Renderer* in_renderer, string in_text, int in_x, int in_y, byte in_r, byte in_g, byte in_b)
    {
      if (g_available == false) return;

      nint w_textPtr = Marshal.StringToHGlobalAnsi(in_text);
      try
      {
        SdlColor w_color = new() { R = in_r, G = in_g, B = in_b, A = 255 };
        nint w_surface = g_ttfRenderUtf8Blended(g_font, w_textPtr, w_color);
        if (w_surface == nint.Zero) return;

        Texture* w_texture = g_sdl.CreateTextureFromSurface(in_renderer, (Surface*)w_surface);
        g_sdl.FreeSurface((Surface*)w_surface);
        if (w_texture == null) return;

        uint w_format = 0;
        int w_access = 0;
        int w_w = 0;
        int w_h = 0;
        g_sdl.QueryTexture(w_texture, ref w_format, ref w_access, ref w_w, ref w_h);
        var w_dst = new Rectangle<int>
        {
          Origin = new Vector2D<int>(in_x, in_y),
          Size = new Vector2D<int>(w_w, w_h),
        };
        g_sdl.RenderCopy(in_renderer, w_texture, null, in w_dst);
        g_sdl.DestroyTexture(w_texture);
      }
      finally
      {
        Marshal.FreeHGlobal(w_textPtr);
      }
    }

    public void Dispose()
    {
      if (g_font != nint.Zero)
      {
        g_ttfCloseFont(g_font);
      }

      if (g_available)
      {
        g_ttfQuit();
      }

      if (g_library != nint.Zero)
      {
        NativeLibrary.Free(g_library);
      }
    }

    private T GetExport<T>(string in_name) where T : Delegate
    {
      nint w_ptr = NativeLibrary.GetExport(g_library, in_name);
      return Marshal.GetDelegateForFunctionPointer<T>(w_ptr);
    }

    private static string GetLibraryName()
    {
      if (OperatingSystem.IsMacOS()) return SdlTtfLibMac;
      return SdlTtfLibLinux;
    }

    private static string ResolveFontPath()
    {
      if (OperatingSystem.IsMacOS())
      {
        return "/System/Library/Fonts/Supplemental/Arial.ttf";
      }

      return "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";
    }
  }
}
