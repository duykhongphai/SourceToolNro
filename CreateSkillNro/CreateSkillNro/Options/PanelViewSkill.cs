using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CreateSkillNro.ChildWindows;
using CreateSkillNro.Skills;
using CreateSkillNro.Windows;

namespace CreateSkillNro.Options;

public class PanelViewSkill : Control
{
    private readonly DispatcherTimer _updateTimer;
    private int _idPaint;
    public bool IsPreview;
    public SkillRender SkillRender;

    public PanelViewSkill()
    {
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _updateTimer.Tick += UpdateTimerOnTick;
    }

    public void TogglePreview()
    {
        IsPreview = !IsPreview;
        InvalidateVisual();
        if (IsPreview)
            _updateTimer.Start();
        else
            _updateTimer.Stop();
    }

    private void UpdateTimerOnTick(object sender, EventArgs e)
    {
        if (!IsPreview || SkillRender == null) return;
        SkillRender.Update();
        if (FormMainEffect.Instance.EffectCharPaints.TryGetValue(SkillRender.SkillPaint.EffectHappenOnMob,
                out var paint) && paint.ArrEffInfo != null)
        {
            _idPaint++;
            if (_idPaint >= paint.ArrEffInfo.Count)
                _idPaint = 0;
        }

        InvalidateVisual();
    }

    public sealed override void Render(DrawingContext context)
    {
        base.Render(context);
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.FillRectangle(Brushes.Transparent, bounds);
        var lineGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.FromArgb(0, 103, 126, 234), 0),
                new GradientStop(Color.FromArgb(80, 103, 126, 234), 0.5),
                new GradientStop(Color.FromArgb(0, 103, 126, 234), 1)
            ]
        };
        var linePen = new Pen(lineGradient, 2);
        context.DrawLine(linePen, new Point(0, Bounds.Height / 1.2), new Point(Bounds.Width, Bounds.Height / 1.2));
        DrawControlsModern(context);
        if (!IsPreview || SkillRender == null) return;
        try
        {
            var charImage =
                WindowExecution.Instance.ImageAction[
                    SkillRender.SkillPaint.SkillStand[SkillRender.IndexSkill].Status];
            var destRect = new Rect(
                Bounds.Width / 4 - charImage.Size.Width / 2,
                Bounds.Height / 1.2 - charImage.Size.Height,
                charImage.Size.Width,
                charImage.Size.Height
            );
            context.DrawImage(charImage, new Rect(0, 0, charImage.Size.Width, charImage.Size.Height),
                destRect);

            var charInjured = WindowExecution.Instance.ImageCharInjured;
            destRect = new Rect(
                Bounds.Width - Bounds.Width / 4 - charInjured.Size.Width / 2,
                Bounds.Height / 1.2 - charInjured.Size.Height,
                charInjured.Size.Width,
                charInjured.Size.Height
            );
            var flipMatrix = Matrix.CreateScale(-1, 1) *
                             Matrix.CreateTranslation(destRect.X + destRect.Width, destRect.Y);
            using (context.PushTransform(flipMatrix))
            {
                var flippedDestRect = new Rect(0, 0, destRect.Width, destRect.Height);
                context.DrawImage(charInjured,
                    new Rect(0, 0, charInjured.Size.Width, charInjured.Size.Height),
                    flippedDestRect);
            }

            SkillRender.Render(context);
            RenderEffect(context);
        }
        catch
        {
        }
    }

    private void RenderEffect(DrawingContext context)
    {
        if (!FormMainEffect.Instance.EffectCharPaints.TryGetValue(SkillRender.SkillPaint.EffectHappenOnMob,
                out var paint) ||
            paint.ArrEffInfo == null || paint.ArrEffInfo.Count == 0 ||
            _idPaint >= paint.ArrEffInfo.Count ||
            !FormMainEffect.Instance.Images.TryGetValue(paint.ArrEffInfo[_idPaint].IdImg, out var paintImage))
            return;
        var info = paint.ArrEffInfo[_idPaint];
        DrawSimple(context, paintImage, info.Dx, info.Dy);
    }

    private void DrawControlsModern(DrawingContext context)
    {
        const double panelX = 10;
        const double panelY = 10;
        const double panelWidth = 180;
        const double panelHeight = 70;
        var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);
        var shadowRect = new Rect(panelX + 2, panelY + 2, panelWidth, panelHeight);
        var shadowBrush = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
        context.FillRectangle(shadowBrush, shadowRect, 12);
        var cardGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Colors.White, 0),
                new GradientStop(Color.FromRgb(248, 250, 255), 1)
            ]
        };
        context.FillRectangle(cardGradient, panelRect, 12);
        var borderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
        var borderPen = new Pen(borderBrush);
        context.DrawRectangle(borderPen, panelRect, 12);
        var accentRect = new Rect(panelX, panelY, panelWidth, 3);
        var accentGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.FromRgb(103, 126, 234), 0),
                new GradientStop(Color.FromRgb(118, 75, 162), 1)
            ]
        };
        context.FillRectangle(accentGradient, accentRect);
        DrawModernControlItem(context, panelX + 12, panelY + 18, "F5", "🔨 Build Data");
        DrawModernControlItem(context, panelX + 12, panelY + 42, "F6",
            $"👁️ {(IsPreview ? "Disable" : "Enable")} Preview", IsPreview);
    }

    private void DrawModernControlItem(DrawingContext context, double x, double y, string key, string description,
        bool isActive = false)
    {
        const double badgeWidth = 26;
        const double badgeHeight = 16;
        var badgeRect = new Rect(x, y, badgeWidth, badgeHeight);
        var badgeGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops = isActive
                ?
                [
                    new GradientStop(Color.FromRgb(34, 197, 94), 0),
                    new GradientStop(Color.FromRgb(21, 128, 61), 1)
                ]
                :
                [
                    new GradientStop(Color.FromRgb(71, 85, 105), 0),
                    new GradientStop(Color.FromRgb(51, 65, 85), 1)
                ]
        };

        context.FillRectangle(badgeGradient, badgeRect, 8);
        var keyBrush = new SolidColorBrush(Colors.White);
        var keyText = new FormattedText(key, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Bold), 8, keyBrush);

        var keyX = x + (badgeWidth - keyText.Width) / 2;
        var keyY = y + (badgeHeight - keyText.Height) / 2;
        context.DrawText(keyText, new Point(keyX, keyY));
        var descBrush = new SolidColorBrush(isActive ? Color.FromRgb(21, 128, 61) : Color.FromRgb(71, 85, 105));
        var descText = new FormattedText(description, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Medium), 10, descBrush);

        context.DrawText(descText, new Point(x + badgeWidth + 8, y + (badgeHeight - descText.Height) / 2));
    }

    public void DrawSimple(DrawingContext context, Bitmap image, int dx, int dy)
    {
        var srcRect = new Rect(image.Size);
        var destRect = new Rect(
            dx + Bounds.Width - Bounds.Width / 4 - image.Size.Width / 2,
            dy + Bounds.Height / 1.2 - image.Size.Height,
            image.PixelSize.Width,
            image.PixelSize.Height);
        context.DrawImage(image, srcRect, destRect);
    }
}