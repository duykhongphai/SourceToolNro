using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ToolEffectNro.Windows;

namespace ToolEffectNro.ChildWindows;

public partial class FormFilmEffect : Window
{
    public static FormFilmEffect Instance;

    public FormFilmEffect()
    {
        InitializeComponent();
        Instance = this;
        SideForm.Instance?.CreatePictureBox();
    }

    public void UpdateLeftDockPosition()
    {
        var mainPosition = WindowExecution.Instance.Position;
        var newPosition = new PixelPoint(
            mainPosition.X + (int)WindowExecution.Instance.Width - 1,
            mainPosition.Y + (int)WindowExecution.Instance.DragPanel.Bounds.Height
        );
        SideForm.Instance.Position = newPosition;
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        if (!SideForm.Instance.IsVisible)
        {
            SideForm.Instance.Show();
            UpdateLeftDockPosition();
        }
        else
        {
            SideForm.Instance.Hide();
        }
    }
}