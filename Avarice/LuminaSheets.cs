using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Linq;
using ECommons.DalamudServices;
using System.Reflection;

namespace Avarice
{
    internal static class LuminaSheets
    {
        internal static HashSet<uint> NonPositionalUnits = new HashSet<uint>();
        private static Dictionary<uint, bool> PositionalStatusCache = new Dictionary<uint, bool>();
        internal static readonly uint[] TrueNorthEffects = new uint[] { 1250 };

        internal static void Init()
        {
            try
            {
                var bnpcSheet = Svc.Data.GetExcelSheet<BNpcBase>();
                if (bnpcSheet != null)
                {
                    PropertyInfo property = typeof(BNpcBase).GetProperty("IsOmnidirectional");
                    if (property != null)
                    {
                        foreach (var bnpc in bnpcSheet)
                        {
                            if ((bool)property.GetValue(bnpc))
                            {
                                NonPositionalUnits.Add(bnpc.RowId);
                            }
                        }
                        Svc.Log.Debug($"Loaded {NonPositionalUnits.Count} non-positional enemy types from BNpcBase");
                    }
                    else
                    {
                        Svc.Log.Debug("IsOmnidirectional property not found in BNpcBase");
                    }
                }
                else
                {
                    Svc.Log.Error("Failed to load BNpcBase sheet");
                }
            }
            catch (System.Exception ex)
            {
                Svc.Log.Error(ex, "Error initializing LuminaSheets");
                NonPositionalUnits = new HashSet<uint>();
            }
        }

        public static bool HasPositional(this IGameObject obj)
        {
            // If the "Only show for positional targets" configuration is disabled,
            // ignore BNpcBase filtering and treat all targets as positional.
            if (!P.config.OnlyDrawIfPositional)
                return true;

            if (obj is not IBattleNpc bnpc)
                return false;

            uint dataId = bnpc.DataId;
            if (PositionalStatusCache.TryGetValue(dataId, out bool hasPositional))
                return hasPositional;

            bool result = !NonPositionalUnits.Contains(dataId);
            if (result && bnpc.BattleNpcKind != Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind.Enemy)
                result = false;

            PositionalStatusCache[dataId] = result;
            return result;
        }

        public static bool HasTrueNorthEffect()
        {
            if (Svc.ClientState.LocalPlayer == null)
                return false;

            foreach (var status in Svc.ClientState.LocalPlayer.StatusList)
            {
                if (TrueNorthEffects.Contains(status.StatusId))
                    return true;
            }
            return false;
        }

        public static void ClearCaches()
        {
            PositionalStatusCache.Clear();
        }
    }
}
