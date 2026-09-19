using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ToolPart.Classes;
using ToolPart.Options;

namespace ToolPart.Windows;

public partial class ImportImage : Window
{
    public ImportImage()
    {
        InitializeComponent();
        Instance = this;
        Loaded += OnLoaded;
    }

    public static ImportImage Instance { get; private set; }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!(WindowExecution.Instance.PartHead?.Count > 0) || !(WindowExecution.Instance.PartBody?.Count > 0) ||
            !(WindowExecution.Instance.PartLeg?.Count > 0)) return;
        for (var i = 0; i < WindowExecution.Instance.PartHead.Count; i++)
        {
            if (PanelHead.Children[i] is not ImagePart control) return;
            var p = WindowExecution.Instance.PartHead[i];
            control.Source = p.Image;
            control.IdImage = p.Id;
        }

        for (var i = 0; i < WindowExecution.Instance.PartBody.Count; i++)
        {
            if (i == 16) continue;
            if (PanelBody.Children[i] is not ImagePart control) return;
            var p = WindowExecution.Instance.PartBody[i];
            control.Source = p.Image;
            control.IdImage = p.Id;
        }

        for (var i = 0; i < WindowExecution.Instance.PartLeg.Count; i++)
        {
            if (i == 13) continue;
            if (PanelLeg.Children[i] is not ImagePart control) return;
            var p = WindowExecution.Instance.PartLeg[i];
            control.Source = p.Image;
            control.IdImage = p.Id;
        }
    }

    private async void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var partResults = new List<List<PartImage>>(3);
            var partHead = ProcessPanel(PanelHead, BodyEnum.Head);
            partResults.Add(partHead);
            var partBody = ProcessPanel(PanelBody, BodyEnum.Body);
            partBody.Add(new PartImage(0, 0, 0, WindowExecution.ImgNull, BodyEnum.Body));
            partResults.Add(partBody);
            var partLeg = ProcessPanel(PanelLeg, BodyEnum.Leg);
            partLeg.Add(new PartImage(0, 0, 0, WindowExecution.ImgNull, BodyEnum.Leg));
            partResults.Add(partLeg);
            Close(partResults);
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("Lỗi", "Không thể thực hiện", ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
            Close();
        }
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private List<PartImage> ProcessPanel(Panel panel, BodyEnum bodyType)
    {
        if (panel == null) return [];
        var sortedImages = panel.Children
            .OfType<ImagePart>()
            .Select(image => new
            {
                ImagePart = image,
                Id = Function.TryParseId(image.Name?.ToString())
            })
            .Where(item => item.Id.HasValue)
            .OrderBy(item => item.Id.Value)
            .ToList();
        return sortedImages
            .Select(item => new PartImage(
                (short)item.ImagePart.IdImage,
                0, 0,
                (Bitmap)item.ImagePart.Source,
                bodyType))
            .ToList();
    }

    private void ClearButton_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var pic in PanelHead.Children.Cast<ImagePart>()) pic.Source = null;

        foreach (var pic in PanelBody.Children.Cast<ImagePart>()) pic.Source = null;

        foreach (var pic in PanelLeg.Children.Cast<ImagePart>()) pic.Source = null;
    }

    private void FillTransparent_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var pic in PanelHead.Children.Cast<ImagePart>())
        {
            pic.IdImage = 0;
            pic.Source = WindowExecution.ImgNull.CloneBitmap();
        }

        foreach (var pic in PanelBody.Children.Cast<ImagePart>())
        {
            pic.IdImage = 0;
            pic.Source = WindowExecution.ImgNull.CloneBitmap();
        }

        foreach (var pic in PanelLeg.Children.Cast<ImagePart>())
        {
            pic.IdImage = 0;
            pic.Source = WindowExecution.ImgNull.CloneBitmap();
        }
    }
}