using Lumina.Excel.Sheets;

namespace Avarice.ConfigurationWindow;

internal static class TabStatistics
{
    internal static void Draw()
    {
        Ui.PageTitle("Statistics", "Positional hits and misses for this profile.");
        DrawTotals();
        DrawEncounter();
    }

    private static void DrawTotals()
    {
        Ui.SectionLabel("Total");
        var hasData = P.currentProfile.Stats.Any(x => x.Value.Hits + x.Value.Missed > 0);
        if (!hasData)
        {
            ImGuiEx.Text(Ui.Muted, "No positional hits recorded on this profile yet.");
            return;
        }

        ImGui.BeginTable("##table", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit, new Vector2(ImGui.GetContentRegionAvail().X, 0));
        ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Hits");
        ImGui.TableSetupColumn("Total");
        ImGui.TableSetupColumn("Success %");
        ImGui.TableHeadersRow();
        var total = new Stats();
        foreach (var x in P.currentProfile.Stats)
        {
            DrawStatsRow(x.Key, x.Value);
            total.Hits += x.Value.Hits;
            total.Missed += x.Value.Missed;
        }
        if (total.Hits > 0 || total.Missed > 0)
            DrawStatsRow(0, total, "Total");
        ImGui.EndTable();

        if (ImGui.SmallButton("Clear data (hold Shift+Ctrl)"))
        {
            if (ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl)
                P.currentProfile.Stats = new();
        }
    }

    private static void DrawEncounter()
    {
        var x = P.currentProfile.CurrentEncounterStats;
        var total = x.Hits + x.Missed;
        Ui.SectionLabel(x.Finished ? "Recent encounter" : "Current encounter");

        if (total == 0)
        {
            ImGuiEx.Text(Ui.Muted, "No encounter data yet. Hits and misses appear after you use a positional.");
            return;
        }

        var success = (int)(100f * x.Hits / total);
        ImGuiEx.Text($"Hits: {x.Hits} out of {total} — ");
        ImGui.SameLine(0, 0);
        ImGuiEx.Text(Util.GetParsedColor(success), $"{success}%");

        if (ImGui.SmallButton("Clear data"))
            P.currentProfile.CurrentEncounterStats = new();
        ImGui.SameLine();
        if (x.Finished)
        {
            ImGuiEx.Text(ImGuiColors.DalamudRed, "Stats reset on the next positional.");
        }
        else if (ImGui.SmallButton("Finalize"))
        {
            P.currentProfile.CurrentEncounterStats.Finished = true;
        }
    }

    private static void DrawStatsRow(uint job, Stats x, string colName = null)
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
        var success = (int)(100f * x.Hits / (float)total);
        ImGuiEx.Text(Util.GetParsedColor(success), $"{success}%");
    }
}
