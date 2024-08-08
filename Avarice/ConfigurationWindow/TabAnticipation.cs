using Dalamud.Interface.Components;
using static Avarice.ConfigurationWindow.ConfigWindow;

namespace Avarice.ConfigurationWindow;

internal static unsafe class TabAnticipation
{
    static InfoBox BoxAnticipated = new()
    {
        Label = "Anticipated Segment Indicator",
        ContentsAction = delegate
        {
            ImGui.SetNextItemWidth(SelectWidth);
            ImGui.Checkbox("Anticipated Segment Indicator", ref P.currentProfile.EnableAnticipatedPie);
            //if (P.currentPrfile.EnableAnticipatedPie)
            {
                ImGui.PushID("AnticipatedPieSettings");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.EnumCombo($"##1", ref P.currentProfile.AnticipatedPieSettings.DisplayCondition);
                ImGuiEx.TextV("Rear:");
                
                //DrawUnfilledSettings("", ref P.currentProfile.AnticipatedPieSettings);


                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();
                P.currentProfile.AnticipatedPieSettings.Fill = Vector4.Zero;
                ImGuiEx.Text($"Thickness:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(50f);
                ImGui.DragFloat($"##2", ref P.currentProfile.AnticipatedPieSettings.Thickness, 0.1f, 0f, 10f);
                ImGui.SameLine();
                ImGuiEx.Text($"  Color:");
                ImGui.SameLine();
                ImGui.ColorEdit4($"##3", ref P.currentProfile.AnticipatedPieSettings.Color, ImGuiColorEditFlags.NoInputs);
                ImGui.PopID();

                ImGuiEx.TextV("Flank:");
                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();

                //DrawUnfilledSettings("AnticipatedPieSettingsFlank", ref P.currentProfile.AnticipatedPieSettingsFlank, false);
                ImGui.PushID("AnticipatedPieSettingsFlank");
                P.currentProfile.AnticipatedPieSettingsFlank.Fill = Vector4.Zero;
                ImGuiEx.Text($"Thickness:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(50f);
                ImGui.DragFloat($"##2", ref P.currentProfile.AnticipatedPieSettingsFlank.Thickness, 0.1f, 0f, 10f);
                ImGui.SameLine();
                ImGuiEx.Text($"  Color:");
                ImGui.SameLine();
                ImGui.ColorEdit4($"##3", ref P.currentProfile.AnticipatedPieSettingsFlank.Color, ImGuiColorEditFlags.NoInputs);
                ImGui.PopID();

                P.currentProfile.AnticipatedPieSettingsFlank.DisplayCondition = P.currentProfile.AnticipatedPieSettings.DisplayCondition;
                ImGui.Checkbox("Disable when under the effect of True North", ref P.currentProfile.AnticipatedDisableTrueNorth);
            }
        }
    };

    private static InfoBox BoxNinja = new()
    {
        Label = "Job Specific: Ninja",
        ContentsAction = delegate
        {
            ImGuiEx.TextWrapped(ImGuiColors.DalamudRed,"Notice: NIN positional anticipation support is experimental.");
            ImGui.SetNextItemWidth(150f);
            var v = (float)P.currentProfile.NinHutinTh / 1000f;
            if (ImGui.SliderFloat("Flank Indication Timer", ref v, 0, 30, $"{v:F1}"))
            {
                P.currentProfile.NinHutinTh = (int)(v * 1000);
            }
            ImGuiComponents.HelpMarker("Threshold (in seconds) for the positional indicator to switch between flank and rear positional based on time remaining in the Huton gauge.");
            ImGui.Checkbox("Enable Trick Attack rear indicator?", ref P.currentProfile.NinRearForTrickAttack);
            ImGuiComponents.HelpMarker("If Trick Attack is off cooldown and you are under the effects of Hidden/Suiton, light up the rear slice.");
        }
    };

    static InfoBox BoxSam = new()
    {
        Label = "Job Specific: Samurai",
        ContentsAction = delegate
        {
            ImGui.Checkbox("Disable when under the effect of Meikyo Shisui", ref P.currentProfile.NinAnticipatedDisableMeikyoShisui);
        }
    };

    static InfoBox BoxMnk = new()
    {
        Label = "Job Specific: Monk",
        ContentsAction = delegate
        {
            ImGui.SetNextItemWidth(150f);
            ImGui.SliderFloat("Demolish DoT Remaining", ref P.currentProfile.MnkDemolish, 1, 8, $"{P.currentProfile.MnkDemolish:F1}");
            ImGuiComponents.HelpMarker("Threshold (in seconds) for the Demolish DoT to highlight the rear positional indicator.");
            ImGui.Checkbox("Disable Anticipation during AoE", ref P.currentProfile.MnkAoEDisable);
            ImGuiComponents.HelpMarker("Disables highlighting positionals after a Four-Point Fury.");
        }
    };

    static InfoBox BoxDrg = new()
    {
        Label = "Job Specific: Dragoon",
        ContentsAction = delegate
        {
            ImGui.Checkbox("Disable when under the effect of Right Eye", ref P.currentProfile.DrgAnticipatedDisableRightEye);
        }
    };

    internal static void Draw()
    {
        ImGuiHelpers.ScaledDummy(5f);
        BoxAnticipated.DrawStretched();
        BoxSam.DrawStretched();
        BoxNinja.DrawStretched();
        BoxMnk.DrawStretched();
        BoxDrg.DrawStretched();
    }
}
