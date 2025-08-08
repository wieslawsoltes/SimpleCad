using System;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfCircle : DxfEntity
{
    private SKPaint _pen;
    private double _thickness = 1.0;
    private SKPath? _path;
    private SKPath? _fillPath;
    private SKRect? _bounds;

    public DxfCircle()
    {
        _pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color,
            StrokeWidth = (float)_thickness,
        };

        AddProperty(0, "CIRCLE");
    }

    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double Radius { get; set; }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_path is null || Radius <= 0)
        {
            return;
        }

        _pen.StrokeWidth = (float)(_thickness / zoomFactor);
        _pen.Color = Color;

        context.DrawPath(_path, _pen);
    }

    public override void UpdateObject()
    {
        base.UpdateObject();

        // Parse center point (codes 10, 20)
        if (Properties.FirstOrDefault(x => x.Code == 10) is { } centerXProp)
        {
            CenterX = double.Parse(centerXProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 20) is { } centerYProp)
        {
            CenterY = double.Parse(centerYProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse radius (code 40)
        if (Properties.FirstOrDefault(x => x.Code == 40) is { } radiusProp)
        {
            Radius = double.Parse(radiusProp.Data.Trim(), CultureInfo.InvariantCulture);
        }
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();

        // Update or add center point properties
        var centerXProp = Properties.FirstOrDefault(x => x.Code == 10);
        if (centerXProp != null)
        {
            centerXProp.Data = CenterX.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(10, CenterX.ToString(CultureInfo.InvariantCulture));
        }

        var centerYProp = Properties.FirstOrDefault(x => x.Code == 20);
        if (centerYProp != null)
        {
            centerYProp.Data = CenterY.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(20, CenterY.ToString(CultureInfo.InvariantCulture));
        }

        // Update or add radius property
        var radiusProp = Properties.FirstOrDefault(x => x.Code == 40);
        if (radiusProp != null)
        {
            radiusProp.Data = Radius.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(40, Radius.ToString(CultureInfo.InvariantCulture));
        }
    }

    public override void Invalidate()
    {
        _path = CreatePath();
        _fillPath = _pen.GetFillPath(_path);
        _bounds = _fillPath?.Bounds ?? SKRect.Empty;
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

        if (Radius > 0)
        {
            var rect = new SKRect(
                (float)(CenterX - Radius),
                (float)(CenterY - Radius),
                (float)(CenterX + Radius),
                (float)(CenterY + Radius)
            );
            path.AddOval(rect);
        }

        return path;
    }
}
