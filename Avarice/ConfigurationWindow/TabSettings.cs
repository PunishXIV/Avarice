using Avarice.ConfigurationWindow.Player;
using Dalamud.Interface.Components;
using static Avarice.ConfigurationWindow.ConfigWindow;

namespace Avarice.ConfigurationWindow;

internal static class TabSettings
{
    /*internal static Dictionary<ClassDisplayCondition, string> ClassDisplayConditionNames = new()
    {
        { ClassDisplayCondition.Do_not_display, "Never" },
        { ClassDisplayCondition.Display_on_positional_jobs, "Melee DPS Only" },
        { ClassDisplayCondition.Display_on_all_jobs, "DoW/DoM/DoH/DoL" },
    };*/

    static InfoBox BoxGeneral = new()
    {
        ContentsAction = delegate
        {
            // Drawing controls section
            ImGui.Text("Drawing Controls:");

            // Profile-specific drawing toggle with styled command hint
            ImGui.Checkbox("Enable Drawing", ref P.currentProfile.DrawingEnabled);
            ImGui.SameLine();
            // Add a little spacing
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);
            // Show command in a softer color as a shortcut hint
            ImGuiEx.Text(new Vector4(0.7f, 0.7f, 1.0f, 1.0f), "(/avarice draw)");
            ImGuiComponents.HelpMarker("Toggle all overlay drawing features. Can also be toggled with /avarice draw command");

            // New option to only draw for positional targets
            bool prevOnlyPositional = P.config.OnlyDrawIfPositional;
            if (ImGui.Checkbox("Only show for positional targets", ref P.config.OnlyDrawIfPositional) && prevOnlyPositional != P.config.OnlyDrawIfPositional)
            {
                // Save change to config
                Safe(() => Svc.PluginInterface.SavePluginConfig(P.config));
            }
            ImGuiComponents.HelpMarker("When enabled, overlays will only be shown when targeting an enemy that requires positional attacks");

            ImGui.Separator();

            // Original options
            ImGui.Checkbox("Enable positional feedback VFX on failed positionals", ref P.currentProfile.EnableVFXFailure);
            ImGuiComponents.HelpMarker("Displays either a checkmark or cross above the player's head when they hit or miss a positional. This feature requires VFXEditor to be installed in order to function.");
            ImGui.Checkbox("Also enable VFX on successful positionals", ref P.currentProfile.EnableVFXSuccess);
            ImGui.Checkbox("Output positional feedback to chat on failed positional", ref P.currentProfile.EnableChatMessagesFailure);
            ImGuiComponents.HelpMarker("Prints either a success or failure message in the chat when you hit or miss a positional.");
            ImGui.Checkbox("Also print feedback to chat on successful positionals", ref P.currentProfile.EnableChatMessagesSuccess);
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
            //ImGui.SetNextItemWidth(SelectWidth);
            ImGui.Checkbox("Current Slice Highlight Settings", ref P.currentProfile.EnableCurrentPie);
            //if (P.currentProfile.EnableCurrentPie)
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
            ImGui.Checkbox("Front Slice Indicator", ref P.currentProfile.EnableFrontSegment);
            //if (P.currentProfile.EnableFrontSegment)
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
            ImGui.Checkbox("Target Ring Settings", ref P.currentProfile.EnableMaxMeleeRing);
            //if (P.currentProfile.EnableMaxMeleeRing)
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
            ImGui.Checkbox("Player Damage Pixel", ref P.currentProfile.EnablePlayerDot);
            //if (P.currentProfile.EnablePlayerDot)
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
            ImGui.Checkbox("Player Reach Outline", ref P.currentProfile.EnablePlayerRing);
            //if (P.currentProfile.EnablePlayerRing)
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
                BoxCompass.Draw();
                BoxPlayerHitbox.DrawStretched();
                BoxPlayerDotOthers.DrawStretched();
                //ImGui.Checkbox("Debug Mode", ref P.currentProfile.Debug);
                //ImGuiComponents.HelpMarker("Displays the debug menu tab, for development purposes.");
            }, null, true),
            ("Target", delegate
            {
                ImGuiHelpers.ScaledDummy(5f);
                BoxCurrentSegment.DrawStretched();
                BoxFront.DrawStretched();
                BoxMeleeRing.DrawStretched();
                BoxHitboxSettings.DrawStretched();
            }, null, true),
            ("Duty Centralisation", TabTank.Draw, null, true),
            (Svc.PluginInterface.TryGetData<bool[]>("Splatoon.IsInUnsafeZone", out _) ? "Splatoon" : null, TabSplatoon.Draw, null, true)
        );
    }
}