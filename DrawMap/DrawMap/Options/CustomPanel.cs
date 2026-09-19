using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DrawMap.ChildWindows;

namespace DrawMap.Options;

public class CustomPanel : Control
{
    private const double ScaleFactor = 0.25;
    private static readonly Pen WhitePen = new(Brushes.White);

    public IImage Image { get; set; }


    public sealed override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Width;
        var height = Height;
        var centerX = width * 0.5;
        var lineY = height * 0.8;
        context.DrawLine(WhitePen, new Point(0, lineY), new Point(width, lineY));
        context.DrawLine(WhitePen, new Point(centerX, 0), new Point(centerX, height));
        if (Image == null) return;
        var imageSize = Image.Size;
        var scaledWidth = imageSize.Width * ScaleFactor;
        var scaledHeight = imageSize.Height * ScaleFactor;
        var imageX = centerX - scaledWidth * 0.5;
        var imageY = lineY - scaledHeight;
        var manager = ManagerTile.Instance;
        if (int.TryParse(manager.OffsetXTextBox.Text, out var offsetX) &&
            int.TryParse(manager.OffsetYTextBox.Text, out var offsetY))
        {
            imageX += offsetX;
            imageY += offsetY;
        }

        var maxX = width - scaledWidth;
        var maxY = height - scaledHeight;
        imageX = Math.Max(0, Math.Min(imageX, maxX));
        imageY = Math.Max(0, Math.Min(imageY, maxY));
        if (imageX < width && imageY < height &&
            imageX + scaledWidth > 0 && imageY + scaledHeight > 0)
            context.DrawImage(Image, new Rect(imageX, imageY, scaledWidth, scaledHeight));
    }
}