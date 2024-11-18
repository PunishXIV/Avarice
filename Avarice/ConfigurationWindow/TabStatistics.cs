using Lumina.Excel.Sheets;

namespace Avarice.ConfigurationWindow;

internal static class TabStatistics
{
    static InfoBox StatsGlobal = new()
    {
        Label = "Total stats",
        ContentsAction = delegate
        {
            ImGui.BeginTable("##table", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit, new Vector2(ImGui.GetContentRegionAvail().X - 20, 0));
            ImGui.TableSetupColumn(" Job ", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(" Hits ");
            ImGui.TableSetupColumn(" Total ");
            ImGui.TableSetupColumn("Success %  ");
            ImGui.TableHeadersRow();
            Stats total = new();
            foreach(var x in P.currentProfile.Stats)
            {
                DrawStatsRow(x.Key, x.Value);
                total.Hits += x.Value.Hits;
                total.Missed += x.Value.Missed;
            }
            if(total.Hits > 0 || total.Missed > 0) DrawStatsRow(0, total, "Total: ");
            ImGui.EndTable();
            if(ImGui.SmallButton("Clear data (hold shift+ctrl)"))
            {
                if(ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl)
                {
                    P.currentProfile.Stats = new();
                }
            }
        }
    };

    static void DrawStatsRow(uint job, Stats x, string colName = null)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGuiEx.Text(colName ?? Svc.Data.GetExcelSheet<ClassJob>().GetRowOrDefault(job)?.NameEnglish.ToString());
        ImGui.TableNextColumn();
        ImGuiEx.Text($"{x.Hits}");
        ImGui.TableNextColumn();
        var total = x.Hits + x.Missed;
        ImGuiEx.Text($"{total}");
        ImGui.TableNextColumn();
        var success = (int)(100f * (float)x.Hits / (float)total);
        ImGuiEx.Text(ImGuiEx.GetParsedColor(success), $"{success}%");
    }

    static InfoBox StatsCurrent = new()
    {
        Label = "Current encounter",
        ContentsAction = delegate
        {
            var x = P.currentProfile.CurrentEncounterStats;
            var total = x.Hits + x.Missed;
            if (total == 0)
            {
                ImGuiEx.Text("No data");
            }
            else
            {
                var success = (int)(100f * (float)x.Hits / (float)total);
                ImGuiEx.Text($"Hits: {x.Hits} out of {total} - ");
                ImGui.SameLine(0, 0);
                ImGuiEx.Text(ImGuiEx.GetParsedColor(success), $"{success}%");
                if (ImGui.SmallButton("Clear data"))
                {
                    P.currentProfile.CurrentEncounterStats = new();
                }
                ImGui.SameLine();
                if (P.currentProfile.CurrentEncounterStats.Finished)
                {
                    StatsCurrent.Label = "Recent encounter";
                    ImGuiEx.Text(ImGuiColors.DalamudRed, "Stats will be reset on next positional cast");
                }
                else
                {
                    StatsCurrent.Label = "Current encounter";
                    if (ImGui.SmallButton("Finalize"))
                    {
                        P.currentProfile.CurrentEncounterStats.Finished = true;
                    }
                }
            }
        }
    };

    internal static void Draw()
    {
        ImGuiHelpers.ScaledDummy(5f);
        StatsGlobal.DrawStretched();
        StatsCurrent.DrawStretched();
    }
}
