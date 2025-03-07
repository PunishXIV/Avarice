using Avarice.Data;
using Avarice.Positional;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.EzSharedDataManager;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.Schedulers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using PunishLib;

#pragma warning disable CS0649

namespace Avarice;

public unsafe class Avarice : IDalamudPlugin
{
    public string Name
    {
        get
        {
            return "Avarice";
        }
    }

    internal Config config;
    internal Profile currentProfile;
    internal static Avarice P;
    private WindowSystem windowSystem;
    internal ConfigWindow configWindow;
    private Canvas canvas;
    internal Memory memory;

    internal static uint[] PositionalJobs = new uint[] { 2, 4, 29, 30, 20, 34, 39, 22 };
    internal uint Job = 0;
    internal HashSet<uint> StaticAutoDetectRadiusData;
    internal PositionalManager PositionalManager;
    internal uint[] PositionalStatus;
    internal RotationSolverWatcher RotationSolverWatcher;

    public Avarice(IDalamudPluginInterface pi)
    {
        P = this;
        ECommonsMain.Init(pi, this, Module.DalamudReflector, Module.ObjectFunctions);
        PunishLibMain.Init(pi, Svc.PluginInterface.InternalName, PunishOption.DefaultKoFi);
        _ = new TickScheduler(delegate
        {
            PositionalStatus = EzSharedData.GetOrCreate<uint[]>("Avarice.PositionalStatus", [0, 0]);
            config = Svc.PluginInterface.GetPluginConfig() as Config ?? new();
            if (config.Profiles.Count == 0)
            {
                config.Profiles.Add(new() { Name = "Default profile", IsDefault = true });
            }
            currentProfile = config.Profiles.FirstOr0(x => x.IsDefault);
            //Svc.GameNetwork.NetworkMessage += OnNetworkMessage;
            RotationSolverWatcher = new();
            memory = new();
            windowSystem = new();
            configWindow = new();
            windowSystem.AddWindow(configWindow);
            canvas = new();
            windowSystem.AddWindow(canvas);
            Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configWindow.IsOpen = true; };
            Svc.Condition.ConditionChange += OnConditionChange;
            _ = Svc.Commands.AddHandler("/avarice", new CommandInfo((string cmd, string args) =>
            {
                if (args == "debug")
                {
                    P.currentProfile.Debug = !P.currentProfile.Debug;
                    Svc.Chat.Print($"Debug mode {(P.currentProfile.Debug ? "enabled" : "disabled")}");
                }
                else if (args == "draw") // Added new command for toggling drawing
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
            //LoadOpcode.Start();
            LuminaSheets.Init();
            Svc.PluginInterface.GetIpcProvider<IntPtr, CardinalDirection>("Avarice.CardinalDirection").RegisterFunc(GetCardinalDirectionForObject);
            Svc.Framework.Update += Tick;
            StaticAutoDetectRadiusData = Util.LoadStaticAutoDetectRadiusData();
            if (config.SplatoonUnsafePixel)
            {
                TabSplatoon.WriteRequest();
            }

            ActionWatching.Enable();
            ComboCache.ComboCacheInstance = new ComboCache();

            PositionalManager = new();
        });
    }

    private CardinalDirection GetCardinalDirectionForObject(IntPtr arg)
    {
        var obj = Svc.Objects.CreateObjectReference(arg);
        if (obj != null && Svc.ClientState.LocalPlayer != null)
        {
            return MathHelper.GetCardinalDirection((MathHelper.GetRelativeAngle(Svc.ClientState.LocalPlayer.Position, obj.Position) + obj.Rotation.RadToDeg()) % 360);
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
                    .AddUiForeground($"{success}%", ECommons.GenericHelpers.GetParsedSeSetingColor(success))
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

    public void Dispose()
    {
        Safe(() => Svc.PluginInterface.SavePluginConfig(config));
        //Svc.GameNetwork.NetworkMessage -= OnNetworkMessage;
        Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        _ = Svc.Commands.RemoveHandler("/avarice");
        Svc.Condition.ConditionChange -= OnConditionChange;
        Svc.Framework.Update -= Tick;
        Safe(() =>
        {
            Svc.PluginInterface.GetIpcProvider<IntPtr, CardinalDirection>("Avarice.CardinalDirection").UnregisterFunc();
        });
        memory.Dispose();
        ActionWatching.Dispose();
        ComboCache.ComboCacheInstance.Dispose();
        PunishLibMain.Dispose();
        ECommonsMain.Dispose();
        P = null;
    }

    private void Tick(object framework)
    {
        if (Framework.Instance()->FrameCounter - PositionalStatus[0] > 1)
        {
            PositionalStatus[1] = 0;
        }
        if (Svc.ClientState.LocalPlayer != null)
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