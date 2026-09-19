using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ToolEffectNro.Classes;
using ToolEffectNro.Options;
using ToolEffectNro.Windows;

namespace ToolEffectNro.ChildWindows;

public partial class SideForm : Window
{
    public static SideForm Instance;
    private Point _offset;
    private ChildFrame _selectedImage;
    private PanelFrame _selectParentFrame;

    public SideForm()
    {
        InitializeComponent();
        Instance = this;
    }

    public ChildFrame GetChildFrame(Point e)
    {
        var key = short.Parse(_selectParentFrame?.Tag?.ToString() ?? "-1");
        return !FormMainEffect.Instance.ParentFrame.TryGetValue(key, out var value)
            ? null
            : value.FirstOrDefault(k => k.Contains(e));
    }

    private void AddItem_OnClick(object sender, RoutedEventArgs e)
    {   
        if (FormMainEffect.Instance.ParentFrame.Keys.Count + 1 > short.MaxValue)
        {
            MessageBoxManager.GetMessageBoxStandard("Thông Báo",
                "Chỉ có thể thêm tối đa " + short.MaxValue + " frame",
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
            return;
        }

        var key = (short)FormMainEffect.Instance.ParentFrame.Keys.Count;
        FormMainEffect.Instance.ParentFrame.Add(key, []);
        CreatePanelFrame(key);
        WindowExecution.Instance.ArrayFrame.UpdateComboBoxAndGrid();
    }

    public void CreatePanelFrame(short key)
    {
        const double scaleX = 154d / 1084;
        const double scaleY = 109d / 610;
        var pic = new PanelFrame(scaleX, scaleY)
        {
            Width = 154,
            Height = 109,
            Tag = key,
            Margin = new Thickness((Width - 154) / 2, 5, 5, 5)
        };
        pic.Selected += PicOnSelected;
        PanelFrame clone = new(scaleX, scaleY)
        {
            IsClone = true,
            Tag = key
        };
        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(CreateMenuItem("Add Image", "add_regular", Brushes.Green, clone.AddImage_OnClick));
        contextMenu.Items.Add(CreateMenuItem("Remove Image", "remove_regular", Brushes.Red, clone.RemoveItem_OnClick));
        clone.AddLeftClickContextMenu(contextMenu);
        clone.PointerPressed += CloneOnPointerPressed;
        clone.PointerMoved += CloneOnPointerMoved;
        clone.PointerReleased += CloneOnPointerReleased;
        pic.Clone = clone;
        PanelParentFrame.Children.Add(pic);
        pic.InvalidateVisual();
    }

    private MenuItem CreateMenuItem(string header, string iconKey, IBrush color,
        EventHandler<RoutedEventArgs> clickHandler)
    {
        var icon = new PathIcon
        {
            Width = 12,
            Height = 12,
            Foreground = color
        };

        if (Application.Current?.TryFindResource(iconKey, out var resource) == true)
            icon.Data = resource as Geometry;

        var item = new MenuItem
        {
            Header = header,
            Icon = icon
        };

        item.Click += clickHandler;
        return item;
    }

    private void PicOnSelected(object sender, EventArgs e)
    {
        foreach (var child in PanelParentFrame.Children)
            if (child is PanelFrame chooser)
            {
                var isThis = ReferenceEquals(sender, chooser);
                chooser.SetSelected(isThis);
                if (!isThis) continue;
                ClearPanelFrame();
                _selectParentFrame = chooser;
                FormFilmEffect.Instance.MainPanel.Children.Add(chooser.Clone);
                chooser.InvalidateVisual();
            }
    }

    private void CloneOnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (_selectedImage == null) return;
        var panelFrame = sender as PanelFrame;
        _selectedImage.BorderPen = new Pen(Brushes.Black);
        _selectedImage = null;
        panelFrame?.InvalidateVisual();
        FormFilmEffect.Instance.MainPanel.InvalidateAll();
        if (IsVisible) PanelParentFrame.InvalidateAll();
    }

    private void CloneOnPointerMoved(object sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(null);
        if (!point.Properties.IsLeftButtonPressed) return;
        if (sender is not PanelFrame panelFrame || _selectedImage == null) return;
        var position = e.GetPosition(panelFrame);
        _selectedImage.Bounds = new Rect(position.X - _offset.X, position.Y - _offset.Y, _selectedImage.Bounds.Width,
            _selectedImage.Bounds.Height);
        panelFrame.InvalidateVisual();
    }

    private void CloneOnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(null);
        var panelFrame = sender as PanelFrame;
        if (!point.Properties.IsLeftButtonPressed) return;
        if (_selectedImage != null) return;
        var position = e.GetPosition(panelFrame);
        var selectedFrame = GetChildFrame(position);
        if (selectedFrame == null) return;
        _selectedImage = selectedFrame;
        _offset = new Point(position.X - selectedFrame.Bounds.Left, position.Y - selectedFrame.Bounds.Top);
        selectedFrame.BorderPen = new Pen(Brushes.Red);
        panelFrame?.InvalidateVisual();
        FormFilmEffect.Instance.MainPanel.InvalidateAll();
        if (IsVisible) PanelParentFrame.InvalidateAll();
    }

    public void CreatePictureBox()
    {
        ClearPanel();
        foreach (var key in FormMainEffect.Instance.ParentFrame.Keys) CreatePanelFrame(key);
    }

    public void ClearPanel()
    {
        ClearPanelFrame();
        foreach (var pnl in PanelParentFrame.Children)
            if (pnl is PanelFrame panelFrame)
            {
                if (panelFrame.Clone is { } cloneFrame)
                    cloneFrame.Dispose();
                panelFrame.Dispose();
            }

        PanelParentFrame.Children.Clear();
    }

    private void ClearPanelFrame()
    {
        FormFilmEffect.Instance.MainPanel.Children.Clear();
    }

    private void RemoveItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_selectParentFrame == null) return;
        if (!short.TryParse(_selectParentFrame?.Tag?.ToString(), out var idParent)) return;

        if (!FormMainEffect.Instance.ParentFrame.ContainsKey(idParent)) return;
        FormMainEffect.Instance.ArrayFrame.Clear();
        ClearPanelFrame();
        if (_selectParentFrame != null)
        {
            PanelParentFrame.Children.Remove(_selectParentFrame);
            _selectParentFrame.Dispose();
            _selectParentFrame = null;
        }

        var panelsToUpdate = PanelParentFrame.Children.OfType<PanelFrame>().ToList();
        for (var i = 0; i < panelsToUpdate.Count; i++)
        {
            var panel = panelsToUpdate[i];
            panel.Tag = i;
            panel.Clone.Tag = i;
        }

        PanelParentFrame.InvalidateVisual();
        FormMainEffect.Instance.ParentFrame.Remove(idParent);
        var updatedDict = FormMainEffect.Instance.ParentFrame
            .Select((kvp, index) => new { Key = (short)index, kvp.Value })
            .ToDictionary(x => x.Key, x => x.Value);

        FormMainEffect.Instance.ParentFrame = updatedDict;
    }
}