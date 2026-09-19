using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ToolPart.Classes;
using ToolPart.Windows;

namespace ToolPart.Options;

public class PanelFrame : Control
{
    public readonly byte Cf;

    public PanelFrame(byte cf)
    {
        Cf = cf;
    }

    public PartImage GetPartImage(BodyEnum bodyEnum)
    {
        return bodyEnum switch
        {
            BodyEnum.Head => WindowExecution.Instance.PartHead[WindowExecution.CharInfo[Cf][0][0]],
            BodyEnum.Body => WindowExecution.Instance.PartBody[WindowExecution.CharInfo[Cf][2][0]],
            BodyEnum.Leg => WindowExecution.Instance.PartLeg[WindowExecution.CharInfo[Cf][1][0]],
            _ => null
        };
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(Brushes.Transparent, new Rect(0, 0, Bounds.Width, Bounds.Height));
        var idHead = WindowExecution.CharInfo[Cf][0][0];
        var idBody = WindowExecution.CharInfo[Cf][2][0];
        var idLeg = WindowExecution.CharInfo[Cf][1][0];

        if (idHead == -1 || idBody == -1 || idLeg == -1) return;

        var partHead = WindowExecution.Instance.PartHead[idHead];
        var partBody = WindowExecution.Instance.PartBody[idBody];
        var partLeg = WindowExecution.Instance.PartLeg[idLeg];

        if (partHead == null || partBody == null || partLeg == null) return;
        var pen = new Pen(new SolidColorBrush(Colors.DimGray));
        context.DrawRectangle(pen, new Rect(0, 0, Bounds.Width - 1, Bounds.Height - 1));

        var bongImage = WindowExecution.ImgBong;
        if (bongImage != null)
            context.DrawImage(bongImage,
                new Rect(WindowExecution.YOriginal, WindowExecution.XOriginal, bongImage.Size.Width,
                    bongImage.Size.Height));
        if (WindowExecution.Instance.TypePaint.SelectedIndex == 0)
        {
            DrawPartImage(context, partLeg);
            DrawPartImage(context, partBody);
            DrawPartImage(context, partHead);
        }
        else
        {
            DrawPartImage(context, partLeg);
            DrawPartImage(context, partHead);
            DrawPartImage(context, partBody);
        }

        var dashPen = new Pen(new SolidColorBrush(Colors.MediumPurple))
        {
            DashStyle = new DashStyle([4, 2], 0)
        };
        if (partHead.IsSelected) context.DrawRectangle(dashPen, partHead.Bounds);
        if (partLeg.IsSelected) context.DrawRectangle(dashPen, partLeg.Bounds);
        if (partBody.IsSelected) context.DrawRectangle(dashPen, partBody.Bounds);
        DrawMoveControls(context);
    }

    private void DrawMoveControls(DrawingContext context)
    {
        const int padding = 8;
        const int arrowSize = 8;
        const int spacing = 12;
        const int panelHeight = 22;
        const int textWidth = 35;
        const int panelWidth = spacing * 4 + textWidth + padding * 2;
        const int panelX = 6;
        const int panelY = 6;
        const int startX = panelX + padding;
        const int startY = panelY + panelHeight / 2;
        var backgroundBrush = new SolidColorBrush(Colors.LightGray);
        var backgroundRect = new Rect(panelX, panelY, panelWidth, panelHeight);
        context.FillRectangle(backgroundBrush, backgroundRect, 15);
        var arrowPen = new Pen(new SolidColorBrush(Colors.Black), 1.2);
        var textBrush = new SolidColorBrush(Colors.Black);
        DrawArrow(context, arrowPen, startX, startY, arrowSize, ArrowDirection.Left);
        DrawArrow(context, arrowPen, startX + spacing, startY, arrowSize, ArrowDirection.Right);
        DrawArrow(context, arrowPen, startX + spacing * 2, startY - 3, arrowSize, ArrowDirection.Up);
        DrawArrow(context, arrowPen, startX + spacing * 3, startY - 3, arrowSize, ArrowDirection.Down);
        var textX = startX + spacing * 4 + 5;
        var textY = startY - 6;
        var formattedText = new FormattedText(
            ": Move",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            10,
            textBrush
        );
        context.DrawText(formattedText, new Point(textX, textY));
    }

    private void DrawArrow(DrawingContext context, Pen pen, double x, double y, double size, ArrowDirection direction)
    {
        var halfSize = size / 2;
        switch (direction)
        {
            case ArrowDirection.Left:
                context.DrawLine(pen, new Point(x + size, y), new Point(x, y));
                context.DrawLine(pen, new Point(x + halfSize, y - halfSize), new Point(x, y));
                context.DrawLine(pen, new Point(x + halfSize, y + halfSize), new Point(x, y));
                break;

            case ArrowDirection.Right:
                context.DrawLine(pen, new Point(x, y), new Point(x + size, y));
                context.DrawLine(pen, new Point(x + halfSize, y - halfSize), new Point(x + size, y));
                context.DrawLine(pen, new Point(x + halfSize, y + halfSize), new Point(x + size, y));
                break;

            case ArrowDirection.Up:
                context.DrawLine(pen, new Point(x + halfSize, y + size), new Point(x + halfSize, y));
                context.DrawLine(pen, new Point(x, y + halfSize), new Point(x + halfSize, y));
                context.DrawLine(pen, new Point(x + size, y + halfSize), new Point(x + halfSize, y));
                break;

            case ArrowDirection.Down:
                context.DrawLine(pen, new Point(x + halfSize, y), new Point(x + halfSize, y + size));
                context.DrawLine(pen, new Point(x, y + halfSize), new Point(x + halfSize, y + size));
                context.DrawLine(pen, new Point(x + size, y + halfSize), new Point(x + halfSize, y + size));
                break;
        }
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
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<PanelFrame, Stretch>(nameof(Stretch), Stretch.Uniform);
    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<PanelFrame, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);


    private void DrawPartImage(DrawingContext context, PartImage part)
    {
        try
        {
            if (part?.Image != null)
            {
                var sourceSize = new Size(part.Image.PixelSize.Width, part.Image.PixelSize.Height);
                Vector scale = Stretch.CalculateScaling(part.Bounds.Size, sourceSize);
                var scaledWidth = sourceSize.Width * scale.X;
                var scaledHeight = sourceSize.Height * scale.Y;
                var destRect = new Rect(
                    part.Bounds.X + (part.Bounds.Width - scaledWidth) / 2,
                    part.Bounds.Y + (part.Bounds.Height - scaledHeight) / 2,
                    scaledWidth,
                    scaledHeight
                );
                context.DrawImage(part.Image, destRect);
            }
        }
        catch
        {
        }
    }
}

public enum ArrowDirection
{
    Left,
    Right,
    Up,
    Down
}