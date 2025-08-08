using System.Collections.ObjectModel;
using System.Linq;
using SimpleCad.Model;

namespace SimpleCad.ViewModels;

public class LayerPanelViewModel : ViewModelBase
{
    private DxfFile? _dxfFile;
    private ObservableCollection<LayerItemViewModel> _layers;

    public LayerPanelViewModel()
    {
        _layers = new ObservableCollection<LayerItemViewModel>();
    }

    public ObservableCollection<LayerItemViewModel> Layers
    {
        get => _layers;
        set => SetProperty(ref _layers, value);
    }

    public DxfFile? DxfFile
    {
        get => _dxfFile;
        set
        {
            if (SetProperty(ref _dxfFile, value))
            {
                UpdateLayers();
            }
        }
    }

    private void UpdateLayers()
    {
        Layers.Clear();
        
        if (_dxfFile != null)
        {
            var layers = _dxfFile.GetLayers();
            foreach (var layer in layers)
            {
                Layers.Add(new LayerItemViewModel(layer));
            }
        }
    }
}

public class LayerItemViewModel : ViewModelBase
{
    private DxfLayer _layer;
    private bool _isVisible;
    private bool _isLocked;

    public LayerItemViewModel(DxfLayer layer)
    {
        _layer = layer;
        _isVisible = layer.IsVisible;
        _isLocked = layer.IsLocked;
    }

    public string Name => _layer.Name;
    
    public int ColorNumber => _layer.ColorNumber;
    
    public string LineType => _layer.LineType;

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (SetProperty(ref _isVisible, value))
            {
                _layer.IsVisible = value;
            }
        }
    }

    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            if (SetProperty(ref _isLocked, value))
            {
                _layer.IsLocked = value;
            }
        }
    }

    public DxfLayer Layer => _layer;
}