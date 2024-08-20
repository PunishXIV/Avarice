using Dalamud.Interface.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avarice.ConfigurationWindow
{
    internal static class TabSplatoon
    {
        internal static void Draw()
        {
            if(ImGui.Checkbox("Enable Splatoon IPC", ref P.config.SplatoonUnsafePixel))
            {
                WriteRequest();
            }
            ImGuiComponents.HelpMarker("Enables changing the colour of your Player Damage Pixel based on if your position is determined to be within the bounds of a preset configured as \"dangerous\".");
            ImGui.ColorEdit4("Danger Pixel Colour", ref P.config.SplatoonPixelCol, ImGuiColorEditFlags.NoInputs);
            ImGuiComponents.HelpMarker("The colour your Player Damage Pixel will change to if you are standing in a configured danger zone. You must have the Player Damage Pixel feature enabled for this to do anything.");
            ImGuiEx.TextWrapped($"This feature will probably not function for any pre-6.5 presets as it specifically requires the preset author to apply the \"Dangerous\" attribute to the preset metadata for Avarice to read it. Additionally, you must enable this feature in Splatoon in General settings.");
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
