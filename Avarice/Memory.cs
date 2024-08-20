using Avarice.Structs;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private void ReceiveActionEffectDetour(ActionEffectSet set)
        {
            try
            {
                if(set.Source?.Address == Svc.ClientState.LocalPlayer?.Address)
                {
                    var positionalState = PositionalState.Ignore;
                    if(P.PositionalManager.IsPositional((int)set.Header.ActionID))
                    {
                        positionalState = PositionalState.Failure;
                        foreach(var effect in set.TargetEffects)
                        {
                            effect.ForEach(entry =>
                            {
                                if(entry.type == ActionEffectType.Damage)
                                    if(P.PositionalManager.IsPositionalHit((int)set.Header.ActionID, entry.param2))
                                        positionalState = PositionalState.Success;
                            });
                        }
                    }
                    if(positionalState == PositionalState.Success)
                    {
                        if(P.currentProfile.EnableChatMessagesSuccess) Svc.Chat.Print("Positional HIT!");
                        if(P.currentProfile.EnableVFXSuccess) VfxEditorManager.DisplayVfx(true);
                        P.RecordStat(false);
                    }
                    else if(positionalState == PositionalState.Failure)
                    {
                        if(P.currentProfile.EnableChatMessagesFailure) Svc.Chat.Print("Positional MISS!");
                        if(P.currentProfile.EnableVFXFailure) VfxEditorManager.DisplayVfx(false);
                        P.RecordStat(true);
                    }
                    PluginLog.Debug($"Positional state: {positionalState}");
                }
            }
            catch(Exception e)
            {
                e.Log();
            }

        }

        public void Dispose()
        {
        }
    }
}
