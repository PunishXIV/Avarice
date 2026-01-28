using Avarice.ConfigurationWindow.Player;
using Dalamud.Interface.Components;
using static Avarice.ConfigurationWindow.ConfigWindow;

namespace Avarice.ConfigurationWindow;

internal static class TabSettings
{
    // Sound effect names for the dropdown (SE1-SE16)
    private static readonly string[] SoundNames = new[]
    {
        "<se.1>",
        "<se.2>",
        "<se.3>",
        "<se.4>",
        "<se.5>",
        "<se.6>",
        "<se.7>",
        "<se.8>",
        "<se.9>",
        "<se.10>",
        "<se.11>",
        "<se.12>",
        "<se.13>",
        "<se.14>",
        "<se.15>",
        "<se.16>"
    };

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

            if (P.config.OnlyDrawIfPositional)
            {
                ImGui.Indent();
                ImGui.Checkbox("Still show distance indicator for non-positional targets", ref P.currentProfile.MaxMeleeIgnorePositionalCheck);
                ImGuiComponents.HelpMarker("When enabled, the Enemy Distance Indicator will still show even when targeting enemies without positionals (like omnidirectional bosses)");
                ImGui.Checkbox("Show positional Ring without Positional checks when non-positional buffs are present", ref P.currentProfile.ShowPositionalWithoutCheckWhenNonPositionalBuffs);
                ImGuiComponents.HelpMarker("When enabled, the Enemy Distance Indicator and positional ring without positional checks when the target has non-positional buffs");
                ImGui.Unindent();
            }

            ImGui.Separator();

            ImGui.Text("Positional Feedback:");

            if (P.config.VisualFeedbackSettings == null)
                P.config.VisualFeedbackSettings = new VisualFeedbackSettings();
            if (P.config.AudioFeedbackSettings == null)
                P.config.AudioFeedbackSettings = new AudioFeedbackSettings();

            var visualSettings = P.config.VisualFeedbackSettings;
            var audioSettings = P.config.AudioFeedbackSettings;

            ImGui.Text("On Hit:");
            ImGui.Indent();

            ImGui.Checkbox("Visual##hit", ref P.currentProfile.EnableVFXSuccess);
            if (P.currentProfile.EnableVFXSuccess)
            {
                ImGui.SameLine();
                var successColor = visualSettings.SuccessColor;
                if (ImGui.ColorEdit4("##hitColor", ref successColor, ImGuiColorEditFlags.NoInputs))
                {
                    visualSettings.SuccessColor = successColor;
                    Safe(() => Svc.PluginInterface.SavePluginConfig(P.config));
                }
            }

            ImGui.Checkbox("Audio##hit", ref P.currentProfile.EnableAudioSuccess);
            if (P.currentProfile.EnableAudioSuccess)
            {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(120f);
                var successIndex = (int)audioSettings.SuccessSoundId - 1;
                if (successIndex < 0 || successIndex > 15) successIndex = 1;
                if (ImGui.Combo("##hitSound", ref successIndex, SoundNames, 16))
                {
                    audioSettings.SuccessSoundId = (uint)(successIndex + 1);
                    Safe(() => Svc.PluginInterface.SavePluginConfig(P.config));
                }
            }

            if (P.currentProfile.EnableVFXSuccess || P.currentProfile.EnableAudioSuccess)
            {
                if (ImGui.Button("Test Hit"))
                    PositionalFeedbackManager.TestFeedback(true);
            }

            ImGui.Unindent();

            ImGui.Text("On Miss:");
            ImGui.Indent();

            ImGui.Checkbox("Visual##miss", ref P.currentProfile.EnableVFXFailure);
            if (P.currentProfile.EnableVFXFailure)
            {
                ImGui.SameLine();
                var failureColor = visualSettings.FailureColor;
                if (ImGui.ColorEdit4("##missColor", ref failureColor, ImGuiColorEditFlags.NoInputs))
                {
                    visualSettings.FailureColor = failureColor;
                    Safe(() => Svc.PluginInterface.SavePluginConfig(P.config));
                }
            }

            ImGui.Checkbox("Audio##miss", ref P.currentProfile.EnableAudioFailure);
            if (P.currentProfile.EnableAudioFailure)
            {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(120f);
                var failureIndex = (int)audioSettings.FailureSoundId - 1;
                if (failureIndex < 0 || failureIndex > 15) failureIndex = 5;
                if (ImGui.Combo("##missSound", ref failureIndex, SoundNames, 16))
                {
                    audioSettings.FailureSoundId = (uint)(failureIndex + 1);
                    Safe(() => Svc.PluginInterface.SavePluginConfig(P.config));
                }
            }

            if (P.currentProfile.EnableVFXFailure || P.currentProfile.EnableAudioFailure)
            {
                if (ImGui.Button("Test Miss"))
                    PositionalFeedbackManager.TestFeedback(false);
            }

            ImGui.Unindent();

            if (P.currentProfile.EnableVFXSuccess || P.currentProfile.EnableVFXFailure)
            {
                ImGui.SetNextItemWidth(150f);
                var iconSize = visualSettings.IconSize;
                if (ImGui.SliderFloat("Icon Size", ref iconSize, 5f, 100f))
                {
                    visualSettings.IconSize = iconSize;
                    Safe(() => Svc.PluginInterface.SavePluginConfig(P.config));
                }
            }

            ImGui.Separator();

            ImGui.Text("Chat Messages:");
            ImGui.Checkbox("Print on miss", ref P.currentProfile.EnableChatMessagesFailure);
            ImGui.SameLine();
            ImGui.Checkbox("Print on hit", ref P.currentProfile.EnableChatMessagesSuccess);
            ImGui.Checkbox("Encounter summary on combat end", ref P.currentProfile.Announce);

            ImGui.Separator();

            // Rendering Settings
            ImGui.Text("Rendering Settings:");
            ImGuiEx.Text(new Vector4(1.0f, 0.8f, 0.0f, 1.0f), "Warning: Pictomancy may have issues on Mac/Linux.");

            if (ImGui.Checkbox("Render under UI (Pictomancy)", ref P.config.UsePictomancyRenderer))
            {
                Safe(() => Svc.PluginInterface.SavePluginConfig(P.config));
            }
            ImGuiComponents.HelpMarker("When enabled, overlays will render underneath the game's native UI elements (action bars, job gauges, etc.) instead of on top.");

            if (P.config.UsePictomancyRenderer)
            {
                ImGui.Indent();

                if (ImGui.Checkbox("Clip around native UI", ref P.config.PictomancyClipNativeUI))
                {
                    Safe(() => Svc.PluginInterface.SavePluginConfig(P.config));
                }
                ImGuiComponents.HelpMarker("Automatically clips rendering around native UI elements.");

                ImGui.SetNextItemWidth(150f);
                int maxAlpha = P.config.PictomancyMaxAlpha;
                if (ImGui.SliderInt("Max Opacity", ref maxAlpha, 0, 255))
                {
                    P.config.PictomancyMaxAlpha = (byte)maxAlpha;
                    Safe(() => Svc.PluginInterface.SavePluginConfig(P.config));
                }
                ImGuiComponents.HelpMarker("Maximum opacity for all rendered overlays (0-255).");

                ImGui.Unindent();
            }
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
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.EnumCombo($"##cb2", ref P.currentProfile.FrontSegmentIndicator.DisplayCondition);
                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();
                ImGuiEx.Text("Colour:");
                ImGui.SameLine();
                ImGui.ColorEdit4($"##ca2", ref P.currentProfile.FrontSegmentIndicator.Fill, ImGuiColorEditFlags.NoInputs);
            }
        }
    };

    static InfoBox BoxMeleeRing = new()
    {
        Label = "Enemy Distance Indicator",
        ContentsAction = delegate
        {
            ImGui.SetNextItemWidth(SelectWidth);
            ImGui.Checkbox("Enemy Distance Indicator", ref P.currentProfile.EnableMaxMeleeRing);
            //if (P.currentProfile.EnableMaxMeleeRing)
            {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.EnumCombo($"##mrd", ref P.currentProfile.MaxMeleeSettingsN.DisplayCondition);
                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();
                ImGuiEx.Text("Radius 3y:");
                ImGui.SameLine();
                ImGui.Checkbox("##r3", ref P.currentProfile.Radius3);
                ImGui.SameLine();
                ImGuiEx.Text("Radius 2y:");
                ImGui.SameLine();
                ImGui.Checkbox("##r2", ref P.currentProfile.Radius2);
                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();
                ImGuiEx.Text("Lines:");
                ImGui.SameLine();
                ImGui.Checkbox("##lines", ref P.currentProfile.DrawLines);
                DrawUnfilledSettings("mr", ref P.currentProfile.MaxMeleeSettingsN, true);
            }
        }
    };

    static InfoBox BoxAnticipation = new()
    {
        Label = "Positional Anticipation Settings",
        ContentsAction = delegate
        {
            ImGui.SetNextItemWidth(SelectWidth);
            ImGui.Checkbox("Positional Anticipation Settings", ref P.currentProfile.EnableAnticipatedPie);
            //if(P.currentProfile.EnableAnticipatedPie)
            {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.EnumCombo($"##adt", ref P.currentProfile.AnticipatedPieSettings.DisplayCondition);
                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();
                ImGuiEx.Text("Color:");
                ImGui.SameLine();
                ImGui.ColorEdit4($"##ca3", ref P.currentProfile.AnticipatedPieSettings.Fill, ImGuiColorEditFlags.NoInputs);
                ImGuiEx.InvisibleButton(3);
                ImGui.SameLine();
                ImGui.Checkbox("Disable on True North", ref P.currentProfile.AnticipatedDisableTrueNorth);
            }
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
