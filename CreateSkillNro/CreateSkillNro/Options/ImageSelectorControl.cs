using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace CreateSkillNro.Options;

public class ImageSelectedEventArgs(sbyte key, Bitmap image) : EventArgs
{
    public sbyte Key { get; } = key;
    public Bitmap Image { get; } = image;
}

public class ImageSelectionWindow : Window
{
    private const double ItemWidth = 90;
    private const double ItemHeight = 110;
    private const double ItemMargin = 5; 
    private readonly Dictionary<sbyte, Bitmap> _imageAction;
    private readonly string _title;

    public ImageSelectionWindow(Dictionary<sbyte, Bitmap> imageAction, string title)
    {
        _imageAction = imageAction ?? throw new ArgumentNullException(nameof(imageAction));
        _title = title;
        InitializeWindow();
        CreateContent();
    }

    public event EventHandler<ImageSelectedEventArgs> ImageSelected;

    private void InitializeWindow()
    {
        Title = $"Select Image for {_title}";
        Width = 500;
        Height = 500;
        MinWidth = 300;
        MinHeight = 300;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaTitleBarHeightHint = -1;
        CanResize = true;
    }

    private void CreateContent()
    {
        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        var header = new TextBlock
        {
            Text = $"Select an image for {_title}",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(header, 0);
        mainGrid.Children.Add(header);

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var imageGrid = CreateImageGrid();
        scrollViewer.Content = imageGrid;
        Grid.SetRow(scrollViewer, 1);
        mainGrid.Children.Add(scrollViewer);

        var footer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10),
            Spacing = 10
        };

        var cancelButton = new Button
        {
            Content = "Cancel",
            Padding = new Thickness(20, 5)
        };
        cancelButton.Click += (s, e) => Close();

        footer.Children.Add(cancelButton);
        Grid.SetRow(footer, 2);
        mainGrid.Children.Add(footer);

        Content = mainGrid;
    }

    private Panel CreateImageGrid()
    {
        var wrapPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(ItemMargin)
        };

        foreach (var kvp in _imageAction)
        {
            var imageItem = CreateImageItem(kvp.Key, kvp.Value);
            wrapPanel.Children.Add(imageItem);
        }

        return wrapPanel;
    }

    private Control CreateImageItem(sbyte key, Bitmap bitmap)
    {
        var stackPanel = new StackPanel
        {
            Width = ItemWidth,
            Height = ItemHeight,
            Margin = new Thickness(ItemMargin),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        var border = new Border
        {
            Width = 70,
            Height = 70,
            BorderThickness = new Thickness(2),
            BorderBrush = Brushes.Gray,
            CornerRadius = new CornerRadius(5),
            Background = Brushes.White,
            ClipToBounds = true,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var image = new Image
        {
            Width = 66,
            Height = 66,
            Stretch = Stretch.Uniform,
            Source = bitmap
        };

        var label = new TextBlock
        {
            Text = $"ID: {key}",
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 11,
            Margin = new Thickness(0, 5, 0, 0),
            Foreground = Brushes.DarkGray
        };

        border.Child = image;
        stackPanel.Children.Add(border);
        stackPanel.Children.Add(label);

        var capturedKey = key;
        var capturedBitmap = bitmap;

        stackPanel.Tapped += (s, e) =>
        {
            ImageSelected?.Invoke(this, new ImageSelectedEventArgs(capturedKey, capturedBitmap));
            Close();
        };

        stackPanel.PointerEntered += (s, e) =>
        {
            border.BorderBrush = Brushes.Blue;
            border.Background = Brushes.LightBlue;
        };

        stackPanel.PointerExited += (s, e) =>
        {
            border.BorderBrush = Brushes.Gray;
            border.Background = Brushes.White;
        };

        return stackPanel;
    }
}