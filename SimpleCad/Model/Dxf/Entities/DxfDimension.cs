using SkiaSharp;

namespace SimpleCad.Model;

public class DxfDimension : DxfEntity
{
    public DxfDimension()
    {
        AddProperty(0, "DIMENSION");
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
