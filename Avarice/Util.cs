using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using PunishLib.ImGuiMethods;
using System.IO;

namespace Avarice;

internal static unsafe class Util
{
    internal static void DrawDot(Vector3 where, float thickness, Vector4 col) => DrawDot(where, thickness, col.ToUint());

    internal static void DrawDot(Vector3 where, float thickness, uint col)
    {
        if(Svc.GameGui.WorldToScreen(where, out var pos))
            ImGui.GetWindowDrawList().AddCircleFilled(
            new Vector2(pos.X, pos.Y),
            thickness,
            col,
            100);
    }

    internal static bool TryAutoDetectMiddleOfArena(out Vector3 mid)
    {
        if(Player.Available)
        {
            var shouldAuto = P.StaticAutoDetectRadiusData.Contains(Svc.ClientState.TerritoryType);
            if(P.config.DutyMiddleOverrides.TryGetValue(Svc.ClientState.TerritoryType, out var v))
            {
                if(v == null)
                {
                    shouldAuto = true;
                }
                else
                {
                    mid = v.Value;
                    return true;
                }
            }
            if(shouldAuto)
            {
                if(Player.Object.Position.X.InRange(-50f, 50f) && Player.Object.Position.Z.InRange(-50f, 50f))
                {
                    mid = Vector3.Zero;
                    return true;
                }
                else if(Player.Object.Position.X.InRange(50, 150) && Player.Object.Position.Z.InRange(50, 150))
                {
                    mid = new(100f, 0f, 100f);
                    return true;
                }
            }
        }
        mid = default;
        return false;
    }

    internal static HashSet<uint> LoadStaticAutoDetectRadiusData()
    {
        var ret = new HashSet<uint>();
        try
        {
            var path = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "AutoDetectTankRadius.csv");
            foreach(var x in File.ReadAllText(path).Split("\n", StringSplitOptions.TrimEntries))
            {
                if(x != "" && uint.TryParse(x, out var res))
                {
                    ret.Add(res);
                }
            }
        }
        catch(Exception e)
        {
            e.Log();
        }
        return ret;
    }

    internal static bool CanExecuteGallows()
    {
        return Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == 2589u)
            && Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId.EqualsAny(2587u, 2854u));
    }

    internal static bool CanExecuteGibbet()
    {
        return Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId.EqualsAny(2588u, 2855u))
            && Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId.EqualsAny(2587u, 2854u));
    }

    internal static bool CanExecuteFangAndClaw()
    {
        return Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == 802u); //Fang and Claw Barred
    }
    internal static bool CanExecuteWheelingThrust()
    {
        return Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == 803u); // Wheel in Motion
    }

    internal static void DrawStretched(this Drawing.InfoBox box)
    {
        /*box.Size = new(ImGui.GetContentRegionAvail().X, box.Size.Y);
        box.Draw();
        ImGuiHelpers.ScaledDummy(20f);*/
        ImGuiGroup.BeginGroupBox(box.Label);
        box.ContentsAction();
        ImGuiGroup.EndGroupBox();
    }

    /*internal static bool IsClassDisplayConditionMatching(this ClassDisplayCondition d)
    {
        if(Svc.ClientState.LocalPlayer == null || d == ClassDisplayCondition.Do_not_display)
        {
            return false;
        }
        else
        {
            return d == ClassDisplayCondition.Display_on_all_jobs ||
                (d == ClassDisplayCondition.Display_on_positional_jobs
                && Svc.ClientState.LocalPlayer.ClassJob.Id.EqualsAny(Avarice.PositionalJobs));
        }
    }*/

    /*internal static bool IsEnabled(this ClassDisplayCondition d)
    {
        return d != ClassDisplayCondition.Do_not_display;
    }*/

    internal static bool IsPositionalJob()
    {
        return Svc.ClientState.LocalPlayer?.ClassJob.Id.EqualsAny(Avarice.PositionalJobs) == true;
    }

    internal static (int min, int max) GetAngleRangeForDirection(CardinalDirection d)
    {
        if(d == CardinalDirection.North) return (-45, 45);
        if(d == CardinalDirection.South) return (180 - 45, 180 + 45);
        if(d == CardinalDirection.West) return (90 - 45, 90 + 45);
        if(d == CardinalDirection.East) return (270 - 45, 270 + 45);
        return (default, default);
    }

    internal static (int min, int max) Get18PieForAngle(float a)
    {
        if(a.InRange(315, 360)) return (0, 45);
        if(a.InRange(0, 45)) return (-45, 0);

        if(a.InRange(45, 90)) return (270, 315);
        if(a.InRange(90, 135)) return (225, 270);

        if(a.InRange(135, 180)) return (180, 225);
        if(a.InRange(180, 225)) return (135, 180);

        if(a.InRange(225, 270)) return (90, 135);
        if(a.InRange(270, 315)) return (45, 90);
        return (default, default);
    }



    internal static float GetConfiguredRadius()
    {
        if(P.currentProfile.EnableCurrentPie && P.currentProfile.Radius2 && !P.currentProfile.Radius3) return GetAttackRadius();
        return GetSkillRadius();
    }

    internal static float GetSkillRadius()
    {
        return P.currentProfile.MeleeSkillAtk + (P.currentProfile.MeleeSkillIncludeHitbox ? Svc.ClientState.LocalPlayer.HitboxRadius : 0);
    }

    internal static float GetAttackRadius()
    {
        return P.currentProfile.MeleeAutoAtk + (P.currentProfile.MeleeAutoIncludeHitbox ? Svc.ClientState.LocalPlayer.HitboxRadius : 0);
    }

    internal static CardinalDirection GetDirection(IGameObject bnpc)
    {
        return MathHelper.GetCardinalDirection(GetAngle(bnpc));
    }

    internal static float GetAngle(IGameObject bnpc)
    {
        return (MathHelper.GetRelativeAngle(Svc.ClientState.LocalPlayer.Position, bnpc.Position) + bnpc.Rotation.RadToDeg()) % 360;
    }

    public static bool IsViperAnticipatedRear()
    {
        //34609 = Swiftskin's Sting    (Lvl 30+)
        //Swiftskin's Sting is used to prime Hindsting Strike/Hindsbane Fang (lvl 30) for rear positional
        //34612 = Hindsting Strike     (Lvl 30+)
        //34613 = Hindsbane Fang       (Lvl 30+)

        //34620 = Dreadwinter          (Lvl 65+)
        //Dreadwinter (lvl 65) is used to prime Swiftskin's Coil for rear positional
        //34622 = Swiftskin's Coil     (Lvl 65+)

        var levelcheckHindsting = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(34612).ClassJobLevel;
        var levelcheckHindsbane = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(34613).ClassJobLevel;
        //var levelcheckSwiftskin = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(34622).ClassJobLevel;
        var move = P.memory.LastComboMove;
        return (levelcheckHindsting && move.EqualsAny(34609u)) || (levelcheckHindsbane && move.EqualsAny(34609u)) || Player.Status.Any(x => x.StatusId.EqualsAny(3647u, 3648u)); //|| (levelcheckSwiftskin && move.EqualsAny(34620u, 34637u));
    }
    public static bool IsViperAnticipatedFlank()
    {
        //34608 = Hunters's Sting       (Lvl 30+)
        //Hunters's Sting is used to prime Flanksting Strike/Flanksbane Fang (lvl 30) for flank positional
        //34610 = Flanksting Strike     (Lvl 30+)
        //34611 = Flanksbane Fang       (Lvl 30+)

        //34620 = Dreadwinter           (Lvl 65+)
        //Dreadwinter (lvl 65) is used to prime Hunters's Coil for flank positional
        //34621 = Hunter's Coil         (Lvl 65+)

        var levelcheckFlanksting = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(34610).ClassJobLevel;
        var levelcheckFlanksbane = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(34611).ClassJobLevel;
        //var levelcheckHuntersCoil = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(34621).ClassJobLevel;
        var move = P.memory.LastComboMove;
        return (levelcheckFlanksting && move.EqualsAny(34608u)) || (levelcheckFlanksbane && move.EqualsAny(34608u)) || Player.Status.Any(x => x.StatusId.EqualsAny(3645u, 3646u)); //|| (levelcheckHuntersCoil && move.EqualsAny(34620u, 34636u));
    }

    public static bool IsDragoonAnticipatedRear()
    {   //87    = Disembowel        (Lvl 18 -> 96)
        //36955 = Spiral Blow       (Lvl 96+)
        //Disembowel & Spiral Blow are used to prime Chaos Thrust/Chaotic Spring (lvl 50) for rear positional

        //88    = Chaos Thrust      (Lvl 50 -> 86)
        //25772 = Chaotic Spring    (Lvl 86+)
        //Chaos Thrust & Chaotic Spring are used to prime Wheeling Thrust (lvl 58) for rear positional
        //3556  = Wheeling Thrust

        var levelcheckChaos = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(88).ClassJobLevel;
        var levelcheckWheel = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(3556).ClassJobLevel;
        var move = P.memory.LastComboMove;
        return (levelcheckChaos && move.EqualsAny(87u, 36955u)) || (levelcheckWheel && move.EqualsAny(88u, 25772u));
    }
    public static bool IsDragoonAnticipatedFlank()
    {   //84    = Full Thrust       (Lvl 26 -> 86)
        //25771 = Heavens' Thrust   (Lvl 86+)
        //Full Thrust & Heavens' Thrust are used to prime Fang and Claw (Lvl 56) for flank positional
        //3554  = Fang and Claw

        var levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(3554).ClassJobLevel;
        var move = P.memory.LastComboMove;
        return levelcheck && move.EqualsAny(84u, 25771u);
    }

    public static bool IsSamuraiAnticipatedRear()
    {   //7478 = Jinpu        (Lvl 18 -> 96)
        //Jinpu are used to prime Gekko (lvl 30) for rear positional
        //7481 = Gekko

        var levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(7481).ClassJobLevel;
        var move = P.memory.LastComboMove;
        return levelcheck && move.EqualsAny(7478u);
    }
    public static bool IsSamuraiAnticipatedFlank()
    {   //7479 = Shifu   (Lvl 18+)
        //Shifu is used to prime Kasha (Lvl 40) for flank positional
        //7482 = Kasha
        var levelcheck = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(7482).ClassJobLevel;
        var move = P.memory.LastComboMove;
        return levelcheck && move.EqualsAny(7479u);
    }

    // Notes on Reaper. Similar to Viper, Reaper has a double positional primer in Executioner and Soul Reaver.
    // Avarice is currently unable to handle double positional anticipation, so for now, this is coded to treat Executioner and Soul Reaver as a single positional primer for Flank only.
    // This means that if Executioner or Soul Reaver is active, Avarice will anticipate Flank positional first (picked over rear due to ease of use) then rotate between rear and flank based on statuses.
    public static bool IsReaperAnticipatedRear()
    {
        //2589 = Enhanced Gallows
        //2587 = Soul Reaver - Able to use Gibbet, Gallows, and Guillotine.
        //2854 = Soul Reaver - Able to execute Guillotine.
        //3858 = Executioner - Able to execute Executioner's Gibbet, Executioner's Gallows, and Executioner's Guillotine.

        //Enhanced Gallows, Soul Reaver, and Executioner are used to prime Exec/Gallows for rear positional
        //24383 = Gallows
        //36971 = Executioner's Gallows

        var levelcheckGallows = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(24383).ClassJobLevel;
        var levelcheckExecutionersGallows = Svc.ClientState.LocalPlayer.Level >= Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(36971).ClassJobLevel;
        var move = P.memory.LastComboMove;
        return (levelcheckGallows && move.EqualsAny(24382u)) || (levelcheckExecutionersGallows && move.EqualsAny(36970u)) || Player.Status.Any(x => x.StatusId.EqualsAny(2589u));
    }
    public static bool IsReaperAnticipatedFlank()
    {
        //2588 = Enhanced Gibbet
        //2855 = EnhancedGibbet_2855
        //2587 = Soul Reaver - Able to use Gibbet, Gallows, and Guillotine.
        //2854 = Soul Reaver - Able to execute Guillotine.
        //3858 = Executioner - Able to execute Executioner's Gibbet, Executioner's Gallows, and Executioner's Guillotine.

        //Enhanced Gibbet, Soul Reaver, and Executioner are used to prime Exec/Gibbet for rear positional
        //24382 = Gibbet
        //36970 = Executioner's Gibbet

        return Player.Status.Any(x => x.StatusId.EqualsAny(2588u, 2855u, 2587u, 2854u, 3858u));
    }
}
