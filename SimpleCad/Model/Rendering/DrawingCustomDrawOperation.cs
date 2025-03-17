using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;

namespace SimpleCad.Model;

public class DrawingCustomDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly IDrawing _drawing;

    public DrawingCustomDrawOperation(Rect bounds, IDrawing drawing)
    {
        _bounds = bounds;
        _drawing = drawing;
    }

    public Rect Bounds => _bounds;

    public bool Equals(ICustomDrawOperation? other)
    {
        return false;
    }

    public void Dispose()
    {
    }

    public bool HitTest(Point p)
    {
        return false;
    }

    public void Render(ImmediateDrawingContext context)
    {
        var skiaSharpApiLeaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (skiaSharpApiLeaseFeature is null)
        {
            return;
        }
        
        using var skiaSharpApiLease = skiaSharpApiLeaseFeature.Lease();

        var bounds = new Rect(0, 0, _bounds.Width, _bounds.Height);

        _drawing.Render(skiaSharpApiLease.SkCanvas, bounds);
    }
}
