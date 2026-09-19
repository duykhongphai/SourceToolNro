using System;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;

namespace ToolEffectNro.Options;

public class ImageCropper : Control
{
    private static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<ImageCropper, IImage?>(nameof(Source));

    private static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImageCropper, Stretch>(nameof(Stretch), Stretch.Uniform);

    private static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<ImageCropper, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);

    private readonly Anchor[] _rgAnchors = new Anchor[(int)AnchorEnum.We + 1];
    private readonly Size _szAnhorSize = new(8, 8);

    private MouseOperation _eMouseOperation = MouseOperation.None;
    private Rect? _imageRect;

    private Point _offset;
    private Rect _rcMoveResizeInitRect;
    private Point _startPoint;
    public Rect CropRectangle;

    public Vector Scale;

    static ImageCropper()
    {
        AffectsRender<ImageCropper>(SourceProperty, StretchProperty, StretchDirectionProperty);
        AffectsMeasure<ImageCropper>(SourceProperty, StretchProperty, StretchDirectionProperty);
    }

    public ImageCropper()
    {
        PointerPressed += ImageCanvas_OnPointerPressed;
        PointerMoved += ImageCanvas_OnPointerMoved;
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

    protected override bool BypassFlowDirectionPolicies => true;

    public sealed override void Render(DrawingContext context)
    {
        base.Render(context);
        var source = Source;

        if (source != null && Bounds.Width > 0 && Bounds.Height > 0)
        {
            var viewPort = new Rect(Bounds.Size);
            var sourceSize = source.Size;

            Scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
            var scaledSize = sourceSize * Scale;
            var destRect = viewPort
                .CenterRect(new Rect(scaledSize))
                .Intersect(viewPort);
            var sourceRect = new Rect(sourceSize)
                .CenterRect(new Rect(destRect.Size / Scale));
            _imageRect = destRect;
            context.DrawImage(source, sourceRect, destRect);
        }

        RenderCroppingInterface(context);
    }

    private void DisposeRegions()
    {
        for (var eAnchor = AnchorEnum.None; eAnchor <= AnchorEnum.We; eAnchor++)
            _rgAnchors[(int)eAnchor] = null;
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

    private void RenderCroppingInterface(DrawingContext context)
    {
        var pen = new Pen(new SolidColorBrush(Colors.Aqua));
        context.DrawRectangle(null, pen, new Rect(0, 0, Bounds.Width - 1, Bounds.Height - 1));

        pen = new Pen(new SolidColorBrush(Colors.Red), 2);
        context.DrawRectangle(null, pen, CropRectangle);

        if (CropRectangle.Width <= 0 || CropRectangle.Height <= 0) return;
        DisposeRegions();
        var rcAnchors = new Rect[(int)AnchorEnum.We + 1];

        rcAnchors[(int)AnchorEnum.Nwse] = new Rect(
            CropRectangle.Left - _szAnhorSize.Width / 2,
            CropRectangle.Top - _szAnhorSize.Height / 2,
            _szAnhorSize.Width,
            _szAnhorSize.Height);

        var tRcNs = new Rect(
            CropRectangle.Left + CropRectangle.Width / 2 - _szAnhorSize.Width / 2,
            CropRectangle.Top - _szAnhorSize.Height / 2,
            _szAnhorSize.Width,
            _szAnhorSize.Height);

        if (CropRectangle.Width > _szAnhorSize.Width * 2)
        {
            rcAnchors[(int)AnchorEnum.Ns] = tRcNs;

            rcAnchors[(int)AnchorEnum.Sn] = new Rect(
                CropRectangle.Left + CropRectangle.Width / 2 - _szAnhorSize.Width / 2,
                CropRectangle.Bottom - _szAnhorSize.Height / 2,
                _szAnhorSize.Width,
                _szAnhorSize.Height);
        }

        if (CropRectangle.Height > _szAnhorSize.Height * 2)
        {
            rcAnchors[(int)AnchorEnum.Ew] = new Rect(
                CropRectangle.Right - _szAnhorSize.Width / 2,
                CropRectangle.Top + CropRectangle.Height / 2 - _szAnhorSize.Height / 2,
                _szAnhorSize.Width,
                _szAnhorSize.Height);

            rcAnchors[(int)AnchorEnum.We] = new Rect(
                CropRectangle.Left - _szAnhorSize.Width / 2,
                CropRectangle.Top + CropRectangle.Height / 2 - _szAnhorSize.Height / 2,
                _szAnhorSize.Width,
                _szAnhorSize.Height);
        }

        rcAnchors[(int)AnchorEnum.Nesw] = new Rect(
            CropRectangle.Right - _szAnhorSize.Width / 2,
            CropRectangle.Top - _szAnhorSize.Height / 2,
            _szAnhorSize.Width,
            _szAnhorSize.Height);

        rcAnchors[(int)AnchorEnum.Senw] = new Rect(
            CropRectangle.Right - _szAnhorSize.Width / 2,
            CropRectangle.Bottom - _szAnhorSize.Height / 2,
            _szAnhorSize.Width,
            _szAnhorSize.Height);

        rcAnchors[(int)AnchorEnum.Swne] = new Rect(
            CropRectangle.Left - _szAnhorSize.Width / 2,
            CropRectangle.Bottom - _szAnhorSize.Height / 2,
            _szAnhorSize.Width,
            _szAnhorSize.Height);

        for (var eAnchor = AnchorEnum.Nwse; eAnchor <= AnchorEnum.We; eAnchor++)
        {
            var rect = rcAnchors[(int)eAnchor];

            if (!(rect.Width > 0) || !(rect.Height > 0)) continue;
            var brush = eAnchor == (AnchorEnum)_eMouseOperation
                ? new SolidColorBrush(Colors.White)
                : new SolidColorBrush(Colors.LightGray);

            context.FillRectangle(brush, rect);

            context.DrawRectangle(
                null,
                new Pen(new SolidColorBrush(Colors.Black)),
                rect);
        }

        for (var eAnchor = AnchorEnum.Nwse; eAnchor <= AnchorEnum.We; eAnchor++)
        {
            var rect = rcAnchors[(int)eAnchor];
            if (rect is { Width: > 0, Height: > 0 })
                _rgAnchors[(int)eAnchor] = new Anchor(rect.X, rect.Y, rect.Width, rect.Height);
            else
                _rgAnchors[(int)eAnchor] = null;
        }
    }

    private void ImageCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed) return;
        if (_eMouseOperation == MouseOperation.None) return;
        if (_eMouseOperation == MouseOperation.Crop &&
            (CropRectangle.Width < 5 || CropRectangle.Height < 5))
            CropRectangle = new Rect(0, 0, 0, 0);
        _eMouseOperation = MouseOperation.None;
        Cursor = new Cursor(StandardCursorType.Arrow);
        InvalidateVisual();
    }

    private void ImageCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed || Source == null) return;
        var position = e.GetPosition(this);
        if (_eMouseOperation != MouseOperation.None) return;
        if (CropRectangle.Width <= 0 || CropRectangle.Height <= 0)
        {
            if (!_imageRect.HasValue || !_imageRect.Value.Contains(position)) return;
            _eMouseOperation = MouseOperation.Crop;
            _startPoint = new Point(position.X, position.Y);
            CropRectangle = new Rect(position.X, position.Y, 1, 1);
            InvalidateVisual();
            return;
        }

        if (CropRectangle.Width > 0 && CropRectangle.Height > 0)
            for (var eAnchor = AnchorEnum.Nwse; eAnchor <= AnchorEnum.We; eAnchor++)
            {
                if (_rgAnchors[(int)eAnchor] == null ||
                    !_rgAnchors[(int)eAnchor].IsVisible(new Point(position.X, position.Y))) continue;
                _eMouseOperation = (MouseOperation)eAnchor;
                _startPoint = new Point(position.X, position.Y);
                _rcMoveResizeInitRect = CropRectangle;
                return;
            }

        if (CropRectangle.Contains(position))
        {
            Cursor = new Cursor(StandardCursorType.SizeAll);
            _eMouseOperation = MouseOperation.Move;
            _offset = new Point(position.X - CropRectangle.X, position.Y - CropRectangle.Y);
            _rcMoveResizeInitRect = CropRectangle;
            return;
        }

        if (!_imageRect.HasValue || !_imageRect.Value.Contains(position)) return;
        _eMouseOperation = MouseOperation.Crop;
        _startPoint = new Point(position.X, position.Y);
        CropRectangle = new Rect(position.X, position.Y, 1, 1);
        InvalidateVisual();
    }

    private Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
    {
        var angleInRadians = angleInDegrees * (Math.PI / 180);
        var cosTheta = Math.Cos(angleInRadians);
        var sinTheta = Math.Sin(angleInRadians);
        return new Point(
            Convert.ToInt32(cosTheta * (pointToRotate.X - centerPoint.X) -
                sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
            Convert.ToInt32(sinTheta * (pointToRotate.X - centerPoint.X) +
                            cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y));
    }

    private StandardCursorType AnchorToCursor(AnchorEnum eAnchor)
    {
        var snAngle = 0.0f;

        switch (eAnchor)
        {
            case AnchorEnum.Nwse:
            case AnchorEnum.Senw:
                snAngle = 45f;
                break;
            case AnchorEnum.Ns:
            case AnchorEnum.Sn:
                snAngle = 90f;
                break;
            case AnchorEnum.Nesw:
            case AnchorEnum.Swne:
                snAngle = 135f;
                break;
            case AnchorEnum.Ew:
            case AnchorEnum.We:
                snAngle = 0f;
                break;
            default:
                snAngle = 0f;
                break;
        }

        if (snAngle > 360) snAngle -= 360;

        if ((snAngle >= 26 && snAngle <= 68) || (snAngle >= 204 && snAngle <= 248))
            return StandardCursorType.BottomRightCorner;
        if ((snAngle >= 69 && snAngle <= 113) || (snAngle >= 249 && snAngle <= 293))
            return StandardCursorType.SizeNorthSouth;
        if ((snAngle >= 114 && snAngle <= 158) || (snAngle >= 294 && snAngle <= 338))
            return StandardCursorType.BottomLeftCorner;

        return StandardCursorType.SizeWestEast;
    }

    private void ImageCanvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (Source == null) return;

        var position = e.GetPosition(this);

        switch (_eMouseOperation)
        {
            case MouseOperation.None:
            {
                var cursorType = StandardCursorType.Arrow;

                for (var eAnchor = AnchorEnum.Nwse; eAnchor <= AnchorEnum.We; eAnchor++)
                    if (_rgAnchors[(int)eAnchor] != null &&
                        _rgAnchors[(int)eAnchor].IsVisible(new Point(position.X, position.Y)))
                    {
                        cursorType = AnchorToCursor(eAnchor);
                        break;
                    }

                if (cursorType == StandardCursorType.Arrow)
                    if (!(CropRectangle.Width <= 0 || CropRectangle.Height <= 0) && CropRectangle.Contains(position))
                        cursorType = StandardCursorType.SizeAll;

                if (cursorType == StandardCursorType.Arrow && _imageRect.HasValue &&
                    _imageRect.Value.Contains(position))
                    cursorType = StandardCursorType.Cross;

                if (Cursor == null || Cursor.Equals(new Cursor(cursorType)) == false)
                    Cursor = new Cursor(cursorType);
                break;
            }
            case MouseOperation.Crop:
            {
                Cursor = new Cursor(StandardCursorType.Cross);

                var bounds = _imageRect ?? Bounds;

                var x = Math.Max(Math.Min(position.X, bounds.Right), bounds.Left);
                var y = Math.Max(Math.Min(position.Y, bounds.Bottom), bounds.Top);

                var width = x - _startPoint.X;
                var height = y - _startPoint.Y;

                var left = width < 0 ? _startPoint.X + width : _startPoint.X;
                var top = height < 0 ? _startPoint.Y + height : _startPoint.Y;
                width = Math.Abs(width);
                height = Math.Abs(height);

                if (left < bounds.Left)
                {
                    width -= bounds.Left - left;
                    left = bounds.Left;
                }

                if (top < bounds.Top)
                {
                    height -= bounds.Top - top;
                    top = bounds.Top;
                }

                if (left + width > bounds.Right)
                    width = bounds.Right - left;

                if (top + height > bounds.Bottom)
                    height = bounds.Bottom - top;

                CropRectangle = new Rect(left, top, width, height);
                InvalidateVisual();
                break;
            }
            case MouseOperation.Move when _rcMoveResizeInitRect.Width <= 0 || _rcMoveResizeInitRect.Height <= 0:
                return;
            case MouseOperation.Move:
            {
                var bounds = _imageRect ?? Bounds;

                var newX = Math.Max(bounds.Left, Math.Min(position.X - _offset.X, bounds.Right - CropRectangle.Width));
                var newY = Math.Max(bounds.Top, Math.Min(position.Y - _offset.Y, bounds.Bottom - CropRectangle.Height));

                CropRectangle = new Rect(
                    newX,
                    newY,
                    CropRectangle.Width,
                    CropRectangle.Height);

                InvalidateVisual();
                break;
            }
            case >= MouseOperation.Nwse and <= MouseOperation.We:
            {
                var tDest = new Point(position.X, position.Y);
                var bounds = _imageRect ?? Bounds;

                if (Convert.ToBoolean(0.0)) tDest = RotatePoint(tDest, _startPoint, Convert.ToDouble(-0.0));

                var tPt = new Point(tDest.X - _startPoint.X, tDest.Y - _startPoint.Y);
                var tRc = _rcMoveResizeInitRect;

                switch (_eMouseOperation)
                {
                    case MouseOperation.Nwse:
                        tRc = new Rect(
                            tRc.X + tPt.X,
                            tRc.Y + tPt.Y,
                            tRc.Width - tPt.X,
                            tRc.Height - tPt.Y);
                        break;
                    case MouseOperation.Ns:
                        tRc = new Rect(
                            tRc.X,
                            tRc.Y + tPt.Y,
                            tRc.Width,
                            tRc.Height - tPt.Y);
                        break;
                    case MouseOperation.Nesw:
                        tRc = new Rect(
                            tRc.X,
                            tRc.Y + tPt.Y,
                            tRc.Width + tPt.X,
                            tRc.Height - tPt.Y);
                        break;
                    case MouseOperation.Ew:
                        tRc = new Rect(
                            tRc.X,
                            tRc.Y,
                            tRc.Width + tPt.X,
                            tRc.Height);
                        break;
                    case MouseOperation.Senw:
                        tRc = new Rect(
                            tRc.X,
                            tRc.Y,
                            tRc.Width + tPt.X,
                            tRc.Height + tPt.Y);
                        break;
                    case MouseOperation.Sn:
                        tRc = new Rect(
                            tRc.X,
                            tRc.Y,
                            tRc.Width,
                            tRc.Height + tPt.Y);
                        break;
                    case MouseOperation.Swne:
                        tRc = new Rect(
                            tRc.X + tPt.X,
                            tRc.Y,
                            tRc.Width - tPt.X,
                            tRc.Height + tPt.Y);
                        break;
                    case MouseOperation.We:
                        tRc = new Rect(
                            tRc.X + tPt.X,
                            tRc.Y,
                            tRc.Width - tPt.X,
                            tRc.Height);
                        break;
                }

                if (tRc.X < bounds.Left)
                    tRc = new Rect(
                        bounds.Left,
                        tRc.Y,
                        tRc.Width + (tRc.X - bounds.Left),
                        tRc.Height);

                if (tRc.Right > bounds.Right)
                    tRc = new Rect(
                        tRc.X,
                        tRc.Y,
                        bounds.Right - tRc.X,
                        tRc.Height);

                if (tRc.Y < bounds.Top)
                    tRc = new Rect(
                        tRc.X,
                        bounds.Top,
                        tRc.Width,
                        tRc.Height + (tRc.Y - bounds.Top));

                if (tRc.Bottom > bounds.Bottom)
                    tRc = new Rect(
                        tRc.X,
                        tRc.Y,
                        tRc.Width,
                        bounds.Bottom - tRc.Y);

                if (tRc.Width < 1 || tRc.Height < 1) return;

                if (CropRectangle.Equals(tRc)) return;

                CropRectangle = tRc;
                InvalidateVisual();
                break;
            }
        }
    }

    private class Anchor
    {
        private Rect _rect;

        public Anchor(double x, double y, double width, double height)
        {
            _rect = new Rect(x, y, width, height);
        }

        public void Update(double x, double y)
        {
            _rect = new Rect(x - _rect.Width / 2, y - _rect.Height / 2, _rect.Width, _rect.Height);
        }

        public bool IsVisible(Point point)
        {
            return _rect.Contains(point);
        }
    }

    private enum MouseOperation
    {
        None,
        Nwse,
        Ns,
        Nesw,
        Ew,
        Senw,
        Sn,
        Swne,
        We,
        Move,
        Crop
    }

    private enum AnchorEnum
    {
        None,
        Nwse,
        Ns,
        Nesw,
        Ew,
        Senw,
        Sn,
        Swne,
        We
    }
}