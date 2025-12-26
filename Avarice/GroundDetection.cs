using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace Avarice;

// Thanks Splatoon for the BGCollision raycast idea
internal static class GroundDetection
{
    public static unsafe Vector3? GetFloorPosition(Vector3 position, float maxDistance = 100f)
    {
        try
        {
            var direction = new Vector3(0, -1, 0);
            if (BGCollisionModule.RaycastMaterialFilter(position, direction, out var hit, maxDistance))
            {
                return hit.Point;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"BGCollision raycast failed: {ex.Message}");
        }
        return null;
    }

    public static Vector3 GetAutoGroundedPosition(Vector3 position, float threshold = 1.0f)
    {
        var floorPoint = GetFloorPosition(position);
        if (floorPoint.HasValue)
        {
            var heightDifference = position.Y - floorPoint.Value.Y;
            if (heightDifference > threshold)
            {
                return new Vector3(position.X, floorPoint.Value.Y, position.Z);
            }
        }
        return position;
    }
}
