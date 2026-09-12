using Avarice.Data;
using Avarice.Positional;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.EzSharedDataManager;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Pictomancy;
using PunishLib;
using System.Threading;
using SysTask = System.Threading.Tasks.Task;
using SysValueTask = System.Threading.Tasks.ValueTask;

#pragma warning disable CS0649

namespace Avarice;

public class Avarice : IAsyncDalamudPlugin
{
    public string Name => "Avarice";

    private readonly IDalamudPluginInterface pluginInterface;
    private bool loaded;
    private bool servicesInited;

    internal Config config;
    internal Profile currentProfile;
    internal static Avarice P;
    internal WindowSystem windowSystem;
    internal ConfigWindow configWindow;
    private Canvas canvas;
    internal PositionalDebugWindow positionalDebugWindow;
    internal Memory memory;

    internal static uint[] PositionalJobs = new uint[] { 2, 4, 29, 30, 20, 34, 39, 22, 41 };
    internal uint Job = 0;
    internal HashSet<uint> StaticAutoDetectRadiusData;
    internal PositionalManager PositionalManager;
    internal uint[] PositionalStatus;
    internal RotationSolverWatcher RotationSolverWatcher;
    internal WrathComboWatcher WrathComboWatcher;

    public Avarice(IDalamudPluginInterface pi)
    {
        pluginInterface = pi;
    }

    public async SysTask LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector, Module.ObjectFunctions);
        PunishLibMain.Init(pluginInterface, Svc.PluginInterface.InternalName, PunishOption.DefaultKoFi);
        servicesInited = true;

        var configTask = SysTask.Run(() => Svc.PluginInterface.GetPluginConfig() as Config ?? new(), cancellationToken);
        var radiusTask = Util.LoadStaticAutoDetectRadiusDataAsync(cancellationToken);

        config = await configTask.ConfigureAwait(false);
        StaticAutoDetectRadiusData = await radiusTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        if (config.Profiles.Count == 0)
            config.Profiles.Add(new() { Name = "Default", IsDefault = true });
        foreach (var pr in config.Profiles)
            if (pr.IsDefault && pr.Name == "Default profile") pr.Name = "Default";
        currentProfile = config.Profiles.FirstOr0(x => x.IsDefault);

        PositionalStatus = EzSharedData.GetOrCreate<uint[]>("Avarice.PositionalStatus", [0, 0]);
        RotationSolverWatcher = new();
        WrathComboWatcher = new();
        memory = new();
        windowSystem = new();
        configWindow = new();
        windowSystem.AddWindow(configWindow);
        canvas = new();
        windowSystem.AddWindow(canvas);
        positionalDebugWindow = new();
        windowSystem.AddWindow(positionalDebugWindow);
        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;
        Svc.PluginInterface.UiBuilder.OpenMainUi += OpenConfigWindow;
        Svc.Condition.ConditionChange += OnConditionChange;
        _ = Svc.Commands.AddHandler("/avarice", new CommandInfo((_, args) =>
        {
            if (args == "debug")
            {
                P.currentProfile.Debug = !P.currentProfile.Debug;
                positionalDebugWindow.IsOpen = P.currentProfile.Debug;
                Svc.Chat.Print($"Debug mode {(P.currentProfile.Debug ? "enabled" : "disabled")}");
            }
            else if (args == "draw")
            {
                P.currentProfile.DrawingEnabled = !P.currentProfile.DrawingEnabled;
                Svc.Chat.Print($"Drawing {(P.currentProfile.DrawingEnabled ? "enabled" : "disabled")}");
            }
            else
            {
                configWindow.IsOpen = !configWindow.IsOpen;
            }
        })
        { HelpMessage = "Toggle configuration window. Use '/avarice draw' to toggle drawing, '/avarice debug' for debug mode." });
        LuminaSheets.Init();
        Svc.PluginInterface.GetIpcProvider<IntPtr, CardinalDirection>("Avarice.CardinalDirection").RegisterFunc(GetCardinalDirectionForObject);
        Svc.Framework.Update += Tick;
        if (config.SplatoonUnsafePixel)
            TabSplatoon.WriteRequest();

        ComboCache.ComboCacheInstance = new ComboCache();
        PositionalManager = new();
        PctService.Initialize(Svc.PluginInterface);
        loaded = true;
    }

    private CardinalDirection GetCardinalDirectionForObject(IntPtr arg)
    {
        var obj = Svc.Objects.CreateObjectReference(arg);
        if (obj != null && Svc.Objects.LocalPlayer != null)
        {
            return MathHelper.GetCardinalDirection((MathHelper.GetRelativeAngle(Svc.Objects.LocalPlayer.Position, obj.Position) + obj.Rotation.RadToDeg()) % 360);
        }
        else
        {
            return (CardinalDirection)(-1);
        }
    }

    private void OnConditionChange(ConditionFlag flag, bool value)
    {

        if (flag == ConditionFlag.InCombat)
        {
            Safe(delegate
            {
                if (value)
                {
                    PluginLog.Debug("Entered combat");
                }
                else
                {
                    PluginLog.Debug("Exited combat");
                    Svc.PluginInterface.SavePluginConfig(config);
                    if (currentProfile.Announce && !currentProfile.CurrentEncounterStats.Finished &&
              (currentProfile.CurrentEncounterStats.Hits > 0 || currentProfile.CurrentEncounterStats.Missed > 0))
                    {
                        var total = currentProfile.CurrentEncounterStats.Hits + currentProfile.CurrentEncounterStats.Missed;
                        var success = (int)(100f * currentProfile.CurrentEncounterStats.Hits / total);
                        Svc.Chat.Print(new SeStringBuilder()
                    .AddText($"Positionals summary for encounter: {currentProfile.CurrentEncounterStats.Hits}/{total} - ")
                    .AddUiForeground($"{success}%", Util.GetParsedSeStringColor(success))
                    .Build());
                    }
                    currentProfile.CurrentEncounterStats.Finished = true;
                }
            });
        }
    }

    internal static bool IsConditionMatching(DisplayCondition c)
    {
        if (c == DisplayCondition.Only_in_combat)
        {
            return Svc.Condition[ConditionFlag.InCombat];
        }
        else if (c == DisplayCondition.Only_in_duty)
        {
            return Svc.Condition[ConditionFlag.BoundByDuty56];
        }
        else if (c == DisplayCondition.In_duty_or_combat)
        {
            return Svc.Condition[ConditionFlag.InCombat] || Svc.Condition[ConditionFlag.BoundByDuty56];
        }
        else if (c == DisplayCondition.In_duty_and_combat)
        {
            return Svc.Condition[ConditionFlag.InCombat] && Svc.Condition[ConditionFlag.BoundByDuty56];
        }
        else
        {
            return true;
        }
    }

    internal void RecordStat(bool isMiss)
    {
        if (currentProfile.CurrentEncounterStats.Finished)
        {
            currentProfile.CurrentEncounterStats = new();
        }
        if (!currentProfile.Stats.ContainsKey((uint)Player.Job))
        {
            currentProfile.Stats[(uint)Player.Job] = new();
        }
        if (isMiss)
        {
            currentProfile.Stats[(uint)Player.Job].Missed++;
            currentProfile.CurrentEncounterStats.Missed++;
        }
        else
        {
            currentProfile.Stats[(uint)Player.Job].Hits++;
            currentProfile.CurrentEncounterStats.Hits++;
        }
    }

    private void OpenConfigWindow()
    {
        if (configWindow != null)
            configWindow.IsOpen = true;
    }

    internal Profile GetProfileForJob(uint job)
    {
        if (P.config.JobProfiles.TryGetValue(job, out var guid))
        {
            if (P.config.Profiles.TryGetFirst(x => x.GUID == guid, out var profile))
            {
                return profile;
            }
        }
        return null;
    }

    public SysValueTask DisposeAsync()
    {
        if (loaded)
        {
            Safe(() => Svc.PluginInterface.SavePluginConfig(config));
            Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;
            Svc.PluginInterface.UiBuilder.OpenMainUi -= OpenConfigWindow;
            _ = Svc.Commands.RemoveHandler("/avarice");
            Svc.Condition.ConditionChange -= OnConditionChange;
            Svc.Framework.Update -= Tick;
            Safe(() =>
            {
                Svc.PluginInterface.GetIpcProvider<IntPtr, CardinalDirection>("Avarice.CardinalDirection").UnregisterFunc();
            });
            memory?.Dispose();
            ComboCache.ComboCacheInstance?.Dispose();
            WrathComboWatcher?.Dispose();
            VisualFeedbackManager.Dispose();
            PctService.Dispose();
            RotationSolverWatcher?.Dispose();
        }
        if (servicesInited)
        {
            PunishLibMain.Dispose();
            ECommonsMain.Dispose();
        }
        P = null;
        return SysValueTask.CompletedTask;
    }

    private void Tick(object framework)
    {
        WrathComboWatcher?.Tick();

        unsafe
        {
            if (Framework.Instance()->FrameCounter - PositionalStatus[0] > 1)
                PositionalStatus[1] = 0;
        }
        if (Svc.Objects.LocalPlayer != null)
        {
            var newJob = (uint)Player.Job;
            if (newJob != Job)
            {
                PluginLog.Debug($"Job changed from {Job} to {newJob}");
                var newJobProfile = GetProfileForJob(newJob);
                if (newJobProfile != null)
                {
                    currentProfile = newJobProfile;
                    PluginLog.Debug($"Switched profile to job profile {newJobProfile.Name}");
                }
                else
                {
                    if (GetProfileForJob(Job) != null)
                    {
                        currentProfile = P.config.Profiles.FirstOr0(x => x.IsDefault);
                        PluginLog.Debug($"Switched profile to default {currentProfile.Name}");
                    }
                }
            }
            Job = newJob;
        }
    }
}