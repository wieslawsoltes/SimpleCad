using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfMText : DxfEntity
{
    private SKPaint _textPaint;
    private SKRect? _bounds;
    private List<string> _textLines = new List<string>();
    private float _lineSpacing = 1.0f;

    public DxfMText()
    {
        _textPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = Color,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial"),
            TextSize = 12
        };

        AddProperty(0, "MTEXT");
    }

    public string TextValue { get; set; } = string.Empty;
    public double InsertionPointX { get; set; }
    public double InsertionPointY { get; set; }
    public double Height { get; set; } = 1.0;
    public double ReferenceRectangleWidth { get; set; } = 0.0;
    public int AttachmentPoint { get; set; } = 1;
    public int DrawingDirection { get; set; } = 1;
    public string TextStyle { get; set; } = "STANDARD";
    public double RotationAngle { get; set; } = 0.0;
    public double LineSpacingFactor { get; set; } = 1.0;
    public int LineSpacingStyle { get; set; } = 1;

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

        // Process text and split into lines
        ProcessText();

        if (_textLines.Count == 0)
        {
            return;
        }

        // Calculate line height
        var fontMetrics = _textPaint.FontMetrics;
        var lineHeight = (fontMetrics.Descent - fontMetrics.Ascent) * _lineSpacing;

        // Calculate starting position
        var x = (float)InsertionPointX;
        var y = (float)InsertionPointY;

        // Adjust position based on attachment point
        var totalTextHeight = _textLines.Count * lineHeight;
        switch (AttachmentPoint)
        {
            case 1: // Top Left
                _textPaint.TextAlign = SKTextAlign.Left;
                y -= fontMetrics.Ascent;
                break;
            case 2: // Top Center
                _textPaint.TextAlign = SKTextAlign.Center;
                y -= fontMetrics.Ascent;
                break;
            case 3: // Top Right
                _textPaint.TextAlign = SKTextAlign.Right;
                y -= fontMetrics.Ascent;
                break;
            case 4: // Middle Left
                _textPaint.TextAlign = SKTextAlign.Left;
                y -= fontMetrics.Ascent + totalTextHeight / 2;
                break;
            case 5: // Middle Center
                _textPaint.TextAlign = SKTextAlign.Center;
                y -= fontMetrics.Ascent + totalTextHeight / 2;
                break;
            case 6: // Middle Right
                _textPaint.TextAlign = SKTextAlign.Right;
                y -= fontMetrics.Ascent + totalTextHeight / 2;
                break;
            case 7: // Bottom Left
                _textPaint.TextAlign = SKTextAlign.Left;
                y -= fontMetrics.Ascent + totalTextHeight;
                break;
            case 8: // Bottom Center
                _textPaint.TextAlign = SKTextAlign.Center;
                y -= fontMetrics.Ascent + totalTextHeight;
                break;
            case 9: // Bottom Right
                _textPaint.TextAlign = SKTextAlign.Right;
                y -= fontMetrics.Ascent + totalTextHeight;
                break;
            default:
                _textPaint.TextAlign = SKTextAlign.Left;
                y -= fontMetrics.Ascent;
                break;
        }

        // Save canvas state for transformations
        context.Save();

        // Apply rotation if needed
        if (Math.Abs(RotationAngle) > 0.001)
        {
            context.RotateRadians((float)RotationAngle, (float)InsertionPointX, (float)InsertionPointY);
        }

        // Draw each line of text
        for (int i = 0; i < _textLines.Count; i++)
        {
            var lineY = y + i * lineHeight;
            context.DrawText(_textLines[i], x, lineY, _textPaint);
        }

        // Restore canvas state
        context.Restore();
    }

    public override void UpdateObject()
    {
        base.UpdateObject();

        // Parse text value (code 1 and continuation codes 3)
        var textParts = new List<string>();
        
        // Get main text (code 1)
        if (Properties.FirstOrDefault(x => x.Code == 1) is { } textProp)
        {
            textParts.Add(textProp.Data);
        }

        // Get continuation text (code 3)
        var continuationProps = Properties.Where(x => x.Code == 3).OrderBy(x => Properties.IndexOf(x));
        foreach (var prop in continuationProps)
        {
            textParts.Add(prop.Data);
        }

        TextValue = string.Join("", textParts);

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

        // Parse reference rectangle width (code 41)
        if (Properties.FirstOrDefault(x => x.Code == 41) is { } widthProp)
        {
            ReferenceRectangleWidth = double.Parse(widthProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse attachment point (code 71)
        if (Properties.FirstOrDefault(x => x.Code == 71) is { } attachProp)
        {
            AttachmentPoint = int.Parse(attachProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse drawing direction (code 72)
        if (Properties.FirstOrDefault(x => x.Code == 72) is { } dirProp)
        {
            DrawingDirection = int.Parse(dirProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse text style (code 7)
        if (Properties.FirstOrDefault(x => x.Code == 7) is { } styleProp)
        {
            TextStyle = styleProp.Data;
        }

        // Parse rotation angle (code 50)
        if (Properties.FirstOrDefault(x => x.Code == 50) is { } rotationProp)
        {
            RotationAngle = double.Parse(rotationProp.Data.Trim(), CultureInfo.InvariantCulture) * Math.PI / 180.0;
        }

        // Parse line spacing factor (code 44)
        if (Properties.FirstOrDefault(x => x.Code == 44) is { } spacingProp)
        {
            LineSpacingFactor = double.Parse(spacingProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse line spacing style (code 73)
        if (Properties.FirstOrDefault(x => x.Code == 73) is { } spacingStyleProp)
        {
            LineSpacingStyle = int.Parse(spacingStyleProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Update line spacing
        _lineSpacing = (float)LineSpacingFactor;
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();

        // Split text into chunks if it's too long (DXF has 250 character limit per line)
        var textChunks = SplitTextIntoChunks(TextValue, 250);
        
        // Update or add main text property (code 1)
        if (textChunks.Count > 0)
        {
            UpdateOrAddProperty(1, textChunks[0]);
        }

        // Remove existing continuation properties
        Properties.RemoveAll(x => x.Code == 3);

        // Add continuation properties (code 3) for remaining chunks
        for (int i = 1; i < textChunks.Count; i++)
        {
            AddProperty(3, textChunks[i]);
        }

        // Update other properties
        UpdateOrAddProperty(10, InsertionPointX.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(20, InsertionPointY.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(40, Height.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(41, ReferenceRectangleWidth.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(71, AttachmentPoint.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(72, DrawingDirection.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(7, TextStyle);
        UpdateOrAddProperty(50, (RotationAngle * 180.0 / Math.PI).ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(44, LineSpacingFactor.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(73, LineSpacingStyle.ToString(CultureInfo.InvariantCulture));
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

    private List<string> SplitTextIntoChunks(string text, int maxLength)
    {
        var chunks = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            return chunks;
        }

        for (int i = 0; i < text.Length; i += maxLength)
        {
            var length = Math.Min(maxLength, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }

        return chunks;
    }

    private void ProcessText()
    {
        _textLines.Clear();

        if (string.IsNullOrEmpty(TextValue))
        {
            return;
        }

        // Clean up MText formatting codes and split into lines
        var cleanText = CleanMTextFormatting(TextValue);
        var lines = cleanText.Split(new[] { "\\P", "\n", "\r\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            if (ReferenceRectangleWidth > 0)
            {
                // Word wrap if reference width is specified
                var wrappedLines = WrapText(line, ReferenceRectangleWidth);
                _textLines.AddRange(wrappedLines);
            }
            else
            {
                _textLines.Add(line);
            }
        }
    }

    private string CleanMTextFormatting(string text)
    {
        // Remove common MText formatting codes
        // This is a simplified implementation - full MText parsing would be more complex
        var cleaned = text;
        
        // Remove font changes: \f...;
        cleaned = Regex.Replace(cleaned, @"\\f[^;]*;", "");
        
        // Remove height changes: \H...;
        cleaned = Regex.Replace(cleaned, @"\\H[^;]*;", "");
        
        // Remove color changes: \C...;
        cleaned = Regex.Replace(cleaned, @"\\C[^;]*;", "");
        
        // Remove width changes: \W...;
        cleaned = Regex.Replace(cleaned, @"\\W[^;]*;", "");
        
        // Remove tracking changes: \T...;
        cleaned = Regex.Replace(cleaned, @"\\T[^;]*;", "");
        
        // Remove oblique angle: \Q...;
        cleaned = Regex.Replace(cleaned, @"\\Q[^;]*;", "");
        
        // Remove alignment: \A...;
        cleaned = Regex.Replace(cleaned, @"\\A[^;]*;", "");
        
        // Convert line breaks
        cleaned = cleaned.Replace("\\P", "\n");
        
        // Remove remaining backslash codes
        cleaned = Regex.Replace(cleaned, @"\\[a-zA-Z][^;]*;", "");
        
        return cleaned;
    }

    private List<string> WrapText(string text, double maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            lines.Add("");
            return lines;
        }

        var words = text.Split(' ');
        var currentLine = "";
        var maxWidthPixels = (float)maxWidth;

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var lineWidth = _textPaint.MeasureText(testLine);

            if (lineWidth <= maxWidthPixels || string.IsNullOrEmpty(currentLine))
            {
                currentLine = testLine;
            }
            else
            {
                lines.Add(currentLine);
                currentLine = word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    public override void Invalidate()
    {
        ProcessText();
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
        if (_textLines.Count == 0 || Height <= 0)
        {
            return SKRect.Empty;
        }

        // Set text size for measurement
        _textPaint.TextSize = (float)Height;

        // Calculate line height
        var fontMetrics = _textPaint.FontMetrics;
        var lineHeight = (fontMetrics.Descent - fontMetrics.Ascent) * _lineSpacing;
        var totalHeight = _textLines.Count * lineHeight;

        // Find the widest line
        var maxWidth = 0f;
        foreach (var line in _textLines)
        {
            var lineWidth = _textPaint.MeasureText(line);
            maxWidth = Math.Max(maxWidth, lineWidth);
        }

        // Calculate bounds based on attachment point
        var x = (float)InsertionPointX;
        var y = (float)InsertionPointY;
        var left = x;
        var top = y;
        var right = x + maxWidth;
        var bottom = y + totalHeight;

        // Adjust based on attachment point
        switch (AttachmentPoint)
        {
            case 1: // Top Left
                break;
            case 2: // Top Center
                left = x - maxWidth / 2;
                right = x + maxWidth / 2;
                break;
            case 3: // Top Right
                left = x - maxWidth;
                right = x;
                break;
            case 4: // Middle Left
                top = y - totalHeight / 2;
                bottom = y + totalHeight / 2;
                break;
            case 5: // Middle Center
                left = x - maxWidth / 2;
                right = x + maxWidth / 2;
                top = y - totalHeight / 2;
                bottom = y + totalHeight / 2;
                break;
            case 6: // Middle Right
                left = x - maxWidth;
                right = x;
                top = y - totalHeight / 2;
                bottom = y + totalHeight / 2;
                break;
            case 7: // Bottom Left
                top = y - totalHeight;
                bottom = y;
                break;
            case 8: // Bottom Center
                left = x - maxWidth / 2;
                right = x + maxWidth / 2;
                top = y - totalHeight;
                bottom = y;
                break;
            case 9: // Bottom Right
                left = x - maxWidth;
                right = x;
                top = y - totalHeight;
                bottom = y;
                break;
        }

        var bounds = new SKRect(left, top, right, bottom);

        // Apply rotation to bounds if needed
        if (Math.Abs(RotationAngle) > 0.001)
        {
            var corners = new SKPoint[]
            {
                new SKPoint(bounds.Left, bounds.Top),
                new SKPoint(bounds.Right, bounds.Top),
                new SKPoint(bounds.Right, bounds.Bottom),
                new SKPoint(bounds.Left, bounds.Bottom)
            };

            var cos = (float)Math.Cos(RotationAngle);
            var sin = (float)Math.Sin(RotationAngle);
            var centerX = (float)InsertionPointX;
            var centerY = (float)InsertionPointY;

            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;

            foreach (var corner in corners)
            {
                var rotatedX = cos * (corner.X - centerX) - sin * (corner.Y - centerY) + centerX;
                var rotatedY = sin * (corner.X - centerX) + cos * (corner.Y - centerY) + centerY;

                minX = Math.Min(minX, rotatedX);
                maxX = Math.Max(maxX, rotatedX);
                minY = Math.Min(minY, rotatedY);
                maxY = Math.Max(maxY, rotatedY);
            }

            bounds = new SKRect(minX, minY, maxX, maxY);
        }

        return bounds;
    }
}
