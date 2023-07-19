using Avarice.Structs;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
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
        internal uint* LastComboMove;

        internal Memory()
        {
            SignatureHelper.Initialise(this);
            LastComboMove = (uint*)(Svc.SigScanner.GetStaticAddressFromSig("F3 0F 11 05 ?? ?? ?? ?? 48 83 C7 08") + 0x4);
            ActionEffect.ActionEffectEvent += ReceiveActionEffectDetour;
        }

        void ReceiveActionEffectDetour(ActionEffectSet set)
        {
            try
            {
                if (set.Source?.Address == Svc.ClientState.LocalPlayer?.Address)
                {
                    var positionalState = PositionalState.Ignore;
                    if (Avarice.PositionalData.TryGetValue(set.Header.AnimationId, out var actionPosData))
                    {
                        positionalState = PositionalState.Failure;
                        foreach (var effect in set.TargetEffects) 
                        {
                            effect.ForEach(entry =>
                            {
                                if (entry.type == ActionEffectType.Damage)
                                    if (actionPosData.Contains(entry.param2))
                                        positionalState = PositionalState.Success;
                            });
                        }
                    }
                    if (positionalState == PositionalState.Success)
                    {
                        if (P.currentProfile.EnableChatMessagesSuccess) Svc.Chat.Print("Positional HIT!");
                        if (P.currentProfile.EnableVFXSuccess) VfxEditorManager.DisplayVfx(true);
                        P.RecordStat(false);
                    }
                    else if (positionalState == PositionalState.Failure)
                    {
                        if (P.currentProfile.EnableChatMessagesFailure) Svc.Chat.Print("Positional MISS!");
                        if (P.currentProfile.EnableVFXFailure) VfxEditorManager.DisplayVfx(false);
                        P.RecordStat(true);
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
