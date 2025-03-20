namespace SimpleCad.Model;

public class DxfProperty
{
    public DxfProperty(int code, string data)
    {
        Code = code;
        Data = data;
    }

    public int Code { get; set; }
    
    public string Data { get; set; }
}
