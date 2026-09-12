using static Avarice.ConfigurationWindow.Ui;

namespace Avarice.ConfigurationWindow.Player;

internal static class BoxCompass
{
    internal static void Draw()
    {
        ImGui.PushID("compass");
        FeatureHeader("Tactical compass", ref P.currentProfile.CompassEnable,
            ref P.currentProfile.CompassCondition, "compass",
            "N / S / E / W letters around you.");
        if (P.currentProfile.CompassEnable)
        {
            ImGui.Indent();
            ImGui.SetNextItemWidth(220f * ImGuiHelpers.GlobalScale);
            ImGuiEx.EnumCombo("Game font", ref Prof.CompassFont);
            ImGui.SetNextItemWidth(150f);
            ImGui.SliderFloat("Font scale", ref Prof.CompassFontScale.ValidateRange(0, 100f), 0.5f, 20f);
            ImGui.SetNextItemWidth(150f);
            ImGui.SliderFloat("Distance from you", ref Prof.CompassDistance.ValidateRange(0, float.MaxValue), 0.01f, 20f);
            ImGui.ColorEdit4("North color", ref Prof.CompassColorN, ImGuiColorEditFlags.NoInputs);
            ImGui.SameLine();
            ImGui.ColorEdit4("Other colors", ref Prof.CompassColor, ImGuiColorEditFlags.NoInputs);
            ImGui.Unindent();
        }
        ImGui.PopID();
    }
}
