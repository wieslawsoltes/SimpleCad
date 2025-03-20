using SkiaSharp;

namespace SimpleCad.Model;

public class DxfSpline : DxfEntity
{
    public DxfSpline()
    {
        AddProperty(0, "SPLINE");
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        // TODO:
    }

    public override void Invalidate()
    {
        // TODO:
    }

    public override bool Contains(float x, float y)
    {
        // TODO:
        return false;
    }

    public override SKRect GetBounds()
    {
        // TODO:
        return SKRect.Empty;
    }
}
