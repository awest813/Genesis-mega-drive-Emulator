namespace MDTracer
{
    //----------------------------------------------------------------
    // Debug-layer pixel buffers (Phase 4).
    //
    // VDP layer viewers previously composited directly into System.Drawing
    // bitmaps inside the core. These flat ARGB buffers keep compositing in
    // the emulator while WinForms converts them for display.
    //----------------------------------------------------------------

    internal static class VdpDebugLayerConstants
    {
        public const int ScrollLayerWidth = 1024;
        public const int ScrollLayerHeight = 1024;
        public const int ScrollLayerPixelCount = ScrollLayerWidth * ScrollLayerHeight;
        public const int SpriteLayerWidth = 512;
        public const int SpriteLayerHeight = 512;
        public const int SpriteLayerPixelCount = SpriteLayerWidth * SpriteLayerHeight;
        public const int PatternWidth = 128;
        public const int PatternHeight = 1024;
        public const int PatternPixelCount = PatternWidth * PatternHeight;
    }
}
