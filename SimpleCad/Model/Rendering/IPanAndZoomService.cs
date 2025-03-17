using Avalonia.Input;
using SkiaSharp;

namespace SimpleCad.Model;

public interface IPanAndZoomService
{
    SKMatrix Transform { get; }
    bool TryStartPan(object? sender, PointerPressedEventArgs e);
    bool TryEndPan(PointerReleasedEventArgs e);
    bool TryMovePan(object? sender, PointerEventArgs e);
    bool Zoom(object? sender, PointerWheelEventArgs e);
}
