using Lumina.Excel.Sheets;

namespace Avarice.ConfigurationWindow;

internal static class TabProfiles
{
    internal static void Draw()
    {
        ImGuiEx.Text("Current profile:");
        ImGui.SameLine();
        ImGuiEx.InputWithRightButtonsArea("CurrentProfiles", delegate
        {
            if (ImGui.BeginCombo("##prof", P.currentProfile.Name.Default("<unnamed>")))
            {
                for (var i = 0; i < P.config.Profiles.Count; i++)
                {
                    var profile = P.config.Profiles[i];
                    if (ImGui.Selectable($"#{i + 1} - {profile.Name.Default("<unnamed>")}", profile == P.currentProfile))
                    {
                        P.currentProfile = profile;
                    }
                }
                ImGui.EndCombo();
            }
        }, delegate
        {
            if (ImGui.Button("Delete"))
            {
                if (P.config.Profiles.Count == 1)
                {
                    Notify.Error("Last profile can not be removed");
                }
                else
                {
                    P.config.Profiles.Remove(P.currentProfile);
                    P.currentProfile = P.config.Profiles.FirstOr0(x => x.IsDefault);
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Add"))
            {
                var prof = new Profile();
                P.config.Profiles.Add(prof);
                P.currentProfile = prof;
            }
        });
        ImGui.Separator();
        ImGuiEx.LineCentered("defaultprofile", delegate
        {
            if (P.currentProfile.IsDefault)
            {
                ImGuiEx.Text("This is default profile.");
            }
            else
            {
                if (ImGui.SmallButton("Make this profile default"))
                {
                    foreach (var x in P.config.Profiles)
                    {
                        x.IsDefault = false;
                    }
                    P.currentProfile.IsDefault = true;
                }
            }
        });
        ImGuiEx.SetNextItemFullWidth();
        ImGui.InputTextWithHint("##namep", "Profile name...", ref P.currentProfile.Name, 100);

        ImGuiHelpers.ScaledDummy(5f);
        BoxJob.DrawStretched();


    }

    static InfoBox BoxJob = new()
    {
        Label = "Assign profiles to jobs",
        ContentsAction = delegate
        {
            foreach (var x in Svc.Data.GetExcelSheet<ClassJob>().Where(x => x.JobIndex > 0))
            {
                ImGuiEx.Text($"{x.NameEnglish}:");
                ImGui.SameLine(100f.Scale());
                ImGuiEx.SetNextItemFullWidth(-15);
                if (ImGui.BeginCombo($"##sel{x.RowId}", P.GetProfileForJob(x.RowId)?.Name ?? "<unassigned>"))
                {
                    if (ImGui.Selectable("Unassign"))
                    {
                        P.config.JobProfiles.Remove(x.RowId);
                    }
                    foreach (var z in P.config.Profiles)
                    {
                        if (ImGui.Selectable(z.Name))
                        {
                            P.config.JobProfiles[x.RowId] = z.GUID;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        }
    };
}
