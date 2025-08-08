using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfPolyline : DxfEntity
{
    private SKPaint _pen;
    private double _thickness = 1.0;
    private SKPath? _path;
    private SKPath? _fillPath;
    private SKRect? _bounds;
    private List<(double X, double Y)> _vertices;
    private bool _isClosed;

    public DxfPolyline()
    {
        _pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color,
            StrokeWidth = (float)_thickness,
        };

        _vertices = new List<(double X, double Y)>();
        _isClosed = false;

        AddProperty(0, "LWPOLYLINE");
    }

    public List<(double X, double Y)> Vertices => _vertices;
    
    public bool IsClosed
    {
        get => _isClosed;
        set => _isClosed = value;
    }

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_path is null)
        {
            return;
        }

        _pen.StrokeWidth = (float)(_thickness / zoomFactor);
        _pen.Color = Color;

        context.DrawPath(_path, _pen);
    }

    public override void UpdateObject()
    {
        base.UpdateObject();

        _vertices.Clear();

        // Get number of vertices (code 90)
        var vertexCountProp = Properties.FirstOrDefault(x => x.Code == 90);
        if (vertexCountProp != null && int.TryParse(vertexCountProp.Data.Trim(), out int vertexCount))
        {
            // Get closed flag (code 70)
            var flagsProp = Properties.FirstOrDefault(x => x.Code == 70);
            if (flagsProp != null && int.TryParse(flagsProp.Data.Trim(), out int flags))
            {
                _isClosed = (flags & 1) == 1;
            }

            // Read vertices (codes 10 and 20 for X and Y coordinates)
            var xCoords = Properties.Where(x => x.Code == 10).ToList();
            var yCoords = Properties.Where(x => x.Code == 20).ToList();

            for (int i = 0; i < Math.Min(xCoords.Count, yCoords.Count); i++)
            {
                if (double.TryParse(xCoords[i].Data.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double x) &&
                    double.TryParse(yCoords[i].Data.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
                {
                    _vertices.Add((x, y));
                }
            }
        }
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();

        // Update vertex count
        var vertexCountProp = Properties.FirstOrDefault(x => x.Code == 90);
        if (vertexCountProp != null)
        {
            vertexCountProp.Data = _vertices.Count.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(90, _vertices.Count.ToString(CultureInfo.InvariantCulture));
        }

        // Update closed flag
        var flagsProp = Properties.FirstOrDefault(x => x.Code == 70);
        int flags = _isClosed ? 1 : 0;
        if (flagsProp != null)
        {
            flagsProp.Data = flags.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            AddProperty(70, flags.ToString(CultureInfo.InvariantCulture));
        }

        // Remove existing vertex properties
        Properties.RemoveAll(x => x.Code == 10 || x.Code == 20);

        // Add vertex coordinates
        foreach (var vertex in _vertices)
        {
            AddProperty(10, vertex.X.ToString(CultureInfo.InvariantCulture));
            AddProperty(20, vertex.Y.ToString(CultureInfo.InvariantCulture));
        }
    }
    
    public override void Invalidate()
    {
        _path = CreatePath();
        _fillPath = _pen.GetFillPath(_path);
        _bounds = _fillPath?.Bounds ?? SKRect.Empty;
    }

    public override bool Contains(float x, float y)
    {
        if (_fillPath is null)
        {
            return false;
        }

        return _fillPath.Contains(x, y);
    }

    public override SKRect GetBounds()
    {
        return _bounds ?? SKRect.Empty;
    }

    private SKPath CreatePath()
    {
        var path = new SKPath();

        if (_vertices.Count < 2)
        {
            return path;
        }

        // Start the path at the first vertex
        path.MoveTo((float)_vertices[0].X, (float)_vertices[0].Y);

        // Add lines to subsequent vertices
        for (int i = 1; i < _vertices.Count; i++)
        {
            path.LineTo((float)_vertices[i].X, (float)_vertices[i].Y);
        }

        // Close the path if needed
        if (_isClosed && _vertices.Count > 2)
        {
            path.Close();
        }

        return path;
    }

    public void AddVertex(double x, double y)
    {
        _vertices.Add((x, y));
    }

    public void ClearVertices()
    {
        _vertices.Clear();
    }
}
