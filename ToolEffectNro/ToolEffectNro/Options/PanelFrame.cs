using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ToolEffectNro.ChildWindows;
using ToolEffectNro.Classes;
using ToolEffectNro.Windows;

namespace ToolEffectNro.Options;

public class PanelFrame : Control
{
    private readonly double _scaleX;
    private readonly double _scaleY;
    private bool _isSelected;
    private Point _position;
    public PanelFrame Clone;
    public bool IsClone;

    public PanelFrame(double scaleX, double scaleY)
    {
        _scaleX = scaleX;
        _scaleY = scaleY;
        IsHitTestVisible = true;
        Focusable = true;
        PointerPressed += PanelFrame_OnPointerPressed;
    }

    public event EventHandler Selected;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(Brushes.Transparent, new Rect(0, 0, Bounds.Width, Bounds.Height));
        DrawCrossLinesAndBorder(context);
        DrawFrames(context);
    }

    private void DrawCrossLinesAndBorder(DrawingContext context)
    {
        IPen borderPen =
            new Pen(
                IsClone ? Brushes.White : _isSelected ? Brushes.Green : new SolidColorBrush(Color.Parse("#FF4500")));
        IPen whitePen = new Pen(Brushes.White);
        if (IsClone)
        {
            context.DrawLine(
                whitePen,
                new Point(0, WindowExecution.XOriginal),
                new Point(Bounds.Width, WindowExecution.XOriginal)
            );
            context.DrawLine(
                whitePen,
                new Point(WindowExecution.YOriginal, 0),
                new Point(WindowExecution.YOriginal, Bounds.Height)
            );
        }

        context.DrawRectangle(null, borderPen, new Rect(0, 0, Bounds.Width - 1, Bounds.Height - 1));
    }

    private void DrawFrames(DrawingContext context)
    {
        var key = short.Parse(Tag.ToString());
        DrawFrameChildren(context, FormMainEffect.Instance.ParentFrame[key], _scaleX, _scaleY);
    }

    public void Dispose()
    {
        Clone = null;
    }

    private void DrawFrameChildren(DrawingContext context, List<ChildFrame> frameChildren, double scaleX, double scaleY)
    {
        foreach (var frameChild in frameChildren)
            if (IsClone)
            {
                context.DrawImage(
                    frameChild.ImageInfo.Image,
                    new Rect(0, 0, frameChild.ImageInfo.Image.PixelSize.Width,
                        frameChild.ImageInfo.Image.PixelSize.Height),
                    new Rect(frameChild.Bounds.X + WindowExecution.YOriginal,
                        frameChild.Bounds.Y + WindowExecution.XOriginal, frameChild.Bounds.Width,
                        frameChild.Bounds.Height)
                );
                context.DrawRectangle(
                    frameChild.BorderPen,
                    new Rect(frameChild.Bounds.X + WindowExecution.YOriginal,
                        frameChild.Bounds.Y + WindowExecution.XOriginal, frameChild.Bounds.Width,
                        frameChild.Bounds.Height)
                );
            }
            else
            {
                var newWidth = (int)(frameChild.ImageInfo.Image.PixelSize.Width * scaleX);
                var newHeight = (int)(frameChild.ImageInfo.Image.PixelSize.Height * scaleY);
                using var resizedImage =
                    frameChild.ImageInfo.Image.CreateScaledBitmap(new PixelSize(newWidth, newHeight));
                context.DrawImage(
                    resizedImage,
                    new Rect(0, 0, resizedImage.PixelSize.Width, resizedImage.PixelSize.Height),
                    new Rect((frameChild.Bounds.X + WindowExecution.YOriginal) * scaleX,
                        (frameChild.Bounds.Y + WindowExecution.XOriginal) * scaleY, resizedImage.Size.Width,
                        resizedImage.Size.Height)
                );
            }
    }

    private void PanelFrame_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Selected?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        InvalidateVisual();
    }

    public void AddLeftClickContextMenu(ContextMenu contextMenu)
    {
        if (!IsClone) return;
        PointerPressed += (sender, e) =>
        {
            var pointer = e.GetCurrentPoint(this);
            if (!pointer.Properties.IsRightButtonPressed) return;
            _position = pointer.Position;
            contextMenu.Open(this);
            e.Handled = true;
        };
    }

    public async void AddImage_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsClone) return;
        var addFrameForm = new AddFrame();
        var result = await addFrameForm.ShowDialog<List<byte>>(WindowExecution.Instance);
        if (result == null) return;
        var idParent = short.Parse(Tag?.ToString() ?? "-1");
        FormMainEffect.Instance.ParentFrame.TryGetValue(idParent, out var chill);
        if (chill == null) return;
        if (chill.Count + 1 > byte.MaxValue)
        {
            await MessageBoxManager.GetMessageBoxStandard("Lỗi", $"Chỉ có thể thêm tối đa {byte.MaxValue} ảnh",
                ButtonEnum.Ok,
                Icon.Error).ShowAsync();
            return;
        }

        foreach (var t in result)
            FormMainEffect.Instance.ParentFrame[idParent].Add(new ChildFrame((byte)chill.Count, idParent,
                t));
        InvalidateVisual();
        FormFilmEffect.Instance.MainPanel.InvalidateAll();
        if (SideForm.Instance.IsVisible) SideForm.Instance.PanelParentFrame.InvalidateAll();
    }

    public void RemoveItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsClone) return;
        var selectedFrame = SideForm.Instance.GetChildFrame(_position);
        if (selectedFrame == null) return;
        var idChild = selectedFrame.Id;
        var idParent = short.Parse(Tag?.ToString() ?? "-1");
        if (!FormMainEffect.Instance.ParentFrame.TryGetValue(idParent, out var value)) return;
        var frame = value.FirstOrDefault(a => a.Id == idChild);
        value.Remove(frame);
        InvalidateVisual();
        FormFilmEffect.Instance.MainPanel.InvalidateAll();
        if (SideForm.Instance.IsVisible) SideForm.Instance.PanelParentFrame.InvalidateAll();
    }
}