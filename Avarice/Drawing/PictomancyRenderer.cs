using Pictomancy;

namespace Avarice.Drawing;

internal static class PictomancyRenderer
{
    private static PctDrawList _drawList;

    public static bool IsDrawing => _drawList != null;

    public static void BeginFrame()
    {
        if (!P.config.UsePictomancyRenderer)
        {
            _drawList = null;
            return;
        }

        try
        {
            var hints = new PctDrawHints(
                autoDraw: true,
                maxAlpha: P.config.PictomancyMaxAlpha,
                clipNativeUI: P.config.PictomancyClipNativeUI
            );
            _drawList = PictoService.Draw(ImGui.GetWindowDrawList(), hints);
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Failed to get Pictomancy draw list: {ex.Message}");
            _drawList = null;
        }
    }

    public static void EndFrame()
    {
        if (_drawList == null) return;
        _drawList.Dispose();
        _drawList = null;
    }

    public static void DrawCircle(Vector3 center, float radius, uint color, float thickness)
    {
        if (_drawList == null) return;
        _drawList.AddCircle(center, radius, color, thickness: thickness);
    }

    public static void DrawCircleFilled(Vector3 center, float radius, uint color)
    {
        if (_drawList == null) return;
        _drawList.AddCircleFilled(center, radius, color);
    }

    public static void DrawFan(Vector3 center, float innerRadius, float outerRadius, float startRads, float endRads, uint color, float thickness)
    {
        if (_drawList == null) return;
        _drawList.AddFan(center, innerRadius, outerRadius, startRads, endRads, color, thickness: thickness);
    }

    public static void DrawFanFilled(Vector3 center, float innerRadius, float outerRadius, float startRads, float endRads, uint color)
    {
        if (_drawList == null) return;
        _drawList.AddFanFilled(center, innerRadius, outerRadius, startRads, endRads, color);
    }

    public static void DrawDot(Vector3 position, float size, uint color)
    {
        if (_drawList == null) return;
        _drawList.AddDot(position, size, color);
    }

    public static void PathLineTo(Vector3 point)
    {
        _drawList?.PathLineTo(point);
    }

    public static void PathStroke(uint color, float thickness, bool closed = false)
    {
        if (_drawList == null) return;
        _drawList.PathStroke(color, closed ? PctStrokeFlags.Closed : PctStrokeFlags.None, thickness);
    }

    public static void PathArcTo(Vector3 center, float radius, float startAngle, float endAngle)
    {
        _drawList?.PathArcTo(center, radius, startAngle, endAngle);
    }
}
