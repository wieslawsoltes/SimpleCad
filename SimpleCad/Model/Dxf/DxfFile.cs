using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using SkiaSharp;

namespace SimpleCad.Model;

public class DxfFile : DxfObject
{
    public static DxfFile Create()
    {
        var dxfFile = new DxfFile();

        // Create default layer "0"
        var defaultLayer = new DxfLayer("0", 7, "CONTINUOUS");
        
        // Create layer table
        var layerTable = new DxfTable();
        layerTable.AddProperty(2, "LAYER");
        layerTable.AddProperty(70, "1"); // Number of layers
        layerTable.Children.Add(defaultLayer);
        
        // Create tables section
        var tablesSection = new DxfSection();
        tablesSection.AddProperty(0, "SECTION");
        tablesSection.AddProperty(2, "TABLES");
        tablesSection.Children.Add(layerTable);
        tablesSection.Children.Add(new DxfEndtab());

        dxfFile.Children =
        [
            new DxfSection
            {
                Properties =
                {
                    new DxfProperty(0, "SECTION"),
                    new DxfProperty(2, "HEADER"),
                }
            },
            new DxfEndsec(),
            tablesSection,
            new DxfEndsec(),
            new DxfSection
            {
                Properties =
                [
                    new DxfProperty(0, "SECTION"),
                    new DxfProperty(2, "ENTITIES"),
                ]
            },
            new DxfEndsec(),
            new DxfEof()
        ];

        return dxfFile;
    }

    public DxfSection? Entities => Children
        .FirstOrDefault(
            x => x.Properties.FirstOrDefault(p => p.Code == 2 && p.Data == "ENTITIES") != null) as DxfSection;
    
    public DxfSection? Blocks => Children
        .FirstOrDefault(
            x => x.Properties.FirstOrDefault(p => p.Code == 2 && p.Data == "BLOCKS") != null) as DxfSection;
    
    public DxfSection? Tables => Children
        .FirstOrDefault(
            x => x.Properties.FirstOrDefault(p => p.Code == 2 && p.Data == "TABLES") != null) as DxfSection;
    
    public DxfTable? LayerTable => Tables?.Children
        .FirstOrDefault(
            x => x.Properties.FirstOrDefault(p => p.Code == 2 && p.Data == "LAYER") != null) as DxfTable;
    
    public IEnumerable<DxfEntity> GetEntities()
    {
       return Entities?.Children.OfType<DxfEntity>() ?? Enumerable.Empty<DxfEntity>();
    }

    public IEnumerable<DxfBlock> GetBlocks()
    {
        return Blocks?.Children.OfType<DxfBlock>() ?? Enumerable.Empty<DxfBlock>();
    }

    public DxfBlock? FindBlockByName(string blockName)
    {
        return GetBlocks().FirstOrDefault(block => 
            string.Equals(block.BlockName, blockName, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<DxfLayer> GetLayers()
    {
        return LayerTable?.Children.OfType<DxfLayer>() ?? Enumerable.Empty<DxfLayer>();
    }

    public DxfLayer? FindLayerByName(string layerName)
    {
        return GetLayers().FirstOrDefault(layer => 
            string.Equals(layer.Name, layerName, StringComparison.OrdinalIgnoreCase));
    }

    public DxfLayer GetOrCreateLayer(string layerName, int colorNumber = 7, string lineType = "CONTINUOUS")
    {
        var existingLayer = FindLayerByName(layerName);
        if (existingLayer != null)
        {
            return existingLayer;
        }

        var newLayer = new DxfLayer(layerName, colorNumber, lineType);
        AddLayer(newLayer);
        return newLayer;
    }

    public void AddLayer(DxfLayer layer)
    {
        LayerTable?.Children.Add(layer);
    }

    public void AddEntity(DxfEntity entity)
    {
        Entities?.Children.Add(entity);
    }
    
    public void RemoveEntity(DxfEntity entity)
    {
        Entities?.Children.Remove(entity);
    }

    public void AddBlock(DxfBlock block)
    {
        Blocks?.Children.Add(block);
    }

    public void ResolveBlockReferences()
    {
        var entities = GetEntities().ToList();
        foreach (var entity in entities)
        {
            ResolveBlockReferencesRecursive(entity);
        }
    }

    public void ResolveLayerReferences()
    {
        var entities = GetEntities().ToList();
        foreach (var entity in entities)
        {
            ResolveLayerReferencesRecursive(entity);
        }
    }

    private void ResolveLayerReferencesRecursive(DxfObject obj)
    {
        if (obj is DxfEntity entity)
        {
            entity.Layer = FindLayerByName(entity.LayerName);
            // Update the resolved color based on the layer
            entity.Color = entity.GetResolvedColor();
        }

        foreach (var child in obj.Children)
        {
            ResolveLayerReferencesRecursive(child);
        }
    }

    private void ResolveBlockReferencesRecursive(DxfObject obj)
    {
        if (obj is DxfBlockReference blockRef)
        {
            var referencedBlock = FindBlockByName(blockRef.BlockName);
            blockRef.SetReferencedBlock(referencedBlock);
        }

        foreach (var child in obj.Children)
        {
            ResolveBlockReferencesRecursive(child);
        }
    }

    public void Render(SKCanvas context, Rect bounds, double zoomFactor)
    {
        context.Save();

        context.Translate((float)0.0, (float)bounds.Height);
        context.Scale((float)1.0, (float)-1.0);

        // Resolve layer references before rendering
        ResolveLayerReferences();
        
        var entities = GetEntities();
        
        foreach (var entity in entities)
        {
            // Ensure color is resolved for rendering
            entity.Color = entity.GetResolvedColor();
            entity.Render(context, zoomFactor);
        }
        
        context.Restore();
    }
}
