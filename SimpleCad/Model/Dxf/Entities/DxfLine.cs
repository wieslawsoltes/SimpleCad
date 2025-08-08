using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfLine : DxfEntity
{
    private SKPaint _pen;
    private double _thickness = 1.0;
    private SKPath? _path;
    private SKPath? _fillPath;
    private SKRect? _bounds;

    public DxfLine()
    {
        _pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color,
            StrokeWidth = (float)_thickness,
        };

        AddProperty(0, "LINE");
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
        _pen.Color = Color;

        context.DrawPath(_path, _pen);
    }

    public override void UpdateObject()
    {
        base.UpdateObject();

        if (Properties.FirstOrDefault(x => x.Code == 10) is { } startPointXProp)
        {
            StartPointX = double.Parse(startPointXProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 20) is { } startPointYProp)
        {
            StartPointY = double.Parse(startPointYProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 11) is { } endPointXProp)
        {
            EndPointX = double.Parse(endPointXProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 21) is { } endPointYProp)
        {
            EndPointY = double.Parse(endPointYProp.Data.Trim(), CultureInfo.InvariantCulture);
        }
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();

        if (Properties.FirstOrDefault(x => x.Code == 10) is { } startPointXProp)
        {
            startPointXProp.Data = StartPointX.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(10, StartPointX.ToString(CultureInfo.InvariantCulture));
        }

        if (Properties.FirstOrDefault(x => x.Code == 20) is { } startPointYProp)
        {
            startPointYProp.Data = StartPointY.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(20, StartPointY.ToString(CultureInfo.InvariantCulture));
        }

        if (Properties.FirstOrDefault(x => x.Code == 11) is { } endPointXProp)
        {
            endPointXProp.Data = EndPointX.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(11, EndPointX.ToString(CultureInfo.InvariantCulture));
        }

        if (Properties.FirstOrDefault(x => x.Code == 21) is { } endPointYProp)
        {
            endPointYProp.Data = EndPointY.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(21, EndPointY.ToString(CultureInfo.InvariantCulture));
        }
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
