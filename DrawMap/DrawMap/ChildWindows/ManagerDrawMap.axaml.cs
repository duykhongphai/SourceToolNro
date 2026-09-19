using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DrawMap.Classes;
using DrawMap.Options;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace DrawMap.ChildWindows;

public partial class ManagerDrawMap : Window
{
    #region Constants

    private const int TileSize = 24;
    private const int DefaultBackgroundId = 20;
    private const int DefaultIdTitle = 1;
    private const int BgOffsetY1 = 75;
    private const int BgOffsetY2 = 50;
    private const int BgOffsetY3 = 50;
    private const int BgOffsetY4 = 90;
    private const string LoadFailedMessage = "Load thất bại. Data không hợp lệ";
    private const string BuildSuccessMessage = "Build Thành Công";

    #endregion

    #region Properties and Fields

    public static ManagerDrawMap Instance;

    public int BackgroundId = DefaultBackgroundId;
    public int IdTitle = DefaultIdTitle;
    public int HMedium;
    public int WMedium;
    public int Tmh;
    public int Tmw;

    public readonly List<BgItem> BgItem = [];
    public byte[] DataMap;
    public readonly List<EffectMap> EffectMap = [];
    public readonly List<ActorMap> MonsterMap = [];
    public readonly List<ActorMap> NpcMap = [];
    public readonly List<ActorMap> WaypointMap = [];

    public int PageEffect = 0;
    public int PageItemBg = 0;
    public int PageMonster = 0;
    public int PageNpc = 0;

    #endregion

    #region Constructor and Initialization

    public ManagerDrawMap()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Instance = this;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadDefaultTile();
    }

    public void SetParentForm(Window parentForm)
    {
        Owner = parentForm;
    }

    #endregion

    #region Map Management

    public void LoadDefaultTile()
    {
        UpdateMapDimensions();
        DataMap = new byte[Tmh * Tmw];
    }

    private void UpdateMapDimensions()
    {
        HMedium = int.Parse(HeightTextBox.Text!) / TileSize * TileSize;
        WMedium = int.Parse(WidthTextBox.Text!) / TileSize * TileSize;
        Tmh = HMedium / TileSize;
        Tmw = WMedium / TileSize;
        DrawMap.Instance.UpdateSize();
    }

    private void RefreshMap()
    {
        DrawMap.Instance.PanelMap.InvalidateVisual();
    }

    private void ClearAllMapData()
    {
        BgItem.Clear();
        EffectMap.Clear();
        MonsterMap.Clear();
        NpcMap.Clear();
        WaypointMap.Clear();
        DataMap = new byte[Tmh * Tmw];
    }

    public void InitYBackground()
    {
        if (!Fields.ResourceBackground.TryGetValue(BackgroundId, out var value)) return;

        for (var i = 0; i < value.Count; i++) DrawMap.Instance.BgH[i] = (int)value[i].Bounds.Height;

        var yb = DrawMap.Instance.Yb;
        var bgH = DrawMap.Instance.BgH;

        yb[0] = HMedium * 2 / 3 - bgH[0] + BgOffsetY1;
        yb[1] = yb[0] - bgH[1] + BgOffsetY2;
        yb[2] = yb[1] - bgH[2] + BgOffsetY3;
        yb[3] = yb[2] - bgH[3] + BgOffsetY4;
    }

    #endregion

    #region Simple Event Handlers

    private void ViewGridCheck_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        RefreshMap();
    }

    private void ViewBackgroundCheck_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        RefreshMap();
    }

    private void ViewEffectCheck_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        RefreshMap();
    }

    private void ViewActorCheck_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        RefreshMap();
    }

    private void ViewItemBackgroundCheck_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        RefreshMap();
    }

    private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void ClearMapBtn_OnClick(object sender, RoutedEventArgs e)
    {
        ClearAllMapData();
        RefreshMap();
    }

    private void WaypointRadio_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (WaypointRadio.IsChecked != true) return;
        DrawMap.Instance.GetItemWaypoint();
        RefreshMap();
    }

    private void TitleTypeCheck_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        RefreshMap();
        foreach (var control in DrawMap.Instance.PanelItems.Children)
            if (control is PictureCustom custom)
                custom.InvalidateVisual();
    }

    private void TextBoxNumeric_OnKeyDown(object sender, KeyEventArgs e)
    {
        var isNumeric = e.Key is >= Key.D0 and <= Key.D9 or >= Key.NumPad0 and <= Key.NumPad9;
        if (!isNumeric) e.Handled = true;
    }

    #endregion

    #region TextBox Events

    private void WidthTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateMapFromTextBox();
    }

    private void HeightTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateMapFromTextBox();
    }

    private void UpdateMapFromTextBox()
    {
        try
        {
            UpdateMapDimensions();

            var newArr = new byte[Tmh * Tmw];
            if (DataMap != null)
                Array.Copy(DataMap, newArr, Math.Min(DataMap.Length, newArr.Length));

            DataMap = newArr;
            InitYBackground();
            RefreshMap();
        }
        catch
        {
        }
    }

    #endregion

    #region Radio Button Events

    private void TileMapRadio_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (TileMapRadio.IsChecked != true) return;
        DrawMap.Instance.GetItemMap(IdTitle, false);
        RefreshMap();
    }

    private async void ItemBackgroundRadio_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (ItemBackgroundRadio.IsChecked != true) return;

        if (!await ValidateForPremiumFeature(Fields.ResourceItemBackground, "Data Item Background"))
        {
            ResetToTileMap();
            return;
        }

        DrawMap.Instance.GetItemBackground(PageItemBg);
        RefreshMap();
    }

    private async void EffectRadio_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (EffectRadio.IsChecked != true) return;

        if (!await ValidateForPremiumFeature(Fields.ResourceEffect, "Data Effect"))
        {
            ResetToTileMap();
            return;
        }

        DrawMap.Instance.GetItemEffect(PageEffect);
        RefreshMap();
    }

    private async void MonsterRadio_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (MonsterRadio.IsChecked != true) return;

        try
        {
            if (!await ValidateForPremiumFeature(Fields.ResourceMonster, "Monster Template"))
            {
                ResetToTileMap();
                return;
            }

            DrawMap.Instance.GetItemMonster(PageMonster);
            RefreshMap();
        }
        catch
        {
            await ShowErrorMessage(LoadFailedMessage);
            ResetToTileMap();
        }
    }

    private async void NpcRadio_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (NpcRadio.IsChecked != true) return;

        try
        {
            if (!await ValidateForPremiumFeature(Fields.ResourceNpc, "Npc Template"))
            {
                ResetToTileMap();
                return;
            }

            DrawMap.Instance.GetItemNpc(PageNpc);
            RefreshMap();
        }
        catch
        {
            await ShowErrorMessage(LoadFailedMessage);
            ResetToTileMap();
        }
    }

    private void ResetToTileMap()
    {
        ItemBackgroundRadio.IsChecked = false;
        EffectRadio.IsChecked = false;
        MonsterRadio.IsChecked = false;
        NpcRadio.IsChecked = false;
        TileMapRadio.IsChecked = true;
    }

    #endregion

    #region File Operations

    private async void LoadMapBtn_OnClick(object sender, RoutedEventArgs e)
    {
        var file = await OpenFileDialog();
        if (file == null) return;

        try
        {
            var fileBytes = await ReadFileBytes(file);
            if (await LoadMapData(fileBytes))
                return;
        }
        catch
        {
        }

        LoadDefaultTile();
        await ShowErrorMessage(LoadFailedMessage);
    }

    private async void LoadItemBgBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateForPremiumFeature(Fields.ResourceItemBackground, "Data Item Background"))
            return;

        await LoadDataWithHandler(LoadBgItemData);
    }

    private async void LoadEffectBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateForPremiumFeature(Fields.ResourceEffect, "Data Effect"))
            return;

        await LoadDataWithHandler(LoadEffectData);
    }

    private async void BuildDataBtn_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await BuildAllMapData();
            await ShowSuccessMessage(BuildSuccessMessage);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Có lỗi xảy ra: \n{ex.Message}");
        }
    }

    #endregion

    #region File Helper Methods

    private async Task<IStorageFile> OpenFileDialog()
    {
        var topLevel = GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Data File",
            AllowMultiple = false
        });
        return files.Count > 0 ? files[0] : null;
    }

    private async Task<byte[]> ReadFileBytes(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private async Task LoadDataWithHandler(Func<byte[], Task<bool>> handler)
    {
        var file = await OpenFileDialog();
        if (file == null) return;

        try
        {
            var fileBytes = await ReadFileBytes(file);
            if (!await handler(fileBytes))
                await ShowErrorMessage(LoadFailedMessage);
        }
        catch
        {
            await ShowErrorMessage(LoadFailedMessage);
        }
    }

    #endregion

    #region Data Loading Methods

    private Task<bool> LoadMapData(byte[] fileBytes)
    {
        try
        {
            var reader = new BinaryDataReader(ByteHelper.AsSBytes(fileBytes));

            Tmw = reader.readByte();
            Tmh = reader.readByte();
            DataMap = new byte[Tmh * Tmw];

            HeightTextBox.Text = (Tmh * TileSize).ToString();
            WidthTextBox.Text = (Tmw * TileSize).ToString();

            UpdateMapDimensions();

            for (var i = 0; i < DataMap.Length; i++)
                DataMap[i] = (byte)reader.readByte();

            RefreshMap();
            reader.Close();
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private Task<bool> LoadBgItemData(byte[] fileBytes)
    {
        try
        {
            var reader = new BinaryDataReader(ByteHelper.AsSBytes(fileBytes));
            BgItem.Clear();

            var count = reader.readShort();
            for (var i = 0; i < count; i++)
            {
                var id = reader.readShort();
                var pic = Fields.ResourceItemBackground[id];
                var x = reader.readShort() * TileSize + pic.GetDx();
                var y = reader.readShort() * TileSize + pic.GetDy();
                BgItem.Add(new BgItem(id, x, y));
            }

            RefreshMap();
            reader.Close();
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private Task<bool> LoadEffectData(byte[] fileBytes)
    {
        try
        {
            var reader = new BinaryDataReader(ByteHelper.AsSBytes(fileBytes));
            EffectMap.Clear();

            var count = reader.readShort();
            for (var i = 0; i < count; i++)
            {
                var eff = reader.readUTF();
                var value = reader.readUTF();

                if (!eff.Equals("eff")) continue;

                var split = value.Split('.');
                if (split.Length >= 4)
                    EffectMap.Add(new EffectMap(
                        int.Parse(split[0]), int.Parse(split[1]),
                        int.Parse(split[2]), int.Parse(split[3])));
            }

            RefreshMap();
            reader.Close();
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    #endregion

    #region Data Building Methods

    private async Task BuildAllMapData()
    {
        EnsureOutputDirectories();
        await BuildTileMapData();
        await BuildBgItemData();
        await BuildEffectData();
    }

    private void EnsureOutputDirectories()
    {
        var directories = new[]
            { Settings.folderOutputMap, Settings.folderOutputItemBgMap, Settings.folderOutputEffect };
        foreach (var dir in directories)
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
    }

    private async Task BuildTileMapData()
    {
        var writer = new BinaryDataWriter();
        writer.writeByte((sbyte)Tmw);
        writer.writeByte((sbyte)Tmh);

        for (var i = 0; i < Tmh; i++)
        for (var j = 0; j < Tmw; j++)
            writer.writeByte((sbyte)DataMap[i * Tmw + j]);

        var filePath = GetNextFilePath(Settings.folderOutputMap);
        await File.WriteAllBytesAsync(filePath, ByteHelper.AsBytes(writer.getData()));
        writer.Close();
    }

    private async Task BuildBgItemData()
    {
        var writer = new BinaryDataWriter();
        writer.writeShort((short)BgItem.Count);

        foreach (var item in BgItem)
        {
            var pictureBox = Fields.ResourceItemBackground[item.Id];
            writer.writeShort((short)item.Id);
            writer.writeShort((short)((item.X - pictureBox.GetDx()) / TileSize));
            writer.writeShort((short)((item.Y - pictureBox.GetDy()) / TileSize));
        }

        var filePath = GetNextFilePath(Settings.folderOutputItemBgMap);
        await File.WriteAllBytesAsync(filePath, ByteHelper.AsBytes(writer.getData()));
        writer.Close();
    }

    private async Task BuildEffectData()
    {
        var writer = new BinaryDataWriter();
        writer.writeShort((short)EffectMap.Count);

        foreach (var item in EffectMap)
        {
            writer.writeUTF("eff");
            writer.writeUTF($"{item.Id}.{item.Layer}.{item.X}.{item.Y + Fields.ResourceEffect[item.Id].Height}");
        }

        var filePath = GetNextFilePath(Settings.folderOutputEffect);
        await File.WriteAllBytesAsync(filePath, ByteHelper.AsBytes(writer.getData()));
        writer.Close();
    }

    private string GetNextFilePath(string directory)
    {
        return Path.Combine(directory, Directory.GetFiles(directory).Length.ToString());
    }

    #endregion

    #region Validation and UI Helper Methods

    private async Task<bool> ValidateForPremiumFeature(object resource, string resourceName)
    {
        var hasResource = resource switch
        {
            IDictionary dict => dict.Count > 0,
            ICollection collection => collection.Count > 0,
            null => false,
            _ => true
        };

        if (hasResource) return true;

        await ShowErrorMessage($"Vui Lòng Load {resourceName} Để Thực Hiện");
        return false;
    }

    private async Task ShowErrorMessage(string message)
    {
        await MessageBoxManager.GetMessageBoxStandard("Thông Báo", message,
            ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
    }

    private async Task ShowSuccessMessage(string message)
    {
        await MessageBoxManager.GetMessageBoxStandard("Thông Báo", message,
            ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success).ShowAsync();
    }

    #endregion
}