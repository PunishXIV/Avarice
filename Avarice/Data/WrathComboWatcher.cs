using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Ipc;

namespace Avarice.Data;

internal sealed class WrathComboWatcher : IDisposable
{
    private const string HintGate = "WrathCombo.GetUpcomingPositionalHint";
    private const string HintChangedGate = "OnUpcomingPositionalHint";
    private const int HintFieldCount = 7;
    private const int PollIntervalMs = 50;
    private const int FailedPollIntervalMs = 250;

    private readonly ICallGateSubscriber<uint[]> getHintSubscriber;
    private readonly ICallGateSubscriber<object> hintChangedSubscriber;

    private WrathComboPositionalHint currentHint = WrathComboPositionalHint.Empty;
    private long nextPollTick;
    private string lastLogKey = "";
    private string lastReject = "";

    internal WrathComboWatcher()
    {
        getHintSubscriber = Svc.PluginInterface.GetIpcSubscriber<uint[]>(HintGate);
        hintChangedSubscriber = Svc.PluginInterface.GetIpcSubscriber<object>(HintChangedGate);

        try
        {
            hintChangedSubscriber.Subscribe(OnHintChanged);
            Log($"Subscribed to {HintChangedGate}. Wrath installed={PluginInstalled}");
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            PluginLog.Debug($"Unable to subscribe to WrathCombo positional hints: {ex.Message}");
            Log($"Subscribe failed: {ex.Message}");
        }

        Refresh();
    }

    internal bool Available { get; private set; }

    internal string LastError { get; private set; } = "";

    internal string LastWire { get; private set; } = "";

    internal bool PluginInstalled =>
        Svc.PluginInterface.InstalledPlugins.Any(p =>
            p.InternalName.Equals("WrathCombo", StringComparison.OrdinalIgnoreCase));

    internal WrathComboPositionalHint CurrentHint => currentHint;

    internal void Tick()
    {
        var now = Environment.TickCount64;

        if (currentHint.IsExpired(now))
        {
            currentHint = WrathComboPositionalHint.Empty;
            LogChange("hint expired");
        }

        if (now < nextPollTick)
            return;

        nextPollTick = now + (Available ? PollIntervalMs : FailedPollIntervalMs);
        Refresh();
    }

    internal bool TryGetHintForTarget(IBattleNpc target, out WrathComboPositionalDirection direction)
    {
        direction = WrathComboPositionalDirection.None;
        var now = Environment.TickCount64;

        if (!currentHint.IsActive(now))
        {
            RememberReject(Available ? "hint inactive or empty" : "Wrath IPC not available");
            return false;
        }

        if ((uint)currentHint.TargetObjectId != (uint)target.GameObjectId)
        {
            RememberReject($"target mismatch hint={currentHint.TargetObjectId} current={target.GameObjectId}");
            return false;
        }

        direction = currentHint.Direction;
        if (direction is WrathComboPositionalDirection.Rear or WrathComboPositionalDirection.Flank)
        {
            RememberReject("");
            return true;
        }

        RememberReject($"direction {direction} is not rear/flank");
        return false;
    }

    public void Dispose()
    {
        try
        {
            hintChangedSubscriber.Unsubscribe(OnHintChanged);
        }
        catch (Exception ex)
        {
            PluginLog.Debug($"Unable to unsubscribe from WrathCombo positional hints: {ex.Message}");
        }
    }

    private void OnHintChanged()
    {
        Refresh();
    }

    private void Refresh()
    {
        try
        {
            var wire = getHintSubscriber.InvokeFunc();
            Available = true;
            LastError = "";
            LastWire = FormatWire(wire);

            currentHint = TryParse(wire, out var hint)
                ? hint
                : WrathComboPositionalHint.Empty;

            if (wire is null)
                LogChange("IPC returned null (no hint)");
            else if (currentHint.Direction is WrathComboPositionalDirection.None)
                LogChange($"IPC wire ignored: {LastWire}");
            else
                LogChange("IPC hint");
        }
        catch (Exception ex)
        {
            Available = false;
            LastError = ex.Message;
            LastWire = "";
            currentHint = WrathComboPositionalHint.Empty;
            LogChange($"IPC invoke failed: {ex.Message}");
        }
    }

    private static string FormatWire(uint[] wire)
    {
        if (wire is null)
            return "null";
        return wire.Length == 0 ? "[]" : string.Join(",", wire);
    }

    private void RememberReject(string reason)
    {
        if (reason == lastReject)
            return;
        lastReject = reason;
        if (reason.Length > 0)
            Log($"Hint not used: {reason}");
    }

    private void LogChange(string reason)
    {
        var key = $"{Available}|{LastError}|{currentHint.Direction}|{currentHint.ActionId}|{currentHint.GcdsUntil}|{currentHint.TargetObjectId}|{currentHint.IsSatisfied}|{LastWire}";
        if (key == lastLogKey)
            return;
        lastLogKey = key;
        Log($"{reason}: installed={PluginInstalled} available={Available} dir={currentHint.Direction} action={currentHint.ActionId} gcds={currentHint.GcdsUntil} target={currentHint.TargetObjectId} satisfied={currentHint.IsSatisfied} wire=[{LastWire}]");
    }

    private static void Log(string message)
    {
        PluginLog.Debug($"[WrathHint] {message}");
        if (P.currentProfile?.Debug == true)
            PluginLog.Information($"[WrathHint] {message}");
    }

    private static bool TryParse(uint[] wire, out WrathComboPositionalHint hint)
    {
        hint = WrathComboPositionalHint.Empty;

        if (wire is null || wire.Length < HintFieldCount)
            return false;

        var direction = (WrathComboPositionalDirection)wire[0];
        var expiresInMs = (int)wire[4];

        if (direction is WrathComboPositionalDirection.None or WrathComboPositionalDirection.Unknown ||
            wire[1] is 0 ||
            wire[2] is 0 ||
            expiresInMs <= 0)
            return false;

        hint = new WrathComboPositionalHint
        {
            Direction = direction,
            ActionId = wire[1],
            GcdsUntil = (int)wire[2],
            TargetObjectId = wire[3],
            ExpiresAtTick = Environment.TickCount64 + expiresInMs,
            CurrentAngle = wire[5],
            IsSatisfied = wire[6] is not 0,
        };

        return true;
    }
}

internal enum WrathComboPositionalDirection : uint
{
    None = 0,
    Rear = 1,
    Flank = 2,
    Unknown = 3,
}

internal readonly struct WrathComboPositionalHint
{
    internal static WrathComboPositionalHint Empty => new();

    internal WrathComboPositionalDirection Direction { get; init; }
    internal uint ActionId { get; init; }
    internal int GcdsUntil { get; init; }
    internal ulong TargetObjectId { get; init; }
    internal long ExpiresAtTick { get; init; }
    internal uint CurrentAngle { get; init; }
    internal bool IsSatisfied { get; init; }

    internal bool IsActive(long now) =>
        Direction is WrathComboPositionalDirection.Rear or WrathComboPositionalDirection.Flank &&
        ActionId is not 0 &&
        GcdsUntil > 0 &&
        ExpiresAtTick > now;

    internal bool IsExpired(long now) =>
        Direction is not WrathComboPositionalDirection.None && ExpiresAtTick <= now;
}
