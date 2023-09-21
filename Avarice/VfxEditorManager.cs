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
            plugin.CallStatic("VfxEditor.Spawn.VfxSpawn", "OnSelf", success ? "vfx/lockon/eff/m0489trg_a0c.avfx" : "vfx/lockon/eff/m0489trg_b0c.avfx", false);
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
            plugin.CallStatic("VfxEditor.Spawn.VfxSpawn", "Remove");
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
