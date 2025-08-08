using System;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfOle2Frame : DxfEntity
{
    private double _insertionPointX, _insertionPointY;
    private double _width, _height;
    private double _rotation;
    private string _oleObjectType = string.Empty;
    private SKRect _bounds = SKRect.Empty;
    private bool _boundsValid = false;

    public DxfOle2Frame()
    {
        AddProperty(0, "OLE2FRAME");
    }

    public double InsertionPointX
    {
        get => _insertionPointX;
        set
        {
            _insertionPointX = value;
            UpdateOrAddProperty(10, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double InsertionPointY
    {
        get => _insertionPointY;
        set
        {
            _insertionPointY = value;
            UpdateOrAddProperty(20, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Width
    {
        get => _width;
        set
        {
            _width = value;
            UpdateOrAddProperty(40, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Height
    {
        get => _height;
        set
        {
            _height = value;
            UpdateOrAddProperty(41, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            UpdateOrAddProperty(50, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public string OleObjectType
    {
        get => _oleObjectType;
        set
        {
            _oleObjectType = value ?? string.Empty;
            UpdateOrAddProperty(1, _oleObjectType);
            Invalidate();
        }
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_width <= 0 || _height <= 0)
            return;

        using var framePaint = new SKPaint
        {
            Color = SKColors.Gray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = (float)(2.0 / zoomFactor),
            IsAntialias = true
        };

        using var fillPaint = new SKPaint
        {
            Color = SKColors.LightGray.WithAlpha(128),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            TextSize = (float)(12.0 / zoomFactor)
        };

        context.Save();
        
        // Apply rotation if needed
        if (Math.Abs(_rotation) > 0.001)
        {
            context.RotateDegrees((float)_rotation, (float)_insertionPointX, (float)_insertionPointY);
        }

        // Draw the frame rectangle
        var rect = new SKRect(
            (float)_insertionPointX,
            (float)_insertionPointY,
            (float)(_insertionPointX + _width),
            (float)(_insertionPointY + _height)
        );

        context.DrawRect(rect, fillPaint);
        context.DrawRect(rect, framePaint);

        // Draw "OLE" text in the center
        var text = string.IsNullOrEmpty(_oleObjectType) ? "OLE" : _oleObjectType;
        var textBounds = new SKRect();
        textPaint.MeasureText(text, ref textBounds);
        
        var textX = (float)(_insertionPointX + _width / 2 - textBounds.Width / 2);
        var textY = (float)(_insertionPointY + _height / 2 - textBounds.Height / 2);
        
        context.DrawText(text, textX, textY, textPaint);

        context.Restore();
    }

    public override void UpdateObject()
    {
        base.UpdateObject();
        
        foreach (var property in Properties)
        {
            switch (property.Code)
            {
                case 10: // Insertion point X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var ipX))
                        _insertionPointX = ipX;
                    break;
                case 20: // Insertion point Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var ipY))
                        _insertionPointY = ipY;
                    break;
                case 40: // Width
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var width))
                        _width = width;
                    break;
                case 41: // Height
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
                        _height = height;
                    break;
                case 50: // Rotation angle
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var rotation))
                        _rotation = rotation;
                    break;
                case 1: // OLE object type
                    _oleObjectType = property.Data ?? string.Empty;
                    break;
            }
        }
        
        Invalidate();
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();
        
        UpdateOrAddProperty(10, _insertionPointX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(20, _insertionPointY.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(40, _width.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(41, _height.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(50, _rotation.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(1, _oleObjectType);
    }

    public override void Invalidate()
    {
        _boundsValid = false;
    }

    public override bool Contains(float x, float y)
    {
        var bounds = GetBounds();
        return bounds.Contains(x, y);
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
        if (_width <= 0 || _height <= 0)
            return SKRect.Empty;

        // For simplicity, return axis-aligned bounds (ignoring rotation)
        return new SKRect(
            (float)_insertionPointX,
            (float)_insertionPointY,
            (float)(_insertionPointX + _width),
            (float)(_insertionPointY + _height)
        );
    }
}
