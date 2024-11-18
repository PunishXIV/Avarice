using Avarice.Data;
using Avarice.StaticData;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using PunishLib.ImGuiMethods;
using System.IO;

namespace Avarice;

internal static unsafe class Util
{
	internal static void DrawDot(Vector3 where, float thickness, Vector4 col)
	{
		DrawDot(where, thickness, col.ToUint());
	}

	internal static void DrawDot(Vector3 where, float thickness, uint col)
	{
		if (Svc.GameGui.WorldToScreen(where, out Vector2 pos))
		{
			ImGui.GetWindowDrawList().AddCircleFilled(
			new Vector2(pos.X, pos.Y),
			thickness,
			col,
			100);
		}
	}

	internal static bool TryAutoDetectMiddleOfArena(out Vector3 mid)
	{
		if (Player.Available)
		{
			bool shouldAuto = P.StaticAutoDetectRadiusData.Contains(Svc.ClientState.TerritoryType);
			if (P.config.DutyMiddleOverrides.TryGetValue(Svc.ClientState.TerritoryType, out Vector3? v))
			{
				if (v == null)
				{
					shouldAuto = true;
				}
				else
				{
					mid = v.Value;
					return true;
				}
			}
			if (shouldAuto)
			{
				if (Player.Object.Position.X.InRange(-50f, 50f) && Player.Object.Position.Z.InRange(-50f, 50f))
				{
					mid = Vector3.Zero;
					return true;
				}
				else if (Player.Object.Position.X.InRange(50, 150) && Player.Object.Position.Z.InRange(50, 150))
				{
					mid = new(100f, 0f, 100f);
					return true;
				}
			}
		}
		mid = default;
		return false;
	}

	internal static HashSet<uint> LoadStaticAutoDetectRadiusData()
	{
		HashSet<uint> ret = new();
		try
		{
			string path = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "AutoDetectTankRadius.csv");
			foreach (string x in File.ReadAllText(path).Split("\n", StringSplitOptions.TrimEntries))
			{
				if (x != "" && uint.TryParse(x, out uint res))
				{
					_ = ret.Add(res);
				}
			}
		}
		catch (Exception e)
		{
			e.Log();
		}
		return ret;
	}

	internal static void DrawStretched(this Drawing.InfoBox box)
	{
		_ = ImGuiGroup.BeginGroupBox(box.Label);
		box.ContentsAction();
		ImGuiGroup.EndGroupBox();
	}

	internal static (int min, int max) GetAngleRangeForDirection(CardinalDirection d)
	{
		if (d == CardinalDirection.North)
		{
			return (-45, 45);
		}

		if (d == CardinalDirection.South)
		{
			return (180 - 45, 180 + 45);
		}

		if (d == CardinalDirection.West)
		{
			return (90 - 45, 90 + 45);
		}

		if (d == CardinalDirection.East)
		{
			return (270 - 45, 270 + 45);
		}

		return (default, default);
	}

	internal static (int min, int max) Get18PieForAngle(float a)
	{
		if (a.InRange(315, 360))
		{
			return (0, 45);
		}

		if (a.InRange(0, 45))
		{
			return (-45, 0);
		}

		if (a.InRange(45, 90))
		{
			return (270, 315);
		}

		if (a.InRange(90, 135))
		{
			return (225, 270);
		}

		if (a.InRange(135, 180))
		{
			return (180, 225);
		}

		if (a.InRange(180, 225))
		{
			return (135, 180);
		}

		if (a.InRange(225, 270))
		{
			return (90, 135);
		}

		if (a.InRange(270, 315))
		{
			return (45, 90);
		}

		return (default, default);
	}

	internal static float GetConfiguredRadius()
	{
		if (P.currentProfile.EnableCurrentPie && P.currentProfile.Radius2 && !P.currentProfile.Radius3)
		{
			return GetAttackRadius();
		}

		return GetSkillRadius();
	}

	internal static float GetSkillRadius()
	{
		return P.currentProfile.MeleeSkillAtk + (P.currentProfile.MeleeSkillIncludeHitbox ? Svc.ClientState.LocalPlayer.HitboxRadius : 0);
	}

	internal static float GetAttackRadius()
	{
		return P.currentProfile.MeleeAutoAtk + (P.currentProfile.MeleeAutoIncludeHitbox ? Svc.ClientState.LocalPlayer.HitboxRadius : 0);
	}

	internal static CardinalDirection GetDirection(IGameObject bnpc)
	{
		return MathHelper.GetCardinalDirection(GetAngle(bnpc));
	}

	internal static float GetAngle(IGameObject bnpc)
	{
		return (MathHelper.GetRelativeAngle(Svc.ClientState.LocalPlayer.Position, bnpc.Position) + bnpc.Rotation.RadToDeg()) % 360;
	}

	internal static bool IsPositionalJob()
	{
		return Svc.ClientState.LocalPlayer?.ClassJob.RowId.EqualsAny(PositionalJobs) == true;
	}

	private static MNKGauge MNKGauge
	{
		get
		{
			return Svc.Gauges.Get<MNKGauge>();
		}
	}

	private static DRGGauge DRGGauge
	{
		get
		{
			return Svc.Gauges.Get<DRGGauge>();
		}
	}

	private static NINGauge NINGauge
	{
		get
		{
			return Svc.Gauges.Get<NINGauge>();
		}
	}

	private static SAMGauge SAMGauge
	{
		get
		{
			return Svc.Gauges.Get<SAMGauge>();
		}
	}

	private static RPRGauge RPRGauge
	{
		get
		{
			return Svc.Gauges.Get<RPRGauge>();
		}
	}

	private static VPRGauge VPRGauge
	{
		get
		{
			return Svc.Gauges.Get<VPRGauge>();
		}
	}

	public static bool IsMNKAnticipatedRear()
	{
		bool levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.Demolish).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return levelcheck && MNKGauge.CoeurlFury == 0
			&& (move.EqualsAny((uint)ActionID.TwinSnakes) || move.EqualsAny((uint)ActionID.TrueStrike) || move.EqualsAny((uint)ActionID.RisingRaptor));
	}
	public static bool IsMNKAnticipatedFlank()
	{
		bool levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.SnapPunch).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return levelcheck && MNKGauge.CoeurlFury > 0
			&& (move.EqualsAny((uint)ActionID.TwinSnakes) || move.EqualsAny((uint)ActionID.TrueStrike) || move.EqualsAny((uint)ActionID.RisingRaptor));
	}

	public static bool IsDRGAnticipatedRear()
	{
		bool levelcheckChaos = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.ChaosThrust).ClassJobLevel;
		bool levelcheckWheeling = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.WheelingThrust).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return levelcheckChaos
			&& (move.EqualsAny((uint)ActionID.Disembowel) || move.EqualsAny((uint)ActionID.SpiralBlow)
			|| (levelcheckWheeling && (move.EqualsAny((uint)ActionID.ChaosThrust) || move.EqualsAny((uint)ActionID.ChaoticSpring))));
	}
	public static bool IsDRGAnticipatedFlank()
	{
		bool levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.FangandClaw).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return levelcheck
			&& (move.EqualsAny((uint)ActionID.FullThrust) || move.EqualsAny((uint)ActionID.HeavensThrust));
	}

	public static bool IsNINAnticipatedRear()
	{
		bool levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.AeolianEdge).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return levelcheck && ((move.EqualsAny((uint)ActionID.GustSlash) && (NINGauge.Kazematoi > 3 || (NINGauge.Kazematoi > 0
			&& ((ComboCache.ComboCacheInstance.GetStatus((uint)ActionID.TrickAttackDebuff, Svc.Targets.Target, Svc.ClientState.LocalPlayer.GameObjectId) != null)
			|| (ComboCache.ComboCacheInstance.GetStatus((uint)ActionID.KunaisBaneDebuff, Svc.Targets.Target, Svc.ClientState.LocalPlayer.GameObjectId) != null)))))
			|| (P.currentProfile.TrickAttack && !ComboCache.ComboCacheInstance.GetCooldown((uint)ActionID.TrickAttack).IsCooldown
			&& (uint)Player.Job == 30));
	}
	public static bool IsNINAnticipatedFlank()
	{
		bool levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.ArmorCrush).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return levelcheck && move.EqualsAny((uint)ActionID.GustSlash) && NINGauge.Kazematoi <= 3
			&& ComboCache.ComboCacheInstance.GetStatus((uint)ActionID.TrickAttackDebuff, Svc.Targets.Target, Svc.ClientState.LocalPlayer.GameObjectId) == null
			&& ComboCache.ComboCacheInstance.GetStatus((uint)ActionID.KunaisBaneDebuff, Svc.Targets.Target, Svc.ClientState.LocalPlayer.GameObjectId) == null;
	}

	public static bool IsSAMAnticipatedRear()
	{
		bool levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.Gekko).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return levelcheck && (move.EqualsAny((uint)ActionID.Jinpu)
			|| (Player.Status.Any(x => x.StatusId.EqualsAny(1233u)) && !SAMGauge.Sen.HasFlag(Sen.GETSU) && SAMGauge.Sen.HasFlag(Sen.KA)
			&& !P.currentProfile.Meikyo));
	}
	public static bool IsSAMAnticipatedFlank()
	{
		bool levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.Kasha).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return levelcheck && (move.EqualsAny((uint)ActionID.Shifu)
			|| (Player.Status.Any(x => x.StatusId.EqualsAny(1233u)) && !SAMGauge.Sen.HasFlag(Sen.KA)
			&& !P.currentProfile.Meikyo));
	}

	public static bool IsRPRAnticipatedRear()
	{
		bool levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.Gallows).ClassJobLevel;
		return levelcheck && Player.Status.Any(x => x.StatusId.EqualsAny(2587u, 3858u))
			&& (Player.Status.Any(x => x.StatusId.EqualsAny(2589u))
			|| (!Player.Status.Any(x => x.StatusId.EqualsAny(2588u)) && !Player.Status.Any(x => x.StatusId.EqualsAny(2589u)) && P.currentProfile.Reaper == 0));
	}
	public static bool IsRPRAnticipatedFlank()
	{
		bool levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.Gibbet).ClassJobLevel;
		return levelcheck && Player.Status.Any(x => x.StatusId.EqualsAny(2587u, 3858u))
			&& (Player.Status.Any(x => x.StatusId.EqualsAny(2588u))
			|| (!Player.Status.Any(x => x.StatusId.EqualsAny(2588u)) && !Player.Status.Any(x => x.StatusId.EqualsAny(2589u)) && P.currentProfile.Reaper == 1));
	}

	public static bool IsVPRAnticipatedRear()
	{
		bool levelcheckMain = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.HindstingStrike).ClassJobLevel;
		bool levelcheckVice = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.Vicewinder).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return (levelcheckMain && (move.EqualsAny((uint)ActionID.HuntersSting) || move.EqualsAny((uint)ActionID.SwiftskinsSting))
			&& (Player.Status.Any(x => x.StatusId.EqualsAny(3647u, 3648u))
			|| (!Player.Status.Any(x => x.StatusId.EqualsAny(3645u, 3646u, 3647u, 3648u)))))
			|| (levelcheckVice && ((ActionWatching.LastWeaponskill == (uint)ActionID.Vicewinder
			&& CustomComboFunctions.GetBuffRemainingTime((ushort)ActionID.Swiftscaled) <=
			   CustomComboFunctions.GetBuffRemainingTime((ushort)ActionID.HuntersInstinct))
			|| (ActionWatching.LastWeaponskill == (uint)ActionID.HuntersCoil && VPRGauge.DreadCombo != 0)));
	}
	public static bool IsVPRAnticipatedFlank()
	{
		bool levelcheckMain = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.FlankstingStrike).ClassJobLevel;
		bool levelcheckVice = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow((uint)ActionID.Vicewinder).ClassJobLevel;
		uint move = P.memory.LastComboMove;
		return (levelcheckMain && (move.EqualsAny((uint)ActionID.HuntersSting) || move.EqualsAny((uint)ActionID.SwiftskinsSting))
			&& Player.Status.Any(x => x.StatusId.EqualsAny(3645u, 3646u)))
			|| (levelcheckVice && ((ActionWatching.LastWeaponskill == (uint)ActionID.Vicewinder
			&& CustomComboFunctions.GetBuffRemainingTime((ushort)ActionID.HuntersInstinct) <
			   CustomComboFunctions.GetBuffRemainingTime((ushort)ActionID.Swiftscaled))
			|| (ActionWatching.LastWeaponskill == (uint)ActionID.SwiftskinsCoil && VPRGauge.DreadCombo != 0)));
	}
}