using Avarice.Structs;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.Events;
using ECommons.MathHelpers;
using ECommons.Schedulers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using PunishLib;
using System;

#pragma warning disable CS0649

namespace Avarice;

public unsafe class Avarice : IDalamudPlugin
{
    public string Name => "Avarice";
    internal Config config;
    internal Profile currentProfile;
    internal static Avarice P;
    WindowSystem windowSystem;
    internal ConfigWindow configWindow;
    Canvas canvas;
    internal Memory memory;

    internal static uint[] PositionalJobs = new uint[] { 2, 4, 29, 30, 20, 34, 39, 22 };
    internal uint Job = 0;
    internal HashSet<uint> StaticAutoDetectRadiusData;

    internal static readonly Dictionary<int, HashSet<int>> PositionalData = new()
    {
        {   56, new HashSet<int> {19, 21}},                 // Snap Punch
        {   66, new HashSet<int> {46, 60}},                 // Demolish
        // {   79, new HashSet<int> {}},                // Heavy Thrust
        {   88, new HashSet<int> {28, 61}},             // Chaos Thrust
        { 2255, new HashSet<int> {30, 37, 68, 75}},             // Aeolian Edge
        { 2258, new HashSet<int> {25}},                 // Trick Attack
        { 3554, new HashSet<int> {10, 13}},             // Fang and Claw
        { 3556, new HashSet<int> {10, 13}},             // Wheeling Thrust
        { 3563, new HashSet<int> {30, 37, 66, 73}},             // Armor Crush
        { 7481, new HashSet<int> {29, 33, 68, 72}},     // Gekko (rear)
        { 7482, new HashSet<int> {29, 33, 68, 72}},     // Kasha (flank)
        {24382, new HashSet<int> {11, 13}},             // Gibbet (flank)
        {24383, new HashSet<int> {11, 13}},             // Gallows (rear)
        {25772, new HashSet<int> {28, 66}},             // Chaotic Spring
    };



    public Avarice(DalamudPluginInterface pi)
    {
        P = this;
        ECommonsMain.Init(pi, this, Module.DalamudReflector, Module.ObjectFunctions);
        PunishLibMain.Init(pi, this, PunishOption.DefaultKoFi);
        new TickScheduler(delegate
        {
            config = Svc.PluginInterface.GetPluginConfig() as Config ?? new();
            if(config.Profiles.Count == 0)
            {
                config.Profiles.Add(new() { Name = "Default profile", IsDefault = true });
            }
            currentProfile = config.Profiles.FirstOr0(x => x.IsDefault);
            //Svc.GameNetwork.NetworkMessage += OnNetworkMessage;
            memory = new();
            windowSystem = new();
            configWindow = new();
            windowSystem.AddWindow(configWindow);
            canvas = new();
            windowSystem.AddWindow(canvas);
            Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configWindow.IsOpen = true; };
            Svc.Condition.ConditionChange += OnConditionChange;
            Svc.Commands.AddHandler("/avarice", new CommandInfo((string cmd, string args) =>
            {
                if (args == "debug")
                {
                    P.currentProfile.Debug = !P.currentProfile.Debug;
                }
                else
                {
                    configWindow.IsOpen = !configWindow.IsOpen;
                }
            })
            { HelpMessage = "Toggle configuration/stats window" });
            //LoadOpcode.Start();
            LuminaSheets.Init();
            Svc.PluginInterface.GetIpcProvider<IntPtr, CardinalDirection>("Avarice.CardinalDirection").RegisterFunc(GetCardinalDirectionForObject);
            Svc.Framework.Update += Tick;
            StaticAutoDetectRadiusData = Util.LoadStaticAutoDetectRadiusData();
        });
        if (ProperOnLogin.PlayerPresent)
        {
            var x = Svc.ClientState.LocalPlayer!.Address;
        }
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
                        var success = (int)(100f * (float)currentProfile.CurrentEncounterStats.Hits / (float)total);
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
        if(c == DisplayCondition.Only_in_combat)
        {
            return Svc.Condition[ConditionFlag.InCombat];
        }
        else if(c == DisplayCondition.Only_in_duty)
        {
            return Svc.Condition[ConditionFlag.BoundByDuty56];
        }
        else if(c == DisplayCondition.In_duty_or_combat)
        {
            return Svc.Condition[ConditionFlag.InCombat] || Svc.Condition[ConditionFlag.BoundByDuty56];
        }
        else if(c == DisplayCondition.In_duty_and_combat)
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
        if (!currentProfile.Stats.ContainsKey(Svc.ClientState.LocalPlayer.ClassJob.Id))
        {
            currentProfile.Stats[Svc.ClientState.LocalPlayer.ClassJob.Id] = new();
        }
        if (isMiss)
        {
            currentProfile.Stats[Svc.ClientState.LocalPlayer.ClassJob.Id].Missed++;
            currentProfile.CurrentEncounterStats.Missed++;
        }
        else
        {
            currentProfile.Stats[Svc.ClientState.LocalPlayer.ClassJob.Id].Hits++;
            currentProfile.CurrentEncounterStats.Hits++;
        }
    }

    internal Profile GetProfileForJob(uint job)
    {
        if(P.config.JobProfiles.TryGetValue(job, out var guid))
        {
            if(P.config.Profiles.TryGetFirst(x => x.GUID == guid, out var profile))
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
        Svc.Commands.RemoveHandler("/avarice");
        Svc.Condition.ConditionChange -= OnConditionChange;
        Svc.Framework.Update -= Tick;
        Safe(() =>
        {
            Svc.PluginInterface.GetIpcProvider<IntPtr, CardinalDirection>("Avarice.CardinalDirection").UnregisterFunc();
        });
        memory.Dispose();
        PunishLibMain.Dispose();
        ECommonsMain.Dispose();
        P = null;
    }

    private void Tick(object framework)
    {
        if (Svc.ClientState.LocalPlayer != null)
        {
            var newJob = Svc.ClientState.LocalPlayer.ClassJob.Id;
            if (newJob != Job)
            {
                PluginLog.Debug($"Job changed from {Job} to {newJob}");
                var newJobProfile = GetProfileForJob(newJob);
                if(newJobProfile != null)
                {
                    currentProfile = newJobProfile;
                    PluginLog.Debug($"Switched profile to job profile {newJobProfile.Name}");
                }
                else
                {
                    if(GetProfileForJob(Job) != null)
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
