using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ToolEffectNro.ChildWindows;
using ToolEffectNro.Classes;
using ToolEffectNro.Windows;

namespace ToolEffectNro.Options;

public class CustomPanel : Control
{
    public readonly DispatcherTimer UpdateEffect;
    private short _currFrame = -1;
    private bool _isGetTime;
    private Point _offset;
    private ChildFrame _selectedImage;
    private int _t;
    public short[] Data;
    public bool StartViewEff;

    public CustomPanel()
    {
        PointerPressed += CustomPanel_PointerPressed;
        PointerMoved += CustomPanel_PointerMoved;
        PointerReleased += CustomPanel_PointerReleased;
        UpdateEffect = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        UpdateEffect.Tick += UpdateEffect_Tick;
    }

    private void CustomPanel_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var position = e.GetPosition(this);
        var selectedFrame = FormMainEffect.Instance.GetChildFrame(new Point(position.X, position.Y));
        if (selectedFrame == null) return;
        _selectedImage = selectedFrame;
        _offset = new Point(position.X - selectedFrame.Bounds.Left, position.Y - selectedFrame.Bounds.Top);
        InvalidateVisual();
    }

    private void CustomPanel_PointerMoved(object sender, PointerEventArgs e)
    {
        var pointer = e.GetCurrentPoint(this);
        if (!pointer.Properties.IsLeftButtonPressed || _selectedImage == null) return;
        var deltaX = pointer.Position.X - _offset.X - _selectedImage.Bounds.Left;
        var deltaY = pointer.Position.Y - _offset.Y - _selectedImage.Bounds.Top;

        var frame = FormMainEffect.Instance.ParentFrame[_selectedImage.IdParent];
        foreach (var childFrame in frame)
            childFrame.Bounds = new Rect(
                childFrame.Bounds.Left + deltaX,
                childFrame.Bounds.Top + deltaY,
                childFrame.Bounds.Width,
                childFrame.Bounds.Height
            );

        InvalidateVisual();
    }

    private void CustomPanel_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (_selectedImage == null) return;
        _selectedImage = null;
        InvalidateVisual();
    }

    public sealed override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawLine(
            new Pen(Brushes.White),
            new Point(0, WindowExecution.XOriginal),
            new Point(Bounds.Width, WindowExecution.XOriginal)
        );

        context.DrawLine(
            new Pen(Brushes.White),
            new Point(WindowExecution.YOriginal, 0),
            new Point(WindowExecution.YOriginal, Bounds.Height)
        );

        if (ManagerEffect.Instance.ViewChar.IsChecked == true)
        {
            var charImage = WindowExecution.Instance.ImageChar;

            var destRect = new Rect(
                WindowExecution.YOriginal - charImage.Size.Width / 2,
                WindowExecution.XOriginal - charImage.Size.Height,
                charImage.Size.Width,
                charImage.Size.Height
            );
            context.DrawImage(charImage, new Rect(0, 0, charImage.Size.Width, charImage.Size.Height), destRect);
        }

        if (!StartViewEff)
        {
            foreach (var listChild in FormMainEffect.Instance.ParentFrame.Values)
            foreach (var frameChild in listChild)
            {
                var frameImage = frameChild.ImageInfo.Image;
                context.DrawImage(
                    frameImage,
                    new Rect(0, 0, frameImage.Size.Width, frameImage.Size.Height),
                    new Rect(frameChild.Bounds.X + WindowExecution.YOriginal,
                        frameChild.Bounds.Y + WindowExecution.XOriginal, frameChild.Bounds.Width,
                        frameChild.Bounds.Height)
                );
            }
        }
        else if (Data != null && _currFrame >= 0)
        {
            if (!FormMainEffect.Instance.ParentFrame.TryGetValue(_currFrame, out var frame)) return;

            foreach (var fl in frame)
            {
                var imageInfo = fl.ImageInfo;
                var frameImage = imageInfo.Image;
                context.DrawImage(
                    frameImage,
                    new Rect(0, 0, frameImage.Size.Width, frameImage.Size.Height),
                    new Rect(fl.Bounds.X + WindowExecution.YOriginal, fl.Bounds.Y + WindowExecution.XOriginal,
                        fl.Bounds.Width, fl.Bounds.Height)
                );
            }
        }

        DrawMoveControls(context);
    }

    private void DrawMoveControls(DrawingContext context)
    {
        const int padding = 8;
        const int spacing = 12;
        const int panelHeight = 22;
        const int textWidth = 28;
        const int panelWidth = spacing * 4 + textWidth + padding * 2;
        const int panelX = 6;
        const int panelY = 6;
        const int startX = panelX + padding;
        const int startY = panelY + panelHeight / 2;
        var backgroundBrush = new SolidColorBrush(Colors.LightGray);
        var backgroundRect = new Rect(panelX, panelY, panelWidth, panelHeight);
        context.FillRectangle(backgroundBrush, backgroundRect, 15);
        var textBrush = new SolidColorBrush(Colors.Black);
        const int textY = startY - 6;
        var formattedText = new FormattedText(
            "F5: Preview",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            10,
            textBrush
        );
        context.DrawText(formattedText, new Point(startX - 4, textY));
    }

    private void UpdateEffect_Tick(object sender, EventArgs e)
    {
        if (Data != null)
        {
            if (!_isGetTime)
            {
                _isGetTime = true;
                var num = Data.Length - 1;
                if (num > 0) _t = Function.NextInt(0, num);
            }

            if (_t < Data.Length) _t++;
            if (_t <= Data.Length - 1) _currFrame = Data[_t];
            if (_t >= Data.Length - 1) _t = 0;
            InvalidateVisual();
        }
        else
        {
            UpdateEffect.Stop();
            StartViewEff = false;
        }
    }
}