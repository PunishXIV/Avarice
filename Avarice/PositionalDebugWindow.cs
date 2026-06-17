using Avarice.Structs;

namespace Avarice;

internal class PositionalDebugWindow : Window
{
    internal class Entry
    {
        public ulong Frame;
        public uint ActionId;
        public string ActionName;
        public bool InTable;
        public string TablePosition;
        public string Param2Display;
        public PositionalState Verdict;
        public string Target;
        public string Detail;
    }

    private static readonly List<Entry> Entries = new();
    private const int MaxEntries = 500;
    private static bool OnlyPositional = true;
    private static bool Paused;

    public PositionalDebugWindow() : base("Avarice Positional Debug###AvaricePosDebug")
    {
        Size = new(780, 460);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    internal static void Record(Entry e)
    {
        if (Paused) return;
        Entries.Add(e);
        if (Entries.Count > MaxEntries) Entries.RemoveRange(0, Entries.Count - MaxEntries);
    }

    private static string BuildExport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("frame\tid\taction\tin_table\tpos\tparam2\tverdict\ttarget\tdetail");
        foreach (var e in Entries)
        {
            if (OnlyPositional && !e.InTable) continue;
            sb.AppendLine($"{e.Frame}\t{e.ActionId}\t{e.ActionName}\t{e.InTable}\t{e.TablePosition}\t{e.Param2Display}\t{e.Verdict}\t{e.Target}\t{e.Detail?.Replace("\n", " | ")}");
        }
        return sb.ToString();
    }

    public override void Draw()
    {
        ImGuiEx.Text(ImGuiColors.DalamudYellow, "Captures param2 from your own action-damage packets (the value the hit/miss table is keyed on).");
        ImGuiEx.TextWrapped("Play any job that uses positionals, land hits from rear and flank, then deliberately miss a few. Compare the Verdict column to where you were actually standing. param2 marked with * matched a 'hit' row in the table. Use Copy export to share the raw values.");
        ImGui.Separator();

        ImGui.Checkbox("Only positional-table actions", ref OnlyPositional);
        ImGui.SameLine();
        ImGui.Checkbox("Pause capture", ref Paused);
        ImGui.SameLine();
        if (ImGui.Button("Clear")) Entries.Clear();
        ImGui.SameLine();
        ImGuiEx.ButtonCopy("Copy export", BuildExport());
        ImGui.Separator();

        var avail = ImGui.GetContentRegionAvail();
        if (ImGui.BeginTable("##posdebug", 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit, avail))
        {
            ImGui.TableSetupColumn("Frame");
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("In table");
            ImGui.TableSetupColumn("Pos");
            ImGui.TableSetupColumn("param2");
            ImGui.TableSetupColumn("Verdict");
            ImGui.TableHeadersRow();

            for (var i = Entries.Count - 1; i >= 0; i--)
            {
                var e = Entries[i];
                if (OnlyPositional && !e.InTable) continue;

                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGuiEx.Text($"{e.Frame}");
                ImGui.TableNextColumn(); ImGuiEx.Text($"{e.ActionId}  {e.ActionName}");
                ImGui.TableNextColumn(); ImGuiEx.Text(e.InTable ? "yes" : "no");
                ImGui.TableNextColumn(); ImGuiEx.Text(e.TablePosition ?? "");
                ImGui.TableNextColumn(); ImGuiEx.Text(e.Param2Display);
                ImGui.TableNextColumn();
                var col = e.Verdict == PositionalState.Success ? ImGuiColors.HealerGreen
                        : e.Verdict == PositionalState.Failure ? ImGuiColors.DalamudRed
                        : ImGuiColors.DalamudGrey;
                ImGuiEx.Text(col, e.Verdict.ToString());
            }
            ImGui.EndTable();
        }
    }
}
