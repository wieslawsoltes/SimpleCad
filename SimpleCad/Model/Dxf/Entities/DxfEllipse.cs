using System;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfEllipse : DxfEntity
{
    private SKPaint _pen;
    private double _thickness = 1.0;
    private SKPath? _path;
    private SKPath? _fillPath;
    private SKRect? _bounds;

    public DxfEllipse()
    {
        _pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color,
            StrokeWidth = (float)_thickness,
        };

        AddProperty(0, "ELLIPSE");
    }

    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double MajorAxisEndpointX { get; set; }
    public double MajorAxisEndpointY { get; set; }
    public double MinorToMajorRatio { get; set; } = 1.0;
    public double StartParameter { get; set; } = 0.0;
    public double EndParameter { get; set; } = 2 * Math.PI;

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

        // Parse center point (codes 10, 20)
        if (Properties.FirstOrDefault(x => x.Code == 10) is { } centerXProp)
        {
            CenterX = double.Parse(centerXProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 20) is { } centerYProp)
        {
            CenterY = double.Parse(centerYProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse major axis endpoint (codes 11, 21)
        if (Properties.FirstOrDefault(x => x.Code == 11) is { } majorXProp)
        {
            MajorAxisEndpointX = double.Parse(majorXProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 21) is { } majorYProp)
        {
            MajorAxisEndpointY = double.Parse(majorYProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse minor to major axis ratio (code 40)
        if (Properties.FirstOrDefault(x => x.Code == 40) is { } ratioProp)
        {
            MinorToMajorRatio = double.Parse(ratioProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse start parameter (code 41)
        if (Properties.FirstOrDefault(x => x.Code == 41) is { } startProp)
        {
            StartParameter = double.Parse(startProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse end parameter (code 42)
        if (Properties.FirstOrDefault(x => x.Code == 42) is { } endProp)
        {
            EndParameter = double.Parse(endProp.Data.Trim(), CultureInfo.InvariantCulture);
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

        // Update or add major axis endpoint properties
        var majorXProp = Properties.FirstOrDefault(x => x.Code == 11);
        if (majorXProp != null)
        {
            majorXProp.Data = MajorAxisEndpointX.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(11, MajorAxisEndpointX.ToString(CultureInfo.InvariantCulture));
        }

        var majorYProp = Properties.FirstOrDefault(x => x.Code == 21);
        if (majorYProp != null)
        {
            majorYProp.Data = MajorAxisEndpointY.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(21, MajorAxisEndpointY.ToString(CultureInfo.InvariantCulture));
        }

        // Update or add ratio property
        var ratioProp = Properties.FirstOrDefault(x => x.Code == 40);
        if (ratioProp != null)
        {
            ratioProp.Data = MinorToMajorRatio.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(40, MinorToMajorRatio.ToString(CultureInfo.InvariantCulture));
        }

        // Update or add start parameter property
        var startProp = Properties.FirstOrDefault(x => x.Code == 41);
        if (startProp != null)
        {
            startProp.Data = StartParameter.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(41, StartParameter.ToString(CultureInfo.InvariantCulture));
        }

        // Update or add end parameter property
        var endProp = Properties.FirstOrDefault(x => x.Code == 42);
        if (endProp != null)
        {
            endProp.Data = EndParameter.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(42, EndParameter.ToString(CultureInfo.InvariantCulture));
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

        // Calculate major axis length
        var majorAxisLength = Math.Sqrt(MajorAxisEndpointX * MajorAxisEndpointX + MajorAxisEndpointY * MajorAxisEndpointY);
        var minorAxisLength = majorAxisLength * MinorToMajorRatio;

        if (majorAxisLength <= 0 || minorAxisLength <= 0)
        {
            return path;
        }

        // Calculate rotation angle from major axis vector
        var rotationAngle = Math.Atan2(MajorAxisEndpointY, MajorAxisEndpointX);

        // Create ellipse bounds
        var rect = new SKRect(
            (float)-majorAxisLength,
            (float)-minorAxisLength,
            (float)majorAxisLength,
            (float)minorAxisLength
        );

        // Check if it's a full ellipse or an arc
        var isFullEllipse = Math.Abs(EndParameter - StartParameter - 2 * Math.PI) < 0.001;

        if (isFullEllipse)
        {
            path.AddOval(rect);
        }
        else
        {
            // Create elliptical arc
            var startAngleDegrees = (float)(StartParameter * 180.0 / Math.PI);
            var sweepAngleDegrees = (float)((EndParameter - StartParameter) * 180.0 / Math.PI);
            path.AddArc(rect, startAngleDegrees, sweepAngleDegrees);
        }

        // Apply transformations: translation and rotation
        var matrix = SKMatrix.CreateIdentity();
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotation((float)rotationAngle));
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)CenterX, (float)CenterY));
        
        path.Transform(matrix);

        return path;
    }
}
