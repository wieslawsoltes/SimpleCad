using SkiaSharp;
using System;
using System.Globalization;
using System.Linq;

namespace SimpleCad.Model;

public class DxfArc : DxfEntity
{
    private double _centerX;
    private double _centerY;
    private double _radius;
    private double _startAngle; // In degrees
    private double _endAngle;   // In degrees
    private SKRect _bounds = SKRect.Empty;
    private bool _boundsValid = false;

    public DxfArc()
    {
        AddProperty(0, "ARC");
    }

    public double CenterX
    {
        get => _centerX;
        set
        {
            _centerX = value;
            UpdateOrAddProperty(10, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double CenterY
    {
        get => _centerY;
        set
        {
            _centerY = value;
            UpdateOrAddProperty(20, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            UpdateOrAddProperty(40, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double StartAngle
    {
        get => _startAngle;
        set
        {
            _startAngle = value;
            UpdateOrAddProperty(50, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double EndAngle
    {
        get => _endAngle;
        set
        {
            _endAngle = value;
            UpdateOrAddProperty(51, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_radius <= 0) return;

        using var paint = new SKPaint
        {
            Color = Color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = (float)(1.0 / zoomFactor),
            IsAntialias = true
        };

        // Convert center and radius to screen coordinates
        var centerX = (float)_centerX;
        var centerY = (float)_centerY;
        var radius = (float)_radius;

        // Convert angles from degrees to radians and adjust for SkiaSharp coordinate system
        var startAngleRad = (float)(_startAngle * Math.PI / 180.0);
        var endAngleRad = (float)(_endAngle * Math.PI / 180.0);
        
        // Calculate sweep angle
        var sweepAngle = endAngleRad - startAngleRad;
        if (sweepAngle <= 0)
            sweepAngle += (float)(2 * Math.PI);

        // Create the arc path
        using var path = new SKPath();
        var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        
        // Convert radians to degrees for SkiaSharp
        var startAngleDeg = (float)(_startAngle);
        var sweepAngleDeg = (float)(sweepAngle * 180.0 / Math.PI);
        
        path.AddArc(rect, startAngleDeg, sweepAngleDeg);
        
        context.DrawPath(path, paint);
    }

    public override void UpdateObject()
    {
        base.UpdateObject();
        
        foreach (var property in Properties)
        {
            switch (property.Code)
            {
                case 10: // Center point X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                        _centerX = x;
                    break;
                case 20: // Center point Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                        _centerY = y;
                    break;
                case 40: // Radius
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var radius))
                        _radius = radius;
                    break;
                case 50: // Start angle
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var startAngle))
                        _startAngle = startAngle;
                    break;
                case 51: // End angle
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var endAngle))
                        _endAngle = endAngle;
                    break;
            }
        }
        
        Invalidate();
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();
        
        UpdateOrAddProperty(10, _centerX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(20, _centerY.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(40, _radius.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(50, _startAngle.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(51, _endAngle.ToString(CultureInfo.InvariantCulture));
    }

    public override void Invalidate()
    {
        _boundsValid = false;
    }

    public override bool Contains(float x, float y)
    {
        if (_radius <= 0) return false;

        // Calculate distance from point to center
        var dx = x - _centerX;
        var dy = y - _centerY;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        
        // Check if point is approximately on the arc (within tolerance)
        var tolerance = Math.Max(1.0, _radius * 0.05); // 5% of radius or minimum 1 unit
        if (Math.Abs(distance - _radius) > tolerance)
            return false;
        
        // Check if point is within the angular range
        var angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
        if (angle < 0) angle += 360;
        
        var startAngle = _startAngle;
        var endAngle = _endAngle;
        
        // Normalize angles
        while (startAngle < 0) startAngle += 360;
        while (endAngle < 0) endAngle += 360;
        while (startAngle >= 360) startAngle -= 360;
        while (endAngle >= 360) endAngle -= 360;
        
        if (startAngle <= endAngle)
        {
            return angle >= startAngle && angle <= endAngle;
        }
        else
        {
            return angle >= startAngle || angle <= endAngle;
        }
    }

    public override SKRect GetBounds()
    {
        if (_boundsValid)
            return _bounds;
            
        _bounds = CalculateBounds();
        _boundsValid = true;
        return _bounds;
    }

    private void UpdateOrAddProperty(int code, string value)
    {
        var existingProperty = Properties.FirstOrDefault(p => p.Code == code);
        if (existingProperty != null)
        {
            existingProperty.Data = value;
        }
        else
        {
            AddProperty(code, value);
        }
    }

    private SKRect CalculateBounds()
    {
        if (_radius <= 0)
            return SKRect.Empty;

        // For a complete circle, bounds would be center ± radius
        // For an arc, we need to check if the arc crosses the extreme points
        var minX = (float)(_centerX - _radius);
        var maxX = (float)(_centerX + _radius);
        var minY = (float)(_centerY - _radius);
        var maxY = (float)(_centerY + _radius);

        // Start with the arc endpoints
        var startAngleRad = _startAngle * Math.PI / 180.0;
        var endAngleRad = _endAngle * Math.PI / 180.0;
        
        var startX = (float)(_centerX + _radius * Math.Cos(startAngleRad));
        var startY = (float)(_centerY + _radius * Math.Sin(startAngleRad));
        var endX = (float)(_centerX + _radius * Math.Cos(endAngleRad));
        var endY = (float)(_centerY + _radius * Math.Sin(endAngleRad));
        
        minX = Math.Min(Math.Min(startX, endX), minX);
        maxX = Math.Max(Math.Max(startX, endX), maxX);
        minY = Math.Min(Math.Min(startY, endY), minY);
        maxY = Math.Max(Math.Max(startY, endY), maxY);
        
        // Check if arc crosses the extreme angles (0°, 90°, 180°, 270°)
        var normalizedStart = _startAngle;
        var normalizedEnd = _endAngle;
        
        while (normalizedStart < 0) normalizedStart += 360;
        while (normalizedEnd < 0) normalizedEnd += 360;
        while (normalizedStart >= 360) normalizedStart -= 360;
        while (normalizedEnd >= 360) normalizedEnd -= 360;
        
        var extremeAngles = new[] { 0, 90, 180, 270 };
        
        foreach (var angle in extremeAngles)
        {
            bool crossesAngle;
            if (normalizedStart <= normalizedEnd)
            {
                crossesAngle = angle >= normalizedStart && angle <= normalizedEnd;
            }
            else
            {
                crossesAngle = angle >= normalizedStart || angle <= normalizedEnd;
            }
            
            if (crossesAngle)
            {
                var angleRad = angle * Math.PI / 180.0;
                var extremeX = (float)(_centerX + _radius * Math.Cos(angleRad));
                var extremeY = (float)(_centerY + _radius * Math.Sin(angleRad));
                
                minX = Math.Min(minX, extremeX);
                maxX = Math.Max(maxX, extremeX);
                minY = Math.Min(minY, extremeY);
                maxY = Math.Max(maxY, extremeY);
            }
        }
        
        return new SKRect(minX, minY, maxX, maxY);
    }
}
