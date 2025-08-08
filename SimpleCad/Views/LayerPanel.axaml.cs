using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SimpleCad.Model;
using SkiaSharp;
using System;
using System.Globalization;

namespace SimpleCad.Views;

public partial class LayerPanel : UserControl
{
    public LayerPanel()
    {
        InitializeComponent();
    }
}

public class ColorConverter : IValueConverter
{
    public static readonly ColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int colorIndex)
        {
            // Create a temporary entity to access the color conversion
            var tempEntity = new DxfLine();
            tempEntity.SetColor(colorIndex);
            var skColor = tempEntity.GetResolvedColor();
            return new SolidColorBrush(Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue));
        }
        
        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}