using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DrawMap.ChildWindows;

public partial class NotificationDialog : Window
{
    public NotificationDialog(string[] data)
    {
        InitializeComponent();
        Cb1RadioButton.Content = data[0];
        Cb2RadioButton.Content = data[1];
        Instance = this;
    }

    public static NotificationDialog Instance { get; private set; } = null!;

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close(new[] { Cb1RadioButton.IsChecked ?? false, Cb2RadioButton.IsChecked ?? false });
    }
}