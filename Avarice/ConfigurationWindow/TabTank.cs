using Dalamud.Interface.Components;
using ECommons.Schedulers;
using Lumina.Excel.Sheets;

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
            Ui.CheckboxHelp("Tank centralisation pixel", ref P.currentProfile.EnableTankMiddle,
                "A dot under the boss to help centre the arena. This is per-profile.");
            Ui.CheckboxHelp("Duty arena centre pixel", ref P.currentProfile.EnableDutyMiddle,
                "A dot at the configured centre of the duty arena. This is per-profile.");
            if (P.currentProfile.EnableTankMiddle || P.currentProfile.EnableDutyMiddle)
            {
                ImGui.ColorEdit4("Duty centre colour", ref P.config.DutyMidPixelCol, ImGuiColorEditFlags.NoInputs);
                ImGui.SetNextItemWidth(100f);
                ImGui.SliderFloat("On-centre distance", ref P.config.DutyMidRadius, 0.5f, 5f);
                ImGui.SameLine();
                ImGuiComponents.HelpMarker("Lower means the boss must sit closer to the arena centre to use the on-centre colour.");
                ImGui.ColorEdit4("On-centre colour", ref P.config.CenteredPixelColor, ImGuiColorEditFlags.NoInputs);
                ImGui.ColorEdit4("Off-centre colour", ref P.config.UncenteredPixelColor, ImGuiColorEditFlags.NoInputs);
                ImGui.SetNextItemWidth(100f);
                ImGui.SliderFloat("Pixel size", ref P.config.CenterPixelThickness, 0.5f, 5f);
            }

            Ui.SectionLabel("Duty centre overrides");
            ImGuiEx.TextV("Add new override:");
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth();
            if(ImGui.BeginCombo("##addoverride", "Select..."))
            {
                ImGui.InputTextWithHint("##fltr", "Filter", ref Filter, 100);
                foreach(var x in Svc.Data.GetExcelSheet<TerritoryType>())
                {
                    if (P.config.DutyMiddleOverrides.ContainsKey(x.RowId)) continue;
                    var cfc = x.ContentFinderCondition.ValueNullable?.Name.ToString();
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
        }

    }
}
