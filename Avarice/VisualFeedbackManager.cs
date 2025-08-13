using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Components;
using ECommons.ImGuiMethods;

namespace Avarice;

internal class VisualFeedbackManager
{
    private static VisualFeedbackWindow feedbackWindow = null;
    
    internal static void DisplayFeedback(bool success, float duration = 2.0f)
    {
        if (feedbackWindow == null)
        {
            feedbackWindow = new VisualFeedbackWindow();
            P.windowSystem.AddWindow(feedbackWindow);
        }
        
        feedbackWindow.ShowFeedback(success, duration);
    }
    
    internal static void RemoveFeedback()
    {
        if (feedbackWindow != null && !feedbackWindow.IsConfiguring)
        {
            feedbackWindow.IsOpen = false;
        }
    }
    
    internal static void ConfigureMode(bool enable)
    {
        if (feedbackWindow == null)
        {
            feedbackWindow = new VisualFeedbackWindow();
            P.windowSystem.AddWindow(feedbackWindow);
        }
        
        feedbackWindow.IsConfiguring = enable;
        feedbackWindow.IsOpen = enable;
        if (enable)
        {
            feedbackWindow.IsSuccess = true;
        }
    }
    
    internal static void Dispose()
    {
        if (feedbackWindow != null)
        {
            feedbackWindow.SaveSettings();
            P.windowSystem.RemoveWindow(feedbackWindow);
            feedbackWindow = null;
        }
    }
}

internal class VisualFeedbackWindow : Window
{
    public bool IsSuccess { get; set; }
    public bool IsConfiguring { get; set; }
    
    private DateTime displayEndTime;
    private float fadeAlpha = 1.0f;
    
    private bool showBackground = true;
    private float backgroundAlpha = 0.7f;
    private float iconSize = 40f;
    private bool enableFadeOut = true;
    private Vector4 successColor = new Vector4(0.2f, 0.9f, 0.2f, 1f);
    private Vector4 failureColor = new Vector4(0.9f, 0.2f, 0.2f, 1f);
    
    public VisualFeedbackWindow() : base("##VisualFeedback")
    {
        LoadSettings();
        UpdateWindowFlags();
    }
    
    public void ShowFeedback(bool success, float duration)
    {
        IsSuccess = success;
        IsOpen = true;
        IsConfiguring = false;
        displayEndTime = DateTime.Now.AddSeconds(duration);
        fadeAlpha = 1.0f;
        
        Task.Delay(TimeSpan.FromSeconds(duration)).ContinueWith(_ =>
        {
            if (!IsConfiguring)
            {
                IsOpen = false;
            }
        });
    }
    
    private void UpdateWindowFlags()
    {
        if (IsConfiguring)
        {
            Flags = ImGuiWindowFlags.NoTitleBar | 
                   ImGuiWindowFlags.NoScrollbar |
                   ImGuiWindowFlags.NoDocking;
            Size = new Vector2(200, 300);
            SizeCondition = ImGuiCond.FirstUseEver;
        }
        else
        {
            Flags = ImGuiWindowFlags.NoTitleBar | 
                   ImGuiWindowFlags.NoResize | 
                   ImGuiWindowFlags.NoMove | 
                   ImGuiWindowFlags.NoScrollbar | 
                   ImGuiWindowFlags.NoBackground | 
                   ImGuiWindowFlags.NoInputs |
                   ImGuiWindowFlags.NoFocusOnAppearing |
                   ImGuiWindowFlags.NoNav |
                   ImGuiWindowFlags.NoDocking;
        }
    }
    
    public override void PreDraw()
    {
        UpdateWindowFlags();
    }
    
    public override void Draw()
    {
        if (!IsConfiguring && enableFadeOut)
        {
            var timeLeft = (displayEndTime - DateTime.Now).TotalSeconds;
            fadeAlpha = timeLeft < 0.5 && timeLeft > 0 ? (float)(timeLeft / 0.5) : timeLeft <= 0 ? 0 : 1.0f;
        }
        
        if (IsConfiguring)
        {
            DrawConfigurationUI();
        }
        
        DrawFeedback();
    }
    
    private void DrawConfigurationUI()
    {
        ImGui.Text("Visual Feedback Config");
        ImGui.Separator();
        
        ImGui.Text("Drag to move, resize corner");
        var pos = ImGui.GetWindowPos();
        ImGui.Text($"Position: X: {pos.X:F0}, Y: {pos.Y:F0}");
        
        ImGui.Separator();
        
        if (ImGui.Checkbox("Show Background", ref showBackground))
            SaveSettings();
        
        if (showBackground && ImGui.SliderFloat("BG Alpha", ref backgroundAlpha, 0f, 1f))
            SaveSettings();
        
        if (ImGui.SliderFloat("Icon Size", ref iconSize, 20f, 100f))
            SaveSettings();
        
        if (ImGui.Checkbox("Fade Out", ref enableFadeOut))
            SaveSettings();
        
        ImGui.Separator();
        
        if (ImGui.ColorEdit4("Success Color", ref successColor, ImGuiColorEditFlags.NoInputs))
            SaveSettings();
        
        if (ImGui.ColorEdit4("Failure Color", ref failureColor, ImGuiColorEditFlags.NoInputs))
            SaveSettings();
        
        ImGui.Separator();
        
        if (ImGui.Button("Test Success"))
            IsSuccess = true;
        ImGui.SameLine();
        if (ImGui.Button("Test Failure"))
            IsSuccess = false;
        
        ImGui.Separator();
        
        if (ImGui.Button("Save & Close"))
        {
            SaveSettings();
            IsConfiguring = false;
            IsOpen = false;
        }
    }
    
    private void DrawFeedback()
    {
        var drawList = ImGui.GetWindowDrawList();
        var pos = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();
        var center = IsConfiguring ? 
            pos + new Vector2(size.X / 2, size.Y - iconSize - 10) :
            pos + size / 2;
        
        if (showBackground)
        {
            var bgAlpha = IsConfiguring ? backgroundAlpha : backgroundAlpha * fadeAlpha;
            var bgColor = ImGui.GetColorU32(new Vector4(0, 0, 0, bgAlpha));
            
            if (!IsConfiguring)
            {
                drawList.AddRectFilled(pos, pos + size, bgColor, 10f);
            }
            else
            {
                var previewHeight = iconSize * 2.5f;
                var bgStart = new Vector2(pos.X, pos.Y + size.Y - previewHeight);
                drawList.AddRectFilled(bgStart, new Vector2(pos.X + size.X, pos.Y + size.Y), bgColor, 10f);
                
                drawList.AddLine(
                    new Vector2(pos.X, pos.Y + size.Y - previewHeight - 5),
                    new Vector2(pos.X + size.X, pos.Y + size.Y - previewHeight - 5),
                    ImGui.GetColorU32(ImGuiCol.Separator)
                );
            }
        }
        
        if (IsSuccess)
            DrawCheckmark(drawList, center, iconSize);
        else
            DrawX(drawList, center, iconSize);
    }
    
    private void DrawCheckmark(ImDrawListPtr drawList, Vector2 center, float size)
    {
        var alpha = IsConfiguring ? 1f : fadeAlpha;
        var color = ImGui.GetColorU32(new Vector4(successColor.X, successColor.Y, successColor.Z, successColor.W * alpha));
        var thickness = Math.Max(2f, size / 8f);
        
        drawList.AddLine(center + new Vector2(-size * 0.5f, 0), center + new Vector2(-size * 0.1f, size * 0.4f), color, thickness);
        drawList.AddLine(center + new Vector2(-size * 0.1f, size * 0.4f), center + new Vector2(size * 0.5f, -size * 0.4f), color, thickness);
        drawList.AddCircle(center, size, color, 0, thickness);
    }
    
    private void DrawX(ImDrawListPtr drawList, Vector2 center, float size)
    {
        var alpha = IsConfiguring ? 1f : fadeAlpha;
        var color = ImGui.GetColorU32(new Vector4(failureColor.X, failureColor.Y, failureColor.Z, failureColor.W * alpha));
        var thickness = Math.Max(2f, size / 8f);
        var offset = size * 0.5f;
        
        drawList.AddLine(center + new Vector2(-offset, -offset), center + new Vector2(offset, offset), color, thickness);
        drawList.AddLine(center + new Vector2(-offset, offset), center + new Vector2(offset, -offset), color, thickness);
        drawList.AddCircle(center, size, color, 0, thickness);
    }
    
    public override void OnClose()
    {
        if (IsConfiguring)
        {
            SaveSettings();
            IsConfiguring = false;
        }
    }
    
    public void SaveSettings()
    {
        var config = P.config;
        
        if (config.VisualFeedbackSettings == null)
            config.VisualFeedbackSettings = new VisualFeedbackSettings();
        
        if (IsConfiguring || !IsOpen)
        {
            config.VisualFeedbackSettings.Position = Position ?? config.VisualFeedbackSettings.Position;
            config.VisualFeedbackSettings.Size = Size ?? config.VisualFeedbackSettings.Size;
        }
        
        config.VisualFeedbackSettings.ShowBackground = showBackground;
        config.VisualFeedbackSettings.BackgroundAlpha = backgroundAlpha;
        config.VisualFeedbackSettings.IconSize = iconSize;
        config.VisualFeedbackSettings.EnableFadeOut = enableFadeOut;
        config.VisualFeedbackSettings.SuccessColor = successColor;
        config.VisualFeedbackSettings.FailureColor = failureColor;
        
        Svc.PluginInterface.SavePluginConfig(config);
    }
    
    private void LoadSettings()
    {
        var settings = P.config.VisualFeedbackSettings;
        if (settings != null)
        {
            Position = settings.Position;
            PositionCondition = ImGuiCond.FirstUseEver;
            Size = settings.Size;
            SizeCondition = ImGuiCond.FirstUseEver;
            showBackground = settings.ShowBackground;
            backgroundAlpha = settings.BackgroundAlpha;
            iconSize = settings.IconSize;
            enableFadeOut = settings.EnableFadeOut;
            successColor = settings.SuccessColor;
            failureColor = settings.FailureColor;
        }
        else
        {
            var screenSize = ImGui.GetMainViewport().Size;
            Position = new Vector2(screenSize.X / 2 - 60, screenSize.Y / 2 - 60);
            PositionCondition = ImGuiCond.FirstUseEver;
            Size = new Vector2(120, 120);
            SizeCondition = ImGuiCond.FirstUseEver;
        }
    }
}

[Serializable]
public class VisualFeedbackSettings
{
    public Vector2 Position { get; set; } = new Vector2(960, 540);
    public Vector2 Size { get; set; } = new Vector2(120, 120);
    public bool ShowBackground { get; set; } = true;
    public float BackgroundAlpha { get; set; } = 0.7f;
    public float IconSize { get; set; } = 40f;
    public bool EnableFadeOut { get; set; } = true;
    public Vector4 SuccessColor { get; set; } = new Vector4(0.2f, 0.9f, 0.2f, 1f);
    public Vector4 FailureColor { get; set; } = new Vector4(0.9f, 0.2f, 0.2f, 1f);
}
