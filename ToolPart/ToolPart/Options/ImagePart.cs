using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using ToolPart.Classes;
using ToolPart.Windows;

namespace ToolPart.Options;

public class ImagePart : Control
{
    public static readonly StyledProperty<IImage> SourceProperty =
        AvaloniaProperty.Register<ImagePart, IImage?>(nameof(Source));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImagePart, Stretch>(nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<ImagePart, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);

    static ImagePart()
    {
        AffectsRender<ImagePart>(SourceProperty, StretchProperty, StretchDirectionProperty);
        AffectsMeasure<ImagePart>(SourceProperty, StretchProperty, StretchDirectionProperty);
    }

    public ImagePart()
    {
        PointerPressed += OnPointerPressed;
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

    public int IdImage { get; set; }

    protected override bool BypassFlowDirectionPolicies => true;

    [Obsolete("Obsolete")]
    private async void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        var fileExplorer = new OpenFileDialog
        {
            AllowMultiple = true,
            Title = "Chọn hình ảnh",
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Name = "Hình ảnh",
                    Extensions = ["png", "jpg", "jpeg", "bmp"]
                }
            }
        };
        var result = await fileExplorer.ShowAsync(ImportImage.Instance);
        if (result == null || result.Length == 0) return;
        if (result.Length == 1)
        {
            await ExportImageAsync(this, result[0]);
            return;
        }

        if (Parent is not WrapPanel parent) return;
        var sortedPictureBoxes = parent.Children
            .OfType<ImagePart>()
            .Select(image => new
            {
                Image = image,
                Id = int.TryParse(Function.ExtractNumbers(image.Name?.ToString()), out var id) ? id : int.MaxValue
            })
            .OrderBy(item => item.Id)
            .Select(item => item.Image)
            .ToList();
        var maxFiles = parent.Name switch
        {
            "PanelHead" => 3,
            "PanelBody" => 17,
            _ => 14
        };
        for (var i = 0; i < result.Length && i < maxFiles && i < sortedPictureBoxes.Count; i++)
            await ExportImageAsync(sortedPictureBoxes[i], result[i]);
    }

    private async Task ExportImageAsync(ImagePart pictureBox, string filePath)
    {
        try
        {
            ((Bitmap)pictureBox.Source)?.Dispose();
            await using var fileStream = new FileStream(filePath, FileMode.Open);
            var bitmap = new Bitmap(fileStream);
            pictureBox.IdImage = int.Parse(Function.ExtractNumbers(Path.GetFileNameWithoutExtension(filePath)));
            pictureBox.Source = bitmap;
        }
        catch
        {
        }
    }


    public sealed override void Render(DrawingContext context)
    {
        var fixedSize = new Size(Width, Height);
        context.FillRectangle(Brushes.LightGray, new Rect(0, 0, Width, Height));
        var source = Source;
        if (source == null || Width <= 0 || Height <= 0)
        {
            const double iconSize = 20;
            const double crossThickness = 4;
            const double crossLength = 12;
            const double radius = iconSize / 2;
            var centerX = Width / 2;
            var centerY = Height / 2;
            var greenBrush = new SolidColorBrush(Color.Parse("#4caf50"));
            var circleGeometry = new EllipseGeometry(new Rect(centerX - radius, centerY - radius, iconSize, iconSize));
            context.DrawGeometry(greenBrush, null, circleGeometry);
            var whiteBrush = new SolidColorBrush(Colors.White);
            var horizontalRect = new Rect(
                centerX - crossLength / 2,
                centerY - crossThickness / 2,
                crossLength,
                crossThickness);
            context.FillRectangle(whiteBrush, horizontalRect);
            var verticalRect = new Rect(
                centerX - crossThickness / 2,
                centerY - crossLength / 2,
                crossThickness,
                crossLength);
            context.FillRectangle(whiteBrush, verticalRect);
            return;
        }

        var sourceSize = source.Size;
        var scale = Stretch.CalculateScaling(fixedSize, sourceSize, StretchDirection);
        var scaledSize = sourceSize * scale;
        var destRect = new Rect(fixedSize).CenterRect(new Rect(scaledSize));
        var sourceRect = new Rect(sourceSize);
        context.DrawImage(source, sourceRect, destRect);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(Width, Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return new Size(Width, Height);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ImageAutomationPeer(this);
    }
}