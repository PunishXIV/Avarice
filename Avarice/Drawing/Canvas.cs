using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.GameFonts;
using ECommons.GameFunctions;
using static Avarice.Drawing.DrawFunctions;
using static Avarice.Drawing.Functions;
using static Avarice.Util;

namespace Avarice.Drawing;

internal unsafe class Canvas : Window
{
    public Canvas() : base("Avarice overlay",
      ImGuiWindowFlags.NoInputs
      | ImGuiWindowFlags.NoTitleBar
      | ImGuiWindowFlags.NoScrollbar
      | ImGuiWindowFlags.NoBackground
      | ImGuiWindowFlags.AlwaysUseWindowPadding
      | ImGuiWindowFlags.NoSavedSettings
      | ImGuiWindowFlags.NoFocusOnAppearing
      , true)
    {
        IsOpen = true;
        RespectCloseHotkey = false;
    }

    public override void PreDraw()
    {
        base.PreDraw();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);
    }

    public override bool DrawConditions()
    {
        // Basic check - player exists
        if (Svc.Objects.LocalPlayer == null)
            return false;

        // Hide during cutscenes
        if (Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Svc.Condition[ConditionFlag.WatchingCutscene78])
            return false;

        // Check if drawing is enabled in profile
        if (!P.currentProfile.DrawingEnabled)
            return false;

        // Check for positional requirements if enabled
        if (P.config.OnlyDrawIfPositional)
        {
            // Make sure we have a valid target with positional requirements
            if (Svc.Targets.Target is IBattleNpc bnpc && bnpc.IsHostile())
            {
                if (bnpc.HasPositional())
                    return true;
                // KEY: Still draw if MaxMeleeIgnorePositionalCheck is enabled
                if (P.currentProfile.EnableMaxMeleeRing && P.currentProfile.MaxMeleeIgnorePositionalCheck)
                    return true;
                return false;
            }

            // No valid target with positionals
            return false;
        }

        // All conditions passed
        return true;
    }

    private bool ShouldShowPositionalFeatures()
    {
        if (!P.config.OnlyDrawIfPositional)
            return true;
        if (Svc.Targets.Target is IBattleNpc bnpc && bnpc.IsHostile())
            return bnpc.HasPositional();
        return false;
    }

    private static bool IsInHousingZone()
    {
        var territoryIntendedUse = ECommons.GameHelpers.Player.TerritoryIntendedUseEnum;
        return territoryIntendedUse == ECommons.ExcelServices.TerritoryIntendedUseEnum.Residential_Area
            || territoryIntendedUse == ECommons.ExcelServices.TerritoryIntendedUseEnum.Housing_Instances;
    }

    private void DrawMaxMeleeForTarget(IBattleNpc bnpc)
    {
        var useSimpleCircle = P.currentProfile.MaxMeleeIgnorePositionalCheck && !bnpc.HasPositional();
        var pos = IsInHousingZone() ? GroundDetection.GetAutoGroundedPosition(bnpc.Position) : bnpc.Position;

        if (useSimpleCircle)
        {
            if (P.currentProfile.Radius3)
            {
                CircleXZ(pos, bnpc.HitboxRadius + GetSkillRadius(), P.currentProfile.MaxMeleeSettingsN);
                if (P.currentProfile.Radius2)
                    CircleXZ(pos, bnpc.HitboxRadius + GetAttackRadius(), P.currentProfile.MaxMeleeSettingsN);
            }
            else if (P.currentProfile.Radius2)
                CircleXZ(pos, bnpc.HitboxRadius + GetAttackRadius(), P.currentProfile.MaxMeleeSettingsN);
        }
        else
        {
            if (P.currentProfile.Radius3)
            {
                DrawSegmentedCircle(bnpc, GetSkillRadius(), P.currentProfile.DrawLines);
                if (P.currentProfile.Radius2)
                    DrawSegmentedCircle(bnpc, GetAttackRadius(), false);
            }
            else if (P.currentProfile.Radius2)
                DrawSegmentedCircle(bnpc, GetAttackRadius(), P.currentProfile.DrawLines);
        }
    }

    public override void Draw()
    {
        // Drawing is already checked in DrawConditions() so no need for early return here
        var showPositionalFeatures = ShouldShowPositionalFeatures();

        PictomancyRenderer.BeginFrame();

        try
        {
            DrawAllOverlays(showPositionalFeatures);
        }
        finally
        {
            PictomancyRenderer.EndFrame();
        }
    }

    private void DrawAllOverlays(bool showPositionalFeatures)
    {
        DrawTankMiddle();
        if (P.currentProfile.CompassEnable && IsConditionMatching(P.currentProfile.CompassCondition))
        {
            static void DrawLetter(string l, Vector2 pos, Vector4? color = null)
            {
                var size = ImGui.CalcTextSize(l);
                ImGui.SetCursorPos(new(pos.X - (size.X / 2), pos.Y - (size.Y / 2)));
                ImGuiEx.Text(color ?? Prof.CompassColor, l);
            }

            if (Prof.CompassFont != GameFontFamilyAndSize.Undefined)
            {
                //ImGui.PushFont(Svc.PluginInterface.UiBuilder.GetGameFontHandle(new(Prof.CompassFont)).ImFont);
            }

            ImGui.SetWindowFontScale(Prof.CompassFontScale);
            {
                if (Svc.GameGui.WorldToScreen(LP.Position with { Z = LP.Position.Z - Prof.CompassDistance }, out var pos))
                {
                    DrawLetter("N", pos, Prof.CompassColorN);
                }
            }
            {
                if (Svc.GameGui.WorldToScreen(LP.Position with { Z = LP.Position.Z + Prof.CompassDistance }, out var pos))
                {
                    DrawLetter("S", pos, Prof.CompassColor);
                }
            }
            {
                if (Svc.GameGui.WorldToScreen(LP.Position with { X = LP.Position.X - Prof.CompassDistance }, out var pos))
                {
                    DrawLetter("W", pos, Prof.CompassColor);
                }
            }
            {
                if (Svc.GameGui.WorldToScreen(LP.Position with { X = LP.Position.X + Prof.CompassDistance }, out var pos))
                {
                    DrawLetter("E", pos, Prof.CompassColor);
                }
            }
            ImGui.SetWindowFontScale(1f);
            if (Prof.CompassFont != GameFontFamilyAndSize.Undefined)
            {
                //ImGui.PopFont();
            }
        }

        if (showPositionalFeatures && P.currentProfile.EnableCurrentPie && IsConditionMatching(P.currentProfile.CurrentPieSettings.DisplayCondition))
        {
            {
                if (Svc.Targets.Target is IBattleNpc bnpc && bnpc.IsHostile())
                {
                    DrawCurrentPos(bnpc);
                }
            }
            {
                if (Svc.Targets.FocusTarget is IBattleNpc bnpc && Svc.Targets.FocusTarget.Address != Svc.Targets.Target?.Address && bnpc.IsHostile())
                {
                    DrawCurrentPos(bnpc);
                }
            }
        }

        if (P.currentProfile.EnableMaxMeleeRing && IsConditionMatching(P.currentProfile.MaxMeleeSettingsN.DisplayCondition))
        {
            {
                if (Svc.Targets.Target is IBattleNpc bnpc && bnpc.IsHostile())
                {
                    DrawMaxMeleeForTarget(bnpc);
                }
            }
            {
                if (Svc.Targets.FocusTarget is IBattleNpc bnpc
                  && Svc.Targets.FocusTarget.Address != Svc.Targets.Target?.Address && bnpc.IsHostile())
                {
                    DrawMaxMeleeForTarget(bnpc);
                }
            }
        }

        if (P.currentProfile.EnablePlayerRing && IsConditionMatching(P.currentProfile.PlayerRingSettings.DisplayCondition))
        {
            CircleXZ(Svc.Objects.LocalPlayer.Position, Svc.Objects.LocalPlayer.HitboxRadius, P.currentProfile.PlayerRingSettings);
        }

        if (showPositionalFeatures && P.currentProfile.EnableFrontSegment && IsConditionMatching(P.currentProfile.FrontSegmentIndicator.DisplayCondition))
        {
            DrawFrontalPosition(Svc.Targets.Target);
            if (Svc.Targets.Target?.Address != Svc.Targets.FocusTarget?.Address)
            {
                DrawFrontalPosition(Svc.Targets.FocusTarget);
            }
        }

        if (showPositionalFeatures && P.currentProfile.EnableAnticipatedPie && IsConditionMatching(P.currentProfile.AnticipatedPieSettings.DisplayCondition)
           && (!P.currentProfile.AnticipatedDisableTrueNorth || !Svc.Objects.LocalPlayer.StatusList.Any(x => x.StatusId.EqualsAny(1250u))))
        {
            {
                if (Svc.Targets.Target is IBattleNpc bnpc && bnpc.IsHostile() && bnpc.HasPositional())
                {
                    DrawAnticipatedPos(bnpc);
                }
            }
            {
                if (Svc.Targets.FocusTarget is IBattleNpc bnpc && Svc.Targets.FocusTarget.Address != Svc.Targets.Target?.Address && bnpc.IsHostile() && bnpc.HasPositional())
                {
                    DrawAnticipatedPos(bnpc);
                }
            }
        }

        if (P.currentProfile.EnablePlayerDot && IsConditionMatching(P.currentProfile.PlayerDotSettings.DisplayCondition))
        {
            var dotColor = TabSplatoon.IsUnsafe() ? P.config.SplatoonPixelCol : P.currentProfile.PlayerDotSettings.Color;
            if (PictomancyRenderer.IsDrawing)
            {
                var worldRadius = P.currentProfile.PlayerDotSettings.Thickness * 0.03f;
                PictomancyRenderer.DrawCircleFilled(Svc.Objects.LocalPlayer.Position, worldRadius, ImGui.ColorConvertFloat4ToU32(dotColor));
            }
            else if (Svc.GameGui.WorldToScreen(Svc.Objects.LocalPlayer.Position, out var pos))
            {
                ImGui.GetWindowDrawList().AddCircleFilled(
                new Vector2(pos.X, pos.Y),
                P.currentProfile.PlayerDotSettings.Thickness,
                ImGui.ColorConvertFloat4ToU32(dotColor),
                100);
            }
        }

        if (P.currentProfile.PartyDot && IsConditionMatching(P.currentProfile.PartyDotSettings.DisplayCondition))
        {
            foreach (var x in Svc.Party)
            {
                if (x.GameObject is IPlayerCharacter pc && x.GameObject.Address != Svc.Objects.LocalPlayer.Address)
                {
                    if (PictomancyRenderer.IsDrawing)
                    {
                        var worldRadius = P.currentProfile.PartyDotSettings.Thickness * 0.03f;
                        PictomancyRenderer.DrawCircleFilled(x.GameObject.Position, worldRadius, ImGui.ColorConvertFloat4ToU32(P.currentProfile.PartyDotSettings.Color));
                    }
                    else if (Svc.GameGui.WorldToScreen(x.GameObject.Position, out var pos))
                    {
                        ImGui.GetWindowDrawList().AddCircleFilled(
                        new Vector2(pos.X, pos.Y),
                        P.currentProfile.PartyDotSettings.Thickness,
                        ImGui.ColorConvertFloat4ToU32(P.currentProfile.PartyDotSettings.Color),
                        100);
                    }
                }
            }
        }

        if (P.currentProfile.AllDot && IsConditionMatching(P.currentProfile.AllDotSettings.DisplayCondition))
        {
            foreach (var x in Svc.Objects)
            {
                if (x is IPlayerCharacter pc && x.Address != Svc.Objects.LocalPlayer.Address
                  && (!P.currentProfile.PartyDot || !Svc.Party.Any(x => x.Address == x.GameObject?.Address)))
                {
                    if (PictomancyRenderer.IsDrawing)
                    {
                        var worldRadius = P.currentProfile.AllDotSettings.Thickness * 0.03f;
                        PictomancyRenderer.DrawCircleFilled(x.Position, worldRadius, ImGui.ColorConvertFloat4ToU32(P.currentProfile.AllDotSettings.Color));
                    }
                    else if (Svc.GameGui.WorldToScreen(x.Position, out var pos))
                    {
                        ImGui.GetWindowDrawList().AddCircleFilled(
                        new Vector2(pos.X, pos.Y),
                        P.currentProfile.AllDotSettings.Thickness,
                        ImGui.ColorConvertFloat4ToU32(P.currentProfile.AllDotSettings.Color),
                        100);
                    }
                }
            }
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();
        ImGui.PopStyleVar();
    }
}