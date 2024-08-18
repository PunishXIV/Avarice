using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using System.Collections.Concurrent;
using DalamudStatus = Dalamud.Game.ClientState.Statuses;

namespace Avarice.Data
{
    internal partial class ComboCache : IDisposable
    {
        private const uint InvalidObjectID = 0xE000_0000;

        private readonly ConcurrentDictionary<(uint StatusID, ulong? TargetID, ulong? SourceID), DalamudStatus.Status?> statusCache = [];

        private readonly ConcurrentDictionary<Type, JobGaugeBase> jobGaugeCache = [];

        public ComboCache()
        {
            Svc.Framework.Update += Framework_Update;
        }

        private delegate IntPtr GetActionCooldownSlotDelegate(IntPtr actionManager, int cooldownGroup);

        public void Dispose()
        {
            Svc.Framework.Update -= Framework_Update;
        }

        internal T GetJobGauge<T>() where T : JobGaugeBase
        {
            if(!jobGaugeCache.TryGetValue(typeof(T), out var gauge))
            {
                gauge = jobGaugeCache[typeof(T)] = Svc.Gauges.Get<T>();
            }

            return (T)gauge;
        }

        internal DalamudStatus.Status GetStatus(uint statusID, IGameObject obj, ulong? sourceID)
        {
            var key = (statusID, obj?.GameObjectId, sourceID);
            if(statusCache.TryGetValue(key, out var found))
            {
                return found;
            }

            if(obj is null)
            {
                return statusCache[key] = null;
            }

            if(obj is not IBattleChara chara)
            {
                return statusCache[key] = null;
            }

            foreach(var status in chara.StatusList)
            {
                if(status.StatusId == statusID && (!sourceID.HasValue || status.SourceId == 0 || status.SourceId == InvalidObjectID || status.SourceId == sourceID))
                {
                    return statusCache[key] = status;
                }
            }

            return statusCache[key] = null;
        }

        internal static ComboCache ComboCacheInstance { get; set; } = null!;

        internal static unsafe int GetResourceCost(uint actionID)
        {
            var actionManager = ActionManager.Instance();
            if(actionManager == null)
            {
                return 0;
            }

            var cost = ActionManager.GetActionCost(ActionType.Action, actionID, 0, 0, 0, 0);

            return cost;
        }

        private unsafe void Framework_Update(IFramework framework)
        {
            statusCache.Clear();
        }
    }
}