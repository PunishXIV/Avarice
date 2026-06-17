using Avarice.Structs;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

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
                    var actionId = (int)set.Header.ActionID;
                    var isPositional = P.PositionalManager?.IsPositional(actionId) == true;
                    var positionalState = isPositional ? PositionalState.Failure : PositionalState.Ignore;

                    var param2Parts = new List<string>();
                    var detailParts = new List<string>();
                    if (set.TargetEffects != null)
                    {
                        foreach (var effect in set.TargetEffects)
                        {
                            effect.ForEach(entry =>
                            {
                                if (entry.type == ActionEffectType.Damage)
                                {
                                    var matched = isPositional && P.PositionalManager?.IsPositionalHit(actionId, entry.param2) == true;
                                    if (matched)
                                        positionalState = PositionalState.Success;
                                    param2Parts.Add(matched ? $"{entry.param2}*" : $"{entry.param2}");
                                    detailParts.Add(entry.ToString());
                                }
                            });
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

                    if (isPositional || param2Parts.Count > 0)
                    {
                        PositionalDebugWindow.Record(new PositionalDebugWindow.Entry
                        {
                            Frame = Framework.Instance()->FrameCounter,
                            ActionId = set.Header.ActionID,
                            ActionName = set.Name,
                            InTable = isPositional,
                            TablePosition = P.PositionalManager?.GetActionPosition(actionId),
                            Param2Display = param2Parts.Count > 0 ? string.Join(", ", param2Parts) : "(no damage)",
                            Verdict = positionalState,
                            Target = set.Target?.Name.ToString() ?? "",
                            Detail = string.Join("\n", detailParts),
                        });
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
