using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using System.Collections.Concurrent;
using DalamudStatus = Dalamud.Game.ClientState.Statuses;

namespace Avarice.Data
{
	internal partial class ComboCache : IDisposable
	{
		private const uint InvalidObjectID = 0xE000_0000;

		private readonly ConcurrentDictionary<(uint StatusID, ulong? TargetID, ulong? SourceID), DalamudStatus.Status> statusCache = new();
		private readonly ConcurrentDictionary<uint, CooldownData> cooldownCache = new();

		public ComboCache()
		{
			Svc.Framework.Update += Framework_Update;
		}

		private delegate IntPtr GetActionCooldownSlotDelegate(IntPtr actionManager, int cooldownGroup);

		public void Dispose()
		{
			Svc.Framework.Update -= Framework_Update;
		}

		internal DalamudStatus.Status GetStatus(uint statusID, IGameObject obj, ulong? sourceID)
		{
			(uint statusID, ulong? GameObjectId, ulong? sourceID) key = (statusID, obj?.GameObjectId, sourceID);
			if (statusCache.TryGetValue(key, out DalamudStatus.Status found))
			{
				return found;
			}

			if (obj is null)
			{
				return statusCache[key] = null;
			}

			if (obj is not IBattleChara chara)
			{
				return statusCache[key] = null;
			}

			foreach (DalamudStatus.Status status in chara.StatusList)
			{
				if (status.StatusId == statusID && (!sourceID.HasValue || status.SourceId == 0 || status.SourceId == InvalidObjectID || status.SourceId == sourceID))
				{
					return statusCache[key] = status;
				}
			}

			return statusCache[key] = null;
		}

		internal unsafe CooldownData GetCooldown(uint actionID)
		{
			if (cooldownCache.TryGetValue(actionID, out CooldownData found))
			{
				return found!;
			}

			CooldownData data = new()
			{
				ActionID = actionID,
			};

			return cooldownCache[actionID] = data;
		}

		internal static ComboCache ComboCacheInstance { get; set; } = null!;

		private unsafe void Framework_Update(IFramework framework)
		{
			statusCache.Clear();
		}
	}
}