using SkiaSharp;
using System;
using System.Globalization;
using System.Linq;

namespace SimpleCad.Model;

public class DxfDimension : DxfEntity
{
    private double _defPoint1X, _defPoint1Y; // First definition point
    private double _defPoint2X, _defPoint2Y; // Second definition point
    private double _dimLineX, _dimLineY;     // Dimension line location
    private double _textX, _textY;           // Text middle point
    private string _dimensionText = "";
    private double _textHeight = 2.5;
    private double _arrowSize = 2.5;
    private int _dimensionType = 0; // 0=Linear, 1=Aligned, 2=Angular, etc.
    private SKRect _bounds = SKRect.Empty;
    private bool _boundsValid = false;

    public DxfDimension()
    {
        AddProperty(0, "DIMENSION");
    }

    public double DefPoint1X
    {
        get => _defPoint1X;
        set
        {
            _defPoint1X = value;
            UpdateOrAddProperty(13, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double DefPoint1Y
    {
        get => _defPoint1Y;
        set
        {
            _defPoint1Y = value;
            UpdateOrAddProperty(23, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double DefPoint2X
    {
        get => _defPoint2X;
        set
        {
            _defPoint2X = value;
            UpdateOrAddProperty(14, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double DefPoint2Y
    {
        get => _defPoint2Y;
        set
        {
            _defPoint2Y = value;
            UpdateOrAddProperty(24, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double DimLineX
    {
        get => _dimLineX;
        set
        {
            _dimLineX = value;
            UpdateOrAddProperty(10, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double DimLineY
    {
        get => _dimLineY;
        set
        {
            _dimLineY = value;
            UpdateOrAddProperty(20, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public string DimensionText
    {
        get => _dimensionText;
        set
        {
            _dimensionText = value;
            UpdateOrAddProperty(1, value);
            Invalidate();
        }
    }

    public double TextHeight
    {
        get => _textHeight;
        set
        {
            _textHeight = value;
            UpdateOrAddProperty(140, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        // Calculate dimension line endpoints and text position
        var distance = Math.Sqrt(Math.Pow(_defPoint2X - _defPoint1X, 2) + Math.Pow(_defPoint2Y - _defPoint1Y, 2));
        
        if (distance < 0.001) return; // Too small to render

        using var linePaint = new SKPaint
        {
            Color = Color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = (float)(1.0 / zoomFactor),
            IsAntialias = true
        };

        using var textPaint = new SKPaint
        {
            Color = Color,
            Style = SKPaintStyle.Fill,
            TextSize = (float)(_textHeight * zoomFactor * 0.8),
            IsAntialias = true,
            Typeface = SKTypeface.Default
        };

        // Calculate dimension line direction
        var dirX = (_defPoint2X - _defPoint1X) / distance;
        var dirY = (_defPoint2Y - _defPoint1Y) / distance;
        
        // Calculate perpendicular direction for extension lines
        var perpX = -dirY;
        var perpY = dirX;

        // Calculate dimension line position
        var dimLineStartX = _defPoint1X;
        var dimLineStartY = _defPoint1Y;
        var dimLineEndX = _defPoint2X;
        var dimLineEndY = _defPoint2Y;

        // If dimension line location is specified, project it
        if (Math.Abs(_dimLineX) > 0.001 || Math.Abs(_dimLineY) > 0.001)
        {
            // Project dimension line location onto the perpendicular
            var midX = (_defPoint1X + _defPoint2X) / 2;
            var midY = (_defPoint1Y + _defPoint2Y) / 2;
            var offsetX = _dimLineX - midX;
            var offsetY = _dimLineY - midY;
            var offset = offsetX * perpX + offsetY * perpY;
            
            dimLineStartX = _defPoint1X + offset * perpX;
            dimLineStartY = _defPoint1Y + offset * perpY;
            dimLineEndX = _defPoint2X + offset * perpX;
            dimLineEndY = _defPoint2Y + offset * perpY;
        }

        // Draw extension lines
        var extLineLength = Math.Abs((_dimLineX - (_defPoint1X + _defPoint2X) / 2) * perpX + (_dimLineY - (_defPoint1Y + _defPoint2Y) / 2) * perpY) + _textHeight;
        
        // Extension line 1
        context.DrawLine(
            (float)_defPoint1X, (float)_defPoint1Y,
            (float)(dimLineStartX + extLineLength * 0.2 * perpX), (float)(dimLineStartY + extLineLength * 0.2 * perpY),
            linePaint);
        
        // Extension line 2
        context.DrawLine(
            (float)_defPoint2X, (float)_defPoint2Y,
            (float)(dimLineEndX + extLineLength * 0.2 * perpX), (float)(dimLineEndY + extLineLength * 0.2 * perpY),
            linePaint);

        // Draw dimension line
        context.DrawLine(
            (float)dimLineStartX, (float)dimLineStartY,
            (float)dimLineEndX, (float)dimLineEndY,
            linePaint);

        // Draw arrows
        var arrowSize = _arrowSize / zoomFactor;
        DrawArrow(context, linePaint, (float)dimLineStartX, (float)dimLineStartY, (float)dirX, (float)dirY, (float)arrowSize);
        DrawArrow(context, linePaint, (float)dimLineEndX, (float)dimLineEndY, -(float)dirX, -(float)dirY, (float)arrowSize);

        // Draw dimension text
        var text = string.IsNullOrEmpty(_dimensionText) ? distance.ToString("F2") : _dimensionText;
        var textX = (float)((dimLineStartX + dimLineEndX) / 2);
        var textY = (float)((dimLineStartY + dimLineEndY) / 2);
        
        if (Math.Abs(_textX) > 0.001 || Math.Abs(_textY) > 0.001)
        {
            textX = (float)_textX;
            textY = (float)_textY;
        }

        // Measure text to center it
        var textBounds = new SKRect();
        textPaint.MeasureText(text, ref textBounds);
        
        context.DrawText(text, textX - textBounds.Width / 2, textY + textBounds.Height / 2, textPaint);
    }

    private void DrawArrow(SKCanvas context, SKPaint paint, float x, float y, float dirX, float dirY, float size)
    {
        var arrowAngle = Math.PI / 6; // 30 degrees
        
        // Calculate arrow points
        var arrow1X = x + size * (dirX * Math.Cos(arrowAngle) - dirY * Math.Sin(arrowAngle));
        var arrow1Y = y + size * (dirX * Math.Sin(arrowAngle) + dirY * Math.Cos(arrowAngle));
        var arrow2X = x + size * (dirX * Math.Cos(-arrowAngle) - dirY * Math.Sin(-arrowAngle));
        var arrow2Y = y + size * (dirX * Math.Sin(-arrowAngle) + dirY * Math.Cos(-arrowAngle));
        
        // Draw arrow lines
        context.DrawLine(x, y, (float)arrow1X, (float)arrow1Y, paint);
        context.DrawLine(x, y, (float)arrow2X, (float)arrow2Y, paint);
    }

    public override void UpdateObject()
    {
        base.UpdateObject();
        
        foreach (var property in Properties)
        {
            switch (property.Code)
            {
                case 1: // Dimension text
                    _dimensionText = property.Data;
                    break;
                case 10: // Dimension line location X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var dimX))
                        _dimLineX = dimX;
                    break;
                case 20: // Dimension line location Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var dimY))
                        _dimLineY = dimY;
                    break;
                case 11: // Text middle point X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var textX))
                        _textX = textX;
                    break;
                case 21: // Text middle point Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var textY))
                        _textY = textY;
                    break;
                case 13: // Definition point 1 X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var def1X))
                        _defPoint1X = def1X;
                    break;
                case 23: // Definition point 1 Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var def1Y))
                        _defPoint1Y = def1Y;
                    break;
                case 14: // Definition point 2 X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var def2X))
                        _defPoint2X = def2X;
                    break;
                case 24: // Definition point 2 Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var def2Y))
                        _defPoint2Y = def2Y;
                    break;
                case 70: // Dimension type
                    if (int.TryParse(property.Data, out var dimType))
                        _dimensionType = dimType;
                    break;
                case 140: // Text height
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
                        _textHeight = height;
                    break;
            }
        }
        
        Invalidate();
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();
        
        UpdateOrAddProperty(1, _dimensionText);
        UpdateOrAddProperty(10, _dimLineX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(20, _dimLineY.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(11, _textX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(21, _textY.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(13, _defPoint1X.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(23, _defPoint1Y.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(14, _defPoint2X.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(24, _defPoint2Y.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(70, _dimensionType.ToString());
        UpdateOrAddProperty(140, _textHeight.ToString(CultureInfo.InvariantCulture));
    }

    public override void Invalidate()
    {
        _boundsValid = false;
    }

    public override bool Contains(float x, float y)
    {
        // Check if point is near any of the dimension elements
        var tolerance = Math.Max(1.0, _textHeight * 0.5);
        
        // Check dimension line
        var distance = PointToLineDistance(x, y, _defPoint1X, _defPoint1Y, _defPoint2X, _defPoint2Y);
        if (distance <= tolerance)
            return true;
        
        // Check text area
        var textCenterX = (_defPoint1X + _defPoint2X) / 2;
        var textCenterY = (_defPoint1Y + _defPoint2Y) / 2;
        if (Math.Abs(_textX) > 0.001 || Math.Abs(_textY) > 0.001)
        {
            textCenterX = _textX;
            textCenterY = _textY;
        }
        
        var textDistance = Math.Sqrt(Math.Pow(x - textCenterX, 2) + Math.Pow(y - textCenterY, 2));
        return textDistance <= _textHeight;
    }

    private double PointToLineDistance(double px, double py, double x1, double y1, double x2, double y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = Math.Sqrt(dx * dx + dy * dy);
        
        if (length < 0.001)
            return Math.Sqrt(Math.Pow(px - x1, 2) + Math.Pow(py - y1, 2));
        
        var t = Math.Max(0, Math.Min(1, ((px - x1) * dx + (py - y1) * dy) / (length * length)));
        var projX = x1 + t * dx;
        var projY = y1 + t * dy;
        
        return Math.Sqrt(Math.Pow(px - projX, 2) + Math.Pow(py - projY, 2));
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
        var points = new[]
        {
            new SKPoint((float)_defPoint1X, (float)_defPoint1Y),
            new SKPoint((float)_defPoint2X, (float)_defPoint2Y),
            new SKPoint((float)_dimLineX, (float)_dimLineY),
            new SKPoint((float)_textX, (float)_textY)
        };

        if (points.Length == 0)
            return SKRect.Empty;

        var minX = points.Min(p => p.X) - (float)_textHeight;
        var minY = points.Min(p => p.Y) - (float)_textHeight;
        var maxX = points.Max(p => p.X) + (float)_textHeight;
        var maxY = points.Max(p => p.Y) + (float)_textHeight;

        return new SKRect(minX, minY, maxX, maxY);
    }
}
