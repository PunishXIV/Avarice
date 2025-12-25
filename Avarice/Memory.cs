using Avarice.Structs;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Avarice
{
    internal unsafe class Memory
    {
        internal uint LastComboMove => ActionManager.Instance()->Combo.Action;

        internal Memory()
        {
            SignatureHelper.Initialise(this);
            ActionEffect.ActionEffectEvent += ReceiveActionEffectDetour;
        }

        void ReceiveActionEffectDetour(ActionEffectSet set)
        {
            try
            {
                if (set.Source?.Address == Svc.Objects.LocalPlayer?.Address)
                {
                    var positionalState = PositionalState.Ignore;
                    if (P.PositionalManager?.IsPositional((int)set.Header.ActionID) == true)
                    {
                        positionalState = PositionalState.Failure;
                        if (set.TargetEffects != null)
                        {
                            foreach (var effect in set.TargetEffects)
                            {
                                effect.ForEach(entry =>
                                {
                                    if (entry.type == ActionEffectType.Damage)
                                        if (P.PositionalManager?.IsPositionalHit((int)set.Header.ActionID, entry.param2) == true)
                                            positionalState = PositionalState.Success;
                                });
                            }
                        }
                    }
                    if (positionalState == PositionalState.Success)
                    {
                        PositionalFeedbackManager.TriggerFeedback(true);
                    }
                    else if (positionalState == PositionalState.Failure)
                    {
                        PositionalFeedbackManager.TriggerFeedback(false);
                    }
                    PluginLog.Debug($"Positional state: {positionalState}");
                }
            }
            catch (Exception e)
            {
                e.Log();
            }
        }

        public void Dispose()
        {
        }
    }
}
