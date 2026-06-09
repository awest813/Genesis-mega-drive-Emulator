namespace GenesisEmu.Game.Portable
{
  internal static class Program
  {
    private static int Main(string[] in_args)
    {
      if (in_args.Length == 1 && (in_args[0] == "-h" || in_args[0] == "--help"))
      {
        PrintHelp();
        return 0;
      }

      using SdlGameApp w_app = new SdlGameApp();
      return w_app.Run(in_args);
    }

    private static void PrintHelp()
    {
      Console.WriteLine("GenesisEmu.Game.Portable — cross-platform Genesis/Mega Drive emulator");
      Console.WriteLine();
      Console.WriteLine("Usage:");
      Console.WriteLine("  GenesisEmu.Game.Portable <rom-path>");
      Console.WriteLine();
      Console.WriteLine("Controls:");
      Console.WriteLine("  Esc     Pause / resume");
      Console.WriteLine("  F1      Save state");
      Console.WriteLine("  F4      Load latest state");
      Console.WriteLine("  F5      Frame advance (while paused)");
      Console.WriteLine("  F12     Hard reset");
      Console.WriteLine("  Ctrl+I  Toggle integer-fit scaling");
      Console.WriteLine("  Ctrl+H  On-screen help");
      Console.WriteLine("  Ctrl+G  Gamepad picker");
      Console.WriteLine("  Ctrl+F4 Save state list");
      Console.WriteLine("  Ctrl+Q  Quit");
      Console.WriteLine();
      Console.WriteLine("Drag and drop a ROM file onto the window to load it.");
    }
  }
}
