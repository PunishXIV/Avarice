using Avarice.ConfigurationWindow.Player;
using Avarice.Data;
using Avarice.Positional;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Components;

namespace Avarice.ConfigurationWindow;

internal static class TabSettings
{
    private static readonly string[] SoundNames =
    {
        "<se.1>", "<se.2>", "<se.3>", "<se.4>", "<se.5>", "<se.6>", "<se.7>", "<se.8>",
        "<se.9>", "<se.10>", "<se.11>", "<se.12>", "<se.13>", "<se.14>", "<se.15>", "<se.16>"
    };

    internal static void DrawOverlays()
    {
        Ui.PageTitle("Overlays", "What Avarice draws on the target, you, and the arena.");
        if (!ImGui.BeginTabBar("##ovtabs"))
            return;
        if (ImGui.BeginTabItem("Target"))
        {
            DrawTarget();
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Player"))
        {
            DrawPlayer();
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("World"))
        {
            DrawWorld();
            ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }

    private static void DrawTarget()
    {
        Ui.SectionLabel("Drawing");
        Ui.CheckboxHelp("Enable drawing", ref P.currentProfile.DrawingEnabled,
            "Toggle all overlay drawing. You can also use /avarice draw.");
        ImGui.SameLine();
        ImGuiEx.Text(new Vector4(0.7f, 0.7f, 1.0f, 1.0f), "(/avarice draw)");

        var prevOnlyPositional = P.config.OnlyDrawIfPositional;
        if (Ui.CheckboxHelp("Only show for positional targets", ref P.config.OnlyDrawIfPositional,
                "When enabled, overlays only show when the target needs positionals.") &&
            prevOnlyPositional != P.config.OnlyDrawIfPositional)
        {
            Svc.PluginInterface.SavePluginConfig(P.config);
        }

        if (P.config.OnlyDrawIfPositional)
        {
            ImGui.Indent();
            Ui.CheckboxHelp("Still show distance indicator for non-positional targets",
                ref P.currentProfile.MaxMeleeIgnorePositionalCheck,
                "Keeps the enemy distance indicator on targets that do not have positionals.");
            Ui.CheckboxHelp("Show the ring when the target has a non-positional buff",
                ref P.currentProfile.ShowPositionalWithoutCheckWhenNonPositionalBuffs,
                "Shows the distance indicator and positional ring when the target has a non-positional buff.");
            ImGui.Unindent();
        }

        Ui.SectionLabel("Anticipation");
        Ui.FeatureHeader("Enable anticipation pie", ref P.currentProfile.EnableAnticipatedPie,
            ref P.currentProfile.AnticipatedPieSettings.DisplayCondition, "ant");
        if (P.currentProfile.EnableAnticipatedPie)
        {
            ImGui.Indent();
            Ui.FillColor("Rear", "antr", ref P.currentProfile.AnticipatedPieSettings.Color);
            ImGui.SameLine(0, 16);
            ImGui.SetNextItemWidth(50f);
            ImGui.DragFloat("##anthr", ref P.currentProfile.AnticipatedPieSettings.Thickness, 0.1f, 0f, 10f);
            ImGui.SameLine();
            ImGuiEx.Text("Thickness");

            Ui.FillColor("Flank", "antf", ref P.currentProfile.AnticipatedPieSettingsFlank.Color);
            ImGui.SameLine(0, 16);
            ImGui.SetNextItemWidth(50f);
            ImGui.DragFloat("##anthf", ref P.currentProfile.AnticipatedPieSettingsFlank.Thickness, 0.1f, 0f, 10f);
            ImGui.SameLine();
            ImGuiEx.Text("Thickness");

            P.currentProfile.AnticipatedPieSettings.Fill = Vector4.Zero;
            P.currentProfile.AnticipatedPieSettingsFlank.Fill = Vector4.Zero;

            Ui.CheckboxHelp("Disable on True North", ref P.currentProfile.AnticipatedDisableTrueNorth,
                "Hides the anticipation pie while True North is active.");

            var wrath = P.WrathComboWatcher.PluginInstalled;
            var rsr = RotationSolverWatcher.IsRSREnabled();
            if (wrath || rsr)
            {
                Ui.SectionLabel("Sources");
                if (wrath)
                {
                    Ui.CheckboxHelp("Use Wrath Combo for the next positional", ref P.currentProfile.UseWrathCombo,
                        rsr
                            ? "Uses Wrath Combo when it has a next positional. Otherwise Rotation Solver or your combo."
                            : "Uses Wrath Combo when it has a next positional. Otherwise your combo.");
                }
                if (rsr)
                {
                    Ui.CheckboxHelp("Use Rotation Solver for the next positional", ref P.currentProfile.UseRotationSolver,
                        wrath
                            ? "Uses Rotation Solver when Wrath is unused or has no hint. Otherwise your combo."
                            : "Uses Rotation Solver when it has a next positional. Otherwise your combo.");
                }
            }

            Ui.SectionLabel("Status");
            DrawAnticipationStatus();

            if (ImGui.CollapsingHeader("Job options"))
            {
                ImGuiEx.Text(Ui.Muted, "These apply when Avarice is reading your combo, not Wrath or Rotation Solver.");
                ImGuiEx.Text("Ninja");
                Ui.CheckboxHelp("Show rear when Trick Attack is ready", ref P.currentProfile.TrickAttack,
                    "When Trick Attack or Kunai's Bane is off cooldown, show rear.");
                Ui.CheckboxHelp("Show both positionals from Kazematoi", ref P.currentProfile.Kazematoi,
                    "With 1–3 Kazematoi charges, show rear and flank.");
                ImGuiEx.Text("Samurai");
                Ui.CheckboxHelp("Hide during Meikyo Shisui", ref P.currentProfile.Meikyo,
                    "Hides the pie while Meikyo Shisui is active.");
                ImGuiEx.Text("Reaper");
                ImGui.SameLine();
                ImGuiEx.Text("Anticipate first:");
                ImGui.SameLine();
                ImGuiComponents.HelpMarker("Which positional to show when both Gibbet and Gallows are available.");
                ImGui.SameLine();
                ImGui.RadioButton("Rear", ref P.currentProfile.Reaper, 0);
                ImGui.SameLine();
                ImGui.RadioButton("Flank", ref P.currentProfile.Reaper, 1);
            }
            ImGui.Unindent();
        }

        Ui.SectionLabel("Current slice");
        Ui.FeatureHeader("Highlight the slice you are in", ref P.currentProfile.EnableCurrentPie,
            ref P.currentProfile.CurrentPieSettings.DisplayCondition, "cur");
        if (P.currentProfile.EnableCurrentPie)
        {
            ImGui.Indent();
            Ui.FillColor("Rear", "ca1", ref P.currentProfile.CurrentPieSettings.Fill);
            ImGui.SameLine(0, 16);
            Ui.FillColor("Flank", "ca1f", ref P.currentProfile.CurrentPieSettingsFlank.Fill);
            ImGui.Unindent();
        }

        Ui.SectionLabel("Front slice");
        Ui.FeatureHeader("Show the front slice", ref P.currentProfile.EnableFrontSegment,
            ref P.currentProfile.FrontSegmentIndicator.DisplayCondition, "front");
        if (P.currentProfile.EnableFrontSegment)
        {
            ImGui.Indent();
            Ui.FillColor("Color", "ca2", ref P.currentProfile.FrontSegmentIndicator.Fill);
            ImGui.Unindent();
        }

        Ui.SectionLabel("Enemy distance");
        Ui.FeatureHeader("Enemy distance indicator", ref P.currentProfile.EnableMaxMeleeRing,
            ref P.currentProfile.MaxMeleeSettingsN.DisplayCondition, "mrd");
        if (P.currentProfile.EnableMaxMeleeRing)
        {
            ImGui.Indent();
            ImGui.Checkbox("Weaponskill range (3y)", ref P.currentProfile.Radius3);
            ImGui.SameLine();
            ImGui.Checkbox("Auto-attack range (2y)", ref P.currentProfile.Radius2);
            ImGui.SameLine();
            ImGui.Checkbox("Lines", ref P.currentProfile.DrawLines);
            Ui.ThicknessAndColor("mr", ref P.currentProfile.MaxMeleeSettingsN);
            ImGui.Unindent();
        }

        Ui.SectionLabel("Melee range");
        ImGui.SetNextItemWidth(50f);
        ImGui.DragFloat("Ability / weaponskill range", ref P.currentProfile.MeleeSkillAtk, 0.01f, 0.1f, 10f);
        ImGui.SameLine();
        ImGui.Checkbox("Include hitbox##skill", ref P.currentProfile.MeleeSkillIncludeHitbox);
        ImGui.SetNextItemWidth(50f);
        ImGui.DragFloat("Melee auto-attack range", ref P.currentProfile.MeleeAutoAtk, 0.01f, 0.1f, 10f);
        ImGui.SameLine();
        ImGui.Checkbox("Include hitbox##auto", ref P.currentProfile.MeleeAutoIncludeHitbox);
    }

    private static void DrawAnticipationStatus()
    {
        if (Svc.Targets.Target is not IBattleNpc)
        {
            Ui.StatusLine("No target", false);
            return;
        }

        var hint = Anticipation.Resolve((IBattleNpc)Svc.Targets.Target);
        var source = hint.Source switch
        {
            "wrath" => "Wrath",
            "rsr" => "Rotation Solver",
            "combo" => "Combo",
            _ => null
        };

        if (source == null || hint.Segments == AnticipatedSegments.None)
        {
            Ui.StatusLine("None", false);
            return;
        }

        var segments = hint.Segments == AnticipatedSegments.Both
            ? "Rear+Flank"
            : hint.Segments.HasFlag(AnticipatedSegments.Rear) ? "Rear" : "Flank";
        var text = $"{source} · {segments}";
        if (P.currentProfile.Debug && hint.Actions.Length > 0)
            text += $" ({string.Join(",", hint.Actions)})";
        Ui.StatusLine(text, true);
    }

    private static void DrawPlayer()
    {
        Ui.SectionLabel("Player");
        Ui.FeatureHeader("Player damage pixel", ref P.currentProfile.EnablePlayerDot,
            ref P.currentProfile.PlayerDotSettings.DisplayCondition, "dot",
            "A small pixel at your feet for the damage hitbox. Default thickness is recommended.");
        if (P.currentProfile.EnablePlayerDot)
        {
            ImGui.Indent();
            Ui.ThicknessAndColor("dot", ref P.currentProfile.PlayerDotSettings);
            ImGui.Unindent();
        }

        Ui.FeatureHeader("Player reach outline", ref P.currentProfile.EnablePlayerRing,
            ref P.currentProfile.PlayerRingSettings.DisplayCondition, "hitbox",
            "A ring around you showing auto-attack reach.");
        if (P.currentProfile.EnablePlayerRing)
        {
            ImGui.Indent();
            Ui.ThicknessAndColor("hitbox", ref P.currentProfile.PlayerRingSettings);
            ImGui.Unindent();
        }

        Ui.SectionLabel("Other people");
        Ui.FeatureHeader("Party members", ref P.currentProfile.PartyDot,
            ref P.currentProfile.PartyDotSettings.DisplayCondition, "dotp");
        if (P.currentProfile.PartyDot)
        {
            ImGui.Indent();
            Ui.ThicknessAndColor("dotp", ref P.currentProfile.PartyDotSettings);
            ImGui.Unindent();
        }

        Ui.FeatureHeader("All players", ref P.currentProfile.AllDot,
            ref P.currentProfile.AllDotSettings.DisplayCondition, "dota");
        if (P.currentProfile.AllDot)
        {
            ImGui.Indent();
            Ui.ThicknessAndColor("dota", ref P.currentProfile.AllDotSettings);
            ImGui.Unindent();
        }
    }

    private static void DrawWorld()
    {
        Ui.SectionLabel("Compass");
        BoxCompass.Draw();
        Ui.SectionLabel("Arena centre");
        TabTank.Draw();
    }

    internal static void DrawFeedback()
    {
        Ui.PageTitle("Feedback", "What happens when you hit or miss a positional.");
        P.config.VisualFeedbackSettings ??= new VisualFeedbackSettings();
        P.config.AudioFeedbackSettings ??= new AudioFeedbackSettings();

        var visualSettings = P.config.VisualFeedbackSettings;
        var audioSettings = P.config.AudioFeedbackSettings;
        var vector = visualSettings.Mode == VisualFeedbackMode.Vector;

        ImGui.AlignTextToFramePadding();
        ImGuiEx.Text("Visual mode");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(220f);
        var currentMode = (int)visualSettings.Mode;
        var modeNames = new[] { "Vector (Checkmark/X)", "Game VFX" };
        if (ImGui.Combo("##visualMode", ref currentMode, modeNames, modeNames.Length))
        {
            visualSettings.Mode = (VisualFeedbackMode)currentMode;
            Svc.PluginInterface.SavePluginConfig(P.config);
        }

        if (!vector)
            ImGuiEx.Text(Ui.Muted, "Game VFX includes built-in sounds.");

        ImGui.Spacing();
        if (ImGui.BeginTable("##fbcols", 2, ImGuiTableFlags.None))
        {
            ImGui.TableNextColumn();
            DrawFeedbackSide("On hit", true, ref P.currentProfile.EnableVFXSuccess, ref P.currentProfile.EnableAudioSuccess,
                visualSettings, audioSettings, vector);
            ImGui.TableNextColumn();
            DrawFeedbackSide("On miss", false, ref P.currentProfile.EnableVFXFailure, ref P.currentProfile.EnableAudioFailure,
                visualSettings, audioSettings, vector);
            ImGui.EndTable();
        }

        if (vector && (P.currentProfile.EnableVFXSuccess || P.currentProfile.EnableVFXFailure))
        {
            ImGui.SetNextItemWidth(150f);
            var iconSize = visualSettings.IconSize;
            if (ImGui.SliderFloat("Icon size", ref iconSize, 5f, 100f))
            {
                visualSettings.IconSize = iconSize;
                Svc.PluginInterface.SavePluginConfig(P.config);
            }
        }

        Ui.SectionLabel("Chat");
        ImGui.Checkbox("Print on miss", ref P.currentProfile.EnableChatMessagesFailure);
        ImGui.SameLine();
        ImGui.Checkbox("Print on hit", ref P.currentProfile.EnableChatMessagesSuccess);
        ImGui.Checkbox("Encounter summary on combat end", ref P.currentProfile.Announce);
    }

    private static void DrawFeedbackSide(string title, bool hit, ref bool visual, ref bool audio,
        VisualFeedbackSettings visualSettings, AudioFeedbackSettings audioSettings, bool vector)
    {
        Ui.SectionLabel(title);
        ImGui.Checkbox(hit ? "Visual##hit" : "Visual##miss", ref visual);
        if (visual && vector)
        {
            ImGui.SameLine();
            var color = hit ? visualSettings.SuccessColor : visualSettings.FailureColor;
            if (ImGui.ColorEdit4(hit ? "##hitColor" : "##missColor", ref color, ImGuiColorEditFlags.NoInputs))
            {
                if (hit) visualSettings.SuccessColor = color;
                else visualSettings.FailureColor = color;
                Svc.PluginInterface.SavePluginConfig(P.config);
            }
        }

        if (vector)
        {
            ImGui.Checkbox(hit ? "Audio##hit" : "Audio##miss", ref audio);
            if (audio)
            {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(120f);
                var index = (int)(hit ? audioSettings.SuccessSoundId : audioSettings.FailureSoundId) - 1;
                if (index < 0 || index > 15)
                    index = hit ? 1 : 5;
                if (ImGui.Combo(hit ? "##hitSound" : "##missSound", ref index, SoundNames, 16))
                {
                    if (hit) audioSettings.SuccessSoundId = (uint)(index + 1);
                    else audioSettings.FailureSoundId = (uint)(index + 1);
                    Svc.PluginInterface.SavePluginConfig(P.config);
                }
            }
        }

        if (visual || (vector && audio))
        {
            if (ImGui.Button(hit ? "Test hit" : "Test miss"))
                PositionalFeedbackManager.TestFeedback(hit);
        }
    }

    internal static void DrawAdvanced()
    {
        Ui.PageTitle("Advanced", "Renderer options and optional Splatoon integration.");
        ImGuiEx.Text(new Vector4(1.0f, 0.8f, 0.0f, 1.0f), "Warning: Pictomancy may have issues on Mac/Linux.");

        if (ImGui.Checkbox("Render under UI (Pictomancy)", ref P.config.UsePictomancyRenderer))
            Svc.PluginInterface.SavePluginConfig(P.config);
        ImGuiComponents.HelpMarker("When enabled, overlays render underneath native UI instead of on top.");

        if (P.config.UsePictomancyRenderer)
        {
            ImGui.Indent();
            if (ImGui.Checkbox("Clip around native UI", ref P.config.PictomancyClipNativeUI))
                Svc.PluginInterface.SavePluginConfig(P.config);
            ImGuiComponents.HelpMarker("Automatically clips rendering around native UI elements.");

            ImGui.SetNextItemWidth(150f);
            int maxAlpha = P.config.PictomancyMaxAlpha;
            if (ImGui.SliderInt("Max opacity", ref maxAlpha, 0, 255))
            {
                P.config.PictomancyMaxAlpha = (byte)maxAlpha;
                Svc.PluginInterface.SavePluginConfig(P.config);
            }
            ImGuiComponents.HelpMarker("Maximum opacity for all rendered overlays (0-255).");
            ImGui.Unindent();
        }

        if (Svc.PluginInterface.TryGetData<bool[]>("Splatoon.IsInUnsafeZone", out _))
        {
            Ui.SectionLabel("Splatoon");
            TabSplatoon.Draw();
        }
    }
}
