using System;
using System.Globalization;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfRasterImage : DxfEntity
{
    private double _insertionPointX, _insertionPointY;
    private double _uVectorX, _uVectorY;
    private double _vVectorX, _vVectorY;
    private double _imageWidth, _imageHeight;
    private string _imagePath = string.Empty;
    private SKRect _bounds = SKRect.Empty;
    private bool _boundsValid = false;
    private SKBitmap? _bitmap;

    public DxfRasterImage()
    {
        AddProperty(0, "IMAGE");
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

    public double UVectorX
    {
        get => _uVectorX;
        set
        {
            _uVectorX = value;
            UpdateOrAddProperty(11, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double UVectorY
    {
        get => _uVectorY;
        set
        {
            _uVectorY = value;
            UpdateOrAddProperty(21, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double VVectorX
    {
        get => _vVectorX;
        set
        {
            _vVectorX = value;
            UpdateOrAddProperty(12, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double VVectorY
    {
        get => _vVectorY;
        set
        {
            _vVectorY = value;
            UpdateOrAddProperty(22, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double ImageWidth
    {
        get => _imageWidth;
        set
        {
            _imageWidth = value;
            UpdateOrAddProperty(13, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public double ImageHeight
    {
        get => _imageHeight;
        set
        {
            _imageHeight = value;
            UpdateOrAddProperty(23, value.ToString(CultureInfo.InvariantCulture));
            Invalidate();
        }
    }

    public string ImagePath
    {
        get => _imagePath;
        set
        {
            _imagePath = value ?? string.Empty;
            UpdateOrAddProperty(1, _imagePath);
            LoadImage();
            Invalidate();
        }
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_bitmap == null || _imageWidth <= 0 || _imageHeight <= 0)
            return;

        // Calculate the transformation matrix
        var matrix = SKMatrix.CreateIdentity();
        
        // Translate to insertion point
        matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)_insertionPointX, (float)_insertionPointY));
        
        // Scale based on image dimensions
        var scaleX = (float)(_imageWidth / _bitmap.Width);
        var scaleY = (float)(_imageHeight / _bitmap.Height);
        matrix = matrix.PostConcat(SKMatrix.CreateScale(scaleX, scaleY));

        using var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High
        };

        context.Save();
        context.SetMatrix(matrix);
        context.DrawBitmap(_bitmap, 0, 0, paint);
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
                case 11: // U-vector X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var uX))
                        _uVectorX = uX;
                    break;
                case 21: // U-vector Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var uY))
                        _uVectorY = uY;
                    break;
                case 12: // V-vector X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var vX))
                        _vVectorX = vX;
                    break;
                case 22: // V-vector Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var vY))
                        _vVectorY = vY;
                    break;
                case 13: // Image width
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var width))
                        _imageWidth = width;
                    break;
                case 23: // Image height
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
                        _imageHeight = height;
                    break;
                case 1: // Image path
                    _imagePath = property.Data ?? string.Empty;
                    break;
            }
        }
        
        LoadImage();
        Invalidate();
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();
        
        UpdateOrAddProperty(10, _insertionPointX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(20, _insertionPointY.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(11, _uVectorX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(21, _uVectorY.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(12, _vVectorX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(22, _vVectorY.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(13, _imageWidth.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(23, _imageHeight.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(1, _imagePath);
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

    private void LoadImage()
    {
        _bitmap?.Dispose();
        _bitmap = null;

        if (string.IsNullOrEmpty(_imagePath) || !File.Exists(_imagePath))
            return;

        try
        {
            _bitmap = SKBitmap.Decode(_imagePath);
        }
        catch
        {
            // Failed to load image, keep bitmap as null
        }
    }

    private SKRect CalculateBounds()
    {
        if (_imageWidth <= 0 || _imageHeight <= 0)
            return SKRect.Empty;

        var corners = new[]
        {
            new SKPoint((float)_insertionPointX, (float)_insertionPointY),
            new SKPoint((float)(_insertionPointX + _imageWidth), (float)_insertionPointY),
            new SKPoint((float)(_insertionPointX + _imageWidth), (float)(_insertionPointY + _imageHeight)),
            new SKPoint((float)_insertionPointX, (float)(_insertionPointY + _imageHeight))
        };

        var minX = corners.Min(p => p.X);
        var minY = corners.Min(p => p.Y);
        var maxX = corners.Max(p => p.X);
        var maxY = corners.Max(p => p.Y);

        return new SKRect(minX, minY, maxX, maxY);
    }
}
