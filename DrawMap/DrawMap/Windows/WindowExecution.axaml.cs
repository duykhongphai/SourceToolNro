using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using DrawMap.ChildWindows;
using DrawMap.Classes;
using DrawMap.Options;
using SharpCompress.Archives;
using SharpCompress.Common;
using Enum = DrawMap.Options.Enum;

namespace DrawMap.Windows;

public partial class WindowExecution : Window
{
    #region Constants

    private const int ExpectedTileMapFiles = 875;
    private const int ExpectedItemBackgroundFiles = 348;
    private const int ProgressUpdateDelay = 200;
    private const int ErrorDisplayDelay = 2000;
    private const int LongErrorDisplayDelay = 5000;

    private const string TileMapUrl =
        "https://www.dropbox.com/scl/fi/bb2nzdwzalah1sakooe7q/ItemMap.zip?rlkey=uovddwd13t8tp7e5hzzpojj6u&st=ktzir5dm&dl=1";

    private const string ItemBackgroundUrl =
        "https://www.dropbox.com/scl/fi/3kn8kjfrrbi9eteq6z397/BackgroundItem.zip?rlkey=ez90d0nlcdo598lv54232zscs&st=u75wll4t&dl=1";

    private const string InitializingMessage = "Đang khởi tạo...";
    private const string DownloadCompleteMessage = "Tải xuống hoàn tất!";
    private const string DownloadCancelledMessage = "Đã hủy tải xuống";
    private const string DownloadStoppedMessage = "Đã dừng";
    private const string ErrorOccurredMessage = "Có lỗi xảy ra! Vui lòng thử lại...";

    #endregion

    #region Fields

    public static WindowExecution Instance { get; private set; }

    private readonly ConnectSQL _connectSql = new();
    private readonly ChildWindows.DrawMap _drawMap = new();
    private readonly ManagerTile _managerTile = new();
    private readonly HttpClient _httpClient;

    private CancellationTokenSource _cancellationTokenSource;
    private Button _currentSelectedButton;

    #endregion

    #region Constructor and Initialization

    public WindowExecution()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        Loaded += OnLoaded;
        Instance = this;
    }

    #endregion

    #region Event Handlers

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await InitializeApplicationData();
    }

    protected override void OnClosed(EventArgs e)
    {
        CleanupResources();
        base.OnClosed(e);
    }

    private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void MinimumButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        Close();
    }

    private void ButtonMap_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonMap);
        AddChildForm(_drawMap);
    }

    private void ButtonFolder_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonFolder);
        AddChildForm(_managerTile);
    }

    private void ButtonSql_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonSql);
        AddChildForm(_connectSql);
    }

    #endregion

    #region Application Initialization

    private async Task InitializeApplicationData()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        ProgressSection.IsVisible = true;

        try
        {
            await ExecuteDownloadProcess();
        }
        catch (OperationCanceledException)
        {
            await HandleDownloadCancellation();
        }
        catch (Exception)
        {
            await HandleDownloadError();
        }
        finally
        {
            await CleanupDownloadProcess();
        }

        await LoadApplicationResources();
    }

    private async Task ExecuteDownloadProcess()
    {
        await Task.Run(() => StartDownloadProcess(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
    }

    private async Task HandleDownloadCancellation()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            StatusText.Text = DownloadCancelledMessage;
            ProgressText.Text = DownloadStoppedMessage;
        });
        await Task.Delay(ErrorDisplayDelay);
    }

    private async Task HandleDownloadError()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            StatusText.Text = ErrorOccurredMessage;
            ProgressSection.IsVisible = true;
        });
        await Task.Delay(LongErrorDisplayDelay);
    }

    private async Task CleanupDownloadProcess()
    {
        await Dispatcher.UIThread.InvokeAsync(() => { ProgressSection.IsVisible = false; });
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private async Task LoadApplicationResources()
    {
        await LoadTileInfo();
        LoadResourceMaps();
        InitializeWaypointPicture();
        EnsureTileIndexConsistency();
    }

    private async Task LoadTileInfo()
    {
        if (!File.Exists(Settings.folderOutputTileInfo)) return;

        try
        {
            var fileBytes = await File.ReadAllBytesAsync(Settings.folderOutputTileInfo);
            var reader = new BinaryDataReader(ByteHelper.AsSBytes(fileBytes));

            var size = reader.readByte();
            Fields.TileIndex = [];
            Fields.TileType = [];

            for (var i = 0; i < size; i++)
            {
                var count = reader.readByte();
                Fields.TileType.Add([]);
                Fields.TileIndex.Add([]);

                for (var j = 0; j < count; j++)
                {
                    Fields.TileType[i].Add(reader.readInt());
                    var length = reader.readByte();
                    Fields.TileIndex[i].Add([]);

                    for (var k = 0; k < length; k++)
                        Fields.TileIndex[i][j].Add(reader.readByte());
                }
            }

            reader.Close();
        }
        catch
        {
        }
    }

    private static void LoadResourceMaps()
    {
        Fields.ResourceTitleMap = Function.GetAllImageTile();
        Fields.ResourceBackground = Function.GetAllImageBackground();
        Fields.ResourceImageItemBackground = Function.GetAllImageItemBackground();
    }

    private static void InitializeWaypointPicture()
    {
        Fields.PictureWaypoint = new PictureCustom
        {
            Source = ImageHelper.LoadFromResource(new Uri("avares://DrawMap/Assets/Resources/portal.png")),
            Width = 70,
            Height = 30,
            TypePicture = Enum.Waypoint
        };
        Fields.PictureWaypoint.PointerPressed += ChildWindows.DrawMap.Instance.PicOnPointerPressed;
    }

    private static void EnsureTileIndexConsistency()
    {
        if (File.Exists(Settings.folderOutputTileInfo)) return;

        var countAdd = Fields.ResourceTitleMap.Count - Fields.TileIndex.Count;
        for (var i = 0; i < countAdd; i++)
        {
            Fields.TileType.Add([]);
            Fields.TileIndex.Add([]);
        }
    }

    private void CleanupResources()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _httpClient?.Dispose();
        ManagerDrawMap.Instance?.Close();
        NotificationDialog.Instance?.Close();
        ManagerTile.Instance.ExportTileInfo();
    }

    #endregion

    #region Download Process

    private async Task StartDownloadProcess(CancellationToken cancellationToken)
    {
        await UpdateDownloadProgress(0, InitializingMessage, "0/2 files hoàn thành");

        var downloadTasks = CreateDownloadTasks(cancellationToken);
        var progressTracker = TrackProgress(downloadTasks, cancellationToken);

        try
        {
            await Task.WhenAll(downloadTasks);
            await UpdateDownloadProgress(100, DownloadCompleteMessage, "2/2 files hoàn thành");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            await HandlePartialDownloadCompletion(downloadTasks);
        }
    }

    private Task[] CreateDownloadTasks(CancellationToken cancellationToken)
    {
        return new[]
        {
            DownloadAndExtract(Settings.folderTileMap, ExpectedTileMapFiles, TileMapUrl, "TileMap", cancellationToken),
            DownloadAndExtract(Settings.folderItemBackground, ExpectedItemBackgroundFiles, ItemBackgroundUrl,
                "ItemBackground", cancellationToken)
        };
    }

    private async Task HandlePartialDownloadCompletion(Task[] downloadTasks)
    {
        var successCount = downloadTasks.Count(t => t.IsCompletedSuccessfully);
        var failureCount = 2 - successCount;

        await UpdateDownloadProgress(100, $"Hoàn thành với {failureCount} lỗi",
            $"{successCount}/2 files thành công");
    }

    private async Task UpdateDownloadProgress(double progress, string statusText, string progressText)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            DownloadProgressBar.Value = progress;
            StatusText.Text = statusText;
            ProgressText.Text = progressText;
        });
    }

    private async Task TrackProgress(Task[] tasks, CancellationToken cancellationToken)
    {
        while (!Task.WhenAll(tasks).IsCompleted && !cancellationToken.IsCancellationRequested)
        {
            var (completedTasks, failedTasks) = CountTaskStatus(tasks);
            var progress = (double)(completedTasks + failedTasks) / tasks.Length * 100;

            await UpdateProgressDisplay(completedTasks, failedTasks, tasks.Length, progress);

            try
            {
                await Task.Delay(ProgressUpdateDelay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private static (int completed, int failed) CountTaskStatus(Task[] tasks)
    {
        var completedTasks = 0;
        var failedTasks = 0;

        foreach (var task in tasks)
            if (task.IsCompleted)
            {
                if (task.IsFaulted)
                    failedTasks++;
                else
                    completedTasks++;
            }

        return (completedTasks, failedTasks);
    }

    private async Task UpdateProgressDisplay(int completedTasks, int failedTasks, int totalTasks, double progress)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            DownloadProgressBar.Value = progress;
            var statusText = $"{completedTasks}/{totalTasks} files hoàn thành";
            if (failedTasks > 0)
                statusText += $", {failedTasks} lỗi";

            ProgressText.Text = statusText;
        });
    }

    private async Task DownloadAndExtract(string targetFolder, int expectedFileCount, string downloadUrl,
        string taskName, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await ShouldSkipDownload(targetFolder, expectedFileCount, taskName))
                return;

            await ExecuteDownload(downloadUrl, targetFolder, taskName, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await UpdateStatusText($"Đã hủy {taskName}");
            throw;
        }
        catch (Exception ex)
        {
            await UpdateStatusText($"Lỗi tải {taskName}: {ex.Message}");
            Console.WriteLine($"Error downloading {taskName}: {ex}");
            throw;
        }
    }

    private async Task<bool> ShouldSkipDownload(string targetFolder, int expectedFileCount, string taskName)
    {
        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        var existingFiles = Directory.GetFiles(targetFolder, "*", SearchOption.AllDirectories);
        if (existingFiles.Length < expectedFileCount) return false;

        await UpdateStatusText($"{taskName} đã tồn tại, bỏ qua...");
        return true;
    }

    private async Task ExecuteDownload(string downloadUrl, string targetFolder, string taskName,
        CancellationToken cancellationToken)
    {
        await UpdateStatusText($"Đang tải {taskName}...");

        cancellationToken.ThrowIfCancellationRequested();
        var tempFilePath = await DownloadFile(downloadUrl, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        await UpdateStatusText($"Đang giải nén {taskName}...");

        ExtractArchive(tempFilePath, targetFolder);
        File.Delete(tempFilePath);

        await UpdateStatusText($"Hoàn thành {taskName}");
    }

    private async Task UpdateStatusText(string message)
    {
        await Dispatcher.UIThread.InvokeAsync(() => { StatusText.Text = message; });
    }

    private async Task<string> DownloadFile(string downloadUrl, CancellationToken cancellationToken)
    {
        using var request = CreateHttpRequest(downloadUrl);

        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response = await HandleRedirect(response, downloadUrl, cancellationToken);

        response.EnsureSuccessStatusCode();
        return await SaveResponseToTempFile(response, cancellationToken);
    }

    private static HttpRequestMessage CreateHttpRequest(string downloadUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        return request;
    }

    private async Task<HttpResponseMessage> HandleRedirect(HttpResponseMessage response, string originalUrl,
        CancellationToken cancellationToken)
    {
        if (response.Headers.Location == null) return response;

        var redirectUrl = response.Headers.Location.ToString();
        using var redirectRequest = new HttpRequestMessage(HttpMethod.Get, redirectUrl);

        response.Dispose();
        return await _httpClient.SendAsync(redirectRequest, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> SaveResponseToTempFile(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var tempFilePath = Path.GetTempFileName();
        await using var fileStream = new FileStream(tempFilePath, FileMode.Create);
        await response.Content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        return tempFilePath;
    }

    private static void ExtractArchive(string archiveFilePath, string extractPath)
    {
        using var archive = ArchiveFactory.Open(archiveFilePath);
        foreach (var entry in archive.Entries)
            if (!entry.IsDirectory)
                entry.WriteToDirectory(extractPath, new ExtractionOptions
                {
                    ExtractFullPath = false,
                    Overwrite = true
                });
    }

    #endregion

    #region UI Management

    private void SetButtonSelected(Button selectedButton)
    {
        ResetPreviousButton();
        ActivateSelectedButton(selectedButton);
        _currentSelectedButton = selectedButton;
    }

    private void ResetPreviousButton()
    {
        if (_currentSelectedButton == null) return;

        var previousImage = (Image)_currentSelectedButton.Content!;
        var previousImageName = GetBaseImageName(previousImage.Name!);
        previousImage.Source = ImageHelper.LoadFromResource(
            new Uri($"avares://DrawMap/Assets/Resources/{previousImageName}.png"));
    }

    private static void ActivateSelectedButton(Button selectedButton)
    {
        if (selectedButton == null) return;

        var selectedImage = (Image)selectedButton.Content!;
        var baseImageName = GetBaseImageName(selectedImage.Name!);
        selectedImage.Source = ImageHelper.LoadFromResource(
            new Uri($"avares://DrawMap/Assets/Resources/{baseImageName}Selected.png"));
    }

    private static string GetBaseImageName(string imagePath)
    {
        var filename = Path.GetFileNameWithoutExtension(imagePath);
        return filename.EndsWith("Selected") ? filename[..^8] : filename;
    }

    private void AddChildForm(Window user)
    {
        SetupChildFormContent(user);
        HandleSpecificFormTypes(user);
    }

    private void SetupChildFormContent(Window user)
    {
        ContentPanel.Children.Clear();

        if (user.Content is Control control)
        {
            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            control.VerticalAlignment = VerticalAlignment.Stretch;
            ContentPanel.Children.Add(control);
        }
    }

    private void HandleSpecificFormTypes(Window user)
    {
        switch (user)
        {
            case ChildWindows.DrawMap drawMap:
                HandleDrawMapForm(drawMap);
                break;
            case ManagerTile tile:
                HandleManagerTileForm(tile);
                break;
            default:
                ManagerDrawMap.Instance.Hide();
                break;
        }
    }

    private void HandleDrawMapForm(ChildWindows.DrawMap drawMap)
    {
        ClearManagerTilePanels();
        LoadDrawMapContent(drawMap);
        ManagerDrawMap.Instance.InitYBackground();
        ManagerDrawMap.Instance.Show();
    }

    private static void ClearManagerTilePanels()
    {
        ManagerTile.Instance.PanelItemMap.Children.Clear();
        ManagerTile.Instance.PanelItemBackground.Children.Clear();
    }

    private static void LoadDrawMapContent(ChildWindows.DrawMap drawMap)
    {
        var manager = ManagerDrawMap.Instance;

        if (manager.TileMapRadio.IsChecked == true)
            drawMap.GetItemMap(manager.IdTitle,false);
        else if (manager.ItemBackgroundRadio.IsChecked == true)
            drawMap.GetItemBackground(manager.PageItemBg);
        else if (manager.NpcRadio.IsChecked == true)
            drawMap.GetItemNpc(manager.PageNpc);
        else if (manager.MonsterRadio.IsChecked == true)
            drawMap.GetItemMonster(manager.PageMonster);
        else if (manager.WaypointRadio.IsChecked == true)
            drawMap.GetItemWaypoint();
    }

    private void HandleManagerTileForm(ManagerTile tile)
    {
        ChildWindows.DrawMap.Instance.PanelItems.Children.Clear();
        tile.GetItemMap(tile.IdTitle);
        tile.GetItemBackground(tile.PageItemBg);
        ManagerDrawMap.Instance.Hide();
    }

    #endregion
}