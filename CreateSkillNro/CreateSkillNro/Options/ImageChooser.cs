using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;
using CreateSkillNro.ChildWindows;

namespace CreateSkillNro.Options;

public class ImageChooser : Control
{
    public static readonly StyledProperty<IImage> SourceProperty =
        AvaloniaProperty.Register<ImageChooser, IImage?>(nameof(Source));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImageChooser, Stretch>(nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<ImageChooser, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);

    public short[] IdEffect;

    public int NumberPaint = -1;

    static ImageChooser()
    {
        AffectsRender<ImageChooser>(SourceProperty, StretchProperty, StretchDirectionProperty);
        AffectsMeasure<ImageChooser>(SourceProperty, StretchProperty, StretchDirectionProperty);
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

    public void SetIdEffect(short[] idEffect)
    {
        IdEffect = idEffect;
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
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.FillRectangle(Brushes.Transparent, bounds);
        if (IdEffect != null)
        {
            try
            {
                foreach (var id in IdEffect)
                {
                    if (id == 0 || FormMainEffect.Instance?.EffectCharPaints?.TryGetValue((short)(id - 1),
                            out var paint) != true) continue;
                    foreach (var t in paint.ArrEffInfo)
                        if (FormMainEffect.Instance.Images.TryGetValue(t.IdImg, out var img))
                        {
                            const double scale = 0.4;
                            var w = img.PixelSize.Width * scale;
                            var h = img.PixelSize.Height * scale;
                            context.DrawImage(
                                img,
                                new Rect(0, 0, img.PixelSize.Width, img.PixelSize.Height),
                                new Rect(t.Dx + Bounds.Width / 2 - w / 2,
                                    t.Dy + Bounds.Height - h, w, h)
                            );
                        }
                }
            }
            catch
            {
            }
        }
        else if (NumberPaint != -1)
        {
            DrawNumberCircle(context);
        }
        else
        {
            var source = Source;
            if (source != null && Bounds is { Width: > 0, Height: > 0 })
            {
                var viewPort = new Rect(Bounds.Size);
                var sourceSize = source.Size;

                var scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
                var scaledSize = sourceSize * scale;
                var destRect = viewPort
                    .CenterRect(new Rect(scaledSize))
                    .Intersect(viewPort);
                var sourceRect = new Rect(sourceSize)
                    .CenterRect(new Rect(destRect.Size / scale));

                context.DrawImage(source, sourceRect, destRect);
            }
        }

        var borderRect = new Rect(0, 0, Bounds.Width - 1, Bounds.Height - 1);
        var pen = IsSelected ? new Pen(Brushes.Red, 2) : new Pen(Brushes.Aquamarine, 2);
        context.DrawRectangle(pen, borderRect);
    }

    private void DrawNumberCircle(DrawingContext context)
    {
        try
        {
            var centerX = Bounds.Width / 2;
            var centerY = Bounds.Height / 2;
            var minSize = Math.Min(Bounds.Width, Bounds.Height);
            var circleRadius = minSize * 0.3;
            var circleGradient = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.FromRgb(59, 130, 246), 0),
                    new GradientStop(Color.FromRgb(37, 99, 235), 1)
                }
            };
            var circleRect = new Rect(centerX - circleRadius, centerY - circleRadius, circleRadius * 2,
                circleRadius * 2);
            context.DrawEllipse(circleGradient, new Pen(Brushes.White, 2), circleRect);
            var numberText = NumberPaint.ToString();
            var textBrush = new SolidColorBrush(Colors.White);
            var fontSize = circleRadius * 0.6;
            var formattedText = new FormattedText(
                numberText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Bold),
                fontSize,
                textBrush
            );
            var textX = centerX - formattedText.Width / 2;
            var textY = centerY - formattedText.Height / 2;
            context.DrawText(formattedText, new Point(textX, textY));
        }
        catch
        {
        }
    }

    private void ImageCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
    }

    private void ImageCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed) return;
        Selected?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        InvalidateVisual();
    }
}