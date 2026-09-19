using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CreateSkillNro.Classes;
using CreateSkillNro.Classes.Enums;
using CreateSkillNro.Options;
using CreateSkillNro.Skills;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CreateSkillNro.ChildWindows;

public partial class FormMainEffect : Window
{
    #region Static Properties

    public static FormMainEffect Instance { get; private set; }

    #endregion

    #region Private Fields

    private ImageInfo _contextTargetLayer;
    private readonly SemaphoreSlim _loadingSemaphore = new(10, 10);
    private readonly int _batchSize = 50;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isLoading;

    #endregion

    #region Constants

    private const int GridItemSize = 75;
    private const short SpecialSkillId = 1111;
    private const int GridPadding = 6;
    private const int BORDER_THICKNESS = 1;
    private const double CORNER_RADIUS = 6;
    private const int MarginSize = 3;
    private const int TextFontSize = 9;
    private const byte BackgroundAlpha = 180;

    #endregion

    #region Public Properties

    public Dictionary<short, ImageInfo> InfoImage { get; set; } = new();
    public string PathFileNrSkill { get; private set; }
    public string PathFileNrEffect { get; private set; }
    public string PathFileNrDart { get; private set; }
    public string PathFileIcon { get; private set; }
    public ConcurrentDictionary<short, SkillPaint> SkillPaints = new();
    public ObservableConcurrentDictionary<short, EffectCharPaint> EffectCharPaints = new();
    public ObservableConcurrentDictionary<short, DartInfo> DartInfos = new();
    public ConcurrentDictionary<short, Bitmap> Images = new();
    public ObservableCollection<short> EffectKeys { get; }
    public ObservableCollection<short> DartKeys { get; }

    #endregion

    #region Constructor and Initialization

    public FormMainEffect()
    {
        InitializeComponent();
        Instance = this;
        EffectKeys =
        [
            -1
        ];
        DartKeys =
        [
            -1
        ];
        EffectCharPaints.ItemAdded += OnEffectAdded;
        EffectCharPaints.ItemRemoved += OnEffectRemoved;

        DartInfos.ItemAdded += OnDartAdded;
        DartInfos.ItemRemoved += OnDartRemoved;
    }

    #endregion

    #region Resource Management

    private void ClearObjects()
    {
        try
        {
            PanelImage.Children.Clear();
            ClearInfoImages();
        }
        catch
        {
        }
    }

    private void ClearInfoImages()
    {
        foreach (var bitmap in InfoImage.Values)
            bitmap?.Dispose();
        InfoImage.Clear();
    }

    #endregion

    #region Event Handlers

    private void OnDartAdded(object sender, KeyValuePair<short, DartInfo> item)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var index = DartKeys.ToList().BinarySearch(item.Key);
            if (index < 0) index = ~index;
            DartKeys.Insert(index, item.Key);
        });
    }

    private void OnDartRemoved(object sender, short key)
    {
        Dispatcher.UIThread.InvokeAsync(() => { DartKeys.Remove(key); });
    }

    private void OnEffectAdded(object sender, KeyValuePair<short, EffectCharPaint> item)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var index = EffectKeys.ToList().BinarySearch(item.Key);
            if (index < 0) index = ~index;
            EffectKeys.Insert(index, item.Key);
        });
    }

    private void OnEffectRemoved(object sender, short key)
    {
        Dispatcher.UIThread.InvokeAsync(() => { EffectKeys.Remove(key); });
    }

    private async void ImportImageButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var files = await OpenFilePickerAsync("Select Images", true, FilePickerFileTypes.ImageAll);
            if (files?.Count > 0)
                await LoadImagesAsync(files);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to import images", ex.Message);
        }
    }

    private async void ImportPathIconButton_OnClick(object sender, RoutedEventArgs e)
    {
        PathFileIcon = await SelectSingleFolderAsync("Select Folder Icon");
    }

    private async void ImportNrSkillButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateEffectPath()) return;

        PathFileNrSkill = await SelectSingleFileAsync("Select NR Skill File");
        if (string.IsNullOrEmpty(PathFileNrSkill)) return;

        await ProcessSkillFileAsync();
    }

    private async void ImportNrDartButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateIconPath()) return;

        PathFileNrDart = await SelectSingleFileAsync("Select NR Dart File");
        if (string.IsNullOrEmpty(PathFileNrDart)) return;

        await ProcessDartFileAsync();
    }

    private async void ImportNrEffectButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateIconPath()) return;

        PathFileNrEffect = await SelectSingleFileAsync("Select NR Effect File");
        if (string.IsNullOrEmpty(PathFileNrEffect)) return;

        await ProcessEffectFileAsync();
    }

    #endregion

    #region Validation Methods

    private async Task<bool> ValidateEffectPath()
    {
        if (!string.IsNullOrEmpty(PathFileNrEffect)) return true;
        await ShowErrorAsync("Thông Báo", "Vui Lòng Nhập Path Nr Effect Trước");
        return false;
    }

    private async Task<bool> ValidateIconPath()
    {
        if (!string.IsNullOrEmpty(PathFileIcon)) return true;
        await ShowErrorAsync("Thông Báo", "Vui Lòng Nhập Path Icon Trước");
        return false;
    }

    #endregion

    #region File Processing Methods

    private async Task ProcessSkillFileAsync()
    {
        ClearSkillData();

        try
        {
            var fileData = await File.ReadAllBytesAsync(PathFileNrSkill);
            var dataInputStream = new myReader(Array.ConvertAll(fileData, a => (sbyte)a));

            try
            {
                await ProcessSkillDataAsync(dataInputStream);
                await ShowSuccessAsync("Thông Báo", "Load Nr Skill Thành Công");
            }
            finally
            {
                dataInputStream.Close();
            }
        }
        catch
        {
        }
    }

    private async Task ProcessDartFileAsync()
    {
        ClearDartData();

        try
        {
            var fileData = await File.ReadAllBytesAsync(PathFileNrDart);
            var dataInputStream = new myReader(Array.ConvertAll(fileData, a => (sbyte)a));

            try
            {
                await ProcessDartData(dataInputStream);
                await ShowSuccessAsync("Thông Báo", "Load Nr Dart Thành Công");
            }
            finally
            {
                dataInputStream.Close();
            }
        }
        catch
        {
        }
    }

    private async Task ProcessEffectFileAsync()
    {
        ClearEffectData();

        try
        {
            var fileData = await File.ReadAllBytesAsync(PathFileNrEffect);
            var reader = new myReader(Array.ConvertAll(fileData, a => (sbyte)a));

            try
            {
                await ProcessEffectDataAsync(reader);
                await ShowSuccessAsync("Thông Báo", "Load Nr Effect Thành Công");
            }
            finally
            {
                reader.Close();
            }
        }
        catch
        {
        }
    }

    #endregion

    #region Data Processing Methods

    private async Task ProcessSkillDataAsync(myReader dataInputStream)
    {
        int skillCount = dataInputStream.readShort();

        for (var i = 0; i < skillCount; i++)
        {
            var skillPaint = ReadSkillPaintData(dataInputStream, skillCount);
            var panelView = CreateSkillPanelView(skillPaint.Id);
            skillPaint.PanelView = panelView;
            skillPaint.SkillRender =
                new SkillRender(skillPaint, 0.45, false, new Size(panelView.Width, panelView.Height));
            SkillPaints.TryAdd(skillPaint.Id, skillPaint);
            await Dispatcher.UIThread.InvokeAsync(() =>
                ViewEffect.Instance.SkillWrapPanel.Children.Add(panelView));
        }
    }

    private SkillPaint ReadSkillPaintData(myReader dataInputStream, int totalCount)
    {
        var skillId = dataInputStream.readShort();
        if (skillId == SpecialSkillId)
            skillId = (short)(totalCount - 1);

        var skillPaint = new SkillPaint
        {
            Id = skillId,
            EffectHappenOnMob = dataInputStream.readShort()
        };

        if (skillPaint.EffectHappenOnMob <= 0)
            skillPaint.EffectHappenOnMob = 80;

        dataInputStream.readByte();

        ReadSkillStandData(dataInputStream, skillPaint);
        ReadSkillFlyData(dataInputStream, skillPaint);

        return skillPaint;
    }

    private void ReadSkillStandData(myReader dataInputStream, SkillPaint skillPaint)
    {
        var standCount = dataInputStream.readByte();
        skillPaint.SkillStand = new List<SkillInfoPaint>(standCount);

        for (var j = 0; j < standCount; j++)
            skillPaint.SkillStand.Add(new SkillInfoPaint
            {
                Status = dataInputStream.readByte(),
                EffS0Id = dataInputStream.readShort(),
                E0dx = dataInputStream.readShort(),
                E0dy = dataInputStream.readShort(),
                EffS1Id = dataInputStream.readShort(),
                E1dx = dataInputStream.readShort(),
                E1dy = dataInputStream.readShort(),
                EffS2Id = dataInputStream.readShort(),
                E2dx = dataInputStream.readShort(),
                E2dy = dataInputStream.readShort(),
                ArrowId = dataInputStream.readShort(),
                Adx = dataInputStream.readShort(),
                Ady = dataInputStream.readShort()
            });
    }

    private void ReadSkillFlyData(myReader dataInputStream, SkillPaint skillPaint)
    {
        var flyCount = dataInputStream.readByte();
        skillPaint.SkillFly = new List<SkillInfoPaint>(flyCount);
        for (var k = 0; k < flyCount; k++)
        {
            skillPaint.SkillFly.Add(new SkillInfoPaint
            {
                Status = dataInputStream.readByte(),
                EffS0Id = dataInputStream.readShort(),
                E0dx = dataInputStream.readShort(),
                E0dy = dataInputStream.readShort(),
                EffS1Id = dataInputStream.readShort(),
                E1dx = dataInputStream.readShort(),
                E1dy = dataInputStream.readShort(),
                EffS2Id = dataInputStream.readShort(),
                E2dx = dataInputStream.readShort(),
                E2dy = dataInputStream.readShort(),
                ArrowId = dataInputStream.readShort(),
                Adx = dataInputStream.readShort(),
                Ady = dataInputStream.readShort()
            });
        }
    }

    private async Task ProcessDartData(myReader dataInputStream)
    {
        int dartCount = dataInputStream.readShort();

        for (short i = 0; i < dartCount; i++)
        {
            var dartInfo = ReadDartInfoData(dataInputStream);
            var panelView = CreateDartPanelView(i);
            dartInfo.PanelView = panelView;
            dartInfo.PlayerDart = new PlayerDart(dartInfo, 0.45, 10, (short)(panelView.Height / 2.0),
                (short)(panelView.Width - panelView.Width * 0.28), (short)(panelView.Height / 2));
            DartInfos.TryAdd(i, dartInfo);
            await Dispatcher.UIThread.InvokeAsync(() =>
                ViewEffect.Instance.DartWrapPanel.Children.Add(panelView));
        }
    }

    private DartInfo ReadDartInfoData(myReader dataInputStream)
    {
        dataInputStream.readShort();
        var dartInfo = new DartInfo
        {
            NUpdate = dataInputStream.readShort(),
            Va = dataInputStream.readShort() * 256,
            XdPercent = dataInputStream.readShort()
        };
        ReadDartArrays(dataInputStream, dartInfo);
        return dartInfo;
    }

    private void ReadDartArrays(myReader dataInputStream, DartInfo dartInfo)
    {
        dartInfo.Tail = ReadShortArray(dataInputStream).ToList();
        dartInfo.TailBorder = ReadShortArray(dataInputStream).ToList();
        dartInfo.Xd1 = ReadShortArray(dataInputStream).ToList();
        dartInfo.Xd2 = ReadShortArray(dataInputStream).ToList();
        dartInfo.Head = ReadJaggedShortArray(dataInputStream);
        dartInfo.HeadBorder = ReadJaggedShortArray(dataInputStream);
    }

    private short[] ReadShortArray(myReader dataInputStream)
    {
        int count = dataInputStream.readShort();
        var array = new short[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = dataInputStream.readShort();
            var imagePath = Path.Combine(PathFileIcon, array[i] + ".png");
            Images.TryAdd(array[i], new Bitmap(imagePath));
        }

        return array;
    }

    private List<List<short>> ReadJaggedShortArray(myReader dataInputStream)
    {
        int outerCount = dataInputStream.readShort();
        var jaggedArray = new List<List<short>>();

        for (var i = 0; i < outerCount; i++)
        {
            int innerCount = dataInputStream.readShort();
            jaggedArray.Add([]);
            for (var j = 0; j < innerCount; j++)
            {
                jaggedArray[i].Add(dataInputStream.readShort());
                var imagePath = Path.Combine(PathFileIcon, jaggedArray[i][j] + ".png");
                Images.TryAdd(jaggedArray[i][j], new Bitmap(imagePath));
            }
        }

        return jaggedArray;
    }

    private async Task ProcessEffectDataAsync(myReader reader)
    {
        int effectCount = reader.readShort();

        for (short i = 0; i < effectCount; i++)
        {
            var effectCharPaint = await ReadEffectPaintDataAsync(reader);
            var panelView = CreateEffectPanelView(i);
            effectCharPaint.PanelView = panelView;
            EffectCharPaints.TryAdd(i, effectCharPaint);
            await Dispatcher.UIThread.InvokeAsync(() =>
                ViewEffect.Instance.EffectWrapPanel.Children.Add(panelView));
        }
    }

    private async Task<EffectCharPaint> ReadEffectPaintDataAsync(myReader reader)
    {
        reader.readShort();
        var size = reader.readByte();
        var effectCharPaint = new EffectCharPaint
        {
            ArrEffInfo = []
        };

        for (var j = 0; j < size; j++)
        {
            effectCharPaint.ArrEffInfo.Add(new EffectInfoPaint
            {
                IdImg = reader.readShort(),
                Dx = reader.readByte(),
                Dy = reader.readByte()
            });
            var imagePath = Path.Combine(PathFileIcon, effectCharPaint.ArrEffInfo[j].IdImg + ".png");
            var img = await Task.Run(() => new Bitmap(imagePath));
            Images.TryAdd(effectCharPaint.ArrEffInfo[j].IdImg, img);
        }

        return effectCharPaint;
    }

    #endregion

    #region Data Clearing Methods

    private void ClearSkillData()
    {
        foreach (var skillPaint in SkillPaints.Values)
            skillPaint.Dispose();
        SkillPaints.Clear();

        foreach (var panel in ViewEffect.Instance.SkillWrapPanel.Children.Cast<PanelView>())
            panel.Dispose();
        ViewEffect.Instance.SkillWrapPanel.Children.Clear();
    }

    private void ClearDartData()
    {
        DartKeys.Clear();
        DartKeys.Add(-1);
        foreach (var dartInfo in DartInfos.Values)
            dartInfo.Dispose();
        DartInfos.Clear();
    }

    private void ClearEffectData()
    {
        EffectKeys.Clear();
        EffectKeys.Add(-1);
        foreach (var panel in ViewEffect.Instance.EffectWrapPanel.Children.Cast<PanelView>())
            panel.Dispose();
        ViewEffect.Instance.EffectWrapPanel.Children.Clear();

        foreach (var effectCharPaint in EffectCharPaints.Values)
            effectCharPaint.Dispose();
        EffectCharPaints.Clear();
    }

    #endregion

    #region Panel Creation Methods

    public PanelView CreateDartPanelView(short skillId)
    {
        var panelView = new PanelView
        {
            TypeData = TypeDataEnum.Dart,
            IdData = skillId,
            Width = 320,
            Height = 220,
            Margin = new Thickness(2)
        };
        panelView.Selected += PanelViewOnSelected;
        return panelView;
    }

    public PanelView CreateSkillPanelView(short skillId)
    {
        var panelView = new PanelView
        {
            TypeData = TypeDataEnum.Skill,
            IdData = skillId,
            Width = 160,
            Height = 160,
            Margin = new Thickness(2)
        };
        panelView.Selected += PanelViewOnSelected;
        return panelView;
    }

    public PanelView CreateEffectPanelView(short effectId)
    {
        var panelView = new PanelView
        {
            TypeData = TypeDataEnum.Effect,
            IdData = effectId,
            Width = 160,
            Height = 160,
            Margin = new Thickness(2)
        };
        panelView.Selected += PanelViewOnSelected;
        return panelView;
    }

    private static void PanelViewOnSelected(object sender, EventArgs e)
    {
        if (sender is not PanelView panelView) return;
        var wrap = panelView.TypeData switch
        {
            TypeDataEnum.Effect => ViewEffect.Instance.EffectWrapPanel,
            TypeDataEnum.Skill => ViewEffect.Instance.SkillWrapPanel,
            _ => ViewEffect.Instance.DartWrapPanel
        };
        foreach (var child in wrap.Children)
            if (child is PanelView chooser)
            {
                var isThis = ReferenceEquals(sender, chooser);
                if (isThis && chooser.IsSelected)
                {
                    isThis = false;
                    ViewEffect.Instance.SelectedItem = null;
                }

                chooser.SetSelected(isThis);
                if (!isThis) continue;
                ViewEffect.Instance.SelectedItem = chooser;
                chooser.InvalidateVisual();
            }
    }

    #endregion

    #region File Operations

    private async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(
        string title,
        bool allowMultiple = false,
        params FilePickerFileType[] fileTypes)
    {
        try
        {
            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple,
                FileTypeFilter = fileTypes?.Length > 0 ? fileTypes : null
            };

            return await GetTopLevel(this)?.StorageProvider.OpenFilePickerAsync(options);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("File Picker Error", ex.Message);
            return null;
        }
    }

    private async Task<string?> SelectSingleFileAsync(string title)
    {
        try
        {
            var files = await OpenFilePickerAsync(title);
            return files?.Count > 0 ? files[0].Path.LocalPath : null;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("File Selection Error", ex.Message);
            return null;
        }
    }

    private async Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(
        string title,
        bool allowMultiple = false)
    {
        try
        {
            var options = new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple
            };

            return await GetTopLevel(this)?.StorageProvider.OpenFolderPickerAsync(options);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Folder Picker Error", ex.Message);
            return null;
        }
    }

    private async Task<string?> SelectSingleFolderAsync(string title)
    {
        try
        {
            var folders = await OpenFolderPickerAsync(title);
            return folders?.Count > 0 ? folders[0].Path.LocalPath : null;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Folder Selection Error", ex.Message);
            return null;
        }
    }

    private async Task LoadImagesAsync(IReadOnlyList<IStorageFile> files)
    {
        if (_isLoading) return;
        _isLoading = true;
        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            ShowProgressPanel(true);
            UpdateProgress(0, 0, files.Count, "Starting image loading...");
            ClearObjects();
            var batches = files
                .Select((file, index) => new { file, index })
                .GroupBy(x => x.index / _batchSize)
                .Select(g => g.Select(x => x.file).ToList())
                .ToList();
            var processedCount = 0;
            var totalCount = files.Count;
            foreach (var batch in batches.TakeWhile(batch => !_cancellationTokenSource.Token.IsCancellationRequested))
            {
                UpdateProgress(
                    (int)((double)processedCount / totalCount * 100),
                    processedCount,
                    totalCount,
                    $"Loading batch {Array.IndexOf(batches.ToArray(), batch) + 1}/{batches.Count}..."
                );
                var batchTasks = batch.Select(file => LoadSingleImageAsync(file, _cancellationTokenSource.Token));
                await Task.WhenAll(batchTasks);
                processedCount += batch.Count;
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UpdateProgress(
                        (int)((double)processedCount / totalCount * 100),
                        processedCount,
                        totalCount,
                        processedCount == totalCount ? "Loading completed!" : "Loading images..."
                    );
                });
                await Task.Delay(10, _cancellationTokenSource.Token);
            }

            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                UpdateProgress(100, totalCount, totalCount, "All images loaded successfully!");
                await Task.Delay(1500);
            }
        }
        catch (OperationCanceledException)
        {
            UpdateProgress(0, 0, 0, "Loading cancelled");
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            UpdateProgress(0, 0, 0, $"Error: {ex.Message}");
            await Task.Delay(2000);
        }
        finally
        {
            _isLoading = false;
            ShowProgressPanel(false);
            _cancellationTokenSource?.Dispose();
        }
    }

    private async Task LoadSingleImageAsync(IStorageFile file, CancellationToken cancellationToken)
    {
        await _loadingSemaphore.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileName = Path.GetFileNameWithoutExtension(file.Path.LocalPath);
            var id = Function.TryParseId(fileName);
            if (!id.HasValue) return;
            var bitmap = await Task.Run(() =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return new Bitmap(file.Path.LocalPath);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    return null;
                }
            }, cancellationToken);
            if (bitmap == null) return;
            Images.TryAdd(id.Value, bitmap);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!cancellationToken.IsCancellationRequested) AddImageToPanel(bitmap, id.Value);
            }, DispatcherPriority.Background, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Debug.WriteLine($"Error loading {file.Name}: {ex.Message}");
            });
        }
        finally
        {
            _loadingSemaphore.Release();
        }
    }

    private void ShowProgressPanel(bool show)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ProgressPanel.IsVisible = show;
            ImportImageButton.IsEnabled = !show;
            ImportPathIconButton.IsEnabled = !show;
            ImportNrSkillButton.IsEnabled = !show;
            ImportNrEffectButton.IsEnabled = !show;
            ImportNrDartButton.IsEnabled = !show;
        });
    }

    private void UpdateProgress(int percentage, int current, int total, string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            LoadingProgressBar.Value = percentage;
            ProgressText.Text = message;
            ProgressCount.Text = $"{current} / {total}";
        });
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        UpdateProgress(0, 0, 0, "Cancelling...");
    }

    #endregion

    #region Image Panel Management

    private void AddImageToPanel(Bitmap bitmap, short id)
    {
        var layer = CreateImageLayer(bitmap, id);
        InfoImage[id] = layer;
        PanelImage.Children.Add(layer.Container);
        UpdateLayerIndices();
    }

    private ImageInfo CreateImageLayer(Bitmap bitmap, short id)
    {
        var image = CreateImageControl(bitmap);
        var indexText = CreateIndexTextBlock(id);
        var container = CreateImageContainer(id, image, indexText);

        return new ImageInfo(id, container, bitmap);
    }

    private Image CreateImageControl(Bitmap bitmap)
    {
        return new Image
        {
            Source = bitmap,
            Width = GridItemSize - GridPadding,
            Height = GridItemSize - GridPadding,
            Stretch = Stretch.Uniform
        };
    }

    private TextBlock CreateIndexTextBlock(short id)
    {
        return new TextBlock
        {
            Text = id.ToString(),
            FontSize = TextFontSize,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.FromArgb(BackgroundAlpha, 0, 0, 0)),
            Padding = new Thickness(3, 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
    }

    private Border CreateImageContainer(short id, Image image, TextBlock indexText)
    {
        var container = new Border
        {
            Width = GridItemSize,
            Height = GridItemSize,
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(BORDER_THICKNESS),
            CornerRadius = new CornerRadius(CORNER_RADIUS),
            Padding = new Thickness(MarginSize),
            Margin = new Thickness(MarginSize),
            Child = new Grid { Children = { image, indexText } },
            Tag = id,
            ContextMenu = CreateContextMenu()
        };

        SetupContainerEvents(container, id);
        return container;
    }

    private ContextMenu CreateContextMenu()
    {
        var contextMenu = new ContextMenu
        {
            Items = { new MenuItem { Header = "Delete Item" } }
        };

        ((MenuItem)contextMenu.Items[0]!).Click += (s, e) => DeleteItem_Click();
        return contextMenu;
    }

    private void SetupContainerEvents(Border container, short id)
    {
        container.PointerPressed += Container_PointerPressed;
        container.ContextRequested += (s, e) => _contextTargetLayer = InfoImage.GetValueOrDefault(id);
    }

    private void UpdateLayerIndices()
    {
        foreach (var key in InfoImage.Keys)
        {
            if (!InfoImage.TryGetValue(key, out var layer)) continue;
            if (layer.Container.Child is Grid grid &&
                grid.Children.OfType<TextBlock>().FirstOrDefault() is { } indexText)
                indexText.Text = key.ToString();
        }
    }

    private void DeleteItem_Click()
    {
        if (_contextTargetLayer?.Container == null) return;

        PanelImage.Children.Remove(_contextTargetLayer.Container);
        InfoImage.Remove(_contextTargetLayer.Id);
        _contextTargetLayer = null;
        UpdateLayerIndices();
    }

    private void Container_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border container) return;
        _contextTargetLayer = InfoImage.Values.FirstOrDefault(l => l.Container == container);
    }

    #endregion

    #region Helper Methods

    public ImageInfo GetImageInfo(short id)
    {
        InfoImage.TryGetValue(id, out var info);
        return info!;
    }

    private async Task ShowErrorAsync(string title, string message)
    {
        try
        {
            var messageBox =
                MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await messageBox.ShowAsync();
        }
        catch
        {
        }
    }

    private async Task ShowSuccessAsync(string title, string message)
    {
        try
        {
            var messageBox = MessageBoxManager
                .GetMessageBoxStandard(title, message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
            await messageBox.ShowAsync();
        }
        catch
        {
        }
    }

    #endregion
}