using System.Collections.Generic;
using Avalonia;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfEntities
{
    public DxfEntities()
    {
        Entities = new List<DxfEntity>();
    }

    public List<DxfEntity> Entities { get; set; }

    public void Invalidate()
    {
        foreach (var entity in Entities)
        {
            entity.Invalidate();
        }
    }

    public void Render(SKCanvas context, Rect bounds, double zoomFactor)
    {
        context.Save();

        context.Translate((float)0.0, (float)bounds.Height);
        context.Scale((float)1.0, (float)-1.0);

        foreach (var entity in Entities)
        {
            entity.Render(context, zoomFactor);
        }
        
        context.Restore();
    }
}
