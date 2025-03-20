using System.Collections.Generic;
using System.Linq;
using Avalonia;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfFile : DxfObject
{
    public static DxfFile Create()
    {
        var dxfFile = new DxfFile();

        dxfFile.Children =
        [
            new DxfSection
            {
                Properties =
                {
                    new DxfProperty(0, "SECTION"),
                    new DxfProperty(2, "HEADER"),
                }
            },
            new DxfEndsec(),
            new DxfSection
            {
                Properties =
                [
                    new DxfProperty(0, "SECTION"),
                    new DxfProperty(2, "ENTITIES"),
                ]
            },
            new DxfEndsec(),
            new DxfEof()
        ];

        return dxfFile;
    }
    
    public IEnumerable<DxfEntity> GetEntities()
    {
       return Children
            .FirstOrDefault(x => x.Properties.FirstOrDefault(p => p.Code == 2 && p.Data == "ENTITIES") != null)
            .Children.OfType<DxfEntity>();
    }

    public void AddEntity(DxfEntity entity)
    {
        var entitiesSection = Children
            .FirstOrDefault(x => x.Properties.FirstOrDefault(p => p.Code == 2 && p.Data == "ENTITIES") != null);

        entitiesSection.Children.Add(entity);
    }

    public void Render(SKCanvas context, Rect bounds, double zoomFactor)
    {
        context.Save();

        context.Translate((float)0.0, (float)bounds.Height);
        context.Scale((float)1.0, (float)-1.0);

        var entities = GetEntities();
        
        foreach (var entity in entities)
        {
            entity.Render(context, zoomFactor);
        }
        
        context.Restore();
    }
}
