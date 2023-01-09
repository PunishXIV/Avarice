namespace Avarice.Drawing;

[Serializable]
public struct Brush
{
    public float Thickness;
    public Vector4 Color;
    public Vector4 Fill;
    public DisplayCondition DisplayCondition;

    public bool HasFill()
    {
        return Fill.W != 0;
    }
}
