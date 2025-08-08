using System;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfBlockReference : DxfEntity
{
    private SKRect? _bounds;
    private DxfBlock? _referencedBlock;
    
    public DxfBlockReference()
    {
        AddProperty(0, "INSERT");
    }

    public string BlockName { get; set; } = string.Empty;
    public double InsertionPointX { get; set; }
    public double InsertionPointY { get; set; }
    public double ScaleX { get; set; } = 1.0;
    public double ScaleY { get; set; } = 1.0;
    public double RotationAngle { get; set; } // in degrees

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_referencedBlock == null)
        {
            return;
        }

        context.Save();

        // Apply transformations: translation, rotation, and scaling
        context.Translate((float)InsertionPointX, (float)InsertionPointY);
        
        if (Math.Abs(RotationAngle) > 0.001)
        {
            context.RotateDegrees((float)RotationAngle);
        }
        
        if (Math.Abs(ScaleX - 1.0) > 0.001 || Math.Abs(ScaleY - 1.0) > 0.001)
        {
            context.Scale((float)ScaleX, (float)ScaleY);
        }

        // Render the referenced block
        _referencedBlock.Render(context, zoomFactor);

        context.Restore();
    }

    public override void UpdateObject()
    {
        base.UpdateObject();

        // Parse block name (code 2)
        if (Properties.FirstOrDefault(x => x.Code == 2) is { } blockNameProp)
        {
            BlockName = blockNameProp.Data.Trim();
        }

        // Parse insertion point (codes 10, 20)
        if (Properties.FirstOrDefault(x => x.Code == 10) is { } insertXProp)
        {
            InsertionPointX = double.Parse(insertXProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 20) is { } insertYProp)
        {
            InsertionPointY = double.Parse(insertYProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse scale factors (codes 41, 42)
        if (Properties.FirstOrDefault(x => x.Code == 41) is { } scaleXProp)
        {
            ScaleX = double.Parse(scaleXProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        if (Properties.FirstOrDefault(x => x.Code == 42) is { } scaleYProp)
        {
            ScaleY = double.Parse(scaleYProp.Data.Trim(), CultureInfo.InvariantCulture);
        }

        // Parse rotation angle (code 50)
        if (Properties.FirstOrDefault(x => x.Code == 50) is { } rotationProp)
        {
            RotationAngle = double.Parse(rotationProp.Data.Trim(), CultureInfo.InvariantCulture);
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

        // Update or add insertion point properties
        var insertXProp = Properties.FirstOrDefault(x => x.Code == 10);
        if (insertXProp != null)
        {
            insertXProp.Data = InsertionPointX.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(10, InsertionPointX.ToString(CultureInfo.InvariantCulture));
        }

        var insertYProp = Properties.FirstOrDefault(x => x.Code == 20);
        if (insertYProp != null)
        {
            insertYProp.Data = InsertionPointY.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(20, InsertionPointY.ToString(CultureInfo.InvariantCulture));
        }

        // Update or add scale factor properties
        var scaleXProp = Properties.FirstOrDefault(x => x.Code == 41);
        if (scaleXProp != null)
        {
            scaleXProp.Data = ScaleX.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(41, ScaleX.ToString(CultureInfo.InvariantCulture));
        }

        var scaleYProp = Properties.FirstOrDefault(x => x.Code == 42);
        if (scaleYProp != null)
        {
            scaleYProp.Data = ScaleY.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(42, ScaleY.ToString(CultureInfo.InvariantCulture));
        }

        // Update or add rotation angle property
        var rotationProp = Properties.FirstOrDefault(x => x.Code == 50);
        if (rotationProp != null)
        {
            rotationProp.Data = RotationAngle.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(50, RotationAngle.ToString(CultureInfo.InvariantCulture));
        }
    }

    public override void Invalidate()
    {
        if (_referencedBlock != null)
        {
            _referencedBlock.Invalidate();
            _bounds = CalculateTransformedBounds();
        }
        else
        {
            _bounds = SKRect.Empty;
        }
    }

    public override bool Contains(float x, float y)
    {
        if (_referencedBlock == null || _bounds == null)
        {
            return false;
        }

        return _bounds.Value.Contains(x, y);
    }

    public override SKRect GetBounds()
    {
        return _bounds ?? SKRect.Empty;
    }

    public void SetReferencedBlock(DxfBlock? block)
    {
        _referencedBlock = block;
        Invalidate();
    }

    private SKRect CalculateTransformedBounds()
    {
        if (_referencedBlock == null)
        {
            return SKRect.Empty;
        }

        var blockBounds = _referencedBlock.GetBounds();
        if (blockBounds.IsEmpty)
        {
            return SKRect.Empty;
        }

        // Apply transformations to the bounds
        var matrix = SKMatrix.CreateIdentity();
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)InsertionPointX, (float)InsertionPointY));
        
        if (Math.Abs(RotationAngle) > 0.001)
        {
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees((float)RotationAngle));
        }
        
        if (Math.Abs(ScaleX - 1.0) > 0.001 || Math.Abs(ScaleY - 1.0) > 0.001)
        {
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale((float)ScaleX, (float)ScaleY));
        }

        return matrix.MapRect(blockBounds);
    }
}
