namespace SimpleCad.Model;

public class DxfEndsec : DxfObject
{
    public DxfEndsec()
    {
        AddProperty(0, "ENDSEC");
    }
}

public class DxfEndtab : DxfObject
{
    public DxfEndtab()
    {
        AddProperty(0, "ENDTAB");
    }
}

public class DxfEndblk : DxfObject
{
    public DxfEndblk()
    {
        AddProperty(0, "ENDBLK");
    }
}
