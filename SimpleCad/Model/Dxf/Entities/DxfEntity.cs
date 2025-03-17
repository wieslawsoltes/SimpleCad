using SkiaSharp;

namespace SimpleCad.Model;

public abstract class DxfEntity
{
    public abstract void Render(SKCanvas context, double zoomFactor);

    public abstract void Invalidate();

    public abstract bool Contains(float x, float y);

    public abstract SKRect GetBounds();
}
