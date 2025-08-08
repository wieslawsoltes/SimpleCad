using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SimpleCad.Model;

public enum ColorMethod
{
    ByBlock = 0,        // Color code 0 - inherit from block
    Explicit = 1,       // Color codes 1-255 - explicit color index
    ByLayer = 256,      // Color code 256 - inherit from layer
    TrueColor = 257     // Color codes > 256 - true color (RGB)
}

public class ColorInfo
{
    public ColorMethod Method { get; set; }
    public int ColorIndex { get; set; }
    public SKColor ExplicitColor { get; set; }
    public uint TrueColorValue { get; set; }

    public ColorInfo()
    {
        Method = ColorMethod.ByLayer;
        ColorIndex = 256;
        ExplicitColor = SKColors.White;
        TrueColorValue = 0xFFFFFF;
    }

    public ColorInfo(int colorCode)
    {
        SetFromColorCode(colorCode);
    }

    public ColorInfo(SKColor color)
    {
        Method = ColorMethod.Explicit;
        ExplicitColor = color;
        ColorIndex = GetIndexFromColor(color);
        TrueColorValue = (uint)((color.Red << 16) | (color.Green << 8) | color.Blue);
    }

    public void SetFromColorCode(int colorCode)
    {
        if (colorCode == 0)
        {
            Method = ColorMethod.ByBlock;
            ColorIndex = 0;
            ExplicitColor = SKColors.White;
        }
        else if (colorCode == 256)
        {
            Method = ColorMethod.ByLayer;
            ColorIndex = 256;
            ExplicitColor = SKColors.White;
        }
        else if (colorCode > 0 && colorCode <= 255)
        {
            Method = ColorMethod.Explicit;
            ColorIndex = colorCode;
            ExplicitColor = GetColorFromIndex(colorCode);
        }
        else if (colorCode < 0)
        {
            // Negative values indicate true color
            Method = ColorMethod.TrueColor;
            TrueColorValue = (uint)(-colorCode);
            ExplicitColor = SKColor.FromHsl(
                (TrueColorValue >> 16) & 0xFF,
                (TrueColorValue >> 8) & 0xFF,
                TrueColorValue & 0xFF);
            ColorIndex = colorCode;
        }
        else
        {
            // Default to ByLayer for unknown codes
            Method = ColorMethod.ByLayer;
            ColorIndex = 256;
            ExplicitColor = SKColors.White;
        }
    }

    public int GetColorCode()
    {
        return Method switch
        {
            ColorMethod.ByBlock => 0,
            ColorMethod.ByLayer => 256,
            ColorMethod.Explicit => ColorIndex,
            ColorMethod.TrueColor => -(int)TrueColorValue,
            _ => 256
        };
    }

    public SKColor ResolveColor(DxfLayer? layer = null, SKColor? blockColor = null)
    {
        return Method switch
        {
            ColorMethod.ByBlock => blockColor ?? SKColors.White,
            ColorMethod.ByLayer => layer != null ? GetColorFromIndex(layer.ColorNumber) : SKColors.White,
            ColorMethod.Explicit => ExplicitColor,
            ColorMethod.TrueColor => SKColor.FromHsl(
                (byte)((TrueColorValue >> 16) & 0xFF),
                (byte)((TrueColorValue >> 8) & 0xFF),
                (byte)(TrueColorValue & 0xFF)),
            _ => SKColors.White
        };
    }

    private static SKColor GetColorFromIndex(int colorIndex)
    {
        // Standard AutoCAD Color Index (ACI) colors
        var standardColors = new Dictionary<int, SKColor>
        {
            { 1, SKColors.Red },
            { 2, SKColors.Yellow },
            { 3, SKColors.Green },
            { 4, SKColors.Cyan },
            { 5, SKColors.Blue },
            { 6, SKColors.Magenta },
            { 7, SKColors.White },
            { 8, SKColor.Parse("#414141") }, // Dark gray
            { 9, SKColor.Parse("#808080") }, // Light gray
            { 10, SKColors.Red },
            { 11, SKColor.Parse("#FFAAAA") }, // Light red
            { 12, SKColor.Parse("#BD0000") }, // Dark red
            { 13, SKColor.Parse("#BD7E7E") }, // Light red
            { 14, SKColor.Parse("#810000") }, // Dark red
            { 15, SKColor.Parse("#810040") }, // Dark red
            { 16, SKColor.Parse("#BD0040") }, // Red
            { 17, SKColor.Parse("#FF0040") }, // Light red
            { 18, SKColor.Parse("#FFAABD") }, // Light red
            { 19, SKColor.Parse("#BD7E7E") }, // Light red
            { 20, SKColors.Orange },
            // Add more standard colors as needed
        };

        if (standardColors.TryGetValue(colorIndex, out var color))
        {
            return color;
        }

        // For colors 10-249, generate colors based on a pattern
        if (colorIndex >= 10 && colorIndex <= 249)
        {
            // Simple color generation for demonstration
            // In a real implementation, you'd use the full ACI color table
            float hue = (colorIndex - 10) * 360f / 240f;
            return SKColor.FromHsl(hue, 100, 50);
        }

        // Colors 250-255 are grayscale
        if (colorIndex >= 250 && colorIndex <= 255)
        {
            byte gray = (byte)((colorIndex - 250) * 255 / 5);
            return new SKColor(gray, gray, gray);
        }

        return SKColors.White; // Default
    }

    private static int GetIndexFromColor(SKColor color)
    {
        var colorMap = new Dictionary<SKColor, int>
        {
            { SKColors.Red, 1 },
            { SKColors.Yellow, 2 },
            { SKColors.Green, 3 },
            { SKColors.Cyan, 4 },
            { SKColors.Blue, 5 },
            { SKColors.Magenta, 6 },
            { SKColors.White, 7 },
            { SKColor.Parse("#414141"), 8 },
            { SKColor.Parse("#808080"), 9 }
        };

        return colorMap.TryGetValue(color, out var index) ? index : 7;
    }
}