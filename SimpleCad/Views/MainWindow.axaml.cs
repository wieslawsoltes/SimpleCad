using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace SimpleCad.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        FileOpen.Click += FileOpenOnClick;
        FileSaveAs.Click += FileSaveAsOnClick;
    }

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

            CadCanvas.Drawing.Open(stream, CadCanvas.Bounds.Height);
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

        CadCanvas.Drawing.SaveAs(stream, CadCanvas.Bounds.Height);
    }
}
