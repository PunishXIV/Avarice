using Lumina.Excel.Sheets;

namespace Avarice.ConfigurationWindow;

internal static class TabProfiles
{
    internal static void Draw()
    {
        Ui.PageTitle("Profiles", "Name this profile, set the default, and assign jobs. Switch profiles from the list on the left.");

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##namep", "Profile name...", ref P.currentProfile.Name, 100);

        if (ImGui.Button("Add"))
        {
            var prof = new Profile();
            P.config.Profiles.Add(prof);
            P.currentProfile = prof;
        }
        ImGui.SameLine();
        if (ImGui.Button("Delete"))
        {
            if (P.config.Profiles.Count == 1)
            {
                Notify.Error("The last profile cannot be removed.");
            }
            else
            {
                P.config.Profiles.Remove(P.currentProfile);
                P.currentProfile = P.config.Profiles.FirstOr0(x => x.IsDefault);
            }
        }
        ImGui.SameLine();
        if (P.currentProfile.IsDefault)
        {
            ImGuiEx.Text(Ui.Muted, "This is the default profile.");
        }
        else if (ImGui.Button("Make default"))
        {
            foreach (var x in P.config.Profiles)
                x.IsDefault = false;
            P.currentProfile.IsDefault = true;
        }

        Ui.SectionLabel("Assign profiles to jobs");
        foreach (var x in Svc.Data.GetExcelSheet<ClassJob>().Where(j => j.JobIndex > 0))
        {
            ImGuiEx.Text($"{x.NameEnglish}:");
            ImGui.SameLine(120f * ImGuiHelpers.GlobalScale);
            ImGuiEx.SetNextItemFullWidth(-15);
            if (ImGui.BeginCombo($"##sel{x.RowId}", P.GetProfileForJob(x.RowId)?.Name ?? "<unassigned>"))
            {
                if (ImGui.Selectable("Unassign"))
                    P.config.JobProfiles.Remove(x.RowId);
                foreach (var z in P.config.Profiles)
                {
                    if (ImGui.Selectable(z.Name))
                        P.config.JobProfiles[x.RowId] = z.GUID;
                }
                ImGui.EndCombo();
            }
        }
    }
}
