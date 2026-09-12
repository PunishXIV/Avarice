using Avarice.Data;
using Avarice.StaticData;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace Avarice.Positional;

[Flags]
internal enum AnticipatedSegments
{
	None = 0,
	Rear = 1,
	Flank = 2,
	Both = Rear | Flank,
}

internal readonly struct AnticipationHint
{
	internal static readonly AnticipationHint Empty = new AnticipationHint("none", AnticipatedSegments.None, Array.Empty<uint>());

	internal AnticipationHint(string source, AnticipatedSegments segments, uint[] actions)
	{
		Source = source;
		Segments = segments;
		Actions = actions ?? Array.Empty<uint>();
	}

	internal string Source { get; }
	internal AnticipatedSegments Segments { get; }
	internal uint[] Actions { get; }
}

/// <summary>
/// Wrath / RSR / combo all resolve to action IDs, then <see cref="Data.ActionPositional"/>.
/// </summary>
internal static unsafe class Anticipation
{
	private static class Status
	{
		internal const uint RaptorForm = 108;
		internal const uint CoeurlForm = 109;
		internal const uint PerfectBalance = 110;
		internal const uint MeikyoShisui = 1233;
		internal const uint FormlessFist = 2513;
		internal const uint SoulReaver = 2587;
		internal const uint EnhancedGibbet = 2588;
		internal const uint EnhancedGallows = 2589;
		internal const uint Enshrouded = 2593;
		internal const uint FlankstungVenom = 3645;
		internal const uint FlanksbaneVenom = 3646;
		internal const uint HindstungVenom = 3647;
		internal const uint HindsbaneVenom = 3648;
		internal const uint HuntersInstinct = 3668;
		internal const uint Swiftscaled = 3669;
		internal const uint Reawakened = 3670;
		internal const uint Executioner = 3858;
	}

	private static string lastResolveKey = "";
	private static uint lastResolveFrame;
	private static ulong lastResolveTarget;
	private static AnticipationHint lastResolveHint = AnticipationHint.Empty;

	internal static AnticipationHint Resolve(IBattleNpc target)
	{
		var frame = Framework.Instance()->FrameCounter;
		if (frame == lastResolveFrame && target.GameObjectId == lastResolveTarget)
			return lastResolveHint;

		AnticipationHint hint;
		if (!TryWrath(target, out hint) && !TryRotationSolver(out hint))
			hint = FromCombo();

		lastResolveFrame = frame;
		lastResolveTarget = target.GameObjectId;
		lastResolveHint = hint;
		return LogResolve(hint);
	}

	private static AnticipationHint LogResolve(AnticipationHint hint)
	{
		var key = $"{hint.Source}|{hint.Segments}|{string.Join(",", hint.Actions)}";
		if (key == lastResolveKey)
			return hint;
		lastResolveKey = key;
		var message = $"[Anticipate] source={hint.Source} segments={hint.Segments} actions=[{string.Join(",", hint.Actions)}]";
		PluginLog.Debug(message);
		if (P.currentProfile?.Debug == true)
			PluginLog.Information(message);
		return hint;
	}

	private static bool TryWrath(IBattleNpc target, out AnticipationHint hint)
	{
		hint = AnticipationHint.Empty;
		if (!P.currentProfile.UseWrathCombo || !P.WrathComboWatcher.TryGetHintForTarget(target, out var direction))
			return false;

		var actionId = P.WrathComboWatcher.CurrentHint.ActionId;
		if (StaticData.Data.TryGetPositional(actionId, out var positional))
		{
			hint = new AnticipationHint("wrath", SegmentFor(positional), new[] { actionId });
			return true;
		}

		var segments = direction == WrathComboPositionalDirection.Rear
			? AnticipatedSegments.Rear
			: direction == WrathComboPositionalDirection.Flank
				? AnticipatedSegments.Flank
				: AnticipatedSegments.None;
		if (segments == AnticipatedSegments.None)
			return false;

		hint = new AnticipationHint("wrath", segments, actionId == 0 ? Array.Empty<uint>() : new[] { actionId });
		return true;
	}

	private static bool TryRotationSolver(out AnticipationHint hint)
	{
		hint = AnticipationHint.Empty;
		if (!P.currentProfile.UseRotationSolver || !P.RotationSolverWatcher.IPCAvailable)
			return false;

		var positional = P.RotationSolverWatcher.DesiredPositional;
		if (positional is EnemyPositional.Rear or EnemyPositional.Flank)
		{
			hint = new AnticipationHint("rsr", SegmentFor(positional), Array.Empty<uint>());
			return true;
		}

		// Front is a real answer (no rear/flank pie). None means RSR has not sent a hint yet.
		if (positional == EnemyPositional.Front)
			return true;

		return false;
	}

	private static AnticipationHint FromCombo()
	{
		if (Svc.Objects.LocalPlayer is null)
			return AnticipationHint.Empty;

		ActionID[] actions;
		switch (Svc.Objects.LocalPlayer.ClassJob.RowId)
		{
			case 2:
			case 20:
				actions = Monk();
				break;
			case 4:
			case 22:
				actions = Dragoon();
				break;
			case 29:
			case 30:
				actions = Ninja();
				break;
			case 34:
				actions = Samurai();
				break;
			case 39:
				actions = Reaper();
				break;
			case 41:
				actions = Viper();
				break;
			default:
				return AnticipationHint.Empty;
		}

		return FromActions("combo", actions);
	}

	private static AnticipationHint FromActions(string source, ActionID[] actions)
	{
		var segments = AnticipatedSegments.None;
		var ids = new List<uint>();
		for (var i = 0; i < actions.Length; i++)
		{
			var id = (uint)actions[i];
			if (!StaticData.Data.TryGetPositional(id, out var positional))
				continue;
			ids.Add(id);
			segments |= SegmentFor(positional);
		}

		return segments == AnticipatedSegments.None
			? AnticipationHint.Empty
			: new AnticipationHint(source, segments, ids.ToArray());
	}

	private static AnticipatedSegments SegmentFor(EnemyPositional positional)
	{
		if (positional == EnemyPositional.Rear)
			return AnticipatedSegments.Rear;
		if (positional == EnemyPositional.Flank)
			return AnticipatedSegments.Flank;
		return AnticipatedSegments.None;
	}

	private static bool Learned(ActionID id)
	{
		var action = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault((uint)id);
		return action != null &&
		       action.Value.ClassJobCategory.ValueNullable != null &&
		       Svc.Objects.LocalPlayer.Level >= action.Value.ClassJobLevel;
	}

	private static bool Has(uint statusId) =>
		Player.Status.Any(x => x.StatusId == statusId);

	private static bool HasAny(params uint[] statusIds) =>
		Player.Status.Any(x => statusIds.Contains(x.StatusId));

	private static float Remaining(uint statusId)
	{
		var status = Player.Status.FirstOrDefault(x => x.StatusId == statusId);
		return status?.RemainingTime ?? 0;
	}

	private static bool ComboIs(params ActionID[] ids)
	{
		if (P.memory.ComboTimer <= 0)
			return false;
		var combo = P.memory.LastComboMove;
		for (var i = 0; i < ids.Length; i++)
		{
			if (combo == (uint)ids[i])
				return true;
		}
		return false;
	}

	private static ActionID[] Monk()
	{
		if (Has(Status.PerfectBalance) || Has(Status.FormlessFist))
			return Array.Empty<ActionID>();
		if (!Has(Status.CoeurlForm) && !Has(Status.RaptorForm))
			return Array.Empty<ActionID>();
		if (Svc.Gauges.Get<MNKGauge>().CoeurlFury == 0 && Learned(ActionID.Demolish))
			return new[] { ActionID.Demolish };
		return Learned(ActionID.SnapPunch) ? new[] { ActionID.SnapPunch } : Array.Empty<ActionID>();
	}

	private static ActionID[] Dragoon()
	{
		if (ComboIs(ActionID.Disembowel, ActionID.SpiralBlow) && Learned(ActionID.ChaosThrust))
			return new[] { ActionID.ChaosThrust };
		if (ComboIs(ActionID.ChaosThrust, ActionID.ChaoticSpring) && Learned(ActionID.WheelingThrust))
			return new[] { ActionID.WheelingThrust };
		if (ComboIs(ActionID.FullThrust, ActionID.HeavensThrust, ActionID.VorpalThrust, ActionID.LanceBarrage) &&
		    Learned(ActionID.FangandClaw))
			return new[] { ActionID.FangandClaw };
		return Array.Empty<ActionID>();
	}

	private static ActionID[] Ninja()
	{
		if (!Learned(ActionID.AeolianEdge))
			return Array.Empty<ActionID>();

		var stacks = Svc.Gauges.Get<NINGauge>().Kazematoi;
		var inTrickWindow =
			ComboCache.ComboCacheInstance.GetStatus((uint)ActionID.TrickAttackDebuff, Svc.Targets.Target, Svc.Objects.LocalPlayer.GameObjectId) != null ||
			ComboCache.ComboCacheInstance.GetStatus((uint)ActionID.KunaisBaneDebuff, Svc.Targets.Target, Svc.Objects.LocalPlayer.GameObjectId) != null;

		if (ComboIs(ActionID.SpinningEdge))
			return NinjaFinisher(stacks, true, inTrickWindow);
		if (ComboIs(ActionID.GustSlash))
			return NinjaFinisher(stacks, false, inTrickWindow);
		return Array.Empty<ActionID>();
	}

	private static ActionID[] NinjaFinisher(int stacks, bool lookahead, bool inTrickWindow)
	{
		if (P.currentProfile.Kazematoi && !lookahead)
		{
			if (stacks == 0)
				return Learned(ActionID.ArmorCrush) ? new[] { ActionID.ArmorCrush } : new[] { ActionID.AeolianEdge };
			if (stacks >= 4)
				return new[] { ActionID.AeolianEdge };
			return Learned(ActionID.ArmorCrush)
				? new[] { ActionID.AeolianEdge, ActionID.ArmorCrush }
				: new[] { ActionID.AeolianEdge };
		}

		if (stacks == 0)
			return Learned(ActionID.ArmorCrush) ? new[] { ActionID.ArmorCrush } : new[] { ActionID.AeolianEdge };
		if (stacks >= 4 || inTrickWindow || (P.currentProfile.TrickAttack && TrickOrKunaiReady()))
			return new[] { ActionID.AeolianEdge };
		if (lookahead)
			return Array.Empty<ActionID>();
		return Learned(ActionID.ArmorCrush) ? new[] { ActionID.ArmorCrush } : new[] { ActionID.AeolianEdge };
	}

	private static bool TrickOrKunaiReady()
	{
		var id = Learned(ActionID.KunaisBane) ? ActionID.KunaisBane : ActionID.TrickAttack;
		return !ComboCache.ComboCacheInstance.GetCooldown((uint)id).IsCooldown;
	}

	private static ActionID[] Samurai()
	{
		if (P.currentProfile.Meikyo && Has(Status.MeikyoShisui))
			return Array.Empty<ActionID>();

		if (Has(Status.MeikyoShisui))
		{
			var sen = Svc.Gauges.Get<SAMGauge>().Sen;
			if (!sen.HasFlag(Sen.Getsu) && sen.HasFlag(Sen.Ka) && Learned(ActionID.Gekko))
				return new[] { ActionID.Gekko };
			if (!sen.HasFlag(Sen.Ka) && Learned(ActionID.Kasha))
				return new[] { ActionID.Kasha };
			return Array.Empty<ActionID>();
		}

		if (ComboIs(ActionID.Jinpu) && Learned(ActionID.Gekko))
			return new[] { ActionID.Gekko };
		if (ComboIs(ActionID.Shifu) && Learned(ActionID.Kasha))
			return new[] { ActionID.Kasha };
		return Array.Empty<ActionID>();
	}

	private static ActionID[] Reaper()
	{
		if (Has(Status.Enshrouded))
			return Array.Empty<ActionID>();
		if (Has(Status.EnhancedGibbet) && Learned(ActionID.Gibbet))
			return new[] { ActionID.Gibbet };
		if (Has(Status.EnhancedGallows) && Learned(ActionID.Gallows))
			return new[] { ActionID.Gallows };
		if (!HasAny(Status.SoulReaver, Status.Executioner) || !Learned(ActionID.Gibbet))
			return Array.Empty<ActionID>();
		return P.currentProfile.Reaper == 1 ? new[] { ActionID.Gibbet } : new[] { ActionID.Gallows };
	}

	private static ActionID[] Viper()
	{
		if (Has(Status.Reawakened))
			return Array.Empty<ActionID>();

		var dread = (byte)Svc.Gauges.Get<VPRGauge>().DreadCombo;
		if (dread == 2)
			return new[] { ActionID.SwiftskinsCoil };
		if (dread == 3)
			return new[] { ActionID.HuntersCoil };
		if (dread == 1 && Learned(ActionID.Vicewinder))
		{
			return Remaining(Status.Swiftscaled) <= Remaining(Status.HuntersInstinct)
				? new[] { ActionID.SwiftskinsCoil }
				: new[] { ActionID.HuntersCoil };
		}

		if (!ComboIs(ActionID.HuntersSting, ActionID.SwiftskinsSting))
			return Array.Empty<ActionID>();
		if (Has(Status.HindsbaneVenom) && Learned(ActionID.HindsbaneFang))
			return new[] { ActionID.HindsbaneFang };
		if (Has(Status.HindstungVenom) && Learned(ActionID.HindstingStrike))
			return new[] { ActionID.HindstingStrike };
		if (Has(Status.FlanksbaneVenom) && Learned(ActionID.FlanksbaneFang))
			return new[] { ActionID.FlanksbaneFang };
		if (Has(Status.FlankstungVenom) && Learned(ActionID.FlankstingStrike))
			return new[] { ActionID.FlankstingStrike };
		return Array.Empty<ActionID>();
	}
}
