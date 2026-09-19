using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ToolEffectNro.ChildWindows;

public partial class SizeCrop : Window
{
    public SizeCrop()
    {
        InitializeComponent();
    }

    private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(WTextBox.Text?.Trim(), out var w))
            w = 0;

        if (!int.TryParse(HTextBox.Text?.Trim(), out var h))
            h = 0;
        Close(new[] { w, h });
    }
}