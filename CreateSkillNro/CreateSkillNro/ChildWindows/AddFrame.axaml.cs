using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CreateSkillNro.Classes;
using CreateSkillNro.Options;

namespace CreateSkillNro.ChildWindows;

public partial class AddFrame : Window
{
    private readonly List<short> _idImages = [];

    public AddFrame()
    {
        InitializeComponent();
        InitPicture();
    }

    private void InitPicture()
    {
        var imageDict = FormMainEffect.Instance?.InfoImage;
        if (imageDict == null) return;
        foreach (var kvp in imageDict) AddPictureBox(kvp.Key, kvp.Value);
    }

    private void AddPictureBox(short key, ImageInfo imageInfo)
    {
        var image = new ImageChooser
        {
            Tag = key,
            Source = imageInfo.Image,
            Width = imageInfo.Image.PixelSize.Width,
            Height = imageInfo.Image.PixelSize.Height,
            Margin = new Thickness(5)
        };
        image.Selected += (sender, args) =>
        {
            image.SetSelected(!image.IsSelected);
            if (!_idImages.Remove(key))
                _idImages.Add(key);
        };
        Content.Children.Add(image);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ApplyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(_idImages);
    }
}