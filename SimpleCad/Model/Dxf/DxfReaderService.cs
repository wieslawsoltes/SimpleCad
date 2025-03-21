using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DynamicData;

namespace SimpleCad.Model;

public class DxfReaderService
{
    public DxfFile ReadDxfFile(StreamReader reader)
    {
        var dxfFile = new DxfFile();

        var objects = new Stack<DxfObject>();

        objects.Push(dxfFile);
        
        var canPop = false;

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
                if (dataLine == "SECTION")
                {
                    var dxfSection = new DxfSection();

                    objects.Peek().Children.Add(dxfSection);
                    objects.Push(dxfSection);
                    canPop = false;
                }
                else if (dataLine == "ENDSEC")
                {
                    if (canPop)
                    {
                        objects.Pop();
                        canPop = false;
                    }

                    var dxfEndsec = new DxfEndsec();

                    objects.Peek().Children.Add(dxfEndsec);
                    objects.Pop();
                    canPop = false;
                }
                else if (dataLine == "TABLE")
                {
                    var dxfTable = new DxfTable();

                    objects.Peek().Children.Add(dxfTable);
                    objects.Push(dxfTable);
                    canPop = false;
                }
                else if (dataLine == "ENDTAB")
                {
                    if (canPop)
                    {
                        objects.Pop();
                        canPop = false;
                    }

                    var dxfEndtab = new DxfEndtab();

                    objects.Peek().Children.Add(dxfEndtab);
                    objects.Pop();
                    canPop = false;
                }
                // TODO: Objects with SEQEND
                /*
                else if (dataLine == "SEQEND")
                {
                    if (canPop)
                    {
                        objects.Pop();
                        canPop = false;
                    }

                    var dxfSeqend = new DxfUnknownObject();
    
                    // TODO: Remove
                    dxfSeqend.AddProperty(code, dataLine);
                    
                    objects.Peek().Children.Add(dxfSeqend);
                    //objects.Push(dxfSeqend);
                    objects.Pop();
                    canPop = false;
                }
                */
                else if (dataLine == "BLOCK")
                {
                    var dxfBlock = new DxfBlock();

                    objects.Peek().Children.Add(dxfBlock);
                    objects.Push(dxfBlock);
                    canPop = false;
                }
                else if (dataLine == "ENDBLK")
                {
                    if (canPop)
                    {
                        objects.Pop();
                        canPop = false;
                    }

                    var dxfEndblk = new DxfEndblk();

                    objects.Peek().Children.Add(dxfEndblk);
                    objects.Pop();
                    canPop = false;
                }
                else if (dataLine == "EOF")
                {
                    var dxfEof = new DxfEof();

                    objects.Peek().Children.Add(dxfEof);
                    objects.Pop();
                    canPop = false;

                    break;
                }
                else
                {
                    if (canPop)
                    {
                        objects.Pop();
                        canPop = false;
                    }
                    
                    DxfObject? dxfUnknownObject = dataLine switch
                    {
                        "ARC" => new DxfArc(),
                        "CIRCLE" => new DxfCircle(),
                        "DIMENSION" => new DxfDimension(),
                        "ELLIPSE" => new DxfEllipse(),
                        "HATCH" => new DxfHatch(),
                        "IMAGE" => new DxfRasterImage(),
                        "INSERT" => new DxfBlockReference(),
                        "LINE" => new DxfLine(),
                        "LWPOLYLINE" => new DxfPolyline(),
                        "MTEXT" => new DxfMText(),
                        "OLE2FRAME" => new DxfOle2Frame(),
                        "SOLID" => new DxfTrace(),
                        "SPLINE" => new DxfSpline(),
                        "TEXT" => new DxfText(),
                        _ => null
                    };

                    if (dxfUnknownObject is null)
                    {
                        dxfUnknownObject = new DxfUnknownObject();

                        dxfUnknownObject.AddProperty(code, dataLine);

                        Console.WriteLine($"Unknown: {dataLine}");
                    }

                    try
                    {
                        var parent = objects.Peek();
                        var type = parent.Properties.Count > 0 ? parent.Properties[0].Data : "-";
                        var name = parent.Properties.Count > 1 ? parent.Properties[1].Data : "-";
                        // Console.WriteLine($"New Child: {dataLine}, Parent: {parent} ({type} - {name})");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    try
                    {
                        objects.Peek().Children.Add(dxfUnknownObject);
                        objects.Push(dxfUnknownObject);
                     
                        canPop = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            else
            {
                objects.Peek().AddProperty(code, dataLine);
            }
        } 

        return dxfFile;
    }
}
