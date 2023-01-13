using Avarice.ConfigurationWindow.Player;
using Dalamud.Interface.Components;
using static Avarice.ConfigurationWindow.ConfigWindow;

namespace Avarice.ConfigurationWindow;

internal static class TabSettings
{
    internal static Dictionary<ClassDisplayCondition, string> ClassDisplayConditionNames = new()
    {
        { ClassDisplayCondition.Do_not_display, "Never" },
        { ClassDisplayCondition.Display_on_positional_jobs, "Melee DPS Only" },
        { ClassDisplayCondition.Display_on_all_jobs, "DoW/DoM/DoH/DoL" },
    };

    static InfoBox BoxGeneral = new() 
    {
        ContentsAction = delegate
        {
            /*
            ImGuiEx.Text("0x");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(50f);
            ImGuiEx.InputHex("ActionEffect1##ae1", ref P.config.ActionEffect1Opcode);
            ImGui.SameLine();
            if (ImGui.Button("Redownload opcode"))
            {
                LoadOpcode.Start();
            }
            ImGuiComponents.HelpMarker("After patching the game it may be required to change this value (usually it is downloaded from remote server automatically). " +
                "You can manually download the latest opcode using the download button to the left.");
            */
            ImGui.Checkbox("Enable positional feedback VFX", ref P.currentProfile.EnableVFX);
            ImGuiComponents.HelpMarker("Displays either a checkmark or cross above the player's head when they hit or miss a positional. This feature requires VFXEditor to be installed in order to function.");
            ImGui.Checkbox("Output positional feedback to chat", ref P.currentProfile.EnableChatMessages);
            ImGuiComponents.HelpMarker("Prints either a success or failure message in the chat when you hit or miss a positional.");
            ImGui.Checkbox("Output encounter performance summary", ref P.currentProfile.Announce);
            ImGuiComponents.HelpMarker("Prints an overall summary of your encounter and the positionals hit/missed when leaving combat.");
        },
        Label = "General settings"
    };

    static InfoBox BoxCurrentSegment = new()
    {
        Label = "Current Slice Highlight Settings",
        ContentsAction = delegate
        {
            ImGui.SetNextItemWidth(SelectWidth);
            ImGuiEx.EnumCombo("##Current Slice Highlight Settings", ref P.currentProfile.EnableCurrentPie, ClassDisplayConditionNames);
            if (P.currentProfile.EnableCurrentPie.IsEnabled())
            {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.EnumCombo($"##cb1", ref P.currentProfile.CurrentPieSettings.DisplayCondition);
                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();
                ImGuiEx.Text("Rear Colour:");
                ImGui.SameLine();
                ImGui.ColorEdit4($"##ca1", ref P.currentProfile.CurrentPieSettings.Fill, ImGuiColorEditFlags.NoInputs);
                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();
                ImGuiEx.Text("Flank Colour:");
                ImGui.SameLine();
                ImGui.ColorEdit4($"##ca1f", ref P.currentProfile.CurrentPieSettingsFlank.Fill, ImGuiColorEditFlags.NoInputs);
            }
        }
    };

    static InfoBox BoxFront = new()
    {
        Label = "Front Slice Indicator",
        ContentsAction = delegate
        {
            ImGui.SetNextItemWidth(200f);
            ImGuiEx.EnumCombo("##Front Slice Indicator", ref P.currentProfile.EnableFrontSegment, ClassDisplayConditionNames);
            if (P.currentProfile.EnableFrontSegment.IsEnabled())
            {
                ImGuiEx.Spacing(20f, true);
                ImGuiEx.Text("Colour:");
                ImGui.SameLine();
                ImGui.ColorEdit4("##Frontal segment highlight1", ref P.currentProfile.FrontSegmentIndicator.Fill, (ImGuiColorEditFlags)32);
                ImGuiEx.Spacing(20f, true);
                ImGui.Checkbox("Colour fill only under player presence?", ref P.currentProfile.FrontStand);
            }
        }
    };

    static InfoBox BoxMeleeRing = new()
    {
        Label = "Target Ring Settings",
        ContentsAction = delegate
        {
            ImGui.SetNextItemWidth(SelectWidth);
            ImGuiEx.EnumCombo("##Target/focus max melee ring", ref P.currentProfile.EnableMaxMeleeRing, ClassDisplayConditionNames);
            if (P.currentProfile.EnableMaxMeleeRing.IsEnabled())
            {
                DrawUnfilledMultiSettings("b", ref P.currentProfile.MaxMeleeSettingsN,
                    ref P.currentProfile.MaxMeleeSettingsS,
                    ref P.currentProfile.MaxMeleeSettingsE,
                    ref P.currentProfile.MaxMeleeSettingsW, 
                    ref P.currentProfile.DrawLines,
                    ref P.currentProfile.SameColor);
            }

            ImGui.Checkbox("Enable horizontal split?", ref P.currentProfile.HLine);
            ImGui.Checkbox("Enable vertical split?", ref P.currentProfile.VLine);
        }
    };

    static InfoBox BoxHitboxSettings = new()
    {
        Label = "Melee Range Options",
        ContentsAction = delegate
        {
            ImGui.SetNextItemWidth(50f);
            ImGui.DragFloat("Ability/weaponskill Range Size", ref P.currentProfile.MeleeSkillAtk, 0.01f, 0.1f, 10f);
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGui.Checkbox("Include hitbox##1", ref P.currentProfile.MeleeSkillIncludeHitbox);
            ImGui.SetNextItemWidth(50f);
            ImGui.DragFloat("Melee Auto-attack Range Size", ref P.currentProfile.MeleeAutoAtk, 0.01f, 0.1f, 10f);
            ImGuiEx.InvisibleButton(3);
            ImGui.SameLine();
            ImGui.Checkbox("Include hitbox##2", ref P.currentProfile.MeleeAutoIncludeHitbox);
        }
    };

    static InfoBox BoxPlayerDot = new()
    {
        Label = "Player Damage Pixel",
        ContentsAction = delegate
        {
            ImGuiEx.TextWrapped("Displays the player's damage hitbox, which in reality is a small pixel between your feet. " +
                "Whilst you can customize the size of this feature with the \"Thickness\" " +
                "parameter, it's recommended to leave it at the default value.");
            ImGui.SetNextItemWidth(SelectWidth);
            ImGuiEx.EnumCombo("##Player dot", ref P.currentProfile.EnablePlayerDot, ClassDisplayConditionNames);
            if (P.currentProfile.EnablePlayerDot.IsEnabled())
            {
                DrawUnfilledSettings("dot", ref P.currentProfile.PlayerDotSettings);
            }
        }
    };

    static InfoBox BoxPlayerDotOthers = new()
    {
        Label = "Entity Damage Pixels",
        ContentsAction = delegate
        {
            ImGui.Checkbox("Party Members", ref P.currentProfile.PartyDot);
            if (P.currentProfile.PartyDot)
            {
                DrawUnfilledSettings("dotp", ref P.currentProfile.PartyDotSettings);
            }
            ImGui.Checkbox("All Players", ref P.currentProfile.AllDot);
            if (P.currentProfile.AllDot)
            {
                DrawUnfilledSettings("dota", ref P.currentProfile.AllDotSettings);
            }
        }
    };

    static InfoBox BoxPlayerHitbox = new()
    {
        Label = "Player Reach Outline",
        ContentsAction = delegate
        {
            ImGuiEx.TextWrapped("Displays a ring around the player character, allowing you to see the reach of auto attacks.");
            ImGui.SetNextItemWidth(SelectWidth);
            ImGuiEx.EnumCombo("##Player hitbox outline", ref P.currentProfile.EnablePlayerRing, ClassDisplayConditionNames);
            if (P.currentProfile.EnablePlayerRing.IsEnabled())
            {
                DrawUnfilledSettings("hitbox", ref P.currentProfile.PlayerRingSettings);
            }
        }
    };

    internal static void Draw()
    {
        ImGuiEx.EzTabBar("settingsbar2", 
            ("Player", delegate
            {
                ImGuiHelpers.ScaledDummy(5f);
                BoxGeneral.DrawStretched();
                BoxPlayerDot.DrawStretched();
                BoxPlayerHitbox.DrawStretched();
                BoxCompass.Draw();
                BoxPlayerDotOthers.DrawStretched();
                ImGui.Checkbox("Debug Mode", ref P.currentProfile.Debug);
                ImGuiComponents.HelpMarker("Displays the debug menu tab, for development purposes.");
            }, null, true),
            ("Target", delegate
            {
                ImGuiHelpers.ScaledDummy(5f);
                BoxCurrentSegment.DrawStretched();
                BoxFront.DrawStretched();
                BoxMeleeRing.DrawStretched();
                BoxHitboxSettings.DrawStretched();
            }, null, true)
        );
        
    }
}
