using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ToolEffectNro.Classes;
using ToolEffectNro.Options;
using ToolEffectNro.Windows;

namespace ToolEffectNro.ChildWindows;

public partial class FormMainEffect : Window
{
    #region Constructor

    public FormMainEffect()
    {
        InitializeComponent();
        Instance = this;
    }

    #endregion

    #region Constants

    private const string SelectFileMessage = "Vui Lòng Chọn File";
    private const string FileNotExistMessage = "File không tồn tại";
    private const string ErrorTitle = "Lỗi";
    private const string NotificationTitle = "Thông Báo";
    private const string SuccessMessage = "Thành Công";
    private const string LoadEffectSuccessMessage = "Load Effect Data Thành Công";
    private const string LoadEffectErrorMessage = "Có Lỗi Xảy Ra Khi Load Data Effect";
    private const int ScaleFactor = 4;

    #endregion

    #region Fields

    public static FormMainEffect Instance;
    public List<short> ArrayFrame = new();
    public Dictionary<short, List<ChildFrame>> ParentFrame = new();
    public Dictionary<byte, ImageInfo> InfoImage = new();

    private Bitmap _bitMapLoaded;
    private string _pathFile;

    #endregion

    #region Event Handlers

    private async void MakeTitle_OnClick(object sender, RoutedEventArgs e)
    {
        if (InfoImage.Count == 0)
        {
            await ShowErrorMessageAsync("Vui Lòng Xử Lý Ảnh Trước");
            return;
        }

        foreach (var childFrames in InfoImage.Keys.Select(key => new ChildFrame(0, (short)ParentFrame.Keys.Count, key,
                     (short)(-InfoImage[key].Image.Size.Width / 2),
                     (short)(-InfoImage[key].Image.Size.Height - 170))).Select(child => new List<ChildFrame>
                 {
                     child
                 }))
        {
            SideForm.Instance.CreatePanelFrame((short)ParentFrame.Keys.Count);
            ParentFrame.Add((short)ParentFrame.Keys.Count, childFrames);
        }

        for (short i = 0; i < ParentFrame.Keys.Count; i++)
        for (var j = 0; j < 5; j++)
            ArrayFrame.Add(i);

        WindowExecution.Instance.ArrayFrame.UpdateComboBoxAndGrid();
        await ShowSuccessMessageAsync("Làm Danh Hiệu Nhanh Thành Công");
    }

    private async void ImportImageButton_OnClick(object sender, RoutedEventArgs e)
    {
        var files = await GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        if (files.Count >= 1) LoadImageAsync(files[0].Path.LocalPath);
    }

    private void SaveCropButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateMaxImageCount()) return;

        try
        {
            var sizeCrop = ImageCanvas.CropRectangle / ImageCanvas.Scale;
            if (sizeCrop.Width <= 0 || sizeCrop.Height <= 0) return;

            var croppedImage = ImageHelper.CropBitmap(_bitMapLoaded,
                (int)sizeCrop.X,
                (int)sizeCrop.Y,
                (int)sizeCrop.Width,
                (int)sizeCrop.Height);

            InfoImage.Add((byte)InfoImage.Keys.Count,
                new ImageInfo((byte)InfoImage.Keys.Count,
                    (short)sizeCrop.X,
                    (short)sizeCrop.Y,
                    croppedImage));

            ResetCropSelection();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving crop: {ex.Message}");
        }
    }

    private async void FastCropButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidatePremiumFeatureAsync()) return;
        await PerformFastCropAsync();
    }

    private async void FastCropWithSizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidatePremiumFeatureAsync()) return;

        var dialog = new SizeCrop();
        var result = await dialog.ShowDialog<int[]>(WindowExecution.Instance);
        if (result != null) await PerformFastCropWithSizeAsync(result[0], result[1]);
    }

    private async void ImportDataButton_OnClick(object sender, RoutedEventArgs e)
    {
        await ImportDataAsync();
    }

    #endregion

    #region Image Operations

    private void LoadImageAsync(string filePath)
    {
        ClearAllResources();
        EmptyDataBitmap();
        _pathFile = filePath;
        _bitMapLoaded = new Bitmap(filePath);
        ImageCanvas.Source = _bitMapLoaded;
    }

    private async Task PerformFastCropAsync()
    {
        ClearAllResources();
        if (string.IsNullOrEmpty(_pathFile)) return;

        var listItem = await SixLaborsImageExtractor.ProcessAndExtractAsync(_pathFile);
        ProcessExtractedImages(listItem);
        await ShowSuccessMessageAsync(SuccessMessage);
    }

    private async Task PerformFastCropWithSizeAsync(int width, int height)
    {
        ClearAllResources();
        if (string.IsNullOrEmpty(_pathFile)) return;

        var listItem = await SixLaborsImageExtractor.CutImageByGridAsync(_pathFile, width, height);
        ProcessExtractedImages(listItem);
        await ShowSuccessMessageAsync(SuccessMessage);
    }

    private void ProcessExtractedImages(List<ExtractedObject> listItem)
    {
        if (listItem.Count + 1 > byte.MaxValue) throw new InvalidOperationException("Too many images.");

        foreach (var image in listItem)
            InfoImage.Add((byte)InfoImage.Keys.Count,
                new ImageInfo((byte)InfoImage.Keys.Count,
                    (short)image.X,
                    (short)image.Y,
                    image.Image));
    }

    private void ResetCropSelection()
    {
        ImageCanvas.CropRectangle = new Rect();
        ImageCanvas.InvalidateVisual();
    }

    #endregion

    #region Data Import Operations

    private async Task ImportDataAsync()
    {
        ClearAllResources();
        EmptyDataBitmap();
        var importData = new ImportData();
        var result = await importData.ShowDialog<string[]>(WindowExecution.Instance);
        if (result == null) return;

        if (importData.EffectFromImageDataRadio.IsChecked == true)
            await LoadEffectsFromImageAndDataAsync(result[0], result[1]);
        else
            await LoadEffectsFromDataOnlyAsync(result[0]);
        WindowExecution.Instance.ArrayFrame.UpdateComboBoxAndGrid();
    }

    private async Task LoadEffectsFromImageAndDataAsync(string imagePath, string dataPath)
    {
        if (!ValidateFilePath(imagePath) || !ValidateFilePath(dataPath)) return;

        try
        {
            await ProcessEffectFileAsync(dataPath, imagePath);
            await ShowSuccessMessageAsync(LoadEffectSuccessMessage);
        }
        catch (Exception ex)
        {
            await ShowErrorMessageAsync($"{LoadEffectErrorMessage}: {ex.Message}");
        }
    }

    private async Task LoadEffectsFromDataOnlyAsync(string dataPath)
    {
        if (!ValidateFilePath(dataPath)) return;

        try
        {
            var fileData = Array.ConvertAll(await File.ReadAllBytesAsync(dataPath), a => (sbyte)a);

            if (TryProcessEffectData(fileData, true, false) ||
                TryProcessEffectData(fileData, true, true) ||
                TryProcessEffectData(fileData, false, false) ||
                TryProcessEffectData(fileData, false, true))
                await ShowSuccessMessageAsync(LoadEffectSuccessMessage);
            else
                await ShowErrorMessageAsync(LoadEffectErrorMessage);
        }
        catch (Exception ex)
        {
            await ShowErrorMessageAsync($"{LoadEffectErrorMessage}: {ex.Message}");
        }
    }

    private async Task ProcessEffectFileAsync(string fileData, string imgData)
    {
        var data = Array.ConvertAll(await File.ReadAllBytesAsync(fileData), a => (sbyte)a);
        var imageBytes = Array.ConvertAll(await File.ReadAllBytesAsync(imgData), a => (sbyte)a);

        using var ms = new MemoryStream(Array.ConvertAll(imageBytes, a => (byte)a));
        _bitMapLoaded = new Bitmap(ms);
        ImageCanvas.Source = _bitMapLoaded;

        try
        {
            var dataInputStream = new myReader(data);
            dataInputStream.readShort();
            var length = dataInputStream.readInt();
            var data2 = new sbyte[length];
            dataInputStream.read(ref data2);
            LoadEffectData(data2);
            dataInputStream.Close();
        }
        catch
        {
            LoadEffectData(data);
        }
    }

    private bool TryProcessEffectData(sbyte[] fileData, bool withShort, bool withByte)
    {
        myReader dataInputStream = null;
        try
        {
            dataInputStream = new myReader(fileData);

            if (withShort) dataInputStream.readShort();

            var length = dataInputStream.readInt();
            var data = new sbyte[length];
            dataInputStream.read(ref data);

            if (withByte) dataInputStream.readByte();

            length = dataInputStream.readInt();
            var image = new sbyte[length];
            dataInputStream.read(ref image);

            using var ms = new MemoryStream(Array.ConvertAll(image, a => (byte)a));
            _bitMapLoaded = new Bitmap(ms);
            ImageCanvas.Source = _bitMapLoaded;

            LoadEffectData(data);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            dataInputStream?.Close();
        }
    }

    private void LoadEffectData(sbyte[] data)
    {
        var dataInputStream = new myReader(data);
        try
        {
            LoadImageInfo(dataInputStream);
            LoadFrameData(dataInputStream);
            LoadArrayFrameData(dataInputStream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading effect data: {ex.Message}");
        }
        finally
        {
            dataInputStream.Close();
        }
    }

    private void LoadImageInfo(myReader dataInputStream)
    {
        var imageCount = dataInputStream.readByte();
        for (var i = 0; i < imageCount; i++)
        {
            var id = dataInputStream.readByte();
            var x0 = dataInputStream.readUnsignedByte();
            var y0 = dataInputStream.readUnsignedByte();
            var w = dataInputStream.readUnsignedByte();
            var h = dataInputStream.readUnsignedByte();

            InfoImage.Add((byte)id,
                new ImageInfo((byte)id,
                    (short)(x0 * ScaleFactor),
                    (short)(y0 * ScaleFactor),
                    SixLaborsImageExtractor.CropBitmap(_bitMapLoaded,
                        x0 * ScaleFactor,
                        y0 * ScaleFactor,
                        w * ScaleFactor,
                        h * ScaleFactor)));
        }
    }

    private void LoadFrameData(myReader dataInputStream)
    {
        var frameCount = dataInputStream.readShort();
        for (var j = 0; j < frameCount; j++)
        {
            var childFrame = new List<ChildFrame>();
            var childCount = dataInputStream.readByte();

            for (var k = 0; k < childCount; k++)
            {
                var dx = dataInputStream.readShort();
                var dy = dataInputStream.readShort();
                var idImage = dataInputStream.readByte();

                childFrame.Add(new ChildFrame((byte)k,
                    (short)j,
                    (byte)idImage,
                    (short)(dx * ScaleFactor),
                    (short)(dy * ScaleFactor)));
            }

            ParentFrame.Add((short)j, childFrame);
            SideForm.Instance.CreatePanelFrame((short)j);
        }
    }

    private void LoadArrayFrameData(myReader dataInputStream)
    {
        var arrayCount = dataInputStream.readShort();
        for (var n = 0; n < arrayCount; n++) ArrayFrame.Add(dataInputStream.readShort());
    }

    #endregion

    #region Validation Methods

    private bool ValidateMaxImageCount()
    {
        if (InfoImage.Keys.Count + 1 <= byte.MaxValue) return true;

        _ = ShowErrorMessageAsync($"Chỉ có thể thêm tối đa {byte.MaxValue} ảnh");
        return false;
    }

    private async Task<bool> ValidatePremiumFeatureAsync()
    {
        return true;
    }

    private bool ValidateFilePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            _ = ShowErrorMessageAsync(SelectFileMessage);
            return false;
        }

        if (!File.Exists(path))
        {
            _ = ShowErrorMessageAsync(FileNotExistMessage);
            return false;
        }

        return true;
    }

    #endregion

    #region Resource Management

    private void EmptyDataBitmap()
    {
        _bitMapLoaded?.Dispose();
        _bitMapLoaded = null;
        ImageCanvas.Source = null;
    }

    private void ClearAllResources()
    {
        WindowExecution.Instance.ManagerCrop.Clear();
        SideForm.Instance.ClearPanel();

        ClearFrameCollections();
        ClearImageInfoCollection();
    }

    private void ClearFrameCollections()
    {
        foreach (var childFrames in ParentFrame.Values)
        {
            foreach (var childFrame in childFrames) childFrame.Dispose();
            childFrames.Clear();
        }

        ParentFrame.Clear();
        ArrayFrame.Clear();
    }

    private void ClearImageInfoCollection()
    {
        foreach (var imageInfo in InfoImage.Values) imageInfo.Dispose();
        InfoImage.Clear();
    }

    #endregion

    #region Message Display Methods

    private async Task ShowErrorMessageAsync(string message)
    {
        await MessageBoxManager.GetMessageBoxStandard(ErrorTitle, message,
            ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
    }

    private async Task ShowSuccessMessageAsync(string message)
    {
        await MessageBoxManager.GetMessageBoxStandard(NotificationTitle, message,
            ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success).ShowAsync();
    }

    #endregion

    #region Public Methods

    public ChildFrame GetChildFrame(Point point)
    {
        try
        {
            foreach (var childFrame in ParentFrame.Values
                         .Select(frameList => frameList.FirstOrDefault(frame => frame.Contains(point)))
                         .Where(childFrame => childFrame != null)) return childFrame;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetChildFrame: {ex.Message}");
        }

        return null;
    }

    public ImageInfo GetImageInfo(byte id)
    {
        InfoImage.TryGetValue(id, out var info);
        return info;
    }

    #endregion
}