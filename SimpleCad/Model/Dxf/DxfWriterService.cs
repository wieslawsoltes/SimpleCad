using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SimpleCad.Model;

public class DxfWriterService
{
    public void WriteDxfFile(StreamWriter writer, DxfFile dxfFile)
    {
        foreach (var dxfObject in dxfFile.Children)
        {
            WriteDxfObject(writer, dxfObject);
        }
    }

    private void WriteDxfObject(StreamWriter writer, DxfObject dxfObject)
    {
        foreach (var property in dxfObject.Properties)
        {
            writer.WriteLine(property.Code);
            writer.WriteLine(property.Data);
        }
        
        foreach (var dxfObjectChild in dxfObject.Children)
        {
            WriteDxfObject(writer, dxfObjectChild);
        }  
    }
    
    /*
    private void WriteHeaderSection(StreamWriter writer)
    {
        writer.WriteLine("0");
        writer.WriteLine("SECTION");
        writer.WriteLine("2");
        writer.WriteLine("HEADER");

        // TODO: Variables

        writer.WriteLine("0");
        writer.WriteLine("ENDSEC");
    }
    
    private void WriteEntitiesSection(StreamWriter writer, List<DxfEntity> entities)
    {
        writer.WriteLine("0");
        writer.WriteLine("SECTION");
        writer.WriteLine("2");
        writer.WriteLine("ENTITIES");

        foreach (var entity in entities)
        {
            switch (entity)
            {
                case DxfLineEntity lineEntity:
                    WriteLineEntity(writer, lineEntity);
                    break;
            }
        }

        writer.WriteLine("0");
        writer.WriteLine("ENDSEC");
    }

    private void WriteLineEntity(StreamWriter writer, DxfLineEntity dxfLineEntity)
    {
        writer.WriteLine("0");
        writer.WriteLine("LINE");

        writer.WriteLine("10");
        writer.WriteLine(dxfLineEntity.StartPointX.ToString(CultureInfo.InvariantCulture));

        writer.WriteLine("20");
        writer.WriteLine(dxfLineEntity.StartPointY.ToString(CultureInfo.InvariantCulture));

        writer.WriteLine("11");
        writer.WriteLine(dxfLineEntity.EndPointX.ToString(CultureInfo.InvariantCulture));

        writer.WriteLine("21");
        writer.WriteLine(dxfLineEntity.EndPointY.ToString(CultureInfo.InvariantCulture));
    }

    private void WriteEofSection(StreamWriter writer)
    {
        writer.WriteLine("0");
        writer.WriteLine("EOF");
    }
    */
}
