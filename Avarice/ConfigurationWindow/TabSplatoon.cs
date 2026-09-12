using Dalamud.Interface.Components;

namespace Avarice.ConfigurationWindow
{
    internal static class TabSplatoon
    {
        internal static void Draw()
        {
            if(ImGui.Checkbox("Change colour in danger zones", ref P.config.SplatoonUnsafePixel))
            {
                WriteRequest();
            }
            ImGuiComponents.HelpMarker("Turns the player damage pixel a warning colour when Splatoon marks your position as dangerous. Needs the player damage pixel on, and Splatoon must expose danger zones.");
            ImGui.ColorEdit4("Danger colour", ref P.config.SplatoonPixelCol, ImGuiColorEditFlags.NoInputs);
            ImGuiComponents.HelpMarker("Colour used while you are standing in a Splatoon danger zone.");
            ImGuiEx.TextWrapped("Works on presets that mark themselves as dangerous. Enable the matching option in Splatoon's general settings. Older presets from before 6.5 usually do not set this.");
        }

        internal static void WriteRequest()
        {
            var array = Svc.PluginInterface.GetOrCreateData<HashSet<string>>("Splatoon.UnsafeElementRequesters", () => []);
            array.Add(Svc.PluginInterface.InternalName);
        }

        internal static bool IsUnsafe()
        {
            if(!P.config.SplatoonUnsafePixel) return false;
            if (Svc.PluginInterface.TryGetData<bool[]>("Splatoon.IsInUnsafeZone", out var data)) return data[0];
            return false;
        }
    }
}
