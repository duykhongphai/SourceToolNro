using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ToolEffectNro.Classes;
using ToolEffectNro.Options;

namespace ToolEffectNro.ChildWindows;

public partial class ManagerCrop : Window
{
    private readonly LinkedList<ImageInfo> _imageInfoDeletes = [];
    private readonly LinkedList<ImageChooser> _pictureBoxDeletes = [];
    private ImageInfo _imageInfoSelect;
    private int _indexChildDelete;
    private ImageChooser _pictureBoxSelect;

    public ManagerCrop()
    {
        InitializeComponent();
    }

    public void VisibleChange()
    {
        PanelImage.Children.Clear();
        foreach (var key in FormMainEffect.Instance.InfoImage.Keys)
        {
            var img = FormMainEffect.Instance.InfoImage[key].Image;
            ImageChooser pictureBox1 = new()
            {
                Tag = key,
                Source = img,
                Width = img.PixelSize.Width,
                Height = img.PixelSize.Height,
                Margin = new Thickness(5)
            };
            pictureBox1.Selected += ImageChooser_OnSelected;
            PanelImage.Children.Insert(key, pictureBox1);
        }
    }

    public void Clear()
    {
        _pictureBoxSelect = null;
        _imageInfoSelect = null;
        foreach (var pictureBox in _pictureBoxDeletes) pictureBox.Source = null;
        _pictureBoxDeletes.Clear();
        foreach (var imageInfo in _imageInfoDeletes) imageInfo.Image.Dispose();
        _imageInfoDeletes.Clear();
    }

    private async Task<bool> EnsurePictureBoxSelectedAsync(string message)
    {
        if (_pictureBoxSelect != null) return true;
        await MessageBoxManager.GetMessageBoxStandard("Thông Báo",
            message,
            ButtonEnum.Ok,
            MsBox.Avalonia.Enums.Icon.Warning).ShowAsync();
        return false;
    }

    private async void Left_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await EnsurePictureBoxSelectedAsync("Vui Lòng Chọn Ảnh Để Di Chuyển")) return;

        var index = PanelImage.Children.IndexOf(_pictureBoxSelect);
        if (index <= 0) return;
        PanelImage.Children.RemoveAt(index);
        PanelImage.Children.Insert(index - 1, _pictureBoxSelect);
        SwapImageInfo(index, index - 1);
    }

    private void SwapImageInfo(int index1, int index2)
    {
        var dict = FormMainEffect.Instance.InfoImage;
        if (!dict.TryGetValue((byte)index1, out var info1) ||
            !dict.TryGetValue((byte)index2, out var info2)) return;
        dict[(byte)index1] = info2;
        dict[(byte)index2] = info1;
        info1.ID = (byte)index2;
        info2.ID = (byte)index1;
    }

    private async void Right_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await EnsurePictureBoxSelectedAsync("Vui Lòng Chọn Ảnh Để Di Chuyển")) return;

        var index = PanelImage.Children.IndexOf(_pictureBoxSelect);
        if (index >= PanelImage.Children.Count - 1) return;
        PanelImage.Children.RemoveAt(index);
        PanelImage.Children.Insert(index + 1, _pictureBoxSelect);
        SwapImageInfo(index, index + 1);
    }

    private async void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await EnsurePictureBoxSelectedAsync("Vui Lòng Chọn Ảnh Để Xóa")) return;

        var index = PanelImage.Children.IndexOf(_pictureBoxSelect);
        if (_imageInfoSelect == null) return;
        _indexChildDelete = index;
        _pictureBoxDeletes.AddLast(_pictureBoxSelect);
        _imageInfoDeletes.AddLast(_imageInfoSelect);
        PanelImage.Children.RemoveAt(index);
        FormMainEffect.Instance.InfoImage.Remove(_imageInfoSelect.ID);
        RebuildInfoImageDictionary();
        _pictureBoxSelect = null;
        _imageInfoSelect = null;
    }

    private void Restore_OnClick(object sender, RoutedEventArgs e)
    {
        if (_pictureBoxDeletes.Count <= 0) return;
        var pic = _pictureBoxDeletes.Last();
        var info = _imageInfoDeletes.Last();
        _pictureBoxDeletes.RemoveLast();
        _imageInfoDeletes.RemoveLast();
        PanelImage.Children.Insert(_indexChildDelete, pic);
        var newId = (byte)Math.Min(_indexChildDelete, FormMainEffect.Instance.InfoImage.Count);
        FormMainEffect.Instance.InfoImage[newId] = info;
        RebuildInfoImageDictionary();
    }

    private void RebuildInfoImageDictionary()
    {
        var newDict = new Dictionary<byte, ImageInfo>();
        byte newId = 0;
        foreach (var child in PanelImage.Children)
        {
            if (child is not ImageChooser { Tag: byte id }) continue;
            var info = FormMainEffect.Instance.InfoImage.TryGetValue(id, out var value)
                ? value
                : _imageInfoDeletes.FirstOrDefault(x => x.ID == id);

            if (info == null) continue;
            info.ID = newId;
            newDict[newId] = info;
            newId++;
        }

        FormMainEffect.Instance.InfoImage = newDict;
        for (byte i = 0; i < PanelImage.Children.Count; i++)
            if (PanelImage.Children[i] is ImageChooser chooser)
                chooser.Tag = i;
    }

    private void ImageChooser_OnSelected(object sender, EventArgs e)
    {
        foreach (var child in PanelImage.Children)
            if (child is ImageChooser chooser)
            {
                var isThis = ReferenceEquals(sender, chooser);
                chooser.SetSelected(isThis);

                if (!isThis) continue;
                _pictureBoxSelect = chooser;
                _imageInfoSelect = FormMainEffect.Instance.InfoImage[byte.Parse(_pictureBoxSelect.Tag.ToString())];
            }
    }
}