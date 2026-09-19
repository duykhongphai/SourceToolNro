using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DrawMap.Classes;
using DrawMap.Options;
using DrawMap.Windows;
using Enum = DrawMap.Options.Enum;

namespace DrawMap.ChildWindows;

public partial class ManagerTile : Window
{
    public ManagerTile()
    {
        InitializeComponent();
        Instance = this;
    }

    private const int TileSize = 24;
    private const int ScalePercent = 25;
    private const int MaxDefaultTileId = 32;
    private const int DefaultBackgroundId = 20;
    private const int NpcWidth = 22;
    private const int NpcHeight = 32;
    private const int NpcMargin = 15;
    private const int EffectMargin = 5;
    private const int EffectSkipRangeStart = 60;
    private const int EffectSkipRangeEnd = 65;

    private const string LoadSuccessMessage = "Load Thành Công";
    private const string DeleteSuccessMessage = "Xóa Thành Công";
    private const string DeleteFailedMessage = "Xóa Thất Bại";
    private const string SaveSuccessMessage = "Lưu Thành Công";
    private const string SelectFolderMessage = "Vui Lòng Chọn Folder";
    private const string FolderNotExistMessage = "Folder không tồn tại";
    private const string DatabaseConnectionRequiredMessage = "Vui Lòng Kết Nối Tới Database";

    public static ManagerTile Instance;
    public int IdTitle = 1;
    public int PageItemBg = 1;

    private PictureCustom _pictureCustomSelected;

    private readonly string[][] _dataNotification =
    [
        ["From Database", "From Data"],
        ["To Database", "To Data"]
    ];

    public void GetItemMap(int id)
    {
        IdTitle = id;
        PanelItemMap.Children.Clear();
        CreatePicture(Fields.ResourceTitleMap[id]);
        TextBlockPage.Text = $"Tile ID: {IdTitle}/{Fields.ResourceTitleMap.Count}";
    }

    public void GetItemBackground(int page)
    {
        if (Fields.ResourceItemBackground == null || Fields.ResourceItemBackground.Count == 0) return;

        PageItemBg = page;
        PanelItemBackground.Children.Clear();

        foreach (var item in Fields.ResourceItemBackground
                     .OrderBy(kvp => kvp.Key)
                     .Skip(page * Settings.numItemInPage)
                     .Take(Settings.numItemInPage))
            CreatePicture(item.Value);

        TextBlockItemBgPage.Text = $"Page: {page}/{Fields.ResourceItemBackground.Count / Settings.numItemInPage}";
    }

    private void CreatePicture(List<PictureCustom> list)
    {
        foreach (var pic in list)
            CreatePicture(pic);
    }

    private void CreatePicture(PictureCustom pic)
    {
        switch (pic.TypePicture)
        {
            case Enum.TileMap:
                pic.PointerPressed -= PicOnPointerPressed;
                pic.PointerPressed += PicOnPointerPressed;
                PanelItemMap.Children.Add(pic);
                break;
            case Enum.Background:
                PanelBackground.Children.Add(pic);
                break;
            case Enum.ItemBackground:
                pic.PointerPressed -= PicOnPointerPressed;
                pic.PointerPressed += PicOnPointerPressed;
                PanelItemBackground.Children.Add(pic);
                break;
        }
    }

    private void GetBackGround(int bgId)
    {
        try
        {
            ManagerDrawMap.Instance.BackgroundId = bgId;
            TextBlockBackgroundId.Text = $"Background Id: {bgId}";
            PanelBackground.Children.Clear();
            CreatePicture(Fields.ResourceBackground[bgId]);
            ManagerDrawMap.Instance.InitYBackground();
            PanelBackground.InvalidateVisual();
        }
        catch
        {
        }
    }

    private async void BrowseEffectImageBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Effect Image Folder", EffectImageFolderTextBox);
    }

    private async void BrowseEffectDataBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Effect Data Folder", EffectDataFolderTextBox);
    }

    private async void BrowseEffectDataOnlyBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Effect Data Folder", EffectDataOnlyFolderTextBox);
    }

    private async void BrowseIconsBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Icons Folder", IconsPathTextBox);
    }

    private async void BrowseMonsterImageBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Monster Image Folder", MonsterImageFolderTextBox);
    }

    private async void BrowseMonsterDataBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Monster Data Folder", MonsterDataFolderTextBox);
    }

    private async void BrowseMonsterDataOnlyBtn_Click(object sender, RoutedEventArgs e)
    {
        await BrowseFolderAndSetTextBox("Select Monster Data Folder", MonsterDataOnlyFolderTextBox);
    }

    private void PicOnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is not PictureCustom image) return;

        _pictureCustomSelected = image;

        switch (image.TypePicture)
        {
            case Enum.TileMap:
                HandleTileMapSelection(image);
                break;
            case Enum.ItemBackground:
                HandleItemBackgroundSelection(image);
                break;
        }
    }

    private void HandleTileMapSelection(PictureCustom image)
    {
        EmptyPicture();
        PictureImageSelected.Source = image.Source;
        PictureImageSelected.DataPicture = image.DataPicture;
        PictureImageSelected.TypeBlock = image.TypeBlock;

        UpdateCheckBoxesFromTypeBlock(image.TypeBlock);
    }

    private void HandleItemBackgroundSelection(PictureCustom image)
    {
        OffsetXTextBox.Text = image.GetDx().ToString();
        OffsetYTextBox.Text = image.GetDy().ToString();
        LayerItemBgComboBox.SelectedIndex = image.GetLayer() - 1;
        PanelViewItemBg.Image = image.Source;
        PanelViewItemBg.InvalidateVisual();
    }

    private void UpdateCheckBoxesFromTypeBlock(List<int> typeBlock)
    {
        if (typeBlock == null) return;

        foreach (var block in typeBlock)
            switch (block)
            {
                case var b when b == Fields.T_BOTTOM:
                    CheckBoxBottom.IsChecked = true;
                    break;
                case var b when b == Fields.T_TOP:
                    CheckBoxTop.IsChecked = true;
                    break;
                case var b when b == Fields.T_LEFT:
                    CheckBoxLeft.IsChecked = true;
                    break;
                case var b when b == Fields.T_RIGHT:
                    CheckBoxRight.IsChecked = true;
                    break;
            }
    }

    private void EmptyPicture()
    {
        PictureImageSelected.Source = null;
        PictureImageSelected.TypeBlock = null;
        PictureImageSelected.DataPicture = null;
        ResetAllCheckBoxes();
    }

    private void ResetAllCheckBoxes()
    {
        CheckBoxBottom.IsChecked = false;
        CheckBoxTop.IsChecked = false;
        CheckBoxLeft.IsChecked = false;
        CheckBoxRight.IsChecked = false;
    }

    private void PrevPageBtn_OnClick(object sender, RoutedEventArgs e)
    {
        EmptyPicture();
        var minId = Fields.ResourceTitleMap.Keys.Min();
        IdTitle = IdTitle > minId ? IdTitle - 1 : Fields.ResourceTitleMap.Keys.Max();
        GetItemMap(IdTitle);
    }

    private void NextPageBtn_OnClick(object sender, RoutedEventArgs e)
    {
        EmptyPicture();
        var maxId = Fields.ResourceTitleMap.Keys.Max();
        IdTitle = IdTitle < maxId ? IdTitle + 1 : Fields.ResourceTitleMap.Keys.Min();
        GetItemMap(IdTitle);
    }

    private void PrevItemBgPageBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateItemBackground()) return;
        var maxPage = Fields.ResourceItemBackground.Count / Settings.numItemInPage;
        PageItemBg = PageItemBg > 0 ? PageItemBg - 1 : maxPage;
        GetItemBackground(PageItemBg);
    }

    private void NextItemBgPageBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateItemBackground()) return;
        var maxPage = Fields.ResourceItemBackground.Count / Settings.numItemInPage;
        PageItemBg = PageItemBg < maxPage ? PageItemBg + 1 : 0;
        GetItemBackground(PageItemBg);
    }

    private async void BgPrevBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccess()) return;
        if (Fields.ResourceBackground.Keys.Count == 0) return;

        var bgId = Fields.ResourceBackground.Keys.Min();
        var currentId = ManagerDrawMap.Instance.BackgroundId;
        ManagerDrawMap.Instance.BackgroundId = currentId > bgId ? currentId - 1 : Fields.ResourceBackground.Keys.Max();
        GetBackGround(ManagerDrawMap.Instance.BackgroundId);
    }

    private async void BgNextBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccess()) return;
        if (Fields.ResourceBackground.Keys.Count == 0) return;

        var bgId = Fields.ResourceBackground.Keys.Max();
        var currentId = ManagerDrawMap.Instance.BackgroundId;
        ManagerDrawMap.Instance.BackgroundId = currentId < bgId ? currentId + 1 : Fields.ResourceBackground.Keys.Min();
        GetBackGround(ManagerDrawMap.Instance.BackgroundId);
    }

    private void CheckBoxLeft_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        HandleCheckBoxChange(Fields.T_LEFT, CheckBoxLeft.IsChecked == true);
    }

    private void CheckBoxRight_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        HandleCheckBoxChange(Fields.T_RIGHT, CheckBoxRight.IsChecked == true);
    }

    private void CheckBoxTop_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        HandleCheckBoxChange(Fields.T_TOP, CheckBoxTop.IsChecked == true);
    }

    private void CheckBoxBottom_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        HandleCheckBoxChange(Fields.T_BOTTOM, CheckBoxBottom.IsChecked == true);
    }

    private void HandleCheckBoxChange(int tileType, bool isChecked)
    {
        if (PictureImageSelected.Source == null) return;

        if (isChecked)
            Fields.AddTileIndex(PictureImageSelected, tileType);
        else
            Fields.RemoveTileIndex(PictureImageSelected, tileType);

        PictureImageSelected.InvalidateVisual();
        PanelItemMap.InvalidateAll();
    }

    private void RotateLeftBtn_OnClick(object sender, RoutedEventArgs e)
    {
        RotateImage(-90);
    }

    private void RotateRightBtn_OnClick(object sender, RoutedEventArgs e)
    {
        RotateImage(90);
    }

    private void FlipHBtn_OnClick(object sender, RoutedEventArgs e)
    {
        FlipImage(true);
    }

    private void FlipVBtn_OnClick(object sender, RoutedEventArgs e)
    {
        FlipImage(false);
    }

    private void RotateImage(int angle)
    {
        if (PictureImageSelected.Source == null) return;
        PictureImageSelected.Source = ((Bitmap)PictureImageSelected.Source).RotateImage(angle);
    }

    private void FlipImage(bool horizontal)
    {
        if (PictureImageSelected.Source == null) return;
        PictureImageSelected.Source = horizontal
            ? ((Bitmap)PictureImageSelected.Source).FlipHorizontal()
            : ((Bitmap)PictureImageSelected.Source).FlipVertical();
    }

    private async void AddNewBtn_OnClick(object sender, RoutedEventArgs e)
    {
        var files = await OpenMultipleFileDialog("Open Image File");
        if (files.Length == 0) return;

        foreach (var t in files)
            try
            {
                await ProcessNewTileFile(t);
            }
            catch
            {
            }
    }

    private void CreateTileId_OnClick(object sender, RoutedEventArgs e)
    {
        var idParent = Fields.ResourceTitleMap.Max(a => a.Key) + 1;
        Fields.ResourceTitleMap.TryAdd(idParent, []);
        PostProcessNewTiles();
        GetItemMap(IdTitle);
    }

    private async Task ProcessNewTileFile(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        var bitmap = new Bitmap(stream);
        var collection = Fields.ResourceTitleMap[IdTitle];
        var id = collection.Count != 0 ? collection.Max(it => it.GetId()) + 1 : 1;
        var info = new PictureCustom
        {
            Source = bitmap,
            Width = TileSize,
            Height = TileSize,
            DataPicture = new FieldsData { Id = id, IdParent = IdTitle },
            TypeBlock = []
        };
        info.PointerPressed += DrawMap.Instance.PicOnPointerPressed;
        Fields.ResourceTitleMap[IdTitle].Add(info);
        CreatePicture(info);
        using var bitNew = bitmap.CreateScaledBitmap(new PixelSize(96, 96));
        bitNew.Save($"{Settings.folderTileMap}//{IdTitle}${id}.png");
    }

    private void PostProcessNewTiles()
    {
        foreach (var i in Fields.ResourceTitleMap.Keys)
            Fields.ResourceTitleMap[i].Sort();

        Fields.TileIndex.Add([]);
        Fields.TileType.Add([]);
    }

    private async void RemoveBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ConfirmDeletion($"Bạn Có Chắc Chắn Muốn Xóa Tile {IdTitle} Không?")) return;

        try
        {
            if (!ValidateTileDeletion()) return;
            await DeleteTile();
            await ShowSuccessMessage(DeleteSuccessMessage);
        }
        catch
        {
            await ShowErrorMessage(DeleteFailedMessage);
        }
    }

    private bool ValidateTileDeletion()
    {
        if (IdTitle <= MaxDefaultTileId)
        {
            _ = ShowErrorMessage("Không Thể Xóa Tile Mặc Định");
            return false;
        }

        if (Fields.ResourceTitleMap.ContainsKey(IdTitle)) return true;
        _ = ShowErrorMessage("Không Tìm Thấy Tile");
        return false;
    }

    private async Task DeleteTile()
    {
        Fields.TileIndex.RemoveAt(IdTitle - 1);
        Fields.TileType.RemoveAt(IdTitle - 1);
        Fields.ResourceTitleMap.TryRemove(IdTitle, out _);

        await ReorganizeResourceTitleMap();

        Function.ClearFolder(Settings.folderTileMap, IdTitle.ToString());
        GetItemMap(IdTitle - 1);
        EmptyPicture();
    }

    private Task ReorganizeResourceTitleMap()
    {
        var updatedDictionary = new ConcurrentDictionary<int, List<PictureCustom>>();
        var newKey = 1;

        foreach (var kvp in Fields.ResourceTitleMap.OrderBy(a => a.Key))
        {
            updatedDictionary.TryAdd(newKey, kvp.Value);
            newKey++;
        }

        Fields.ResourceTitleMap = updatedDictionary;
        return Task.CompletedTask;
    }

    private void DeleteTileBtn_OnClick(object sender, RoutedEventArgs e)
    {
        var info = Fields.ResourceTitleMap[IdTitle].First(a => a.GetId() == PictureImageSelected.GetId());
        Fields.ResourceTitleMap[IdTitle].Remove(info);
        EmptyPicture();
        PanelItemMap.Children.Remove(info);
        File.Delete($"{Settings.folderTileMap}//{info.GetIdParent()}${info.GetId()}.png");
    }

    private void SaveImageBtn_OnClick(object sender, RoutedEventArgs e)
    {
        var info = Fields.ResourceTitleMap[IdTitle].First(a => a.GetId() == PictureImageSelected.GetId());
        if (PictureImageSelected.Source is not Bitmap bitmap) return;

        info.Source = bitmap.CloneBitmap();
        ((Bitmap)info.Source)?.Save($"{Settings.folderTileMap}//{info.GetIdParent()}${info.GetId()}.png");
    }

    private void CreateTileBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (PictureImageSelected.Source == null) return;

        var idMax = Fields.ResourceTitleMap[IdTitle].Max(a => a.GetId());
        var bitmap = (Bitmap)PictureImageSelected.Source;

        var info = new PictureCustom
        {
            DataPicture = new FieldsData { Id = idMax + 1, IdParent = IdTitle },
            Width = TileSize,
            Height = TileSize,
            Source = bitmap.CloneBitmap(),
            TypeBlock = []
        };

        Fields.ResourceTitleMap[IdTitle].Add(info);
        CreatePicture(info);
        bitmap.Save($"{Settings.folderTileMap}//{info.GetIdParent()}${info.GetId()}.png");
    }

    private async void AddBgBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccess()) return;

        var files = await OpenMultipleFileDialog("Open Background File");
        if (files.Length == 0) return;

        if (files.Length > 4)
        {
            await ShowErrorMessage("Chỉ Có Thể Chọn Tối Đa 4 Ảnh");
            return;
        }

        try
        {
            await ProcessBackgroundFiles(files);
        }
        catch
        {
            await ShowErrorMessage("Thêm background thất bại");
        }
    }

    private async Task ProcessBackgroundFiles(IStorageFile[] files)
    {
        if (!Directory.Exists(Settings.folderBackground))
            Directory.CreateDirectory(Settings.folderBackground);

        var bgId = Fields.ResourceBackground.Count == 0
            ? DefaultBackgroundId
            : Fields.ResourceBackground.Keys.Max() + 1;
        PanelBackground.Children.Clear();

        for (var i = 0; i < files.Length; i++) await ProcessSingleBackgroundFile(files[i], bgId, i);

        Fields.ResourceBackground[bgId].Sort();
        GetBackGround(bgId);
    }

    private async Task ProcessSingleBackgroundFile(IStorageFile file, int bgId, int index)
    {
        await using var stream = await file.OpenReadAsync();
        var img = new Bitmap(stream);
        var nameBg = $"b{bgId}{index}";

        var pic = new PictureCustom
        {
            DataPicture = new FieldsData
            {
                BackgroundId = (byte)index,
                BackgroundType = (byte)bgId
            },
            Width = 100,
            Height = 50,
            Source = img,
            TypeBlock = null,
            TypePicture = Enum.Background
        };

        PanelBackground.Children.Add(pic);

        if (!Fields.ResourceBackground.TryGetValue(bgId, out var value))
        {
            value = [];
            Fields.ResourceBackground[bgId] = value;
        }

        value.Add(pic);
        img.Save($"{Settings.folderBackground}//{nameBg}.png");
    }

    private async void RemoveBgBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccess()) return;
        if (!await ConfirmDeletion(
                $"Bạn Có Chắc Chắn Muốn Xóa Background {ManagerDrawMap.Instance.BackgroundId} Không?")) return;

        try
        {
            await RemoveBackground();
            await ShowSuccessMessage(DeleteSuccessMessage);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Xóa Thất Bại\n{ex}");
        }
    }

    private async Task RemoveBackground()
    {
        var currentBgId = ManagerDrawMap.Instance.BackgroundId;

        if (!Fields.ResourceBackground.ContainsKey(currentBgId))
        {
            await ShowErrorMessage("Không Tìm Thấy Background");
            return;
        }

        Function.ClearFolder(Settings.folderBackground);
        Fields.ResourceBackground.TryRemove(currentBgId, out _);

        await ReorganizeBackgrounds();
        GetBackGround(ManagerDrawMap.Instance.BackgroundId);
    }

    private Task ReorganizeBackgrounds()
    {
        var updatedDictionary = new ConcurrentDictionary<int, List<PictureCustom>>();
        var newKey = DefaultBackgroundId;

        foreach (var kvp in Fields.ResourceBackground.OrderBy(a => a.Key))
        {
            foreach (var pic in kvp.Value)
            {
                var nameBg = $"b{newKey}{pic.GetBgId()}";
                pic.DataPicture.BackgroundType = (byte)newKey;
                ((Bitmap)pic.Source!).Save($"{Settings.folderBackground}//{nameBg}.png");
            }

            updatedDictionary.TryAdd(newKey, kvp.Value);
            newKey++;
        }

        Fields.ResourceBackground = updatedDictionary;
        return Task.CompletedTask;
    }

    private void ExportBtn_OnClick(object sender, RoutedEventArgs e)
    {
        ExportDataTileMap();
    }

    private async void ExportBgBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccess()) return;

        ExportBgBtn.IsEnabled = false;
        try
        {
            await ExportDataBackground();
            await ShowSuccessMessage("Xuất Background Thành Công");
        }
        finally
        {
            ExportBgBtn.IsEnabled = true;
        }
    }

    private async void ExportDataItemBg_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccess()) return;

        var result = await new NotificationDialog(_dataNotification[1]).ShowDialog<bool[]>(WindowExecution.Instance);
        if (result == null) return;

        ExportDataItemBgBtn.IsEnabled = false;
        try
        {
            await ExportDataItemBg();
            await ExportItemBackgroundTemplate(result);
        }
        finally
        {
            ExportDataItemBgBtn.IsEnabled = true;
        }
    }

    private async void LoadDataItemBg_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccess()) return;

        var result = await new NotificationDialog(_dataNotification[0]).ShowDialog<bool[]>(WindowExecution.Instance);
        if (result == null) return;

        try
        {
            if (result[0])
                await LoadItemBackgroundFromDatabase();
            else
                await LoadItemBackgroundFromFile();

            await ShowSuccessMessage(LoadSuccessMessage);
            GetItemBackground(0);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Có lỗi xảy ra\n{ex}");
        }
    }

    private async void LoadEffectBtn_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            ClearExistingEffects();

            if (EffectFromImageDataRadio.IsChecked == true)
                await LoadEffectsFromImageAndData();
            else if (EffectFromDataRadio.IsChecked == true)
                await LoadEffectsFromDataOnly();
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Có lỗi không mong muốn: {ex.Message}");
        }
    }

    private async void LoadPartBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ValidatePartLoading()) return;

        var iconsPath = IconsPathTextBox.Text;
        var isJsonArray = JsonArrayRadio.IsChecked == true;
        var isIdDxDyOrder = IdDxDyRadio.IsChecked == true;

        LoadPartBtn.IsEnabled = false;
        try
        {
            await LoadPartNpc(iconsPath, isJsonArray, isIdDxDyOrder);
        }
        finally
        {
            LoadPartBtn.IsEnabled = true;
        }
    }

    private async void LoadMonsterBtn_OnClick(object sender, RoutedEventArgs e)
    {
        LoadMonsterBtn.IsEnabled = false;
        try
        {
            if (!ValidateMonsterLoading()) return;
            await LoadMonsters();
        }
        finally
        {
            LoadMonsterBtn.IsEnabled = true;
        }
    }

    private async void ImportBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccessAndItemBackground()) return;

        var file = await OpenSingleFileDialog("Open Data File");
        if (file == null) return;

        try
        {
            await ImportItemBackground(file);
            await ShowSuccessMessage("Thêm Item Background Thành Công");
            GetItemBackground(PageItemBg);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Có Lỗi Xảy Ra Trong Quá Trình Chọn Image Item Background\n{ex}");
        }
    }

    private async void SaveBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccessAndItemBackground()) return;
        if (_pictureCustomSelected == null) return;

        if (!int.TryParse(OffsetXTextBox.Text, out var offsetX) ||
            !int.TryParse(OffsetYTextBox.Text, out var offsetY)) return;

        _pictureCustomSelected.DataPicture.Dx = offsetX;
        _pictureCustomSelected.DataPicture.Dy = offsetY;
        _pictureCustomSelected.DataPicture.Layer = LayerItemBgComboBox.SelectedIndex + 1;

        await ShowSuccessMessage(SaveSuccessMessage);
    }

    private async void DeleteBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await ValidateVipAccessAndItemBackground()) return;
        if (_pictureCustomSelected == null) return;

        await DeleteItemBackground();
        await ShowSuccessMessage("Xóa Item Background Thành Công");
        GetItemBackground(PageItemBg);
    }

    private void OnTabClick(object sender, RoutedEventArgs e)
    {
        var clickedButton = sender as Button;
        var tabName = clickedButton?.Tag?.ToString();

        ResetTabClasses();
        clickedButton?.Classes.Add("active");

        switch (tabName)
        {
            case "Tile":
                TileTabContent.Classes.Add("active");
                break;
            case "ItemBackground":
                ItemBackgroundTabContent.Classes.Add("active");
                break;
            case "ActorsAndEffects":
                ActorsAndEffectsTabContent.Classes.Add("active");
                break;
        }
    }

    private void ResetTabClasses()
    {
        TileTabBtn.Classes.Remove("active");
        ItemBgTabBtn.Classes.Remove("active");
        ActorsAndEffectsTabBtn.Classes.Remove("active");
        TileTabContent.Classes.Remove("active");
        ItemBackgroundTabContent.Classes.Remove("active");
        ActorsAndEffectsTabContent.Classes.Remove("active");
    }

    private void OffsetNumeric_OnKeyDown(object sender, KeyEventArgs e)
    {
        var isNumeric = e.Key is >= Key.D0 and <= Key.D9 or
            >= Key.NumPad0 and <= Key.NumPad9 or
            Key.OemMinus or Key.Subtract;
        if (!isNumeric) e.Handled = true;
    }

    private void OffsetTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        PanelViewItemBg.InvalidateVisual();
    }
}
