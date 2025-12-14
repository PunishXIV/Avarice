global using System.Runtime.InteropServices;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using Dalamud.Plugin;
global using Dalamud.Bindings.ImGui;
global using ECommons.DalamudServices;
global using Dalamud.Interface;
global using Dalamud.Interface.Colors;
global using Dalamud.Interface.Windowing;
global using ECommons.Logging;
global using ECommons;
global using static ECommons.GenericHelpers;
global using static Avarice.Avarice;
global using System.Numerics;
global using Avarice.Drawing;
global using ECommons.ImGuiMethods;
global using Avarice.Configuration;
global using Avarice.ConfigurationWindow;
global using static Avarice.Global;
global using ECommons.DalamudServices.Legacy;
global using Dalamud.Interface.Utility;
global using Dalamud.Game.ClientState.Objects.SubKinds;
global using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;

namespace Avarice;
internal static class Global
{
		internal static Profile Prof => P.currentProfile;
		internal static IPlayerCharacter LP => Player.Object;
}
