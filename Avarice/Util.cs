using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;
using ECommons.MathHelpers;

namespace Avarice;

internal static class Util
{
	internal static Vector4 GetParsedColor(int percent)
	{
		if (percent < 25)
			return ImGuiColors.ParsedGrey;
		else if (percent < 50)
			return ImGuiColors.ParsedGreen;
		else if (percent < 75)
			return ImGuiColors.ParsedBlue;
		else if (percent < 95)
			return ImGuiColors.ParsedPurple;
		else if (percent < 99)
			return ImGuiColors.ParsedOrange;
		else if (percent == 99)
			return ImGuiColors.ParsedPink;
		else if (percent == 100)
			return ImGuiColors.ParsedGold;
		else
			return ImGuiColors.DalamudRed;
	}

	internal static ushort GetParsedSeStringColor(int percent)
	{
		if (percent < 25)
			return 3;
		else if (percent < 50)
			return 45;
		else if (percent < 75)
			return 37;
		else if (percent < 95)
			return 541;
		else if (percent < 99)
			return 500;
		else if (percent == 99)
			return 561;
		else if (percent == 100)
			return 573;
		else
			return 518;
	}

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

	internal static async Task<HashSet<uint>> LoadStaticAutoDetectRadiusDataAsync(CancellationToken cancellationToken)
	{
		HashSet<uint> ret = new();
		try
		{
			string path = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "AutoDetectTankRadius.csv");
			var text = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
			foreach (string x in text.Split("\n", StringSplitOptions.TrimEntries))
			{
				if (x != "" && uint.TryParse(x, out uint res))
				{
					_ = ret.Add(res);
				}
			}
		}
		catch (Exception e)
		{
			e.LogDebug();
		}
		return ret;
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
		return P.currentProfile.MeleeSkillAtk + (P.currentProfile.MeleeSkillIncludeHitbox ? Svc.Objects.LocalPlayer.HitboxRadius : 0);
	}

	internal static float GetAttackRadius()
	{
		return P.currentProfile.MeleeAutoAtk + (P.currentProfile.MeleeAutoIncludeHitbox ? Svc.Objects.LocalPlayer.HitboxRadius : 0);
	}

	internal static CardinalDirection GetDirection(IGameObject bnpc)
	{
		return MathHelper.GetCardinalDirection(GetAngle(bnpc));
	}

	internal static float GetAngle(IGameObject bnpc)
	{
		return (MathHelper.GetRelativeAngle(Svc.Objects.LocalPlayer.Position, bnpc.Position) + bnpc.Rotation.RadToDeg()) % 360;
	}

	internal static bool IsPositionalJob()
	{
		return Svc.Objects.LocalPlayer?.ClassJob.RowId.EqualsAny(PositionalJobs) == true;
	}
}