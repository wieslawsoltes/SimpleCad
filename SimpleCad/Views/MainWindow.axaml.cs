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

    private void FileOpenOnClick(object? sender, RoutedEventArgs e)
    {

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

        if (file is not null)
        {
            await using var stream = await file.OpenWriteAsync();
            CadCanvas.Drawing.SaveAs(stream, CadCanvas.Bounds.Height);
        }
    }
}
