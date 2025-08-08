using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfBlock : DxfEntity
{
    private SKRect? _bounds;
    
    public DxfBlock()
    {
        AddProperty(0, "BLOCK");
    }

    public string BlockName { get; set; } = string.Empty;
    public double BasePointX { get; set; }
    public double BasePointY { get; set; }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        foreach (var child in Children)
        {
            if (child is DxfEntity entity)
            {
                entity.Render(context, zoomFactor);
            }
        }
    }

    public override void UpdateObject()
    {
        base.UpdateObject();

        // Parse block name (code 2)
        if (Properties.FirstOrDefault(x => x.Code == 2) is { } blockNameProp)
        {
            BlockName = blockNameProp.Data.Trim();
        }

        // Parse base point (codes 10, 20)
        if (Properties.FirstOrDefault(x => x.Code == 10) is { } baseXProp)
        {
            BasePointX = double.Parse(baseXProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 20) is { } baseYProp)
        {
            BasePointY = double.Parse(baseYProp.Data.Trim(), CultureInfo.InvariantCulture);
        }
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();

        // Update or add block name property
        var blockNameProp = Properties.FirstOrDefault(x => x.Code == 2);
        if (blockNameProp != null)
        {
            blockNameProp.Data = BlockName;
        }
        else
        {
            AddProperty(2, BlockName);
        }

        // Update or add base point properties
        var baseXProp = Properties.FirstOrDefault(x => x.Code == 10);
        if (baseXProp != null)
        {
            baseXProp.Data = BasePointX.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(10, BasePointX.ToString(CultureInfo.InvariantCulture));
        }

        var baseYProp = Properties.FirstOrDefault(x => x.Code == 20);
        if (baseYProp != null)
        {
            baseYProp.Data = BasePointY.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(20, BasePointY.ToString(CultureInfo.InvariantCulture));
        }
    }

    public override void Invalidate()
    {
        foreach (var child in Children)
        {
            if (child is DxfEntity entity)
            {
                entity.Invalidate();
            }
        }
        
        _bounds = CalculateBounds();
    }

    public override bool Contains(float x, float y)
    {
        foreach (var child in Children)
        {
            if (child is DxfEntity entity && entity.Contains(x, y))
            {
                return true;
            }
        }
        return false;
    }

    public override SKRect GetBounds()
    {
        return _bounds ?? SKRect.Empty;
    }

    private SKRect CalculateBounds()
    {
        var bounds = SKRect.Empty;
        bool first = true;

        foreach (var child in Children)
        {
            if (child is DxfEntity entity)
            {
                var entityBounds = entity.GetBounds();
                if (!entityBounds.IsEmpty)
                {
                    if (first)
                    {
                        bounds = entityBounds;
                        first = false;
                    }
                    else
                    {
                        bounds = SKRect.Union(bounds, entityBounds);
                    }
                }
            }
        }

        return bounds;
    }
}
