using FFXIVClientStructs.FFXIV.Client.Game;

namespace Avarice.Data
{
    internal class CooldownData
    {
        public bool IsCooldown => CooldownRemaining > 0;

        public uint ActionID;

        private unsafe float CooldownElapsed =>
            ActionManager.Instance()->GetRecastTimeElapsed(ActionType.Action, ActionID);

        private unsafe float CooldownTotal =>
            ActionManager.GetAdjustedRecastTime(ActionType.Action, ActionID) / 1000f * MaxCharges;

        private unsafe float CooldownRemaining =>
            CooldownElapsed == 0 ? 0 : Math.Max(0, CooldownTotal - CooldownElapsed);

        private ushort MaxCharges => ActionManager.GetMaxCharges(ActionID, 0);
    }
}
