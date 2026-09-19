using System;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;

namespace ToolEffectNro.Options;

public class ImageChooser : Control
{
    public static readonly StyledProperty<IImage> SourceProperty =
        AvaloniaProperty.Register<ImageCropper, IImage?>(nameof(Source));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImageCropper, Stretch>(nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<ImageCropper, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);

    static ImageChooser()
    {
        AffectsRender<ImageCropper>(SourceProperty, StretchProperty, StretchDirectionProperty);
        AffectsMeasure<ImageCropper>(SourceProperty, StretchProperty, StretchDirectionProperty);
    }

    public ImageChooser()
    {
        PointerPressed += ImageCanvas_OnPointerPressed;
        PointerReleased += ImageCanvas_OnPointerReleased;
    }

    [Content]
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    public bool IsSelected { get; set; }

    protected override bool BypassFlowDirectionPolicies => true;

    public event EventHandler? Selected;

    public sealed override void Render(DrawingContext context)
    {
        base.Render(context);
        var source = Source;

        if (source != null && Bounds.Width > 0 && Bounds.Height > 0) context.DrawImage(source, new Rect(source.Size));
        var borderRect = new Rect(0, 0, Bounds.Width - 1, Bounds.Height - 1);
        var pen = IsSelected ? new Pen(Brushes.Red, 2) : new Pen(Brushes.Aquamarine, 2);
        context.DrawRectangle(pen, borderRect);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var source = Source;
        var result = new Size();

        if (source != null)
            result = Stretch.CalculateSize(availableSize, source.Size, StretchDirection);

        return result;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var source = Source;

        if (source == null) return new Size();
        var sourceSize = source.Size;
        var result = Stretch.CalculateSize(finalSize, sourceSize);
        return result;
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ImageAutomationPeer(this);
    }


    private void ImageCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
    }

    private void ImageCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Selected?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        InvalidateVisual();
    }
}