using static Avarice.ConfigurationWindow.ConfigWindow;

namespace Avarice.ConfigurationWindow;

internal static unsafe class TabAnticipation
{
	private static readonly InfoBox BoxAnticipated = new()
	{
		Label = "Anticipated Segment Indicator",
		ContentsAction = delegate
		{
			ImGui.SetNextItemWidth(SelectWidth);
			_ = ImGui.Checkbox("Anticipated Segment Indicator", ref P.currentProfile.EnableAnticipatedPie);
			//if (P.currentPrfile.EnableAnticipatedPie)
			{
				ImGui.PushID("AnticipatedPieSettings");
				ImGui.SameLine();
				ImGui.SetNextItemWidth(150f);
				_ = ImGuiEx.EnumCombo($"##1", ref P.currentProfile.AnticipatedPieSettings.DisplayCondition);
				ImGuiEx.TextV("Rear:");

				//DrawUnfilledSettings("", ref P.currentProfile.AnticipatedPieSettings);

				ImGuiEx.InvisibleButton(3);
				ImGui.SameLine();
				P.currentProfile.AnticipatedPieSettings.Fill = Vector4.Zero;
				ImGuiEx.Text($"Thickness:");
				ImGui.SameLine();
				ImGui.SetNextItemWidth(50f);
				_ = ImGui.DragFloat($"##2", ref P.currentProfile.AnticipatedPieSettings.Thickness, 0.1f, 0f, 10f);
				ImGui.SameLine();
				ImGuiEx.Text($"  Color:");
				ImGui.SameLine();
				_ = ImGui.ColorEdit4($"##3", ref P.currentProfile.AnticipatedPieSettings.Color, ImGuiColorEditFlags.NoInputs);
				ImGui.PopID();

				ImGuiEx.TextV("Flank:");
				ImGuiEx.InvisibleButton(3);
				ImGui.SameLine();

				//DrawUnfilledSettings("AnticipatedPieSettingsFlank", ref P.currentProfile.AnticipatedPieSettingsFlank, false);
				ImGui.PushID("AnticipatedPieSettingsFlank");
				P.currentProfile.AnticipatedPieSettingsFlank.Fill = Vector4.Zero;
				ImGuiEx.Text($"Thickness:");
				ImGui.SameLine();
				ImGui.SetNextItemWidth(50f);
				_ = ImGui.DragFloat($"##2", ref P.currentProfile.AnticipatedPieSettingsFlank.Thickness, 0.1f, 0f, 10f);
				ImGui.SameLine();
				ImGuiEx.Text($"  Color:");
				ImGui.SameLine();
				_ = ImGui.ColorEdit4($"##3", ref P.currentProfile.AnticipatedPieSettingsFlank.Color, ImGuiColorEditFlags.NoInputs);
				ImGui.PopID();

				P.currentProfile.AnticipatedPieSettingsFlank.DisplayCondition = P.currentProfile.AnticipatedPieSettings.DisplayCondition;
				_ = ImGui.Checkbox("Disable when under the effect of True North", ref P.currentProfile.AnticipatedDisableTrueNorth);
			}
		}
	};

	private static readonly InfoBox BoxMnk = new()
	{
		Label = "Monk",
		ContentsAction = delegate
		{

		}
	};

	private static readonly InfoBox BoxDrg = new()
	{
		Label = "Dragoon",
		ContentsAction = delegate
		{

		}
	};

	private static readonly InfoBox BoxNin = new()
	{
		Label = "Ninja",
		ContentsAction = delegate
		{
			_ = ImGui.Checkbox("Show anticipation for rear when Trick Attack is off cooldown", ref P.currentProfile.TrickAttack);
			_ = ImGui.Checkbox("Show both valid positionals based on Kazematoi charges.", ref P.currentProfile.Kazematoi);
		}
	};

	private static readonly InfoBox BoxSam = new()
	{
		Label = "Samurai",
		ContentsAction = delegate
		{
			_ = ImGui.Checkbox("Disable anticipation when you have the Meikyo Shisui buff", ref P.currentProfile.Meikyo);
		}
	};

	private static readonly InfoBox BoxRpr = new()
	{
		Label = "Reaper",
		ContentsAction = delegate
		{
			ImGui.Text("Rear or Flank anticipation first?");
			_ = ImGui.RadioButton("Rear", ref P.currentProfile.Reaper, 0);
			_ = ImGui.RadioButton("Flank", ref P.currentProfile.Reaper, 1);
		}
	};

	private static readonly InfoBox BoxVpr = new()
	{
		Label = "Viper",
		ContentsAction = delegate
		{

		}
	};

	private static readonly InfoBox BoxRotationSolver = new() {
		Label = "Rotation Solver Integration",
		ContentsAction = delegate
		{
			_ = ImGui.Checkbox("Use Rotation Solver to anticipate positionals", ref P.currentProfile.UseRotationSolver);
		}
	};

	internal static void Draw()
	{
		ImGuiHelpers.ScaledDummy(5f);
		BoxAnticipated.DrawStretched();
		//BoxMnk.DrawStretched();
		//BoxDrg.DrawStretched();
		BoxNin.DrawStretched();
		BoxSam.DrawStretched();
		BoxRpr.DrawStretched();
		//BoxVpr.DrawStretched();

		if (P.currentProfile.UseRotationSolver || P.RotationSolverWatcher.Available)
		{
			BoxRotationSolver.DrawStretched();
		}
	}
}