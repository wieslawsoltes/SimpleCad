using SkiaSharp;

namespace SimpleCad.Model;

public class DxfLineEntity : DxfEntity
{
    private SKPaint _pen;
    private double _thickness = 1.0;
    private SKPath? _path;
    private SKPath? _fillPath;
    private SKRect? _bounds;

    public DxfLineEntity()
    {
        _pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = (float)_thickness,
        };
    }

    public double StartPointX { get; set; }
    
    public double StartPointY { get; set; }
    
    public double EndPointX { get; set; }
    
    public double EndPointY { get; set; }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_path is null)
        {
            return;
        }

        _pen.StrokeWidth = (float)(_thickness / zoomFactor);

        context.DrawPath(_path, _pen);
    }

    public override void Invalidate()
    {
        _path = CreatePath();
        _fillPath = _pen.GetFillPath(_path);
        _bounds = _fillPath.Bounds;
    }

    public override bool Contains(float x, float y)
    {
        if (_fillPath is null)
        {
            return false;
        }

        return _fillPath.Contains(x, y);
    }

    public override SKRect GetBounds()
    {
        return _bounds ?? SKRect.Empty;
    }

    private SKPath CreatePath()
    {
        var path = new SKPath();

        path.MoveTo((float)StartPointX, (float)StartPointY);
        path.LineTo((float)EndPointX, (float)EndPointY);

        return path;
    }
}
