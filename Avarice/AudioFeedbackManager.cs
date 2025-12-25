using FFXIVClientStructs.FFXIV.Client.UI;

namespace Avarice;

internal static class AudioFeedbackManager
{
    public const uint DefaultSuccessSound = 2;
    public const uint DefaultFailureSound = 6;

    private static uint ToGameSoundId(uint userSoundId) => userSoundId + 36;

    internal static unsafe void PlaySound(uint userSoundId)
    {
        if (userSoundId == 0 || userSoundId > 16) return;

        try
        {
            UIGlobals.PlaySoundEffect(ToGameSoundId(userSoundId));
        }
        catch (Exception e)
        {
            PluginLog.Error($"Failed to play sound effect: {e.Message}");
        }
    }

    internal static void PlaySuccessSound()
    {
        var soundId = P.config.AudioFeedbackSettings?.SuccessSoundId ?? DefaultSuccessSound;
        PlaySound(soundId);
    }

    internal static void PlayFailureSound()
    {
        var soundId = P.config.AudioFeedbackSettings?.FailureSoundId ?? DefaultFailureSound;
        PlaySound(soundId);
    }
}
