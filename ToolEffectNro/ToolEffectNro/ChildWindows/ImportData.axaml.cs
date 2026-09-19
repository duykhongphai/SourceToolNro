using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace ToolEffectNro.ChildWindows;

public partial class ImportData : Window
{
    public ImportData()
    {
        InitializeComponent();
        Instance = this;
    }

    public static ImportData Instance { get; private set; } = null!;

    private async Task<string> ShowFilePickerAsync(string title = "Select File")
    {
        try
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return null;

            var folders = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

            return folders.Count > 0 ? folders[0].Path.LocalPath : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting file: {ex.Message}");
            return null;
        }
    }

    private void LoadEffectBtn_OnClick(object sender, RoutedEventArgs e)
    {
        Close(EffectFromImageDataRadio.IsChecked == true
            ? new[] { EffectImageFolderTextBox.Text, EffectDataFolderTextBox.Text }
            : new[] { EffectDataOnlyFolderTextBox.Text });
    }

    private async Task BrowseFolderAndSetTextBox(string title, TextBox textBox)
    {
        var folderPath = await ShowFilePickerAsync(title);
        if (!string.IsNullOrEmpty(folderPath))
            textBox.Text = folderPath;
    }

    private async void BrowseEffectImageBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Effect Image File", EffectImageFolderTextBox);
    }

    private async void BrowseEffectDataBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Effect Data File", EffectDataFolderTextBox);
    }

    private async void BrowseEffectDataOnlyBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Effect Data File", EffectDataOnlyFolderTextBox);
    }

    private void CloseBtn_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnRadioButtonChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton radioButton) return;
        Height = radioButton.Name switch
        {
            "EffectFromImageDataRadio" when radioButton.IsChecked == true => 270,
            "EffectFromDataRadio" when radioButton.IsChecked == true => 200,
            _ => Height
        };
    }

    private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}