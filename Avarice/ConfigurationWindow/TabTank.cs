using Dalamud.Interface.Components;
using ECommons.Schedulers;
using Lumina.Excel.GeneratedSheets;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Avarice.ConfigurationWindow
{
    internal static class TabTank
    {
        static string Filter = "";
        internal static void Draw()
        {
            var cur = ImGui.GetCursorPos();
            ImGui.PushFont(UiBuilder.IconFont);
            var text = FontAwesomeIcon.Heart.ToIconString();
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X);
            ImGuiEx.Text(EColor.RedBright, text);
            ImGui.PopFont();
            ImGuiEx.Tooltip($"Ich habe es für sie gemacht.\nEs tut mir leid, dass ich nicht der Grösste bin.\nAber du bist meine Inspiration.");
            ImGui.SetCursorPos(cur);
            ImGuiEx.TextWrapped($"Configurable arena centre pixel with additional boss alignment pixel with colour validation.");
            ImGuiGroup.BeginGroupBox();
            ImGui.Checkbox($"Enable Tank Centralisation Pixel", ref P.currentProfile.EnableTankMiddle);
            ImGuiComponents.HelpMarker($"Displays a dot directly under the boss to assist with arena centralisation. This is a per-profile option.");
            ImGui.Checkbox($"Enable Duty Arena Centre Pixel", ref P.currentProfile.EnableDutyMiddle);
            ImGuiComponents.HelpMarker($"Displays a dot at the configured centre of the duty arena. This is a per-profile option.");
            ImGui.ColorEdit4("Duty Centre Pixel Colour", ref P.config.DutyMidPixelCol, ImGuiColorEditFlags.NoInputs);
            ImGui.SetNextItemWidth(100f);
            ImGui.SliderFloat("Duty Centre Radius", ref P.config.DutyMidRadius, 0.5f, 5f);
            ImGui.ColorEdit4("Centred Pixel Colour", ref P.config.CenteredPixelColor, ImGuiColorEditFlags.NoInputs);
            ImGui.ColorEdit4("Uncentred Pixel Colour", ref P.config.UncenteredPixelColor, ImGuiColorEditFlags.NoInputs);
            ImGui.SetNextItemWidth(100f);
            ImGui.SliderFloat("Tank Pixel Size", ref P.config.CenterPixelThickness, 0.5f, 5f);
            ImGuiGroup.EndGroupBox();

            ImGuiEx.Text($"Duty Centralisation zone overrides");
            ImGuiGroup.BeginGroupBox();
            ImGuiEx.TextV("Add new override:");
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth();
            if(ImGui.BeginCombo("##addoverride", "Select..."))
            {
                ImGui.InputTextWithHint("##fltr", "Filter", ref Filter, 100);
                foreach(var x in Svc.Data.GetExcelSheet<TerritoryType>())
                {
                    if (P.config.DutyMiddleOverrides.ContainsKey(x.RowId)) continue;
                    var cfc = x.ContentFinderCondition.Value?.Name?.ToString();
                    if(cfc != null && cfc != "" && (Filter == "" || cfc.Contains(Filter, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (ImGui.Selectable($"{(P.StaticAutoDetectRadiusData.Contains(x.RowId)?"*":"")}{cfc}##{x.RowId}"))
                        {
                            P.config.DutyMiddleOverrides[x.RowId] = null;
                        }
                    }
                }
                ImGui.EndCombo();
            }
            if(P.config.DutyMiddleOverrides.Count > 0 && ImGui.BeginTable("TableOverrides", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Middle point override");
                ImGui.TableSetupColumn(" ");
                ImGui.TableHeadersRow();
                foreach (var x in P.config.DutyMiddleOverrides.ToArray())
                {
                    ImGui.PushID((int)x.Key);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGuiEx.TextV($"{Svc.Data.GetExcelSheet<TerritoryType>().GetRow(x.Key).ContentFinderCondition.Value.Name}");
                    ImGui.TableNextColumn();
                    var isAuto = x.Value == null;
                    if(ImGui.Checkbox("Auto", ref isAuto))
                    {
                        if (isAuto)
                        {
                            P.config.DutyMiddleOverrides[x.Key] = null;
                        }
                        else
                        {
                            P.config.DutyMiddleOverrides[x.Key] = Vector3.Zero;
                        }
                    }
                    if (!isAuto && x.Value != null)
                    {
                        var vector = x.Value.Value;
                        ImGui.SetNextItemWidth(200f);
                        ImGui.SameLine();
                        if (ImGui.DragFloat3("##input", ref vector, 0.1f))
                        {
                            P.config.DutyMiddleOverrides[x.Key] = vector;
                        }
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("+"))
                    {
                        P.config.DutyMiddleExtras.Add(new() { TerritoryType = x.Key });
                    }
                    for (int i = 0; i < P.config.DutyMiddleExtras.Count; i++)
                    {
                        var point = P.config.DutyMiddleExtras[i];
                        if (point.TerritoryType != x.Key) continue;
                        ImGui.PushID(i);
                        ImGui.SetNextItemWidth(200f);
                        ImGui.DragFloat3("##input", ref point.Position, 0.1f);
                        ImGui.SameLine();
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                        {
                            var rem = i;
                            new TickScheduler(() => P.config.DutyMiddleExtras.RemoveAt(rem));
                        }
                        ImGui.PopID();
                    }
                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                    {
                        P.config.DutyMiddleOverrides.Remove(x.Key);
                    }
                    ImGui.SameLine();
                    ImGuiEx.Text($" ");
                    ImGui.PopID();
                }
                ImGui.EndTable();
            }
            ImGuiGroup.EndGroupBox();
        }

    }
}
