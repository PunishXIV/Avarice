using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Avarice.ConfigurationWindow.ConfigWindow;
using static Avarice.ConfigurationWindow.TabSettings;

namespace Avarice.ConfigurationWindow.Player
{
		internal static class BoxCompass
		{
				internal static void Draw() => PunishLib.ImGuiMethods.InfoBox.DrawBox("Tactical Compass", DrawInternal);

				static void DrawInternal()
				{
						ImGui.PushID("compass");
						ImGui.SetNextItemWidth(SelectWidth);
						ImGuiEx.EnumCombo("##compass", ref P.currentProfile.CompassEnable, ClassDisplayConditionNames);
						if (P.currentProfile.CompassEnable.IsEnabled())
						{
								ImGui.SameLine();
								ImGui.SetNextItemWidth(150f);
								ImGuiEx.EnumCombo($"##cb1", ref P.currentProfile.CompassCondition);
								ImGuiEx.InvisibleButton(3);
								ImGui.SameLine();
								ImGui.SetNextItemWidth(150f);
								ImGui.SliderFloat("Size", ref Prof.CompassSize.ValidateRange(0, 100f), 0.5f, 20f);

								ImGuiEx.InvisibleButton(3);
								ImGui.SameLine();
								ImGui.SetNextItemWidth(150f);
								ImGui.SliderFloat("Distance", ref Prof.CompassDistance.ValidateRange(0, float.MaxValue), 0.01f, 20f);
						}
						ImGui.PopID();
				}
		}
}
