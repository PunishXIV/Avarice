using Dalamud.Interface.Components;

namespace Avarice.ConfigurationWindow;

internal static class Ui
{
    internal static readonly Vector4 Accent = new(0.941f, 0.475f, 0.310f, 1f);
    internal static readonly Vector4 AccentBright = new(0.972f, 0.580f, 0.427f, 1f);
    internal static readonly Vector4 Muted = new(0.70f, 0.70f, 0.70f, 1f);
    internal static readonly Vector4 Section = new(0.96f, 0.62f, 0.44f, 1f);

    internal static Vector4 AccentA(float a) => new(Accent.X, Accent.Y, Accent.Z, a);

    internal static void PageTitle(string title, string subtitle)
    {
        ImGui.SetWindowFontScale(1.15f);
        ImGuiEx.Text(AccentBright, title);
        ImGui.SetWindowFontScale(1f);
        ImGuiEx.TextWrapped(Muted, subtitle);
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }

    internal static void SectionLabel(string label)
    {
        ImGui.Spacing();
        ImGui.SetWindowFontScale(0.82f);
        ImGuiEx.Text(Section, label.ToUpperInvariant());
        ImGui.SetWindowFontScale(1f);
        ImGui.Separator();
        ImGui.Spacing();
    }

    internal static bool CheckboxHelp(string label, ref bool value, string help = null)
    {
        var changed = ImGui.Checkbox(label, ref value);
        if (!string.IsNullOrEmpty(help))
        {
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(help);
        }
        return changed;
    }

    internal static void DisplayCondition(string id, ref DisplayCondition condition)
    {
        ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
        ImGuiEx.EnumCombo($"##cond{id}", ref condition);
    }

    internal static bool FeatureHeader(string label, ref bool enabled, ref DisplayCondition condition, string id, string help = null)
    {
        var changed = CheckboxHelp(label, ref enabled, help);
        if (enabled)
        {
            ImGui.SameLine();
            DisplayCondition(id, ref condition);
        }
        return changed;
    }

    internal static void ThicknessAndColor(string id, ref Brush brush, bool clearFill = true)
    {
        if (clearFill)
            brush.Fill = Vector4.Zero;

        ImGui.AlignTextToFramePadding();
        ImGuiEx.Text("Thickness");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(50f);
        ImGui.DragFloat($"##th{id}", ref brush.Thickness, 0.1f, 0f, 10f);
        ImGui.SameLine();
        ImGuiEx.Text("Color");
        ImGui.SameLine();
        ImGui.ColorEdit4($"##col{id}", ref brush.Color, ImGuiColorEditFlags.NoInputs);
    }

    internal static void FillColor(string label, string id, ref Vector4 color)
    {
        ImGui.AlignTextToFramePadding();
        ImGuiEx.Text(label);
        ImGui.SameLine();
        ImGui.ColorEdit4($"##fill{id}", ref color, ImGuiColorEditFlags.NoInputs);
    }

    internal static void StatusLine(string text, bool active)
    {
        ImGuiEx.Text(active ? AccentBright : Muted, text);
    }
}
