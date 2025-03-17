using Avalonia;
using SkiaSharp;

namespace SimpleCad.Model;

public interface IDrawing
{
    void Render(SKCanvas context, Rect bounds);
}
