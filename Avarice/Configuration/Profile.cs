using Dalamud.Interface.Style;
using System.IO;

namespace Avarice.Configuration;

[Serializable]
public class Profile
{
    public string Name = "";
    public bool IsDefault = false;
    public bool EnableChatMessages = false;
    public bool EnableVFX = true; 
    public bool Announce = false;
    public bool Debug = false;
    public float MeleeSkillAtk = 3f;
    public bool MeleeSkillIncludeHitbox = true;
    public float MeleeAutoAtk = 2.1f;
    public bool MeleeAutoIncludeHitbox = true;
    public string GUID = Guid.NewGuid().ToString();
    public int NinHutinTh = 30000;

    public ClassDisplayCondition EnableCurrentPie = ClassDisplayCondition.Display_on_all_jobs;
    public Brush CurrentPieSettings = new()
    {
        Color = Vector4.Zero,
        Fill = ImGui.ColorConvertU32ToFloat4(0x508BDC35),
        Thickness = 0f
    };
    public Brush CurrentPieSettingsFlank = new()
    {
        Color = Vector4.Zero,
        Fill = ImGui.ColorConvertU32ToFloat4(0x5051C8CF),
        Thickness = 0f
    };
    public ClassDisplayCondition EnableAnticipatedPie = ClassDisplayCondition.Display_on_positional_jobs;
    public Brush AnticipatedPieSettings = new()
    {
        Color = ImGui.ColorConvertU32ToFloat4(0xFF17F000),
        Fill = Vector4.Zero,
        Thickness = 8f
    };
    public Brush AnticipatedPieSettingsFlank = new()
    {
        Color = ImGui.ColorConvertU32ToFloat4(0xFF17F000),
        Fill = Vector4.Zero,
        Thickness = 8f
    };
    public bool AnticipatedDisableTrueNorth = true;

    public ClassDisplayCondition EnableMaxMeleeRing = ClassDisplayCondition.Display_on_all_jobs;
    public Brush MaxMeleeSettingsN = new()
    {
        Color = ImGui.ColorConvertU32ToFloat4(0x501400E6),
        Fill = Vector4.Zero,
        Thickness = 2f
    };
    public Vector4 MaxMeleeSettingsS = ImGui.ColorConvertU32ToFloat4(0x501400E6);
    public Vector4 MaxMeleeSettingsE = ImGui.ColorConvertU32ToFloat4(0x501400E6);
    public Vector4 MaxMeleeSettingsW = ImGui.ColorConvertU32ToFloat4(0x501400E6);
    public bool DrawLines = true;
    public bool SameColor = true;
    public bool Radius3 = true;
    public bool Radius2 = true;
    public bool HLine = false;
    public bool VLine = false;

    public ClassDisplayCondition EnablePlayerDot = ClassDisplayCondition.Display_on_all_jobs;
    public Brush PlayerDotSettings = new()
    {
        Color = ImGui.ColorConvertU32ToFloat4(0xFFFFFFFF),
        Fill = Vector4.Zero,
        Thickness = 2f
    };
    public DisplayCondition PlayerDotDisplayCondition = DisplayCondition.Always;
    public ClassDisplayCondition EnablePlayerRing = ClassDisplayCondition.Do_not_display;
    public Brush PlayerRingSettings = new()
    {
        Color = ImGui.ColorConvertU32ToFloat4(0x50000000),
        Fill = Vector4.Zero,
        Thickness = 2f
    };

    public Dictionary<uint, Stats> Stats = new();
    public Stats CurrentEncounterStats = new();

    public bool PartyDot = false;
    public Brush PartyDotSettings = new()
    {
        Color = ImGui.ColorConvertU32ToFloat4(0x5014F000),
        Fill = Vector4.Zero,
        Thickness = 2f
    };

    public bool AllDot = false;
    public Brush AllDotSettings = new()
    {
        Color = ImGui.ColorConvertU32ToFloat4(0x5014F000),
        Fill = Vector4.Zero,
        Thickness = 2f
    };

    public ClassDisplayCondition EnableFrontSegment = ClassDisplayCondition.Display_on_all_jobs;
    public Brush FrontSegmentIndicator = new()
    {
        Color = Vector4.Zero,
        Fill = ImGui.ColorConvertU32ToFloat4(0x503535D6),
        Thickness = 2f
    };
    public bool FrontStand = false;

    public bool NinRearForTrickAttack = false;
    public bool NinAnticipatedDisableMeikyoShisui = false;

    public float MnkDemolish = 6f;
    public bool MnkAoEDisable = false;


		public DisplayCondition CompassCondition = DisplayCondition.Always;
		public ClassDisplayCondition CompassEnable = ClassDisplayCondition.Do_not_display;
		public Vector4 CompassColorN = ImGuiColors.DalamudRed;
    public Vector4 CompassColor = ImGuiColors.DalamudYellow;
    public float CompassDistance = 1f;
    public float CompassSize = 5f;
}
