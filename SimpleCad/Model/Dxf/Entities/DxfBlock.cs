using SkiaSharp;

namespace SimpleCad.Model;

public class DxfBlock : DxfEntity
{
    public DxfBlock()
    {
        AddProperty(0, "BLOCK");
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        // TODO:
        foreach (var child in Children)
        {
            if (child is DxfEntity entity)
            {
                entity.Render(context, zoomFactor);
            }
        }
    }

    public override void Invalidate()
    {
        // TODO:
        foreach (var child in Children)
        {
            if (child is DxfEntity entity)
            {
                entity.Invalidate();
            }
        }
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
