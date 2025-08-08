using System;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfText : DxfEntity
{
    private SKPaint _textPaint;
    private SKRect? _bounds;
    private string _measuredText = string.Empty;

    public DxfText()
    {
        _textPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = Color,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial"),
            TextSize = 12
        };

        AddProperty(0, "TEXT");
    }

    public string TextValue { get; set; } = string.Empty;
    public double InsertionPointX { get; set; }
    public double InsertionPointY { get; set; }
    public double Height { get; set; } = 1.0;
    public double RotationAngle { get; set; } = 0.0;
    public double WidthScaleFactor { get; set; } = 1.0;
    public double ObliqueAngle { get; set; } = 0.0;
    public string TextStyle { get; set; } = "STANDARD";
    public int TextGenerationFlags { get; set; } = 0;
    public int HorizontalAlignment { get; set; } = 0;
    public int VerticalAlignment { get; set; } = 0;
    public double AlignmentPointX { get; set; }
    public double AlignmentPointY { get; set; }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (string.IsNullOrEmpty(TextValue) || Height <= 0)
        {
            return;
        }

        // Calculate text size based on height and zoom
        var textSize = (float)(Height / zoomFactor);
        _textPaint.TextSize = textSize;
        _textPaint.Color = Color;

        // Apply width scale factor
        _textPaint.TextScaleX = (float)WidthScaleFactor;

        // Calculate text position
        var x = (float)InsertionPointX;
        var y = (float)InsertionPointY;

        // Adjust position based on alignment
        if (HorizontalAlignment != 0 || VerticalAlignment != 0)
        {
            x = (float)AlignmentPointX;
            y = (float)AlignmentPointY;
        }

        // Apply horizontal alignment
        switch (HorizontalAlignment)
        {
            case 1: // Center
                _textPaint.TextAlign = SKTextAlign.Center;
                break;
            case 2: // Right
                _textPaint.TextAlign = SKTextAlign.Right;
                break;
            default: // Left
                _textPaint.TextAlign = SKTextAlign.Left;
                break;
        }

        // Calculate vertical offset based on vertical alignment
        var verticalOffset = 0f;
        var fontMetrics = _textPaint.FontMetrics;
        switch (VerticalAlignment)
        {
            case 1: // Bottom
                verticalOffset = -fontMetrics.Descent;
                break;
            case 2: // Middle
                verticalOffset = -(fontMetrics.Ascent + fontMetrics.Descent) / 2;
                break;
            case 3: // Top
                verticalOffset = -fontMetrics.Ascent;
                break;
            default: // Baseline
                verticalOffset = 0;
                break;
        }

        y += verticalOffset;

        // Save canvas state for transformations
        context.Save();

        // Apply rotation if needed
        if (Math.Abs(RotationAngle) > 0.001)
        {
            context.RotateRadians((float)RotationAngle, x, y);
        }

        // Apply oblique angle (skew) if needed
        if (Math.Abs(ObliqueAngle) > 0.001)
        {
            var skewMatrix = SKMatrix.CreateSkew((float)Math.Tan(ObliqueAngle), 0);
            context.Concat(ref skewMatrix);
        }

        // Draw the text
        context.DrawText(TextValue, x, y, _textPaint);

        // Restore canvas state
        context.Restore();
    }

    public override void UpdateObject()
    {
        base.UpdateObject();

        // Parse text value (code 1)
        if (Properties.FirstOrDefault(x => x.Code == 1) is { } textProp)
        {
            TextValue = textProp.Data;
        }

        // Parse insertion point (codes 10, 20)
        if (Properties.FirstOrDefault(x => x.Code == 10) is { } xProp)
        {
            InsertionPointX = double.Parse(xProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 20) is { } yProp)
        {
            InsertionPointY = double.Parse(yProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse height (code 40)
        if (Properties.FirstOrDefault(x => x.Code == 40) is { } heightProp)
        {
            Height = double.Parse(heightProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse rotation angle (code 50)
        if (Properties.FirstOrDefault(x => x.Code == 50) is { } rotationProp)
        {
            RotationAngle = double.Parse(rotationProp.Data.Trim(), CultureInfo.InvariantCulture) * Math.PI / 180.0;
        }

        // Parse width scale factor (code 41)
        if (Properties.FirstOrDefault(x => x.Code == 41) is { } widthProp)
        {
            WidthScaleFactor = double.Parse(widthProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse oblique angle (code 51)
        if (Properties.FirstOrDefault(x => x.Code == 51) is { } obliqueProp)
        {
            ObliqueAngle = double.Parse(obliqueProp.Data.Trim(), CultureInfo.InvariantCulture) * Math.PI / 180.0;
        }

        // Parse text style (code 7)
        if (Properties.FirstOrDefault(x => x.Code == 7) is { } styleProp)
        {
            TextStyle = styleProp.Data;
        }

        // Parse text generation flags (code 71)
        if (Properties.FirstOrDefault(x => x.Code == 71) is { } flagsProp)
        {
            TextGenerationFlags = int.Parse(flagsProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse horizontal alignment (code 72)
        if (Properties.FirstOrDefault(x => x.Code == 72) is { } hAlignProp)
        {
            HorizontalAlignment = int.Parse(hAlignProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse vertical alignment (code 73)
        if (Properties.FirstOrDefault(x => x.Code == 73) is { } vAlignProp)
        {
            VerticalAlignment = int.Parse(vAlignProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse alignment point (codes 11, 21)
        if (Properties.FirstOrDefault(x => x.Code == 11) is { } alignXProp)
        {
            AlignmentPointX = double.Parse(alignXProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 21) is { } alignYProp)
        {
            AlignmentPointY = double.Parse(alignYProp.Data.Trim(), CultureInfo.InvariantCulture);
        }
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();

        // Update or add text value property
        var textProp = Properties.FirstOrDefault(x => x.Code == 1);
        if (textProp != null)
        {
            textProp.Data = TextValue;
        }
        else
        {
            AddProperty(1, TextValue);
        }

        // Update insertion point properties
        UpdateOrAddProperty(10, InsertionPointX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(20, InsertionPointY.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(40, Height.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(50, (RotationAngle * 180.0 / Math.PI).ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(41, WidthScaleFactor.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(51, (ObliqueAngle * 180.0 / Math.PI).ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(7, TextStyle);
        UpdateOrAddProperty(71, TextGenerationFlags.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(72, HorizontalAlignment.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(73, VerticalAlignment.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(11, AlignmentPointX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(21, AlignmentPointY.ToString(CultureInfo.InvariantCulture));
    }

    private void UpdateOrAddProperty(int code, string value)
    {
        var prop = Properties.FirstOrDefault(x => x.Code == code);
        if (prop != null)
        {
            prop.Data = value;
        }
        else
        {
            AddProperty(code, value);
        }
    }

    public override void Invalidate()
    {
        _bounds = CalculateBounds();
    }

    public override bool Contains(float x, float y)
    {
        var bounds = GetBounds();
        return bounds.Contains(x, y);
    }

    public override SKRect GetBounds()
    {
        return _bounds ?? CalculateBounds();
    }

    private SKRect CalculateBounds()
    {
        if (string.IsNullOrEmpty(TextValue) || Height <= 0)
        {
            return SKRect.Empty;
        }

        // Set text size for measurement
        _textPaint.TextSize = (float)Height;
        _textPaint.TextScaleX = (float)WidthScaleFactor;

        // Measure text
        var textBounds = new SKRect();
        _textPaint.MeasureText(TextValue, ref textBounds);

        // Calculate position
        var x = (float)InsertionPointX;
        var y = (float)InsertionPointY;

        if (HorizontalAlignment != 0 || VerticalAlignment != 0)
        {
            x = (float)AlignmentPointX;
            y = (float)AlignmentPointY;
        }

        // Adjust bounds based on alignment
        switch (HorizontalAlignment)
        {
            case 1: // Center
                textBounds.Offset(x - textBounds.Width / 2, y);
                break;
            case 2: // Right
                textBounds.Offset(x - textBounds.Width, y);
                break;
            default: // Left
                textBounds.Offset(x, y);
                break;
        }

        // Apply rotation to bounds if needed
        if (Math.Abs(RotationAngle) > 0.001)
        {
            // For rotated text, we need to calculate the bounding box of the rotated rectangle
            var corners = new SKPoint[]
            {
                new SKPoint(textBounds.Left, textBounds.Top),
                new SKPoint(textBounds.Right, textBounds.Top),
                new SKPoint(textBounds.Right, textBounds.Bottom),
                new SKPoint(textBounds.Left, textBounds.Bottom)
            };

            var cos = (float)Math.Cos(RotationAngle);
            var sin = (float)Math.Sin(RotationAngle);

            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;

            foreach (var corner in corners)
            {
                var rotatedX = cos * (corner.X - x) - sin * (corner.Y - y) + x;
                var rotatedY = sin * (corner.X - x) + cos * (corner.Y - y) + y;

                minX = Math.Min(minX, rotatedX);
                maxX = Math.Max(maxX, rotatedX);
                minY = Math.Min(minY, rotatedY);
                maxY = Math.Max(maxY, rotatedY);
            }

            textBounds = new SKRect(minX, minY, maxX, maxY);
        }

        return textBounds;
    }
}
