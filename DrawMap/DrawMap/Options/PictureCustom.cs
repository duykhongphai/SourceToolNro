using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;
using DrawMap.ChildWindows;

namespace DrawMap.Options;

public class PictureCustom : Control, IComparable<PictureCustom>
{
    public static readonly StyledProperty<IImage> SourceProperty =
        AvaloniaProperty.Register<PictureCustom, IImage?>(nameof(Source));

    private static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<PictureCustom, Stretch>(nameof(Stretch), Stretch.Uniform);

    private static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<PictureCustom, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);

    private static readonly Pen AquaBorderPen = new(Brushes.Aqua);
    private static readonly Pen BlueVioletBorderPen = new(Brushes.BlueViolet);
    private static readonly Pen RedPen = new(Brushes.Red, 2);
    private static readonly IBrush TransparentBrush = Brushes.Transparent;

    static PictureCustom()
    {
        AffectsRender<PictureCustom>(SourceProperty, StretchProperty, StretchDirectionProperty);
        AffectsMeasure<PictureCustom>(SourceProperty, StretchProperty, StretchDirectionProperty);
    }

    public PictureCustom()
    {
        PointerExited += OnPointerExited;
        PointerEntered += OnPointerEntered;
        Margin = new Thickness(5);
        _colorBorder = AquaBorderPen;
    }

    public FieldsData DataPicture { get; set; }

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

    protected override bool BypassFlowDirectionPolicies => true;

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _colorBorder = BlueVioletBorderPen;
        InvalidateVisual();
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _colorBorder = AquaBorderPen;
        InvalidateVisual();
    }

    public sealed override void Render(DrawingContext context)
    {
        switch (TypePicture)
        {
            case Enum.Effect:
                EffectData.Paint(context);
                break;
            case Enum.Monster:
                EffectData.Paint(context, 0, Width / 2, Height);
                break;
            default:
                RenderImage(context);
                break;
        }

        var bounds = Bounds;
        var borderRect = new Rect(0, 0, bounds.Width - 1, bounds.Height - 1);
        context.FillRectangle(TransparentBrush, borderRect);
        if (ShouldRenderBorder())
            context.DrawRectangle(_colorBorder, borderRect);
        if (TypePicture == Enum.Npc)
            RenderNpc(context);
        if (ShouldRenderTileTypeIndicators())
            RenderTileTypeIndicators(context);
    }

    private void RenderNpc(DrawingContext context)
    {
        RenderBodyPart(context, DataPicture.BodyId, 1, -9, 17);
        RenderBodyPart(context, DataPicture.HeadId, 0, -13, 34);
        RenderBodyPart(context, DataPicture.LegId, 1, -8, 10);
    }

    public void RenderNpc(DrawingContext context, double cx, double cy)
    {
        RenderBodyPart(context, DataPicture.BodyId, 1, -9, 17, cx, cy);
        RenderBodyPart(context, DataPicture.HeadId, 0, -13, 34, cx, cy);
        RenderBodyPart(context, DataPicture.LegId, 1, -8, 10, cx, cy);
    }

    private void RenderBodyPart(DrawingContext context, short partId, int piIndex, double x, double y, double cx = 0,
        double cy = 0)
    {
        if (!Fields.ResourcePartData.TryGetValue(partId, out var part) ||
            !Fields.ResourceImagePart.TryGetValue(part.Pi[piIndex].Id, out var img)) return;
        var data = part.Pi[piIndex];
        var finalX = cx + data.Dx + (Width / 2 + x);
        var finalY = cy + data.Dy + (Height - y);
        context.DrawImage(img, new Rect(finalX, finalY, img.Size.Width, img.Size.Height));
    }

    private void RenderImage(DrawingContext context)
    {
        base.Render(context);
        var source = Source;
        if (source?.Size is not { Width: > 0, Height: > 0 } || Bounds is not { Width: > 0, Height: > 0 })
            return;

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

    private bool ShouldRenderBorder()
    {
        return TypePicture is Enum.TileMap or Enum.Effect or Enum.Monster or Enum.Npc or Enum.Eraser;
    }

    private bool ShouldRenderTileTypeIndicators()
    {
        var manager = ManagerDrawMap.Instance;
        if (manager?.TitleTypeCheck == null)
            return false;
        if (manager.TitleTypeCheck.IsChecked != true)
            return false;
        if (TypePicture != Enum.TileMap)
            return false;
        return TypeBlock != null && TypeBlock.Count != 0;
    }

    private void RenderTileTypeIndicators(DrawingContext context)
    {
        var width = Width;
        var height = Height;

        if (TypeBlock.Contains(Fields.T_TOP))
            context.DrawLine(RedPen, new Point(5, 5), new Point(width - 5, 5));

        if (TypeBlock.Contains(Fields.T_BOTTOM))
            context.DrawLine(RedPen, new Point(5, height - 5), new Point(width - 5, height - 5));

        if (TypeBlock.Contains(Fields.T_LEFT))
            context.DrawLine(RedPen, new Point(5, 5), new Point(5, height - 5));

        if (TypeBlock.Contains(Fields.T_RIGHT))
            context.DrawLine(RedPen, new Point(width - 5, 5), new Point(width - 5, height - 5));
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

    #region Variable

    private Pen _colorBorder;
    public List<int> TypeBlock { get; set; }
    public Enum TypePicture { get; set; }
    public EffectData EffectData { get; set; }

    #endregion

    #region Functions

    public int CompareTo(PictureCustom other)
    {
        if (other == null) return 1;

        return TypePicture switch
        {
            Enum.TileMap or Enum.ItemBackground or Enum.Effect or Enum.Monster or Enum.Npc
                => GetId().CompareTo(other.GetId()),
            Enum.Background => GetBgId().CompareTo(other.GetBgId()),
            _ => 0
        };
    }

    public int GetId()
    {
        return DataPicture?.Id ?? -1;
    }

    public int GetLayer()
    {
        return DataPicture?.Layer ?? -1;
    }

    public int GetDx()
    {
        return DataPicture?.Dx ?? -1;
    }

    public int GetDy()
    {
        return DataPicture?.Dy ?? -1;
    }

    public int GetIdImage()
    {
        return DataPicture?.IdImage ?? -1;
    }

    public int GetIdParent()
    {
        return DataPicture?.IdParent ?? -1;
    }

    public byte GetBgType()
    {
        return DataPicture.BackgroundType;
    }

    public byte GetBgId()
    {
        return DataPicture.BackgroundId;
    }

    #endregion
}