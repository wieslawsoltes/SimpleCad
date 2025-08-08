using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SimpleCad.Model;
using SimpleCad.ViewModels;

namespace SimpleCad.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize the MainWindowViewModel and connect it to the CadCanvas drawing
        var mainViewModel = new MainWindowViewModel();
        DataContext = mainViewModel;
        
        // Set the DrawingViewModel after the component is initialized
        Loaded += (sender, e) => {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.DrawingViewModel = CadCanvas.Drawing;
            }
        };
        
        FileOpen.Click += FileOpenOnClick;
        FileSaveAs.Click += FileSaveAsOnClick;
        ToolSelection.Click += ToolSelectionOnClick;
        ToolLine.Click += ToolLineOnClick;
        ToolRectangle.Click += ToolRectangleOnClick;
        ToolCircle.Click += ToolCircleOnClick;
        ToolEllipse.Click += ToolEllipseOnClick;
        ToolArc.Click += ToolArcOnClick;
        ToolText.Click += ToolTextOnClick;
        ToolMText.Click += ToolMTextOnClick;
        ToolPolyline.Click += ToolPolylineOnClick;
        ToolHatch.Click += ToolHatchOnClick;
        ToolWipeout.Click += ToolWipeoutOnClick;
    }
    
    private MainWindowViewModel? MainViewModel => DataContext as MainWindowViewModel;

    private async void FileOpenOnClick(object? sender, RoutedEventArgs e)
    {
        var storageProvider = StorageProvider;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open",
            AllowMultiple = false,
            FileTypeFilter = 
            [
                new FilePickerFileType("DXF Files") { Patterns = ["*.dxf"] }
            ]
        });

        if (files.Count <= 0)
        {
            return;
        }

        foreach (var file in files)
        {
            await using var stream = await file.OpenReadAsync();

            CadCanvas.Drawing.Open(stream);
            
            // Update the panels after opening a file
            MainViewModel?.UpdatePanels();
        }
    }
    
    private async void FileSaveAsOnClick(object? sender, RoutedEventArgs e)
    {
        var storageProvider = StorageProvider;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save As",
            SuggestedFileName = "drawing.dxf",
            FileTypeChoices =
            [
                new FilePickerFileType("DXF Files") { Patterns = ["*.dxf"] }
            ]
        });

        if (file is null)
        {
            return;
        }

        await using var stream = await file.OpenWriteAsync();

        CadCanvas.Drawing.SaveAs(stream);
    }

    private void ToolSelectionOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new SelectionTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolLineOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new LineTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolRectangleOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new RectangleTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolCircleOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new CircleTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolEllipseOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new EllipseTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolArcOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new ArcTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolTextOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new TextTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolMTextOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new MTextTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolPolylineOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new PolylineTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolHatchOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new HatchTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }

    private void ToolWipeoutOnClick(object? sender, RoutedEventArgs e)
    {
        CadCanvas.Drawing.CurrentTool = new WipeoutTool(CadCanvas.Drawing, CadCanvas, CadCanvas.Drawing.PanAndZoomService);
    }
}
