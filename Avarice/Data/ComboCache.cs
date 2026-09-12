using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using System.Collections.Concurrent;

namespace Avarice.Data
{
	internal class ComboCache : IDisposable
	{
		private const uint InvalidObjectID = 0xE000_0000;

		private readonly ConcurrentDictionary<(uint StatusID, ulong? TargetID, ulong? SourceID), StatusInfo?> statusCache = new();
		private readonly ConcurrentDictionary<uint, CooldownData> cooldownCache = new();

		public ComboCache()
		{
			Svc.Framework.Update += Framework_Update;
		}

		public void Dispose()
		{
			Svc.Framework.Update -= Framework_Update;
		}

		internal StatusInfo? GetStatus(uint statusID, IGameObject obj, ulong? sourceID)
		{
			(uint statusID, ulong? GameObjectId, ulong? sourceID) key = (statusID, obj?.GameObjectId, sourceID);
			if (statusCache.TryGetValue(key, out StatusInfo? found))
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

			foreach (var status in chara.StatusList)
			{
				if (status.StatusId == statusID && (!sourceID.HasValue || status.SourceId == 0 || status.SourceId == InvalidObjectID || status.SourceId == sourceID))
				{
					return statusCache[key] = new StatusInfo
					{
						StatusId = status.StatusId,
						Param = status.Param,
						RemainingTime = status.RemainingTime,
						SourceId = status.SourceId
					};
				}
			}

			return statusCache[key] = null;
		}

		internal CooldownData GetCooldown(uint actionID)
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

		private void Framework_Update(IFramework framework)
		{
			statusCache.Clear();
		}
	}
}
