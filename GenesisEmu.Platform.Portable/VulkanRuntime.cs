using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace MDTracer.Platform.Portable
{
  internal static class VulkanRuntime
  {
    private static Vk? g_vk;
    private static Instance g_instance;
    private static nint g_appNamePtr;
    private static nint g_engineNamePtr;
    private static bool g_initialized;

    public static void EnsureInitialized()
    {
      if (g_initialized) return;

      g_vk = Vk.GetApi();
      g_appNamePtr = Marshal.StringToHGlobalAnsi("GenesisEmu");
      g_engineNamePtr = Marshal.StringToHGlobalAnsi("GenesisEmu.Core");

      unsafe
      {
        ApplicationInfo w_appInfo = new()
        {
          SType = StructureType.ApplicationInfo,
          PApplicationName = (byte*)g_appNamePtr,
          ApplicationVersion = Vk.MakeVersion(1, 0, 0),
          PEngineName = (byte*)g_engineNamePtr,
          EngineVersion = Vk.MakeVersion(1, 0, 0),
          ApiVersion = Vk.Version11,
        };

        InstanceCreateInfo w_createInfo = new()
        {
          SType = StructureType.InstanceCreateInfo,
          PApplicationInfo = &w_appInfo,
        };

        if (g_vk.CreateInstance(in w_createInfo, null, out g_instance) != Result.Success)
        {
          throw new InvalidOperationException("Vulkan instance creation failed.");
        }
      }

      g_initialized = true;
    }

    public static void Shutdown()
    {
      if (g_initialized == false || g_vk == null) return;

      unsafe
      {
        g_vk.DestroyInstance(g_instance, null);
      }

      g_vk.Dispose();
      g_vk = null;
      g_initialized = false;

      if (g_appNamePtr != nint.Zero)
      {
        Marshal.FreeHGlobal(g_appNamePtr);
        g_appNamePtr = nint.Zero;
      }

      if (g_engineNamePtr != nint.Zero)
      {
        Marshal.FreeHGlobal(g_engineNamePtr);
        g_engineNamePtr = nint.Zero;
      }
    }
  }
}
