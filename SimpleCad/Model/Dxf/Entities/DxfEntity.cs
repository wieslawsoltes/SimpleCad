using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public abstract class DxfEntity : DxfObject
{
    public SKColor Color { get; set; } = SKColors.White;
    public ColorInfo ColorInfo { get; set; } = new ColorInfo();
    public string LayerName { get; set; } = "0";
    public DxfLayer? Layer { get; set; }
    
    public abstract void Render(SKCanvas context, double zoomFactor);

    public abstract void Invalidate();

    public abstract bool Contains(float x, float y);

    public abstract SKRect GetBounds();
    
    public override void UpdateObject()
    {
        base.UpdateObject();
        
        // Parse layer name (code 8)
        if (Properties.FirstOrDefault(x => x.Code == 8) is { } layerProp)
        {
            LayerName = layerProp.Data.Trim();
        }
        
        // Parse color (code 62)
        if (Properties.FirstOrDefault(x => x.Code == 62) is { } colorProp)
        {
            if (int.TryParse(colorProp.Data.Trim(), out int colorIndex))
            {
                ColorInfo.SetFromColorCode(colorIndex);
                Color = ColorInfo.ResolveColor(Layer);
            }
        }
        else
        {
            // Default to ByLayer if no color specified
            ColorInfo.SetFromColorCode(256);
            Color = ColorInfo.ResolveColor(Layer);
        }
    }
    
    public override void UpdateProperties()
    {
        base.UpdateProperties();
        
        // Update layer name (code 8)
        if (Properties.FirstOrDefault(x => x.Code == 8) is { } layerProp)
        {
            layerProp.Data = LayerName;
        }
        else if (LayerName != "0") // Only add if not default layer
        {
            AddProperty(8, LayerName);
        }
        
        // Update color property (code 62)
        var colorCode = ColorInfo.GetColorCode();
        if (Properties.FirstOrDefault(x => x.Code == 62) is { } colorProp)
        {
            colorProp.Data = colorCode.ToString(CultureInfo.InvariantCulture);
        }
        else if (colorCode != 256) // Only add if not ByLayer (default)
        {
            AddProperty(62, colorCode.ToString(CultureInfo.InvariantCulture));
        }
    }
    
    /// <summary>
    /// Resolves the actual color for rendering based on the color method, layer, and block context
    /// </summary>
    public SKColor GetResolvedColor(SKColor? blockColor = null)
    {
        return ColorInfo.ResolveColor(Layer, blockColor);
    }
    
    /// <summary>
    /// Sets the entity color using different methods
    /// </summary>
    public void SetColor(int colorCode)
    {
        ColorInfo.SetFromColorCode(colorCode);
        Color = ColorInfo.ResolveColor(Layer);
    }
    
    /// <summary>
    /// Sets the entity color to ByLayer
    /// </summary>
    public void SetColorByLayer()
    {
        ColorInfo.SetFromColorCode(256);
        Color = ColorInfo.ResolveColor(Layer);
    }
    
    /// <summary>
    /// Sets the entity color to ByBlock
    /// </summary>
    public void SetColorByBlock()
    {
        ColorInfo.SetFromColorCode(0);
        Color = ColorInfo.ResolveColor(Layer);
    }
    
    /// <summary>
    /// Sets the entity to use an explicit color
    /// </summary>
    public void SetExplicitColor(SKColor color)
    {
        ColorInfo = new ColorInfo(color);
        Color = color;
    }

    // Backward compatibility methods
    private static SKColor GetColorFromIndex(int colorIndex)
    {
        var colorInfo = new ColorInfo(colorIndex);
        return colorInfo.ResolveColor();
    }

    private static int GetIndexFromColor(SKColor color)
    {
        var colorInfo = new ColorInfo(color);
        return colorInfo.ColorIndex;
    }
}
