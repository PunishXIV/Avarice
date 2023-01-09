using ECommons.Reflection;

namespace Avarice;

internal class VfxEditorManager
{
    internal static void DisplayVfx(bool success)
    {
        try
        {
            RemoveVfx();
            var plugin = Get();
            plugin.GetType().GetMethod("SpawnOnSelf").Invoke(plugin, new object[] { success ? "vfx/lockon/eff/m0489trg_a0c.avfx" : "vfx/lockon/eff/m0489trg_b0c.avfx" });
        }
        catch (Exception e)
        {
            PluginLog.Error(e.Message);
            PluginLog.Error(e.StackTrace);
        }
    }

    internal static void RemoveVfx()
    {
        try
        {
            var plugin = Get();
            plugin.GetType().GetMethod("RemoveSpawn").Invoke(plugin, new object[] { });
        }
        catch (Exception e)
        {
            PluginLog.Error(e.Message);
            PluginLog.Error(e.StackTrace);
        }
    }

    static IDalamudPlugin Get()
    {
        DalamudReflector.TryGetDalamudPlugin("VFXEditor", out var instance);
        return instance;
    }
}
