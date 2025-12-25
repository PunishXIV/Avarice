namespace Avarice;

internal static class PositionalFeedbackManager
{
    internal static void TriggerFeedback(bool success)
    {
        var profile = P.currentProfile;
        if (profile == null) return;

        if (success && profile.EnableVFXSuccess)
            VisualFeedbackManager.DisplayFeedback(true);
        else if (!success && profile.EnableVFXFailure)
            VisualFeedbackManager.DisplayFeedback(false);

        if (success && profile.EnableAudioSuccess)
            AudioFeedbackManager.PlaySuccessSound();
        else if (!success && profile.EnableAudioFailure)
            AudioFeedbackManager.PlayFailureSound();

        if (success && profile.EnableChatMessagesSuccess)
            Svc.Chat?.Print("Positional HIT!");
        else if (!success && profile.EnableChatMessagesFailure)
            Svc.Chat?.Print("Positional MISS!");

        P.RecordStat(!success);
    }

    internal static void TestFeedback(bool success)
    {
        var profile = P.currentProfile;
        if (profile == null) return;

        if (success && profile.EnableVFXSuccess)
            VisualFeedbackManager.DisplayFeedback(true);
        else if (!success && profile.EnableVFXFailure)
            VisualFeedbackManager.DisplayFeedback(false);

        if (success && profile.EnableAudioSuccess)
            AudioFeedbackManager.PlaySuccessSound();
        else if (!success && profile.EnableAudioFailure)
            AudioFeedbackManager.PlayFailureSound();
    }
}
