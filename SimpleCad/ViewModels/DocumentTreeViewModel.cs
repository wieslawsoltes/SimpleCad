using System.Collections.ObjectModel;
using System.Linq;
using SimpleCad.Model;

namespace SimpleCad.ViewModels;

public class DocumentTreeViewModel : ViewModelBase
{
    private DxfFile? _dxfFile;
    private ObservableCollection<TreeNodeViewModel> _rootNodes;

    public DocumentTreeViewModel()
    {
        _rootNodes = new ObservableCollection<TreeNodeViewModel>();
    }

    public ObservableCollection<TreeNodeViewModel> RootNodes
    {
        get => _rootNodes;
        set => SetProperty(ref _rootNodes, value);
    }

    public DxfFile? DxfFile
    {
        get => _dxfFile;
        set
        {
            if (SetProperty(ref _dxfFile, value))
            {
                UpdateTree();
            }
        }
    }

    private void UpdateTree()
    {
        RootNodes.Clear();
        
        if (_dxfFile != null)
        {
            // Add Layers node
            var layersNode = new TreeNodeViewModel("Layers", TreeNodeType.Folder);
            var layers = _dxfFile.GetLayers();
            foreach (var layer in layers)
            {
                var layerNode = new TreeNodeViewModel(layer.Name, TreeNodeType.Layer, layer);
                
                // Add entities for this layer
                var entities = _dxfFile.GetEntities().Where(e => e.LayerName == layer.Name);
                foreach (var entity in entities)
                {
                    var entityNode = new TreeNodeViewModel($"{entity.GetType().Name.Replace("Dxf", "")}", TreeNodeType.Entity, entity);
                    layerNode.Children.Add(entityNode);
                }
                
                layersNode.Children.Add(layerNode);
            }
            RootNodes.Add(layersNode);

            // Add Blocks node
            var blocksNode = new TreeNodeViewModel("Blocks", TreeNodeType.Folder);
            var blocks = _dxfFile.GetBlocks();
            foreach (var block in blocks)
            {
                var blockNode = new TreeNodeViewModel(block.BlockName, TreeNodeType.Block, block);
                
                // Add entities in this block
                foreach (var child in block.Children)
                {
                    if (child is DxfEntity entity)
                    {
                        var entityNode = new TreeNodeViewModel($"{entity.GetType().Name.Replace("Dxf", "")}", TreeNodeType.Entity, entity);
                        blockNode.Children.Add(entityNode);
                    }
                }
                
                blocksNode.Children.Add(blockNode);
            }
            RootNodes.Add(blocksNode);

            // Add Entities node (entities not in blocks)
            var entitiesNode = new TreeNodeViewModel("Entities", TreeNodeType.Folder);
            var allEntities = _dxfFile.GetEntities();
            foreach (var entity in allEntities)
            {
                var entityNode = new TreeNodeViewModel($"{entity.GetType().Name.Replace("Dxf", "")}", TreeNodeType.Entity, entity);
                entitiesNode.Children.Add(entityNode);
            }
            RootNodes.Add(entitiesNode);
        }
    }
}

public class TreeNodeViewModel : ViewModelBase
{
    private bool _isExpanded;
    private bool _isSelected;
    private ObservableCollection<TreeNodeViewModel> _children;

    public TreeNodeViewModel(string name, TreeNodeType nodeType, object? data = null)
    {
        Name = name;
        NodeType = nodeType;
        Data = data;
        _children = new ObservableCollection<TreeNodeViewModel>();
        _isExpanded = nodeType == TreeNodeType.Folder;
    }

    public string Name { get; }
    
    public TreeNodeType NodeType { get; }
    
    public object? Data { get; }

    public ObservableCollection<TreeNodeViewModel> Children
    {
        get => _children;
        set => SetProperty(ref _children, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string Icon => NodeType switch
    {
        TreeNodeType.Folder => "📁",
        TreeNodeType.Layer => "📄",
        TreeNodeType.Block => "🧩",
        TreeNodeType.Entity => "📐",
        _ => "📄"
    };
}

public enum TreeNodeType
{
    Folder,
    Layer,
    Block,
    Entity
}