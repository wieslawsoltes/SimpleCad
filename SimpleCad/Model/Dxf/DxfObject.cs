using System.Collections.Generic;

namespace SimpleCad.Model;

public abstract class DxfObject
{
    public DxfObject()
    {
        Children = [];
        Properties = [];
    }
    
    public DxfObject? Parent { get; set; }

    public List<DxfObject> Children { get; set; }

    public List<DxfProperty> Properties { get; set; }

    public virtual void UpdateObject()
    {
        foreach (var child in Children)
        {
            child.UpdateObject();
        }
    }

    public virtual void UpdateProperties()
    {
        foreach (var child in Children)
        {
            child.UpdateObject();
        }
    }

    public void AddProperty(int code, string data)
    {
        Properties.Add(new DxfProperty(code, data));
    }
}
