using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;
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
				internal static void Draw()
				{
						ImGuiGroup.BeginGroupBox("Tactical Compass");
						DrawInternal();
						ImGuiGroup.EndGroupBox();
				}

				static void DrawInternal()
				{
						ImGui.PushID("compass");
						ImGui.SetNextItemWidth(SelectWidth);
            ImGui.Checkbox("Tactical Compass", ref P.currentProfile.CompassEnable);
						//if (P.currentProfile.CompassEnable)
						{
								ImGui.SameLine();
								ImGui.SetNextItemWidth(150f);
								ImGuiEx.EnumCombo($"##cb1", ref P.currentProfile.CompassCondition);

								ImGuiEx.InvisibleButton(3);
								ImGui.SameLine();
								ImGui.SetNextItemWidth(150f);
								ImGuiEx.EnumCombo("Game font family and size", ref Prof.CompassFont);

								ImGuiEx.InvisibleButton(3);
								ImGui.SameLine();
								ImGui.SetNextItemWidth(150f);
								ImGui.SliderFloat("Font Scale", ref Prof.CompassFontScale.ValidateRange(0, 100f), 0.5f, 20f);

								ImGuiEx.InvisibleButton(3);
								ImGui.SameLine();
								ImGui.SetNextItemWidth(150f);
								ImGui.SliderFloat("Distance Offset", ref Prof.CompassDistance.ValidateRange(0, float.MaxValue), 0.01f, 20f);

								ImGuiEx.InvisibleButton(3);
								ImGui.SameLine();
								ImGui.SetNextItemWidth(150f);
								ImGui.ColorEdit4("North Color", ref Prof.CompassColorN, ImGuiColorEditFlags.NoInputs);

								ImGuiEx.InvisibleButton(3);
								ImGui.SameLine();
								ImGui.SetNextItemWidth(150f);
								ImGui.ColorEdit4("Other Colors", ref Prof.CompassColor, ImGuiColorEditFlags.NoInputs);
						}
						ImGui.PopID();
				}
		}
}
