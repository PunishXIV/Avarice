using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;

namespace Avarice.Data
{
	internal abstract partial class CustomComboFunctions
	{
		public static bool HasEffect(ushort effectID)
		{
			return FindEffect(effectID) is not null;
		}

		public static ushort GetBuffStacks(ushort effectId)
		{
			Status eff = FindEffect(effectId);
			return eff?.Param ?? 0;
		}

		public static float GetBuffRemainingTime(ushort effectId)
		{
			Status eff = FindEffect(effectId);
			return eff?.RemainingTime ?? 0;
		}

		public static Status FindEffect(ushort effectID)
		{
			return FindEffect(effectID, Svc.ClientState.LocalPlayer, Svc.ClientState.LocalPlayer?.GameObjectId);
		}

		public static bool TargetHasEffect(ushort effectID)
		{
			return FindTargetEffect(effectID) is not null;
		}

		public static Status FindTargetEffect(ushort effectID)
		{
			return FindEffect(effectID, Svc.Targets.Target, Svc.ClientState.LocalPlayer?.GameObjectId);
		}

		public static float GetDebuffRemainingTime(ushort effectId)
		{
			Status eff = FindTargetEffect(effectId);
			return eff?.RemainingTime ?? 0;
		}

		public static bool HasEffectAny(ushort effectID)
		{
			return FindEffectAny(effectID) is not null;
		}

		public static Status FindEffectAny(ushort effectID)
		{
			return FindEffect(effectID, Svc.ClientState.LocalPlayer, null);
		}

		public static bool TargetHasEffectAny(ushort effectID)
		{
			return FindTargetEffectAny(effectID) is not null;
		}

		public static Status FindTargetEffectAny(ushort effectID)
		{
			return FindEffect(effectID, Svc.Targets.Target, null);
		}

		public static Status FindEffect(ushort effectID, IGameObject obj, ulong? sourceID)
		{
			return ComboCache.ComboCacheInstance.GetStatus(effectID, obj, sourceID);
		}

		public static Status FindEffectOnMember(ushort effectID, IGameObject obj)
		{
			return ComboCache.ComboCacheInstance.GetStatus(effectID, obj, null);
		}

		public static string GetStatusName(uint id)
		{
			return ActionWatching.GetStatusName(id);
		}

		public static bool HasSilence()
		{
			foreach (uint status in ActionWatching.GetStatusesByName(ActionWatching.GetStatusName(7)))
			{
				if (HasEffectAny((ushort)status))
				{
					return true;
				}
			}

			return false;
		}

		public static bool HasPacification()
		{
			foreach (uint status in ActionWatching.GetStatusesByName(ActionWatching.GetStatusName(6)))
			{
				if (HasEffectAny((ushort)status))
				{
					return true;
				}
			}

			return false;
		}

		public static bool HasAmnesia()
		{
			foreach (uint status in ActionWatching.GetStatusesByName(ActionWatching.GetStatusName(5)))
			{
				if (HasEffectAny((ushort)status))
				{
					return true;
				}
			}

			return false;
		}

		public static bool TargetHasDamageDown(IGameObject target)
		{
			foreach (uint status in ActionWatching.GetStatusesByName(GetStatusName(62)))
			{
				if (FindEffectOnMember((ushort)status, target) is not null)
				{
					return true;
				}
			}

			return false;
		}

		public static bool TargetHasRezWeakness(IGameObject target)
		{
			foreach (uint status in ActionWatching.GetStatusesByName(GetStatusName(43)))
			{
				if (FindEffectOnMember((ushort)status, target) is not null)
				{
					return true;
				}
			}
			foreach (uint status in ActionWatching.GetStatusesByName(GetStatusName(44)))
			{
				if (FindEffectOnMember((ushort)status, target) is not null)
				{
					return true;
				}
			}

			return false;
		}

		public static bool NoBlockingStatuses(uint actionId)
		{
			switch (ActionWatching.GetAttackType(actionId))
			{
				case ActionWatching.ActionAttackType.Weaponskill:
					if (HasPacification())
					{
						return false;
					}

					return true;
				case ActionWatching.ActionAttackType.Spell:
					if (HasSilence())
					{
						return false;
					}

					return true;
				case ActionWatching.ActionAttackType.Ability:
					if (HasAmnesia())
					{
						return false;
					}

					return true;

			}

			return true;
		}
	}
}