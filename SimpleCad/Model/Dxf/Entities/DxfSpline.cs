using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfSpline : DxfEntity
{
    private SKPaint _pen;
    private double _thickness = 1.0;
    private SKPath? _path;
    private SKRect? _bounds;
    private List<(double X, double Y)> _controlPoints = new List<(double, double)>();
    private List<double> _knotValues = new List<double>();
    private List<double> _weights = new List<double>();

    public DxfSpline()
    {
        _pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color,
            StrokeWidth = (float)_thickness,
            IsAntialias = true
        };

        AddProperty(0, "SPLINE");
    }

    public int Degree { get; set; } = 3;
    public int Flags { get; set; } = 0;
    public int NumberOfKnots { get; set; } = 0;
    public int NumberOfControlPoints { get; set; } = 0;
    public int NumberOfFitPoints { get; set; } = 0;
    public double KnotTolerance { get; set; } = 0.0000001;
    public double ControlPointTolerance { get; set; } = 0.0000001;
    public double FitTolerance { get; set; } = 0.0000001;
    public double StartTangentX { get; set; } = 0.0;
    public double StartTangentY { get; set; } = 0.0;
    public double EndTangentX { get; set; } = 0.0;
    public double EndTangentY { get; set; } = 0.0;

    public override void Render(SKCanvas context, double zoomFactor)
    {
        if (_path == null || _controlPoints.Count < 2)
            return;

        // Update pen thickness based on zoom
        _pen.StrokeWidth = (float)(_thickness / zoomFactor);
        _pen.Color = Color;

        // Draw the spline path
        context.DrawPath(_path, _pen);
    }

    public override void UpdateObject()
    {
        base.UpdateObject();
        
        // Parse control points, knot values, and weights from properties
        _controlPoints.Clear();
        _knotValues.Clear();
        _weights.Clear();

        var controlPointsX = new List<double>();
        var controlPointsY = new List<double>();

        foreach (var property in Properties)
        {
            switch (property.Code)
            {
                case 10: // Control point X
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                        controlPointsX.Add(x);
                    break;
                case 20: // Control point Y
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                        controlPointsY.Add(y);
                    break;
                case 40: // Knot value
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var knot))
                        _knotValues.Add(knot);
                    break;
                case 41: // Weight
                    if (double.TryParse(property.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out var weight))
                        _weights.Add(weight);
                    break;
            }
        }

        // Combine X and Y coordinates into control points
        for (int i = 0; i < Math.Min(controlPointsX.Count, controlPointsY.Count); i++)
        {
            _controlPoints.Add((controlPointsX[i], controlPointsY[i]));
        }

        // Create the spline path
        _path = CreateSplinePath();
    }

    public override void UpdateProperties()
    {
        base.UpdateProperties();
        
        UpdateOrAddProperty(71, Degree.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(72, NumberOfKnots.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(73, NumberOfControlPoints.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(74, NumberOfFitPoints.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(70, Flags.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(42, KnotTolerance.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(43, ControlPointTolerance.ToString(CultureInfo.InvariantCulture));
        UpdateOrAddProperty(44, FitTolerance.ToString(CultureInfo.InvariantCulture));
        
        if (Math.Abs(StartTangentX) > 0.001 || Math.Abs(StartTangentY) > 0.001)
        {
            UpdateOrAddProperty(12, StartTangentX.ToString(CultureInfo.InvariantCulture));
            UpdateOrAddProperty(22, StartTangentY.ToString(CultureInfo.InvariantCulture));
        }
        
        if (Math.Abs(EndTangentX) > 0.001 || Math.Abs(EndTangentY) > 0.001)
        {
            UpdateOrAddProperty(13, EndTangentX.ToString(CultureInfo.InvariantCulture));
            UpdateOrAddProperty(23, EndTangentY.ToString(CultureInfo.InvariantCulture));
        }
    }

    public override void Invalidate()
    {
        _bounds = null;
        _path = null;
        _bounds = CalculateBounds();
    }

    public override bool Contains(float x, float y)
    {
        if (_path == null)
            return false;

        // Create a small region around the point for hit testing
        var hitTestRadius = 3.0f;
        var hitTestRect = new SKRect(x - hitTestRadius, y - hitTestRadius, 
                                   x + hitTestRadius, y + hitTestRadius);
        
        // Check if the path intersects with the hit test rectangle
        return _path.Bounds.IntersectsWith(hitTestRect);
    }

    public override SKRect GetBounds()
    {
        return _bounds ?? SKRect.Empty;
    }

    private void UpdateOrAddProperty(int code, string value)
    {
        var existingProperty = Properties.FirstOrDefault(p => p.Code == code);
        if (existingProperty != null)
        {
            existingProperty.Data = value;
        }
        else
        {
            AddProperty(code, value);
        }
    }

    private SKPath CreateSplinePath()
    {
        var path = new SKPath();

        if (_controlPoints.Count < 2)
            return path;

        // For simplicity, we'll approximate the spline using cubic Bezier curves
        // This is a simplified implementation - a full NURBS implementation would be more complex
        
        if (_controlPoints.Count == 2)
        {
            // Simple line for 2 points
            path.MoveTo((float)_controlPoints[0].X, (float)_controlPoints[0].Y);
            path.LineTo((float)_controlPoints[1].X, (float)_controlPoints[1].Y);
        }
        else if (_controlPoints.Count == 3)
        {
            // Quadratic curve for 3 points
            path.MoveTo((float)_controlPoints[0].X, (float)_controlPoints[0].Y);
            path.QuadTo((float)_controlPoints[1].X, (float)_controlPoints[1].Y,
                       (float)_controlPoints[2].X, (float)_controlPoints[2].Y);
        }
        else
        {
            // Cubic spline approximation for 4+ points
            path.MoveTo((float)_controlPoints[0].X, (float)_controlPoints[0].Y);
            
            for (int i = 1; i < _controlPoints.Count - 2; i += 3)
            {
                var p1 = _controlPoints[Math.Min(i, _controlPoints.Count - 1)];
                var p2 = _controlPoints[Math.Min(i + 1, _controlPoints.Count - 1)];
                var p3 = _controlPoints[Math.Min(i + 2, _controlPoints.Count - 1)];
                
                path.CubicTo((float)p1.X, (float)p1.Y,
                           (float)p2.X, (float)p2.Y,
                           (float)p3.X, (float)p3.Y);
            }
            
            // Handle remaining points
            if (_controlPoints.Count % 3 != 1)
            {
                var lastIndex = _controlPoints.Count - 1;
                var secondLastIndex = Math.Max(0, lastIndex - 1);
                
                if (_controlPoints.Count % 3 == 0)
                {
                    // Two remaining points - use quadratic
                    path.QuadTo((float)_controlPoints[secondLastIndex].X, (float)_controlPoints[secondLastIndex].Y,
                              (float)_controlPoints[lastIndex].X, (float)_controlPoints[lastIndex].Y);
                }
                else
                {
                    // One remaining point - use line
                    path.LineTo((float)_controlPoints[lastIndex].X, (float)_controlPoints[lastIndex].Y);
                }
            }
        }

        return path;
    }

    private SKRect CalculateBounds()
    {
        if (_path == null || _controlPoints.Count == 0)
            return SKRect.Empty;

        return _path.Bounds;
    }

    public void AddControlPoint(double x, double y, double weight = 1.0)
    {
        _controlPoints.Add((x, y));
        _weights.Add(weight);
        NumberOfControlPoints = _controlPoints.Count;
        Invalidate();
    }

    public void AddKnotValue(double knot)
    {
        _knotValues.Add(knot);
        NumberOfKnots = _knotValues.Count;
    }

    public void ClearControlPoints()
    {
        _controlPoints.Clear();
        _weights.Clear();
        NumberOfControlPoints = 0;
        Invalidate();
    }

    public void ClearKnotValues()
    {
        _knotValues.Clear();
        NumberOfKnots = 0;
    }
}
