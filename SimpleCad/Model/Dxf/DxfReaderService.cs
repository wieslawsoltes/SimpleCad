using System.Globalization;
using System.IO;

namespace SimpleCad.Model;

public class DxfReaderService
{
    private void ReadDxfLineEntity(DxfLineEntity dxfLineEntity, int code, string data)
    {
        if (code == 10)
        {
            dxfLineEntity.StartPointX = double.Parse(data.Trim(), CultureInfo.InvariantCulture);
        }
        else if (code == 20)
        {
            dxfLineEntity.StartPointY = double.Parse(data.Trim(), CultureInfo.InvariantCulture);
        }
        else if (code == 11)
        {
            dxfLineEntity.EndPointX = double.Parse(data.Trim(), CultureInfo.InvariantCulture);   
        }
        else if (code == 21)
        {
            dxfLineEntity.EndPointY = double.Parse(data.Trim(), CultureInfo.InvariantCulture); 
        }
    }

    public DxfEntities ReadDxfDrawing(StreamReader reader)
    {
        var dxfEntities = new DxfEntities();

        object? currentObject = null;

        while (true)
        {
            var codeLine = reader.ReadLine();
            var dataLine = reader.ReadLine();

            if (string.IsNullOrEmpty(codeLine) || dataLine is null)
            {
                break;
            }

            var code = int.Parse(codeLine.Trim(), CultureInfo.InvariantCulture);

            if (code == 0)
            {
                currentObject = null;

                if (dataLine == "EOF")
                {
                }
                else if (dataLine == "SECTION")
                {
                    
                }
                else if (dataLine == "ENDSEC")
                {
                    
                }
                else if (dataLine == "LINE")
                {
                    var lineEntity = new DxfLineEntity();
                    dxfEntities.Entities.Add(lineEntity);
                    currentObject = lineEntity;
                }
            }
            else
            {
                if (currentObject is DxfLineEntity lineEntity)
                {
                    ReadDxfLineEntity(lineEntity, code, dataLine);
                }
            }
        } 

        return dxfEntities;
    }
}
