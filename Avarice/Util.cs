using Dalamud.Game.ClientState.Objects.Types;
using ECommons.MathHelpers;

namespace Avarice;

internal static unsafe class Util
{
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
        return Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId  == 803u); // Wheel in Motion
    }

    internal static void DrawStretched(this InfoBox box)
    {
        box.Size = new(ImGui.GetContentRegionAvail().X, box.Size.Y);
        box.Draw();
        ImGuiHelpers.ScaledDummy(20f);
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
        if (d == CardinalDirection.North) return (-45, 45);
        if (d == CardinalDirection.South) return (180 - 45, 180 + 45);
        if (d == CardinalDirection.West) return (90 - 45, 90 + 45);
        if (d == CardinalDirection.East) return (270 - 45, 270 + 45);
        return (default, default);
    }

    internal static (int min, int max) Get18PieForAngle(float a)
    {
        if (a.InRange(315, 360)) return (0, 45);
        if (a.InRange(0, 45)) return (-45, 0);

        if (a.InRange(45, 90)) return (270, 315);
        if (a.InRange(90, 135)) return (225, 270);

        if (a.InRange(135, 180)) return (180, 225);
        if (a.InRange(180, 225)) return (135, 180);

        if (a.InRange(225, 270)) return (90, 135);
        if (a.InRange(270, 315)) return (45, 90);
        return (default, default);
    }



    internal static float GetConfiguredRadius()
    {
        if (P.currentProfile.EnableCurrentPie && P.currentProfile.Radius2 && !P.currentProfile.Radius3) return GetAttackRadius();
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

    internal static CardinalDirection GetDirection(GameObject bnpc)
    {
        return MathHelper.GetCardinalDirection(GetAngle(bnpc));
    }

    internal static float GetAngle(GameObject bnpc)
    {
        return (MathHelper.GetRelativeAngle(Svc.ClientState.LocalPlayer.Position, bnpc.Position) + bnpc.Rotation.RadToDeg()) % 360;
    }
}
