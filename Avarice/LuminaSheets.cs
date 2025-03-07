using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Linq;
using ECommons.DalamudServices;
using System.Reflection;

namespace Avarice;

internal static class LuminaSheets
{
    // Cache of DataIDs for enemies that don't have positional requirements
    internal static HashSet<uint> NonPositionalUnits = new();

    // Cache of known positional requirement status for enemies
    private static Dictionary<uint, bool> PositionalStatusCache = new();

    // Status effect IDs that disable positional requirements
    internal static readonly uint[] TrueNorthEffects = new uint[] { 1250 }; // True North status ID

    internal static void Init()
    {
        try
        {
            // Get the BNpcBase sheet
            var bnpcSheet = Svc.Data.GetExcelSheet<BNpcBase>();
            if (bnpcSheet != null)
            {
                // Try to find the positional flag property
                PropertyInfo property = typeof(BNpcBase).GetProperty("IsOmnidirectional");

                if (property != null)
                {
                    // Found the property, initialize using it
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
                    Svc.Log.Debug("IsOmnidirectional property not found in BNpcBase, will use hitbox-based detection");
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

    // Extension method to check if an object has positional requirements
    public static bool HasPositional(this IGameObject obj)
    {
        if (obj is not IBattleNpc bnpc)
            return false;

        // Check cache first for performance
        uint dataId = bnpc.DataId;
        if (PositionalStatusCache.TryGetValue(dataId, out bool hasPositional))
            return hasPositional;

        // Primary check: Is this enemy in our non-positional list from BNpcBase?
        bool result = !NonPositionalUnits.Contains(dataId);

        // Secondary checks for special cases
        if (result)
        {
            // Training dummies always have positionals regardless of database
            if (IsTrainingDummy(bnpc))
                result = true;
            // Very large enemies typically don't have positionals
            else if (bnpc.HitboxRadius > 10.0f)
                result = false;
            // Only consider enemies that can be fought
            else if (bnpc.BattleNpcKind != Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind.Enemy)
                result = false;
        }

        // Cache the result
        PositionalStatusCache[dataId] = result;

        return result;
    }

    // Check if an object is a training dummy
    public static bool IsTrainingDummy(IGameObject obj)
    {
        if (obj is not IBattleNpc bnpc)
            return false;

        // Common training dummy NameIDs
        return bnpc.NameId == 674 ||  // Striking Dummy
               bnpc.NameId == 675 ||  // Stone Striking Dummy
               bnpc.NameId == 676 ||  // Wooden Striking Dummy
               bnpc.NameId == 677;    // Popoto Striking Dummy
    }

    // Check if the player has True North or similar effect active
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

    // Clear caches (e.g., on zone change)
    public static void ClearCaches()
    {
        PositionalStatusCache.Clear();
    }
}