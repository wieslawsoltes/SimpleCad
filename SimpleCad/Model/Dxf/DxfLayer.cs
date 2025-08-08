using System.Linq;

namespace SimpleCad.Model;

public class DxfLayer : DxfObject
{
    public string Name { get; set; } = "0";
    public int Flags { get; set; } = 0;
    public int ColorNumber { get; set; } = 7; // Default white
    public string LineType { get; set; } = "CONTINUOUS";
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; } = false;
    public bool IsPlottable { get; set; } = true;

    public DxfLayer()
    {
        AddProperty(0, "LAYER");
    }

    public DxfLayer(string name, int colorNumber = 7, string lineType = "CONTINUOUS")
    {
        Name = name;
        ColorNumber = colorNumber;
        LineType = lineType;
        AddProperty(0, "LAYER");
        UpdateProperties();
    }

    public override void UpdateObject()
    {
        base.UpdateObject();

        // Parse layer name (code 2)
        if (Properties.FirstOrDefault(x => x.Code == 2) is { } nameProp)
        {
            Name = nameProp.Data.Trim();
        }

        // Parse flags (code 70)
        if (Properties.FirstOrDefault(x => x.Code == 70) is { } flagsProp)
        {
            if (int.TryParse(flagsProp.Data.Trim(), out int flags))
            {
                Flags = flags;
                IsVisible = (flags & 1) == 0; // Bit 0: layer is invisible if set
                IsLocked = (flags & 4) != 0;  // Bit 2: layer is locked if set
                IsPlottable = (flags & 16) == 0; // Bit 4: layer is not plottable if set
            }
        }

        // Parse color number (code 62)
        if (Properties.FirstOrDefault(x => x.Code == 62) is { } colorProp)
        {
            if (int.TryParse(colorProp.Data.Trim(), out int colorNumber))
            {
                ColorNumber = colorNumber;
            }
        }

        // Parse line type (code 6)
        if (Properties.FirstOrDefault(x => x.Code == 6) is { } lineTypeProp)
        {
            LineType = lineTypeProp.Data.Trim();
        }
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();

        // Update layer name (code 2)
        UpdateOrAddProperty(2, Name);

        // Update flags (code 70)
        int flags = 0;
        if (!IsVisible) flags |= 1;
        if (IsLocked) flags |= 4;
        if (!IsPlottable) flags |= 16;
        UpdateOrAddProperty(70, flags.ToString());

        // Update color number (code 62)
        UpdateOrAddProperty(62, ColorNumber.ToString());

        // Update line type (code 6)
        UpdateOrAddProperty(6, LineType);
    }

    private void UpdateOrAddProperty(int code, string data)
    {
        var existingProp = Properties.FirstOrDefault(x => x.Code == code);
        if (existingProp != null)
        {
            existingProp.Data = data;
        }
        else
        {
            AddProperty(code, data);
        }
    }
}