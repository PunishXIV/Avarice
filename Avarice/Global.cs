global using Avarice.Configuration;
global using Avarice.ConfigurationWindow;
global using Avarice.Drawing;
global using Dalamud.Game.ClientState.Objects.SubKinds;
global using Dalamud.Interface;
global using Dalamud.Interface.Colors;
global using Dalamud.Interface.Utility;
global using Dalamud.Interface.Windowing;
global using Dalamud.Plugin;
global using ECommons;
global using ECommons.DalamudServices;
global using ECommons.DalamudServices.Legacy;
global using ECommons.ImGuiMethods;
global using ECommons.Logging;
global using ImGuiNET;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Numerics;
global using System.Runtime.InteropServices;
global using System.Text;
global using System.Threading.Tasks;
global using static Avarice.Avarice;
global using static Avarice.Global;
global using static ECommons.GenericHelpers;
using ECommons.GameHelpers;

namespace Avarice;
internal static class Global
{
    internal static Profile Prof => P.currentProfile;
    internal static IPlayerCharacter LP => Player.Object;
}
