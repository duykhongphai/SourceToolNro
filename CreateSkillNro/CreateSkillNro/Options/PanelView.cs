using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CreateSkillNro.ChildWindows;
using CreateSkillNro.Classes;
using CreateSkillNro.Classes.Enums;
using CreateSkillNro.Skills;
using CreateSkillNro.Windows;

namespace CreateSkillNro.Options;

public class PanelView : Control, IDisposable
{
    #region Constants

    private const double ScaleFactor = 0.45;

    #endregion

    #region Constructor

    public PanelView()
    {
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed) return;
        if (IsEditor)
        {
            if (!FormMainEffect.Instance.EffectCharPaints.TryGetValue(IdData, out var paint)) return;
            if (FormSkillEffect.Instance.EffectSelected.Count == 0 || IsPreview) return;

            var point = e.GetPosition(this);

            for (short i = 0; i < paint.ArrEffInfo.Count; i++)
            {
                var frameChild = paint.ArrEffInfo[i];
                if (!FormMainEffect.Instance.Images.TryGetValue(frameChild.IdImg, out var img) ||
                    !FormSkillEffect.Instance.EffectSelected.Contains(i) ||
                    !img.Contains(frameChild.Dx, frameChild.Dy, point, Bounds.Size)) continue;
                _isPressed = true;
                _startPoint = point;
                break;
            }

            return;
        }

        Selected?.Invoke(this, EventArgs.Empty);
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (!_isPressed) return;
        if (FormSkillEffect.Instance.EffectSelected.Count == 0 || IsPreview) return;
        if (!FormMainEffect.Instance.EffectCharPaints.TryGetValue(IdData, out var paint)) return;
        var point = e.GetPosition(this);
        var deltaX = point.X - _startPoint.X;
        var deltaY = point.Y - _startPoint.Y;
        var snappedDeltaX = Math.Round(deltaX / 4.0) * 4;
        var snappedDeltaY = Math.Round(deltaY / 4.0) * 4;
        if (snappedDeltaX == 0 && snappedDeltaY == 0) return;
        foreach (var i in FormSkillEffect.Instance.EffectSelected)
        {
            var child = paint.ArrEffInfo[i];
            if (!FormMainEffect.Instance.Images.TryGetValue(child.IdImg, out _)) continue;
            var x = child.Dx + snappedDeltaX / 4;
            var y = child.Dy + snappedDeltaY / 4;
            if (x is > sbyte.MinValue and < sbyte.MaxValue) child.Dx = (sbyte)x;
            if (y is > sbyte.MinValue and < sbyte.MaxValue) child.Dy = (sbyte)y;
        }
        _startPoint = new Point(_startPoint.X + snappedDeltaX, _startPoint.Y + snappedDeltaY);
        InvalidateVisual();
    }


    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (FormSkillEffect.Instance.EffectSelected.Count == 0 || IsPreview) return;
        _isPressed = false;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        InvalidateVisual();
    }

    #endregion

    #region Public Properties

    public event EventHandler Selected;
    public bool IsSelected { get; set; }

    public short IdData = -1;
    public TypeDataEnum TypeData { get; set; }
    public bool IsEditor { get; set; }
    public SkillRender SkillRender;

    #endregion

    #region Functions

    public void StartPreview()
    {
        IsPreview ^= true;
        if (IsPreview)
            if (TypeData == TypeDataEnum.Dart)
            {
                if (!FormMainEffect.Instance.DartInfos.TryGetValue(IdData, out var dart))
                    return;
                if (DartPreview == null)
                {
                    DartPreview = new PlayerDart(dart, 1, (short)(Bounds.Width / 4),
                        (short)(Bounds.Height / 2),
                        (short)(Bounds.Width - Bounds.Width * 0.28), (short)(Bounds.Height / 2));
                }
                else
                {
                    DartPreview.Dart = dart;
                    DartPreview.Renew();
                }
            }

        InvalidateVisual();
    }

    public void ShowChar()
    {
        _isViewChar ^= true;
        InvalidateVisual();
    }

    #endregion

    #region Private Fields

    private short _idPaint;
    private bool _needsRedraw = true;
    private Size _lastBounds;
    private bool _disposed;
    private bool _isPressed;
    public bool IsPreview;
    private bool _isViewChar;
    private Point _startPoint;
    public PlayerDart DartPreview;

    #endregion

    #region Render Methods

    public override void Render(DrawingContext context)
    {
        ThrowIfDisposed();
        base.Render(context);
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.FillRectangle(Brushes.Transparent, bounds);
        if (IsEditor)
            DrawCrossLinesAndBorder(context);
        else
            DrawBackground(context);
        switch (TypeData)
        {
            case TypeDataEnum.Skill:
                RenderSkill(context);
                break;
            case TypeDataEnum.Effect:
                if (!IsPreview && IsEditor)
                {
                    if (FormMainEffect.Instance?.EffectCharPaints?.TryGetValue(IdData, out var paint) == true)
                        DrawFrameChildren(context, paint.ArrEffInfo);
                }
                else
                {
                    RenderEffect(context);
                }

                break;
            case TypeDataEnum.Dart:
                if (IsPreview || !IsEditor) RenderDart(context);
                break;
        }

        _needsRedraw = false;
    }

    private void DrawFrameChildren(DrawingContext context, List<EffectInfoPaint> frameChildren)
    {
        var dashPen = new Pen(new SolidColorBrush(Colors.MediumPurple))
        {
            DashStyle = new DashStyle([4, 2], 0)
        };
        for (short i = 0; i < frameChildren.Count; i++)
        {
            var frameChild = frameChildren[i];
            if (!FormMainEffect.Instance.Images.TryGetValue(frameChild.IdImg, out var img)) continue;
            context.DrawImage(
                img,
                new Rect(0, 0, img.PixelSize.Width, img.PixelSize.Height),
                new Rect(frameChild.Dx * 4 + Bounds.Size.Width / 2 - img.Size.Width / 2,
                    frameChild.Dy * 4 + Bounds.Size.Height / 1.2 - img.Size.Height / 2, img.PixelSize.Width,
                    img.PixelSize.Height)
            );
            if (FormSkillEffect.Instance.EffectSelected.Contains(i))
                context.DrawRectangle(
                    dashPen,
                    new Rect(frameChild.Dx * 4 + Bounds.Size.Width / 2 - img.Size.Width / 2,
                        frameChild.Dy * 4 + Bounds.Size.Height / 1.2 - img.Size.Height / 2, img.PixelSize.Width,
                        img.PixelSize.Height)
                );
        }
    }

    private void DrawCrossLinesAndBorder(DrawingContext context)
    {
        IPen blackPen = new Pen(Brushes.Black);
        if (TypeData != TypeDataEnum.Dart || IsPreview)
            context.DrawLine(
                blackPen,
                new Point(0, Bounds.Size.Height / 1.2),
                new Point(Bounds.Width, Bounds.Size.Height / 1.2)
            );

        if (TypeData != TypeDataEnum.Dart)
            context.DrawLine(
                blackPen,
                new Point(Bounds.Size.Width / 2, 0),
                new Point(Bounds.Size.Width / 2, Bounds.Height)
            );
        context.DrawRectangle(null, new Pen(new SolidColorBrush(Color.Parse("#a6d189"))),
            new Rect(0, 0, Bounds.Width - 1, Bounds.Height - 1));
        try
        {
            if (_isViewChar)
                switch (TypeData)
                {
                    case TypeDataEnum.Effect:
                    {
                        var charImage = WindowExecution.Instance.ImageChar;
                        var destRect = new Rect(
                            Bounds.Size.Width / 2 - charImage.Size.Width / 2,
                            Bounds.Size.Height / 1.2 - charImage.Size.Height,
                            charImage.Size.Width,
                            charImage.Size.Height
                        );
                        context.DrawImage(charImage, new Rect(0, 0, charImage.Size.Width, charImage.Size.Height),
                            destRect);
                        break;
                    }
                    case TypeDataEnum.Skill when IsEditor:
                    {
                        if (SkillRender == null ||
                            !FormMainEffect.Instance.SkillPaints.TryGetValue(IdData, out var paint) ||
                            paint.SkillStand == null || paint.SkillStand.Count == 0)
                            return;
                        var charImage =
                            WindowExecution.Instance.ImageAction[
                                paint.SkillStand[SkillRender.IndexSkill].Status];
                        var destRect = new Rect(
                            Bounds.Size.Width / 2 - charImage.Size.Width / 2,
                            Bounds.Size.Height / 1.2 - charImage.Size.Height,
                            charImage.Size.Width,
                            charImage.Size.Height
                        );
                        context.DrawImage(charImage, new Rect(0, 0, charImage.Size.Width, charImage.Size.Height),
                            destRect);
                        break;
                    }
                }
        }
        catch
        {
        }

        DrawControlsModern(context);
    }

    private void DrawControlsModern(DrawingContext context)
    {
        const float cornerRadius = 12;
        const double padding = 16;
        const double panelHeight = 60;
        const double panelWidth = 180;
        const double panelX = 12;
        const double panelY = 12;
        var backgroundGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.FromArgb(245, 255, 255, 255), 0),
                new GradientStop(Color.FromArgb(235, 248, 250, 252), 1)
            ]
        };
        var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);
        var shadowBrush = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
        var shadowRect = new Rect(panelX + 2, panelY + 2, panelWidth, panelHeight);
        context.FillRectangle(shadowBrush, shadowRect, cornerRadius);
        context.FillRectangle(backgroundGradient, panelRect, cornerRadius);
        var borderGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.FromArgb(100, 139, 92, 246), 0),
                new GradientStop(Color.FromArgb(60, 226, 232, 240), 0.5),
                new GradientStop(Color.FromArgb(100, 59, 130, 246), 1)
            ]
        };
        var borderPen = new Pen(borderGradient, 1.5);
        context.DrawRectangle(borderPen, panelRect, cornerRadius);
        const double itemY = panelY + padding - 2;
        DrawModernControlItem(context, panelX + padding, itemY, "C", $"{(_isViewChar ? "Hide" : "Show")} Character",
            TypeData == TypeDataEnum.Dart);
        DrawModernControlItem(context, panelX + padding, itemY + 22, "F5",
            $"{(IsPreview ? "Disable" : "Enable")} Preview");
    }

    private void DrawModernControlItem(DrawingContext context, double x, double y, string key, string description,
        bool disable = false)
    {
        const double badgeWidth = 28;
        const double badgeHeight = 18;
        const float badgeRadius = 9;
        var badgeGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops = disable
                ?
                [
                    new GradientStop(Color.FromRgb(156, 163, 175), 0),
                    new GradientStop(Color.FromRgb(107, 114, 128), 0.6),
                    new GradientStop(Color.FromRgb(75, 85, 99), 1)
                ]
                :
                [
                    new GradientStop(Color.FromRgb(167, 139, 250), 0),
                    new GradientStop(Color.FromRgb(139, 92, 246), 0.5),
                    new GradientStop(Color.FromRgb(124, 58, 237), 1)
                ]
        };

        var badgeRect = new Rect(x, y, badgeWidth, badgeHeight);
        var badgeShadowBrush = new SolidColorBrush(Color.FromArgb(25, 0, 0, 0));
        var badgeShadowRect = new Rect(x + 1, y + 1, badgeWidth, badgeHeight);
        context.FillRectangle(badgeShadowBrush, badgeShadowRect, badgeRadius);
        context.FillRectangle(badgeGradient, badgeRect, badgeRadius);
        var badgeBorderBrush = new SolidColorBrush(disable
            ? Color.FromArgb(40, 255, 255, 255)
            : Color.FromArgb(80, 255, 255, 255));
        var badgeBorderPen = new Pen(badgeBorderBrush, 0.5);
        context.DrawRectangle(badgeBorderPen, badgeRect, badgeRadius);
        var keyTextBrush = new SolidColorBrush(disable ? Color.FromRgb(209, 213, 219) : Colors.White);
        var keyText = new FormattedText(
            key,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Bold),
            9,
            keyTextBrush
        );
        var keyTextX = x + (badgeWidth - keyText.Width) / 2;
        var keyTextY = y + (badgeHeight - keyText.Height) / 2;
        context.DrawText(keyText, new Point(keyTextX, keyTextY));
        var descBrush = new SolidColorBrush(disable
            ? Color.FromRgb(156, 163, 175)
            : Color.FromRgb(51, 65, 85));
        var descText = new FormattedText(
            description,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI", FontStyle.Normal, FontWeight.SemiBold),
            10,
            descBrush
        );
        context.DrawText(descText, new Point(x + badgeWidth + 12, y + (badgeHeight - descText.Height) / 2));
        if (disable) return;
        var glowBrush = new RadialGradientBrush
        {
            GradientStops =
            [
                new GradientStop(Color.FromArgb(20, 139, 92, 246), 0),
                new GradientStop(Color.FromArgb(0, 139, 92, 246), 1)
            ]
        };
        var glowRect = new Rect(x - 2, y - 2, badgeWidth + 4, badgeHeight + 4);
        context.FillRectangle(glowBrush, glowRect, badgeRadius + 2);
    }

    private void DrawBackground(DrawingContext context)
    {
        var pen = IsSelected ? new Pen(Brushes.DarkTurquoise, 2) : new Pen(Brushes.Red, 2);
        context.DrawRectangle(pen, new Rect(0, 0, Bounds.Width - 1, Bounds.Height - 1));
        if (!IsEditor) DrawIdView(context);
    }

    private void DrawIdView(DrawingContext context)
    {
        var text = new FormattedText(
            IdData.ToString(),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI", FontStyle.Normal, FontWeight.SemiBold),
            12,
            Brushes.Black);
        const double padding = 4;
        var textPosition = new Point(padding, padding);
        context.DrawText(text, textPosition);
    }

    private void RenderDart(DrawingContext context)
    {
        if (IsEditor && IsPreview && DartPreview != null)
        {
            DartPreview.Render(context);
            return;
        }

        if (!FormMainEffect.Instance.DartInfos.TryGetValue(IdData, out var dart))
            return;
        dart.PlayerDart?.Render(context);
    }

    private void RenderSkill(DrawingContext context)
    {
        if (IsEditor)
        {
            SkillRender?.Render(context);
            return;
        }

        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(IdData, out var skill))
            return;
        skill.SkillRender?.Render(context);
    }

    private void RenderEffect(DrawingContext context)
    {
        if (!FormMainEffect.Instance.EffectCharPaints.TryGetValue(IdData, out var paint) ||
            paint.ArrEffInfo == null || paint.ArrEffInfo.Count == 0 ||
            _idPaint >= paint.ArrEffInfo.Count ||
            !FormMainEffect.Instance.Images.TryGetValue(paint.ArrEffInfo[_idPaint].IdImg, out var paintImage))
            return;
        var info = paint.ArrEffInfo[_idPaint];
        Function.DrawSimple(context, paintImage, info.Dx, info.Dy, IsEditor, Bounds.Size, IsEditor ? 1 : ScaleFactor,
            false);
    }

    #endregion

    #region Update Logic

    public void Update()
    {
        try
        {
            if (_disposed) return;
            if (!IsPreview && IsEditor && TypeData != TypeDataEnum.Skill) return;
            var shouldInvalidate = false;
            if (_lastBounds != Bounds.Size)
            {
                _lastBounds = Bounds.Size;
                shouldInvalidate = true;
            }

            switch (TypeData)
            {
                case TypeDataEnum.Effect:
                    shouldInvalidate |= UpdateEffect();
                    break;
                case TypeDataEnum.Skill:
                    shouldInvalidate |= UpdateSkill();
                    break;
                case TypeDataEnum.Dart:
                    shouldInvalidate |= UpdateDart();
                    break;
            }

            if (shouldInvalidate || _needsRedraw) InvalidateVisual();
        }
        catch
        {
        }
    }

    private bool UpdateDart()
    {
        if (IsEditor && IsPreview && DartPreview != null)
        {
            DartPreview.Update();
            return true;
        }

        if (!FormMainEffect.Instance.DartInfos.TryGetValue(IdData, out var dart))
            return false;
        dart.PlayerDart?.Update();
        return true;
    }

    private bool UpdateEffect()
    {
        if (!FormMainEffect.Instance.EffectCharPaints.TryGetValue(IdData, out var paint) || paint.ArrEffInfo == null)
            return false;
        var oldIdPaint = _idPaint;
        _idPaint++;
        if (_idPaint >= paint.ArrEffInfo.Count)
            _idPaint = 0;
        return _idPaint != oldIdPaint;
    }

    private bool UpdateSkill()
    {
        if (IsEditor)
        {
            SkillRender?.Update();
            return true;
        }

        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(IdData, out var skillPaint))
            return false;
        skillPaint.SkillRender?.Update();
        return true;
    }

    #endregion

    #region IDisposable Implementation

    ~PanelView()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing) ClearCachedData();

        _disposed = true;
    }

    private void ClearCachedData()
    {
        _idPaint = 0;
        DartPreview?.Dispose();
        SkillRender?.Dispose();
        DartPreview = null;
        SkillRender = null;
        _needsRedraw = true;
        _lastBounds = default;
    }

    private void ThrowIfDisposed()
    {
        if (!_disposed) return;
        throw new ObjectDisposedException(nameof(PanelView));
    }

    #endregion
}