using ECommons.Reflection;

namespace Avarice;

internal static class VfxEditorManager
{
    private const string SuccessVfx = "vfx/lockon/eff/m0489trg_a0c.avfx";
    private const string FailureVfx = "vfx/lockon/eff/m0489trg_b0c.avfx";

    internal static bool IsVfxEditorAvailable()
    {
        try
        {
            return DalamudReflector.TryGetDalamudPlugin("VFXEditor", out _);
        }
        catch
        {
            return false;
        }
    }

    internal static void DisplayVfx(bool success)
    {
        try
        {
            RemoveVfx();
            var plugin = Get();
            if (plugin == null)
            {
                PluginLog.Warning("VFXEditor plugin not found. Please install VFXEditor or switch to Vector visual mode.");
                return;
            }
            plugin.CallStatic("VfxEditor.Spawn.VfxSpawn", "OnSelf", [success ? SuccessVfx : FailureVfx, false]);
        }
        catch (Exception e)
        {
            PluginLog.Error($"VfxEditorManager.DisplayVfx error: {e.Message}");
            PluginLog.Error(e.StackTrace);
        }
    }

    internal static void RemoveVfx()
    {
        try
        {
            var plugin = Get();
            if (plugin == null) return;
            plugin.CallStatic("VfxEditor.Spawn.VfxSpawn", "Remove", []);
        }
        catch (Exception e)
        {
            PluginLog.Error($"VfxEditorManager.RemoveVfx error: {e.Message}");
            PluginLog.Error(e.StackTrace);
        }
    }

    private static IDalamudPlugin Get()
    {
        DalamudReflector.TryGetDalamudPlugin("VFXEditor", out var instance);
        return instance;
    }
}
