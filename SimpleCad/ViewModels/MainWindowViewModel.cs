using System.Linq;
using SimpleCad.Model;

namespace SimpleCad.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DrawingViewModel? _drawingViewModel;
    
    public MainWindowViewModel()
    {
        LayerPanel = new LayerPanelViewModel();
        DocumentTree = new DocumentTreeViewModel();
    }
    
    public string Greeting => "Welcome to Avalonia!";
    
    public LayerPanelViewModel LayerPanel { get; }
    
    public DocumentTreeViewModel DocumentTree { get; }
    
    public DrawingViewModel? DrawingViewModel
    {
        get => _drawingViewModel;
        set
        {
            if (_drawingViewModel != null)
            {
                _drawingViewModel.FileOpened -= OnFileOpened;
            }
            
            if (SetProperty(ref _drawingViewModel, value))
            {
                if (_drawingViewModel != null)
                {
                    _drawingViewModel.FileOpened += OnFileOpened;
                    LayerPanel.DxfFile = _drawingViewModel.DxfFile;
                    DocumentTree.DxfFile = _drawingViewModel.DxfFile;
                }
            }
        }
    }
    
    public void UpdatePanels()
    {
        if (DrawingViewModel?.DxfFile != null)
        {
            LayerPanel.DxfFile = DrawingViewModel.DxfFile;
            DocumentTree.DxfFile = DrawingViewModel.DxfFile;
        }
    }
    
    private void OnFileOpened(DxfFile dxfFile)
    {
        UpdatePanels();
    }
}
