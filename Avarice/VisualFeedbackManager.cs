using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ECommons.GameHelpers;

namespace Avarice;

internal class VisualFeedbackManager
{
    private static VisualFeedbackOverlay feedbackOverlay = null;
    
    internal static void DisplayFeedback(bool success, float duration = 2.0f)
    {
        if (feedbackOverlay == null)
        {
            feedbackOverlay = new VisualFeedbackOverlay();
            P.windowSystem.AddWindow(feedbackOverlay);
        }
        
        feedbackOverlay.ShowFeedback(success, duration);
    }
    
    internal static void RemoveFeedback()
    {
        if (feedbackOverlay != null)
        {
            feedbackOverlay.HideFeedback();
        }
    }
    
    internal static void TestFeedback(bool success)
    {
        DisplayFeedback(success, 2.0f);
    }
    
    internal static void Dispose()
    {
        if (feedbackOverlay != null)
        {
            P.windowSystem.RemoveWindow(feedbackOverlay);
            feedbackOverlay = null;
        }
    }
}

internal class VisualFeedbackOverlay : Window
{
    public bool IsSuccess { get; set; }
    public bool IsShowingFeedback { get; set; }
    
    private DateTime displayEndTime;
    private const float DEFAULT_HEIGHT_OFFSET = 2.0f; // Fixed height above player
    
    public VisualFeedbackOverlay() : base("##VisualFeedbackOverlay",
        ImGuiWindowFlags.NoInputs
        | ImGuiWindowFlags.NoTitleBar
        | ImGuiWindowFlags.NoScrollbar
        | ImGuiWindowFlags.NoBackground
        | ImGuiWindowFlags.AlwaysUseWindowPadding
        | ImGuiWindowFlags.NoSavedSettings
        | ImGuiWindowFlags.NoFocusOnAppearing
        | ImGuiWindowFlags.NoDocking)
    {
        IsOpen = true;
        RespectCloseHotkey = false;
    }
    
    public void ShowFeedback(bool success, float duration)
    {
        IsSuccess = success;
        IsShowingFeedback = true;
        displayEndTime = DateTime.Now.AddSeconds(duration);
        
        Task.Delay(TimeSpan.FromSeconds(duration)).ContinueWith(_ =>
        {
            HideFeedback();
        });
    }
    
    public void HideFeedback()
    {
        IsShowingFeedback = false;
    }
    
    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);
    }
    
    public override bool DrawConditions()
    {
        return IsShowingFeedback && Svc.Objects.LocalPlayer != null;
    }
    
    public override void Draw()
    {
        var settings = P.config.VisualFeedbackSettings ?? new VisualFeedbackSettings();
        
        // Get position above player's head
        var playerPos = Svc.Objects.LocalPlayer.Position;
        var worldPos = playerPos with { Y = playerPos.Y + DEFAULT_HEIGHT_OFFSET };
        
        // Convert world position to screen position
        if (!Svc.GameGui.WorldToScreen(worldPos, out var screenPos)) return;
        
        var drawList = ImGui.GetWindowDrawList();
        
        // Draw a subtle dark circle behind the icon for visibility
        var bgColor = ImGui.GetColorU32(new Vector4(0, 0, 0, 0.3f));
        drawList.AddCircleFilled(screenPos, settings.IconSize * 1.2f, bgColor);
        
        // Draw the feedback icon
        if (IsSuccess)
            DrawCheckmark(drawList, screenPos, settings);
        else
            DrawX(drawList, screenPos, settings);
    }
    
    public override void PostDraw()
    {
        ImGui.PopStyleVar();
    }
    
    private void DrawCheckmark(ImDrawListPtr drawList, Vector2 center, VisualFeedbackSettings settings)
    {
        var color = ImGui.GetColorU32(settings.SuccessColor);
        var thickness = Math.Max(2f, settings.IconSize / 8f);
        
        // Draw checkmark
        drawList.AddLine(
            center + new Vector2(-settings.IconSize * 0.5f, 0), 
            center + new Vector2(-settings.IconSize * 0.1f, settings.IconSize * 0.4f), 
            color, thickness);
        drawList.AddLine(
            center + new Vector2(-settings.IconSize * 0.1f, settings.IconSize * 0.4f), 
            center + new Vector2(settings.IconSize * 0.5f, -settings.IconSize * 0.4f), 
            color, thickness);
        
        // Draw circle outline
        drawList.AddCircle(center, settings.IconSize, color, 0, thickness);
    }
    
    private void DrawX(ImDrawListPtr drawList, Vector2 center, VisualFeedbackSettings settings)
    {
        var color = ImGui.GetColorU32(settings.FailureColor);
        var thickness = Math.Max(2f, settings.IconSize / 8f);
        var offset = settings.IconSize * 0.5f;
        
        // Draw X
        drawList.AddLine(
            center + new Vector2(-offset, -offset), 
            center + new Vector2(offset, offset), 
            color, thickness);
        drawList.AddLine(
            center + new Vector2(-offset, offset), 
            center + new Vector2(offset, -offset), 
            color, thickness);
        
        // Draw circle outline
        drawList.AddCircle(center, settings.IconSize, color, 0, thickness);
    }
}

[Serializable]
public class VisualFeedbackSettings
{
    public float IconSize { get; set; } = 40f;
    public Vector4 SuccessColor { get; set; } = new Vector4(0.2f, 0.9f, 0.2f, 1f);
    public Vector4 FailureColor { get; set; } = new Vector4(0.9f, 0.2f, 0.2f, 1f);
    
    // Keep old properties for backward compatibility but mark obsolete
    [Obsolete] public bool EnableFadeOut { get; set; }
    [Obsolete] public float VerticalOffset { get; set; }
    [Obsolete] public Vector2 Position { get; set; }
    [Obsolete] public Vector2 Size { get; set; }
    [Obsolete] public bool ShowBackground { get; set; }
    [Obsolete] public float BackgroundAlpha { get; set; }
}
