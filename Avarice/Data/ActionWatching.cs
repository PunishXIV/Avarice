using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using ECommons.GameFunctions;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;

namespace Avarice.Data
{
	public unsafe static class ActionWatching
	{
		internal static Dictionary<uint, Lumina.Excel.Sheets.Action> ActionSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>()!
			.Where(i => i.RowId is not 7)
			.ToDictionary(i => i.RowId, i => i);

		internal static Dictionary<uint, Lumina.Excel.Sheets.Status> StatusSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>()!
			.ToDictionary(i => i.RowId, i => i);

		internal static Dictionary<uint, Trait> TraitSheet = Svc.Data.GetExcelSheet<Trait>()!
			.Where(i => i.ClassJobCategory.ValueNullable != null) //All player traits are assigned to a category. Chocobo and other garbage lacks this, thus excluded.
			.ToDictionary(i => i.RowId, i => i);

		internal static Dictionary<uint, BNpcBase> BNpcSheet = Svc.Data.GetExcelSheet<BNpcBase>()!
			.ToDictionary(i => i.RowId, i => i);

		private static readonly Dictionary<string, List<uint>> statusCache = [];

		internal static readonly List<uint> CombatActions = [];

		private static readonly Hook<ActionEffect.ProcessActionEffect> ReceiveActionEffectHook;

		private static void ReceiveActionEffectDetour(uint sourceId, Character* sourceCharacter, Vector3* pos, EffectHeader* effectHeader, EffectEntry* effectArray, ulong* effectTail)
		{
			if (!Svc.Condition[ConditionFlag.InCombat])
			{
				CombatActions.Clear();
			}

			ReceiveActionEffectHook!.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTail);
			ActionEffectHeader header = Marshal.PtrToStructure<ActionEffectHeader>((nint)effectHeader);

			if (ActionType is 13 or 2)
			{
				return;
			}

			if (header.ActionId != 7 &&
				header.ActionId != 8 &&
        sourceId == Svc.ClientState.LocalPlayer.EntityId)
			{
				TimeLastActionUsed = DateTime.Now;
				LastActionUseCount++;
				if (header.ActionId != LastAction)
				{
					LastActionUseCount = 1;
				}

				LastAction = header.ActionId;

				if (ActionSheet.TryGetValue(header.ActionId, out Lumina.Excel.Sheets.Action sheet))
				{
					switch (sheet.ActionCategory.Value.RowId)
					{
						case 2: //Spell
							LastSpell = header.ActionId;
							break;
						case 3: //Weaponskill
							LastWeaponskill = header.ActionId;
							break;
						case 4: //Ability
							LastAbility = header.ActionId;
							break;
					}
				}

				CombatActions.Add(header.ActionId);
			}
		}

		private delegate void SendActionDelegate(ulong targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9);
		private static readonly Hook<SendActionDelegate> SendActionHook;
		private static unsafe void SendActionDetour(ulong targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9)
		{
			try
			{
				SendActionHook!.Original(targetObjectId, actionType, actionId, sequence, a5, a6, a7, a8, a9);
				TimeLastActionUsed = DateTime.Now;
				ActionType = actionType;

				//Dalamud.Logging.PluginLog.Debug($"{actionId} {sequence} {a5} {a6} {a7} {a8} {a9}");
			}
			catch (Exception ex)
			{
				Svc.Log.Error(ex, "SendActionDetour");
				SendActionHook!.Original(targetObjectId, actionType, actionId, sequence, a5, a6, a7, a8, a9);
			}
		}

		public static unsafe bool OutOfRange(uint actionId, IGameObject source, IGameObject target)
		{
			return ActionManager.GetActionInRangeOrLoS(actionId, source.Struct(), target.Struct()) is 566;
		}

		public static uint WhichOfTheseActionsWasLast(params uint[] actions)
		{
			if (CombatActions.Count == 0)
			{
				return 0;
			}

			int currentLastIndex = 0;
			foreach (uint action in actions)
			{
				if (CombatActions.Any(x => x == action))
				{
					int index = CombatActions.LastIndexOf(action);

					if (index > currentLastIndex)
					{
						currentLastIndex = index;
					}
				}
			}

			return CombatActions[currentLastIndex];
		}

		public static int HowManyTimesUsedAfterAnotherAction(uint lastUsedIDToCheck, uint idToCheckAgainst)
		{
			if (CombatActions.Count < 2)
			{
				return 0;
			}

			if (WhichOfTheseActionsWasLast(lastUsedIDToCheck, idToCheckAgainst) != lastUsedIDToCheck)
			{
				return 0;
			}

			int startingIndex = CombatActions.LastIndexOf(idToCheckAgainst);
			if (startingIndex == -1)
			{
				return 0;
			}

			int count = 0;
			for (int i = startingIndex + 1; i < CombatActions.Count; i++)
			{
				if (CombatActions[i] == lastUsedIDToCheck)
				{
					count++;
				}
			}

			return count;
		}

		public static int NumberOfGcdsUsed
		{
			get
			{
				return CombatActions.Count(x => GetAttackType(x) is ActionAttackType.Weaponskill or ActionAttackType.Spell);
			}
		}

		public static bool HasDoubleWeaved()
		{
			if (CombatActions.Count < 2)
			{
				return false;
			}

			uint lastAction = CombatActions.Last();
			uint secondLastAction = CombatActions[^2];

			return GetAttackType(lastAction) == GetAttackType(secondLastAction) && GetAttackType(lastAction) == ActionAttackType.Ability;
		}

		public static uint LastAction { get; set; } = 0;
		public static int LastActionUseCount { get; set; } = 0;
		public static uint ActionType { get; set; } = 0;
		public static uint LastWeaponskill { get; set; } = 0;
		public static uint LastAbility { get; set; } = 0;
		public static uint LastSpell { get; set; } = 0;

		public static TimeSpan TimeSinceLastAction
		{
			get
			{
				return DateTime.Now - TimeLastActionUsed;
			}
		}

		private static DateTime TimeLastActionUsed { get; set; } = DateTime.Now;

		public static void Dispose()
		{
			ReceiveActionEffectHook?.Dispose();
			SendActionHook?.Dispose();
		}

		static unsafe ActionWatching()
		{
      ReceiveActionEffectHook ??= Svc.Hook.HookFromSignature<ActionEffect.ProcessActionEffect>(ActionEffect.Sig, ReceiveActionEffectDetour);
			SendActionHook ??= Svc.Hook.HookFromSignature<SendActionDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B E9 41 0F B7 D9", SendActionDetour);
		}


		public static void Enable()
		{
			ReceiveActionEffectHook?.Enable();
			SendActionHook?.Enable();
			Svc.Condition.ConditionChange += ResetActions;
		}

		private static void ResetActions(ConditionFlag flag, bool value)
		{
			if (flag == ConditionFlag.InCombat && !value)
			{
				CombatActions.Clear();
				LastAbility = 0;
				LastAction = 0;
				LastWeaponskill = 0;
				LastSpell = 0;
			}
		}

		public static void Disable()
		{
			ReceiveActionEffectHook.Disable();
			SendActionHook?.Disable();
			Svc.Condition.ConditionChange -= ResetActions;
		}

		public static int GetLevel(uint id)
		{
			return ActionSheet.TryGetValue(id, out Lumina.Excel.Sheets.Action action) && action.ClassJobCategory.ValueNullable != null ? action.ClassJobLevel : 255;
		}

		public static float GetActionCastTime(uint id)
		{
			return ActionSheet.TryGetValue(id, out Lumina.Excel.Sheets.Action action) ? action.Cast100ms / (float)10 : 0;
		}

		public static int GetActionRange(uint id)
		{
			return ActionSheet.TryGetValue(id, out Lumina.Excel.Sheets.Action action) ? action.Range : -2; // 0 & -1 are valid numbers. -2 is our failure code for InActionRange
		}

		public static int GetActionEffectRange(uint id)
		{
			return ActionSheet.TryGetValue(id, out Lumina.Excel.Sheets.Action action) ? action.EffectRange : -1;
		}

		public static int GetTraitLevel(uint id)
		{
			return TraitSheet.TryGetValue(id, out Trait trait) ? trait.Level : 255;
		}

		public static string GetActionName(uint id)
		{
			return ActionSheet.TryGetValue(id, out Lumina.Excel.Sheets.Action action) ? (string)action.Name.ToString() : "UNKNOWN ABILITY";
		}

		public static string GetBLUIndex(uint id)
		{
			uint aozKey = Svc.Data.GetExcelSheet<AozAction>()!.First(x => x.Action.RowId == id).RowId;
			byte index = Svc.Data.GetExcelSheet<AozActionTransient>().GetRow(aozKey).Number;

			return $"#{index} ";
		}
		public static string GetStatusName(uint id)
		{
			return StatusSheet.TryGetValue(id, out Lumina.Excel.Sheets.Status status) ? (string)status.Name.ToString() : "Unknown Status";
		}

		public static List<uint> GetStatusesByName(string status)
		{
			return statusCache.TryGetValue(status, out List<uint> list)
				? list
				: statusCache.TryAdd(status, StatusSheet.Where(x => x.Value.Name.ToString().Equals(status, StringComparison.CurrentCultureIgnoreCase)).Select(x => x.Key).ToList())
				? statusCache[status]
				: null;
		}

		public static ActionAttackType GetAttackType(uint id)
		{
			return !ActionSheet.TryGetValue(id, out Lumina.Excel.Sheets.Action action)
				? ActionAttackType.Unknown
				: action.ActionCategory.RowId switch
				{
					2 => ActionAttackType.Spell,
					3 => ActionAttackType.Weaponskill,
					4 => ActionAttackType.Ability,
					_ => ActionAttackType.Unknown
				};
		}

		public enum ActionAttackType
		{
			Ability,
			Spell,
			Weaponskill,
			Unknown
		}
	}

	internal static unsafe class ActionManagerHelper
	{
		private static readonly nint actionMgrPtr;

		public static ushort CurrentSeq
		{
			get
			{
				return actionMgrPtr != nint.Zero ? (ushort)Marshal.ReadInt16(actionMgrPtr + 0x110) : (ushort)0;
			}
		}

		public static ushort LastRecievedSeq
		{
			get
			{
				return actionMgrPtr != nint.Zero ? (ushort)Marshal.ReadInt16(actionMgrPtr + 0x112) : (ushort)0;
			}
		}

		public static bool IsCasting
		{
			get
			{
				return actionMgrPtr != nint.Zero && Marshal.ReadByte(actionMgrPtr + 0x28) != 0;
			}
		}

		public static uint CastingActionId
		{
			get
			{
				return actionMgrPtr != nint.Zero ? (uint)Marshal.ReadInt32(actionMgrPtr + 0x24) : 0u;
			}
		}

		public static uint CastTargetObjectId
		{
			get
			{
				return actionMgrPtr != nint.Zero ? (uint)Marshal.ReadInt32(actionMgrPtr + 0x38) : 0u;
			}
		}

		static ActionManagerHelper()
		{
			actionMgrPtr = (nint)ActionManager.Instance();
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct ActionEffectHeader
	{
		[FieldOffset(0x0)] public long TargetObjectId;
		[FieldOffset(0x8)] public uint ActionId;
		[FieldOffset(0x14)] public uint UnkObjectId;
		[FieldOffset(0x18)] public ushort Sequence;
		[FieldOffset(0x1A)] public ushort Unk_1A;
	}
}