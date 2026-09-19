using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CreateSkillNro.Class;
using CreateSkillNro.Classes;
using CreateSkillNro.Skills;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CreateSkillNro.ChildWindows;

public partial class ViewSkill : Window
{
    private readonly int[] _scales = [1, 2, 3, 4];
    private CancellationTokenSource _buildCancellationTokenSource;
    private bool _isBuilding;

    public ViewSkill()
    {
        InitializeComponent();
        Instance = this;
    }

    public static ViewSkill Instance { get; private set; }

    public async Task LoadData()
    {
        FormMainEffect.Instance.SkillPaints.TryGetValue(FormSkillEffect.Instance.SkillPanelFrame.IdData, out var chill);
        if (chill == null) return;
        await WaitForControlInitialization();
        if (MainSkillPanel.SkillRender == null)
        {
            MainSkillPanel.SkillRender = new SkillRender(chill, 1, false, PanelTotal.Bounds.Size);
            MainSkillPanel.SkillRender.SetPreviewAll();
        }
        else
        {
            MainSkillPanel.SkillRender.Renew(chill);
        }
    }

    private async Task WaitForControlInitialization()
    {
        while (PanelTotal.Bounds.Width <= 0 || PanelTotal.Bounds.Height <= 0) await Task.Delay(10);
    }

    private async Task BuildDataAsync()
    {
        if (_isBuilding) return;

        _isBuilding = true;
        _buildCancellationTokenSource = new CancellationTokenSource();
        try
        {
            var images = FormMainEffect.Instance.InfoImage.Values.ToList();
            var totalOperations = _scales.Length * images.Count;
            var currentOperation = 0;
            ShowBuildProgressPanel(true);
            UpdateBuildProgress(0, 0, totalOperations, "Starting build process...", "Preparing directories...");
            foreach (var scale in _scales)
            {
                var outputDir = $"Output//Image//x{scale}";
                if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
                Directory.CreateDirectory(outputDir);
            }

            for (var scaleIndex = 0; scaleIndex < _scales.Length; scaleIndex++)
            {
                var scale = _scales[scaleIndex];
                if (_buildCancellationTokenSource.Token.IsCancellationRequested)
                    break;
                var scaleText = $"Processing x{scale} scale ({scaleIndex + 1}/{_scales.Length})";
                UpdateBuildProgress(
                    (int)((double)currentOperation / totalOperations * 100),
                    currentOperation,
                    totalOperations,
                    "Building images...",
                    scaleText
                );
                await ProcessScaleAsync(scale, images, scaleIndex + 1, _scales.Length,
                    currentOperation, totalOperations, _buildCancellationTokenSource.Token);

                currentOperation += images.Count;
            }

            if (!_buildCancellationTokenSource.Token.IsCancellationRequested)
            {
                await WriteNrSkill();
                await WriteNrEffect();
                await WriteNrDart();
                UpdateBuildProgress(100, totalOperations, totalOperations,
                    "Build completed successfully!", "All images processed");

                await Task.Delay(1500);
                var messageBox = MessageBoxManager
                    .GetMessageBoxStandard("Thông Báo", "Build Thành Công", ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Success);
                await messageBox.ShowAsync();
            }
        }
        catch (OperationCanceledException)
        {
            UpdateBuildProgress(0, 0, 0, "Build cancelled", "Operation was stopped by user");
            await Task.Delay(1500);
        }
        catch (Exception ex)
        {
            UpdateBuildProgress(0, 0, 0, "Build failed", $"Error: {ex.Message}");
            var errorBox = MessageBoxManager
                .GetMessageBoxStandard("Lỗi", $"Build thất bại: {ex.Message}", ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
            await errorBox.ShowAsync();
            await Task.Delay(2000);
        }
        finally
        {
            _isBuilding = false;
            ShowBuildProgressPanel(false);
            _buildCancellationTokenSource?.Dispose();
        }
    }

    private async Task ProcessScaleAsync(int scale, List<ImageInfo> images, int currentScale, int totalScales,
        int baseProgress, int totalOperations, CancellationToken cancellationToken)
    {
        const int batchSize = 10;
        var batches = images
            .Select((img, index) => new { img, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.img).ToList())
            .ToList();
        var processedInScale = 0;
        foreach (var batch in batches.TakeWhile(_ => !cancellationToken.IsCancellationRequested))
        {
            await Task.Run(async () =>
            {
                foreach (var image in batch.TakeWhile(_ => !cancellationToken.IsCancellationRequested))
                    try
                    {
                        var scaleFactor = scale * 0.25;
                        var newWidth = (int)(image.Image.Size.Width * scaleFactor);
                        var newHeight = (int)(image.Image.Size.Height * scaleFactor);
                        var scaledBitmap = image.Image.CreateScaledBitmap(new PixelSize(newWidth, newHeight));
                        var outputPath = $"Output//Image//x{scale}//{image.Id}.png";
                        scaledBitmap.Save(outputPath);
                        processedInScale++;
                        var currentTotal = baseProgress + processedInScale;
                        var percentage = (int)((double)currentTotal / totalOperations * 100);

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            UpdateBuildProgress(
                                percentage,
                                currentTotal,
                                totalOperations,
                                "Building images...",
                                $"x{scale} scale: {processedInScale}/{images.Count} ({currentScale}/{totalScales})"
                            );
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing image {image.Id} for scale x{scale}: {ex.Message}");
                    }
            }, cancellationToken);
            await Task.Delay(10, cancellationToken);
        }
    }

    private async Task WriteNrSkill()
    {
        var myWriter = new myWriter(999999);
        myWriter.writeShort(FormMainEffect.Instance.SkillPaints.Count);
        foreach (var skillPaint in FormMainEffect.Instance.SkillPaints.Values)
        {
            myWriter.writeShort(skillPaint.Id);
            myWriter.writeShort(skillPaint.EffectHappenOnMob);
            myWriter.writeByte(0);
            WriteSkillInfoList(myWriter, skillPaint.SkillStand);
            WriteSkillInfoList(myWriter, skillPaint.SkillFly);
        }

        await File.WriteAllBytesAsync("Output//NrSkill", Array.ConvertAll(myWriter.getData(), a => (byte)a));
        myWriter.Close();
    }

    private static void WriteSkillInfoList(myWriter writer, List<SkillInfoPaint> list)
    {
        writer.writeByte(list.Count);
        foreach (var info in list)
        {
            writer.writeSByte(info.Status);
            writer.writeShort(info.EffS0Id);
            writer.writeShort(info.E0dx);
            writer.writeShort(info.E0dy);
            writer.writeShort(info.EffS1Id);
            writer.writeShort(info.E1dx);
            writer.writeShort(info.E1dy);
            writer.writeShort(info.EffS2Id);
            writer.writeShort(info.E2dx);
            writer.writeShort(info.E2dy);
            writer.writeShort(info.ArrowId);
            writer.writeShort(info.Adx);
            writer.writeShort(info.Ady);
        }
    }

    private async Task WriteNrEffect()
    {
        var myWriter = new myWriter(999999);
        myWriter.writeShort(FormMainEffect.Instance.EffectCharPaints.Count);
        for (short i = 0; i < FormMainEffect.Instance.EffectCharPaints.Count; i++)
        {
            var effect = FormMainEffect.Instance.EffectCharPaints[i];
            myWriter.writeShort(i);
            myWriter.writeByte(effect.ArrEffInfo.Count);
            foreach (var t in effect.ArrEffInfo)
            {
                myWriter.writeShort(t.IdImg);
                myWriter.writeByte(t.Dx);
                myWriter.writeByte(t.Dy);
            }
        }

        await File.WriteAllBytesAsync("Output//NrEffect", Array.ConvertAll(myWriter.getData(), a => (byte)a));
        myWriter.Close();
    }

    private async Task WriteNrDart()
    {
        var myWriter = new myWriter();
        myWriter.writeShort(FormMainEffect.Instance.DartInfos.Count);
        for (short i = 0; i < FormMainEffect.Instance.DartInfos.Count; i++)
        {
            var info = FormMainEffect.Instance.DartInfos[i];
            myWriter.writeShort(i);
            myWriter.writeShort(info.NUpdate);
            myWriter.writeShort(info.Va / 256);
            myWriter.writeShort(info.XdPercent);
            myWriter.writeShort(info.Tail.Count);
            foreach (var t in info.Tail) myWriter.writeShort(t);

            myWriter.writeShort(info.TailBorder.Count);
            foreach (var t in info.TailBorder) myWriter.writeShort(t);

            myWriter.writeShort(info.Xd1.Count);
            foreach (var t in info.Xd1) myWriter.writeShort(t);

            myWriter.writeShort(info.Xd2.Count);
            foreach (var t in info.Xd2) myWriter.writeShort(t);

            myWriter.writeShort(info.Head.Count);
            foreach (var t in info.Head)
            {
                myWriter.writeShort(t.Count);
                foreach (var h in t) myWriter.writeShort(h);
            }

            myWriter.writeShort(info.HeadBorder.Count);
            foreach (var t in info.HeadBorder)
            {
                myWriter.writeShort(t.Count);
                foreach (var h in t) myWriter.writeShort(h);
            }
        }

        await File.WriteAllBytesAsync("Output//NrDart", Array.ConvertAll(myWriter.getData(), a => (byte)a));
        myWriter.Close();
    }

    private void ShowBuildProgressPanel(bool show)
    {
        Dispatcher.UIThread.Post(() =>
        {
            BuildProgressPanel.IsVisible = show;
            MainSkillPanel.IsEnabled = !show;
        });
    }

    private void UpdateBuildProgress(int percentage, int current, int total, string mainText, string detailText)
    {
        Dispatcher.UIThread.Post(() =>
        {
            BuildProgressBar.Value = percentage;
            BuildProgressText.Text = mainText;
            BuildProgressDetail.Text = detailText;
            BuildProgressCount.Text = $"{current} / {total}";
            BuildProgressPercent.Text = $"{percentage}%";
        });
    }

    private void BuildCancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        _buildCancellationTokenSource?.Cancel();
        UpdateBuildProgress(0, 0, 0, "Cancelling build...", "Stopping current operations...");
    }

    private void InputElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F5 when !_isBuilding:
                _ = BuildDataAsync();
                break;
            case Key.F6:
                MainSkillPanel.TogglePreview();
                break;
            case Key.Escape when _isBuilding:
                _buildCancellationTokenSource?.Cancel();
                break;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _buildCancellationTokenSource?.Cancel();
        _buildCancellationTokenSource?.Dispose();
        base.OnClosed(e);
    }
}