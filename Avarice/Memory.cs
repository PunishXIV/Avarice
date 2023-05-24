using Avarice.Structs;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
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

        delegate void ReceiveActionEffect(uint sourceId, Character* sourceCharacter, IntPtr pos, EffectHeader* effectHeader, EffectEntry* effectArray, ulong* effectTail);
        [Signature("4C 89 44 24 ?? 55 56 41 54 41 55 41 56", Fallibility = Fallibility.Infallible, DetourName = nameof(ReceiveActionEffectDetour))]
        Hook<ReceiveActionEffect> ReceiveActionEffectHook;

        internal Memory()
        {
            SignatureHelper.Initialise(this);
            LastComboMove = (uint*)(Svc.SigScanner.GetStaticAddressFromSig("F3 0F 11 05 ?? ?? ?? ?? 48 83 C7 08") + 0x4);
            ReceiveActionEffectHook.Enable();
        }

        void ReceiveActionEffectDetour(uint sourceId, Character* sourceCharacter, IntPtr pos, EffectHeader* effectHeader, EffectEntry* effectArray, ulong* effectTail)
        {
            try
            {
                if (sourceId == Svc.ClientState.LocalPlayer?.ObjectId)
                {
                    var entryCount = effectHeader->TargetCount switch
                    {
                        0 => 0,
                        1 => 8,
                        <= 8 => 64,
                        <= 16 => 128,
                        <= 24 => 192,
                        <= 32 => 256,
                        _ => 0
                    };

                    var positionalState = PositionalState.Ignore;
                    if (Avarice.PositionalData.TryGetValue(effectHeader->AnimationId, out var actionPosData))
                    {
                        positionalState = PositionalState.Failure;
                        for (int i = 0; i < entryCount; i++)
                            if (effectArray[i].type == ActionEffectType.Damage)
                                if (actionPosData.Contains(effectArray[i].param2))
                                    positionalState = PositionalState.Success;
                    }
                    if (positionalState == PositionalState.Success)
                    {
                        if (P.currentProfile.EnableChatMessages) Svc.Chat.Print("Positional HIT!");
                        if (P.currentProfile.EnableVFX) VfxEditorManager.DisplayVfx(true);
                        P.RecordStat(false);
                    }
                    else if (positionalState == PositionalState.Failure)
                    {
                        if (P.currentProfile.EnableChatMessages) Svc.Chat.Print("Positional MISS!");
                        if (P.currentProfile.EnableVFX) VfxEditorManager.DisplayVfx(false);
                        P.RecordStat(true);
                    }
                    PluginLog.Debug($"Positional state: {positionalState}");
                }
            }
            catch (Exception e)
            {
                e.Log();
            }

            ReceiveActionEffectHook.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTail);
        }

        public void Dispose()
        {
            Safe(ReceiveActionEffectHook.Disable);
            Safe(ReceiveActionEffectHook.Dispose);
        }
    }
}
