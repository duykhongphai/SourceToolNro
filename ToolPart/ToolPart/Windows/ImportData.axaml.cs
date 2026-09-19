using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace ToolPart.Windows;

public partial class ImportData : Window
{
    public ImportData()
    {
        InitializeComponent();
    }

    private void OnImportClick(object sender, RoutedEventArgs e)
    {
        var partHead = PartHeadTextBox?.Text ?? "";
        var partBody = PartBodyTextBox?.Text ?? "";
        var partLeg = PartLegTextBox?.Text ?? "";
        var iconPath = IconPathTextBox?.Text ?? "";
        Close(new[]
        {
            partHead, partBody, partLeg, iconPath, JsonArrayRadio?.IsChecked == true ? "1" : "0",
            IdFirstRadio?.IsChecked == true ? "1" : "0"
        });
    }

    private async void OnBrowseButtonClick(object sender, RoutedEventArgs e)
    {
        await OpenFolderDialog();
    }

    private async Task OpenFolderDialog()
    {
        var storageProvider = GetTopLevel(this)?.StorageProvider;
        if (storageProvider != null)
        {
            var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Icon Folder",
                AllowMultiple = false
            });

            if (folders.Count > 0) IconPathTextBox.Text = folders[0].Path.LocalPath;
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}