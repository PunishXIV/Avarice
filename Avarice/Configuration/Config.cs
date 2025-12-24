using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Avarice.Configuration;

[Serializable]
internal class Config : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public uint ActionEffect1Opcode = 0x398;
    public List<Profile> Profiles = new();
    public Dictionary<uint, string> JobProfiles = new();

    public Vector4 DutyMidPixelCol = EColor.YellowBright;
    public float DutyMidRadius = 1f;
    public Vector4 CenteredPixelColor = EColor.GreenBright;
    public Vector4 UncenteredPixelColor = EColor.RedBright;
    public float CenterPixelThickness = 2f;

    public Dictionary<uint, Vector3?> DutyMiddleOverrides = new();

    public List<ExtraPoint> DutyMiddleExtras = new();
    public bool SplatoonUnsafePixel = false;
    public Vector4 SplatoonPixelCol = EColor.RedBright;

    /// <summary>
    /// When true, overlays are only drawn if the current target has positional vulnerabilities.
    /// </summary>
    public bool OnlyDrawIfPositional = false;
    
    /// <summary>
    /// Settings for the visual feedback system (checkmark/X display)
    /// </summary>
    public VisualFeedbackSettings VisualFeedbackSettings { get; set; }

    /// <summary>
    /// Settings for the audio feedback system
    /// </summary>
    public AudioFeedbackSettings AudioFeedbackSettings { get; set; }

    // Pictomancy renderer settings
    public bool UsePictomancyRenderer = false;
    public byte PictomancyMaxAlpha = 255;
    public bool PictomancyClipNativeUI = true;
}