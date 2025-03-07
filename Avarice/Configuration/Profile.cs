using Dalamud.Interface.GameFonts;

namespace Avarice.Configuration;

[Serializable]
public class Profile
{
    public string Name = "";
    public bool IsDefault = false;
    public bool EnableChatMessagesSuccess = false;
    public bool EnableChatMessagesFailure = true;
    public bool EnableVFXSuccess = false;
    public bool EnableVFXFailure = true;
    public bool Announce = false;
    public bool Debug = false;
    public bool DrawingEnabled = true; // New property for drawing toggle
    public float MeleeSkillAtk = 3f;
    public bool MeleeSkillIncludeHitbox = true;
    public float MeleeAutoAtk = 2.1f;
    public bool MeleeAutoIncludeHitbox = true;
    public string GUID = Guid.NewGuid().ToString();

    public bool EnableCurrentPie = true;
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
    public bool EnableAnticipatedPie = true;
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

    public bool EnableMaxMeleeRing = true;
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

    public bool EnablePlayerDot = true;
    public Brush PlayerDotSettings = new()
    {
        Color = ImGui.ColorConvertU32ToFloat4(0xFFFFFFFF),
        Fill = Vector4.Zero,
        Thickness = 2f
    };
    public DisplayCondition PlayerDotDisplayCondition = DisplayCondition.Always;
    public bool EnablePlayerRing = false;
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

    public bool EnableFrontSegment = true;
    public Brush FrontSegmentIndicator = new()
    {
        Color = Vector4.Zero,
        Fill = ImGui.ColorConvertU32ToFloat4(0x503535D6),
        Thickness = 2f
    };
    public bool FrontStand = false;

    public bool TrickAttack;
    public bool Meikyo;
    public int Reaper;

    public DisplayCondition CompassCondition = DisplayCondition.Always;
    public bool CompassEnable = false;
    public Vector4 CompassColorN = ImGuiColors.DalamudRed;
    public Vector4 CompassColor = ImGuiColors.DalamudYellow;
    public float CompassDistance = 1f;
    public float CompassFontScale = 1f;
    public GameFontFamilyAndSize CompassFont = GameFontFamilyAndSize.MiedingerMid36;

    public bool EnableTankMiddle = false;
    public bool EnableDutyMiddle = false;

    public bool UseRotationSolver = false;
}