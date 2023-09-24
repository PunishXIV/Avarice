using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.GameFonts;

using ECommons;
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
        this.IsOpen = true;
        this.RespectCloseHotkey = false;
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
        return Svc.ClientState.LocalPlayer != null;
    }

    public override void Draw()
    {
        DrawTankMiddle();
        if (P.currentProfile.CompassEnable && IsConditionMatching(P.currentProfile.CompassCondition))
        {
            static void DrawLetter(string l, Vector2 pos, Vector4? color = null)
            {
								var size = ImGui.CalcTextSize(l);
								ImGui.SetCursorPos(new(pos.X - size.X / 2, pos.Y - size.Y / 2));
								ImGuiEx.Text(color ?? Prof.CompassColor, l);
						}

            if(Prof.CompassFont != GameFontFamilyAndSize.Undefined)
            {
                ImGui.PushFont(Svc.PluginInterface.UiBuilder.GetGameFontHandle(new(Prof.CompassFont)).ImFont);
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
                ImGui.PopFont();
						}
				}

        if (P.currentProfile.EnableCurrentPie && IsConditionMatching(P.currentProfile.CurrentPieSettings.DisplayCondition))
        {
            {
                if (Svc.Targets.Target is BattleNpc bnpc && bnpc.IsHostile())
                {
                    DrawCurrentPos(bnpc);
                }
            }
            {
                if (Svc.Targets.FocusTarget is BattleNpc bnpc && Svc.Targets.FocusTarget.Address != Svc.Targets.Target?.Address && bnpc.IsHostile())
                {
                    DrawCurrentPos(bnpc);
                }
            }
        }

        if (P.currentProfile.EnableMaxMeleeRing && IsConditionMatching(P.currentProfile.MaxMeleeSettingsN.DisplayCondition))
        {
            {
                if (Svc.Targets.Target is BattleNpc bnpc && bnpc.IsHostile())
                {
                    if (P.currentProfile.Radius3)
                    {
                        DrawSegmentedCircle(bnpc, GetSkillRadius(), P.currentProfile.DrawLines);
                        if (P.currentProfile.Radius2)
                        {
                            DrawSegmentedCircle(bnpc, GetAttackRadius(), false);
                        }
                    }
                    else if (P.currentProfile.Radius2)
                    {
                        DrawSegmentedCircle(bnpc, GetAttackRadius(), P.currentProfile.DrawLines);
                    }
                }
            }
            {
                if (Svc.Targets.FocusTarget is BattleNpc bnpc
                    && Svc.Targets.FocusTarget.Address != Svc.Targets.Target?.Address && bnpc.IsHostile())
                {
                    if (P.currentProfile.Radius3)
                    {
                        DrawSegmentedCircle(bnpc, GetSkillRadius(), P.currentProfile.DrawLines);
                        if (P.currentProfile.Radius2)
                        {
                            DrawSegmentedCircle(bnpc, GetAttackRadius(), false);
                        }
                    }
                    else if (P.currentProfile.Radius2)
                    {
                        DrawSegmentedCircle(bnpc, GetAttackRadius(), P.currentProfile.DrawLines);
                    }
                }
            }
        }


        /*if (P.config.EnableMaxMeleeAttackRing.IsClassDisplayConditionMatching() 
            && IsConditionMatching(P.config.MaxMeleeAttackSettings.DisplayCondition))
        {
            {
                if (Svc.Targets.Target is BattleNpc bnpc && bnpc.IsHostile())
                {
                    CircleXZ(bnpc.Position, bnpc.HitboxRadius + 2.0f, P.config.MaxMeleeAttackSettings);
                }
            }
            {
                if (Svc.Targets.FocusTarget is BattleNpc bnpc && Svc.Targets.FocusTarget.Address != Svc.Targets.Target?.Address && bnpc.IsHostile())
                {
                    CircleXZ(bnpc.Position, bnpc.HitboxRadius + 2.0f, P.config.MaxMeleeAttackSettings);
                }
            }
        }*/

        if (P.currentProfile.EnablePlayerRing && IsConditionMatching(P.currentProfile.PlayerRingSettings.DisplayCondition))
        {
            CircleXZ(Svc.ClientState.LocalPlayer.Position, Svc.ClientState.LocalPlayer.HitboxRadius, P.currentProfile.PlayerRingSettings);
        }

        if (P.currentProfile.EnableFrontSegment && IsConditionMatching(P.currentProfile.FrontSegmentIndicator.DisplayCondition))
        {
            DrawFrontalPosition(Svc.Targets.Target);
            if (Svc.Targets.Target?.Address != Svc.Targets.FocusTarget?.Address) DrawFrontalPosition(Svc.Targets.FocusTarget);
        }

        if (P.currentProfile.EnableAnticipatedPie && IsConditionMatching(P.currentProfile.AnticipatedPieSettings.DisplayCondition)
             && (!P.currentProfile.AnticipatedDisableTrueNorth || !Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId.EqualsAny(1250u)))
             && (!P.currentProfile.DrgAnticipatedDisableRightEye || !Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId.EqualsAny(1910u)))
             && (!P.currentProfile.NinAnticipatedDisableMeikyoShisui || !Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId.EqualsAny(1233u))))
        {
            {
                if (Svc.Targets.Target is BattleNpc bnpc && bnpc.IsHostile() && bnpc.HasPositional())
                {
                    DrawAnticipatedPos(bnpc);
                }
            }
            {
                if (Svc.Targets.FocusTarget is BattleNpc bnpc && Svc.Targets.FocusTarget.Address != Svc.Targets.Target?.Address && bnpc.IsHostile() && bnpc.HasPositional())
                {
                    DrawAnticipatedPos(bnpc);
                }
            }
        }

        if (P.currentProfile.EnablePlayerDot && IsConditionMatching(P.currentProfile.PlayerDotSettings.DisplayCondition))
        {
            if (Svc.GameGui.WorldToScreen(Svc.ClientState.LocalPlayer.Position, out Vector2 pos))
                ImGui.GetWindowDrawList().AddCircleFilled(
                new Vector2(pos.X, pos.Y),
                P.currentProfile.PlayerDotSettings.Thickness,
                ImGui.ColorConvertFloat4ToU32(P.currentProfile.PlayerDotSettings.Color),
                100);
        }

        if (P.currentProfile.PartyDot && IsConditionMatching(P.currentProfile.PartyDotSettings.DisplayCondition))
        {
            foreach (var x in Svc.Party)
            {
                if (x.GameObject is PlayerCharacter pc && x.GameObject.Address != Svc.ClientState.LocalPlayer.Address)
                {
                    if (Svc.GameGui.WorldToScreen(x.GameObject.Position, out Vector2 pos))
                        ImGui.GetWindowDrawList().AddCircleFilled(
                        new Vector2(pos.X, pos.Y),
                        P.currentProfile.PartyDotSettings.Thickness,
                        ImGui.ColorConvertFloat4ToU32(P.currentProfile.PartyDotSettings.Color),
                        100);
                }
            }
        }

        if (P.currentProfile.AllDot && IsConditionMatching(P.currentProfile.AllDotSettings.DisplayCondition))
        {
            foreach (var x in Svc.Objects)
            {
                if (x is PlayerCharacter pc && x.Address != Svc.ClientState.LocalPlayer.Address
                    && (!P.currentProfile.PartyDot || !Svc.Party.Any(x => x.Address == x.GameObject?.Address)))
                {
                    if (Svc.GameGui.WorldToScreen(x.Position, out Vector2 pos))
                        ImGui.GetWindowDrawList().AddCircleFilled(
                        new Vector2(pos.X, pos.Y),
                        P.currentProfile.AllDotSettings.Thickness,
                        ImGui.ColorConvertFloat4ToU32(P.currentProfile.AllDotSettings.Color),
                        100);
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
