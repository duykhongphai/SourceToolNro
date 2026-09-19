using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CreateSkillNro.Classes;
using CreateSkillNro.Classes.Enums;
using CreateSkillNro.Options;
using CreateSkillNro.Skills;
using CreateSkillNro.Windows;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CreateSkillNro.ChildWindows;

public partial class FormSkillEffect : Window
{
    #region Constants

    private const int UpdateIntervalMs = 150;
    private const byte MaxEffectImages = byte.MaxValue;
    private const short MaxDartImages = short.MaxValue;
    private const sbyte MaxSkillStands = sbyte.MaxValue;
    private const int StandardImageSize = 130;
    private const int ImageMarginLeft = 18;
    private const int ImageMarginVertical = 5;

    #endregion

    #region Fields

    public static FormSkillEffect Instance;
    private DispatcherTimer _updateTimer;
    private int _idHeadEdit;
    private bool _lastVisibleDartForm;
    private TypeDartEdit _typeDartEdit;
    private TypeDataEnum _typeEditEnum;
    public List<short> EffectSelected = [];
    public int IdSkillStandEdit = -1;

    #endregion

    #region Constructor & Initialization

    public FormSkillEffect()
    {
        InitializeComponent();
        Instance = this;
        InitializeUpdateTimer();
    }

    private void InitializeUpdateTimer()
    {
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(UpdateIntervalMs)
        };
        _updateTimer.Tick += UpdateEffect_Tick;
        _updateTimer.Start();
    }

    #endregion

    #region Timer & Update Logic

    private void UpdateEffect_Tick(object sender, EventArgs e)
    {
        var activeTabUpdater = GetActiveTabUpdater();
        activeTabUpdater?.Invoke();
    }

    private Action GetActiveTabUpdater()
    {
        if (EditEffectTabContent.Classes.Contains("active"))
            return () => UpdatePanelEdits(EffectPanelFrame);
        if (EditSkillTabContent.Classes.Contains("active"))
            return () => UpdatePanelEdits(SkillPanelFrame);
        if (EditDartTabContent.Classes.Contains("active"))
            return () => UpdatePanelEdits(DartPanelFrame);
        return null;
    }

    private void OnSkillTabClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button clickedButton) return;
        var tabName = clickedButton.Tag?.ToString();
        SkillSetupTabBtn.Classes.Remove("active");
        SkillFramesTabBtn.Classes.Remove("active");
        SkillSetupTabContent.Classes.Remove("active");
        SkillFramesTabContent.Classes.Remove("active");
        clickedButton.Classes.Add("active");
        switch (tabName)
        {
            case "Setup":
                SkillSetupTabContent.Classes.Add("active");
                break;
            case "Frames":
                SkillFramesTabContent.Classes.Add("active");
                break;
        }
    }

    private static void UpdatePanelEdits(PanelView wrapPanel)
    {
        wrapPanel.Update();
    }

    #endregion

    #region Tab Management

    private void OnDartTabClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button clickedButton) return;
        var tabName = clickedButton.Tag?.ToString();
        HeadFramesTabBtn.Classes.Remove("active");
        DartFramesTabBtn.Classes.Remove("active");
        HeadFramesTabContent.Classes.Remove("active");
        DartFramesTabContent.Classes.Remove("active");
        clickedButton.Classes.Add("active");
        switch (tabName)
        {
            case "HeadFrames":
                HeadFramesTabContent.Classes.Add("active");
                break;
            case "DartFrames":
                DartFramesTabContent.Classes.Add("active");
                break;
        }
    }

    public void SetHeadFramesVisibility(bool showHeadFrames)
    {
        if (showHeadFrames)
        {
            DartTabHeader.IsVisible = true;
            HeadFramesTabContent.IsVisible = true;
            HeadFramesTabContent.Classes.Add("active");
            DartFramesTabContent.Classes.Remove("active");
            HeadFramesTabBtn.Classes.Add("active");
            DartFramesTabBtn.Classes.Remove("active");
            DartFramesTitle.Text = "Dart Frames";
        }
        else
        {
            DartTabHeader.IsVisible = false;
            HeadFramesTabContent.IsVisible = false;
            DartFramesTabContent.Classes.Add("active");
            DartFramesTitle.Text = "Dart Configuration";
        }
    }

    private void OnTabClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button clickedButton) return;

        var tabName = clickedButton.Tag?.ToString();
        ResetTabClasses();
        clickedButton.Classes.Add("active");

        ActivateTab(tabName);
    }

    private void ActivateTab(string tabName)
    {
        switch (tabName)
        {
            case "EditEffect":
                EditEffectTabContent.Classes.Add("active");
                _typeEditEnum = TypeDataEnum.Effect;
                break;
            case "EditDart":
                EditDartTabContent.Classes.Add("active");
                _typeEditEnum = TypeDataEnum.Dart;
                break;
            case "EditSkill":
                EditSkillTabContent.Classes.Add("active");
                _typeEditEnum = TypeDataEnum.Skill;
                break;
        }
    }

    private void ResetTabClasses()
    {
        var buttons = new[] { EditEffectTabBtn, EditSkillTabBtn, EditDartTabBtn };
        var contents = new[] { EditEffectTabContent, EditSkillTabContent, EditDartTabContent };

        foreach (var button in buttons)
            button.Classes.Remove("active");

        foreach (var content in contents)
            content.Classes.Remove("active");
    }

    public void SwitchTab(TypeDataEnum editType)
    {
        ResetTabClasses();

        switch (editType)
        {
            case TypeDataEnum.Dart:
                ActivateDartTab();
                break;
            case TypeDataEnum.Effect:
                ActivateEffectTab();
                break;
            case TypeDataEnum.Skill:
                ActivateSkillTab();
                break;
        }
    }

    private void ActivateDartTab()
    {
        EditDartTabBtn.Classes.Add("active");
        EditDartTabContent.Classes.Add("active");
        _typeEditEnum = TypeDataEnum.Dart;
        ClearData(DartPanelFrame.TypeData);

        if (FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            LoadDartData(dartInfo);
    }

    private void LoadDartData(DartInfo dartInfo)
    {
        NUpdateTextBox.Text = dartInfo.NUpdate.ToString();
        AngleTextBox.Text = (dartInfo.Va / 256).ToString();
        XdPercentTextBox.Text = dartInfo.XdPercent.ToString();
    }

    private void ActivateEffectTab()
    {
        EditEffectTabBtn.Classes.Add("active");
        EditEffectTabContent.Classes.Add("active");
        _typeEditEnum = TypeDataEnum.Effect;
        ClearData(EffectPanelFrame.TypeData);

        if (FormMainEffect.Instance.EffectCharPaints.TryGetValue(EffectPanelFrame.IdData, out var effectCharPaint))
            LoadEffectImages(effectCharPaint);
    }

    private void LoadEffectImages(EffectCharPaint effectCharPaint)
    {
        foreach (var idImg in effectCharPaint.ArrEffInfo.Select(t => t.IdImg))
            if (FormMainEffect.Instance.Images.TryGetValue(idImg, out var image))
                AddPictureBoxEffect(image);
    }

    private async void ActivateSkillTab()
    {
        EditSkillTabBtn.Classes.Add("active");
        EditSkillTabContent.Classes.Add("active");
        _typeEditEnum = TypeDataEnum.Skill;
        ClearData(SkillPanelFrame.TypeData);
        if (FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skill))
            await LoadSkillImages(skill);
    }

    private async Task LoadSkillImages(SkillPaint skill)
    {
        for (var i = 0; i < skill.SkillStand.Count; i++)
            AddPictureBoxSkill(i);
        await WaitForControlInitialization();
        if (SkillPanelFrame.SkillRender == null)
            SkillPanelFrame.SkillRender = new SkillRender(skill, 1, true, SkillPanelFrame.Bounds.Size);
        else
            SkillPanelFrame.SkillRender.Renew(skill);
        EffectMob.SelectedIndex = skill.EffectHappenOnMob;
    }

    private async Task WaitForControlInitialization()
    {
        while (SkillPanelFrame.Bounds.Width <= 0 || SkillPanelFrame.Bounds.Height <= 0) await Task.Delay(10);
    }

    #endregion

    #region Data Management

    public void ClearData(TypeDataEnum editType, bool isClearPanel = true)
    {
        switch (editType)
        {
            case TypeDataEnum.Dart:
                ClearDartData();
                break;
            case TypeDataEnum.Skill:
                ClearSkillData(isClearPanel);
                break;
            case TypeDataEnum.Effect:
                ClearEffectData();
                break;
        }
    }

    private void ClearDartData()
    {
        PanelImgHead.Children.Clear();
        DartWrapPanel.Children.Clear();
    }

    private void ClearSkillData(bool isClearPanel)
    {
        if (isClearPanel)
            SkillWrapPanel.Children.Clear();

        IdSkillStandEdit = -1;
        ResetEffectControls();
        ResetDartControls();
        CharImageStand.Source = null;
    }

    private void ResetEffectControls()
    {
        Effect1.SelectedIndex = 0;
        Effect2.SelectedIndex = 0;
        Effect3.SelectedIndex = 0;

        Effect1Dx.Text = "";
        Effect1Dy.Text = "";
        Effect2Dx.Text = "";
        Effect2Dy.Text = "";
        Effect3Dx.Text = "";
        Effect3Dy.Text = "";
    }

    private void ResetDartControls()
    {
        DartId.SelectedIndex = 0;
        DartIdDx.Text = "";
        DartIdDy.Text = "";
    }

    private void ClearEffectData()
    {
        EffectSelected.Clear();
        EffectWrapPanel.Children.Clear();
    }

    #endregion

    #region Image Management - Effect

    private async void AddImageEffect_OnClick(object sender, RoutedEventArgs e)
    {
        if (EffectPanelFrame.IsPreview) return;
        if (!await IsLoadedData() || EffectPanelFrame.IdData == -1) return;

        var result = await ShowAddFrameDialog();
        if (result == null) return;

        if (!FormMainEffect.Instance.EffectCharPaints.TryGetValue(EffectPanelFrame.IdData, out var effectCharPaint))
            return;

        if (!await ValidateEffectImageCount(effectCharPaint.ArrEffInfo.Count + result.Count))
            return;

        AddEffectImages(effectCharPaint, result);
    }

    private async Task<List<short>> ShowAddFrameDialog()
    {
        var addFrameForm = new AddFrame();
        return await addFrameForm.ShowDialog<List<short>>(WindowExecution.Instance);
    }

    private async Task<bool> ValidateEffectImageCount(int newCount)
    {
        if (newCount <= MaxEffectImages) return true;

        await ShowMaxImagesError(MaxEffectImages);
        return false;
    }

    private void AddEffectImages(EffectCharPaint effectCharPaint, List<short> imageIds)
    {
        foreach (var imageId in imageIds)
        {
            effectCharPaint.ArrEffInfo.Add(new EffectInfoPaint { IdImg = imageId });
            AddPictureBoxEffect(FormMainEffect.Instance.InfoImage[imageId]);
        }

        EffectPanelFrame.InvalidateVisual();
    }

    private void RemoveImageEffect_OnClick(object sender, RoutedEventArgs e)
    {
        if (EffectPanelFrame.IsPreview) return;

        if (!FormMainEffect.Instance.EffectCharPaints.TryGetValue(EffectPanelFrame.IdData, out var effectCharPaint))
            return;

        RemoveSelectedEffectImages(effectCharPaint);
    }

    private void RemoveSelectedEffectImages(EffectCharPaint effectCharPaint)
    {
        var selectedImages = GetSelectedImageChoosers(EffectWrapPanel);
        var indicesToRemove = GetSelectedIndices(EffectWrapPanel);

        RemoveItemsFromCollection(effectCharPaint.ArrEffInfo, indicesToRemove);
        RemoveImagesFromPanel(EffectWrapPanel, selectedImages);
        ClearSelectionData(indicesToRemove);
        EffectPanelFrame.InvalidateVisual();
    }

    private void ClearSelectionData(List<short> indicesToRemove)
    {
        foreach (var index in indicesToRemove)
            EffectSelected.Remove(index);
    }

    private void AddPictureBoxEffect(ImageInfo imageInfo)
    {
        AddPictureBoxEffect(imageInfo.Image);
    }

    public void AddPictureBoxEffect(Bitmap img)
    {
        var image = CreateImageChooser(img);
        EffectWrapPanel.Children.Add(image);
        image.Selected += HandleEffectImageSelection;
    }

    private void HandleEffectImageSelection(object sender, EventArgs args)
    {
        if (sender is not ImageChooser image) return;

        image.SetSelected(!image.IsSelected);
        var index = (short)EffectWrapPanel.Children.IndexOf(image);

        if (image.IsSelected)
            EffectSelected.Add(index);
        else
            EffectSelected.Remove(index);
        EffectPanelFrame.InvalidateVisual();
    }

    #endregion

    #region Image Management - Dart

    private async void AddDart_OnClick(object sender, RoutedEventArgs e)
    {
        if (DartPanelFrame.IsPreview) return;
        if (!await IsLoadedData()) return;

        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        if (IsDartHeadType())
            await AddDartHeadType(dartInfo);
        else
            await AddDartNonHeadType(dartInfo);
    }

    private bool IsDartHeadType()
    {
        return _typeDartEdit is TypeDartEdit.Head or TypeDartEdit.HeadBorder;
    }

    private async Task AddDartHeadType(DartInfo dartInfo)
    {
        var dataAddHead = GetDartHeadData(dartInfo);
        if (dataAddHead == null) return;

        if (!await ValidateDartImageCount(dataAddHead.Count + 1))
            return;

        AddPictureBoxDartHead(dataAddHead.Count);
        dataAddHead.Add([]);
    }

    private async Task AddDartNonHeadType(DartInfo dartInfo)
    {
        var result = await ShowAddFrameDialog();
        if (result == null) return;

        var dataAdd = GetDartNonHeadData(dartInfo);
        if (dataAdd == null) return;

        if (!await ValidateDartImageCount(dataAdd.Count + result.Count))
            return;

        AddDartNonHeadImages(dataAdd, result);
    }

    private List<List<short>> GetDartHeadData(DartInfo dartInfo)
    {
        return _typeDartEdit switch
        {
            TypeDartEdit.Head => dartInfo.Head,
            TypeDartEdit.HeadBorder => dartInfo.HeadBorder,
            _ => null
        };
    }

    private List<short> GetDartNonHeadData(DartInfo dartInfo)
    {
        return _typeDartEdit switch
        {
            TypeDartEdit.TailBorder => dartInfo.TailBorder,
            TypeDartEdit.Tail => dartInfo.Tail,
            TypeDartEdit.Xd1 => dartInfo.Xd1,
            TypeDartEdit.Xd2 => dartInfo.Xd2,
            _ => null
        };
    }

    private async Task<bool> ValidateDartImageCount(int newCount)
    {
        if (newCount <= MaxDartImages) return true;

        await ShowMaxImagesError(MaxDartImages);
        return false;
    }

    private void AddDartNonHeadImages(List<short> dataAdd, List<short> imageIds)
    {
        foreach (var imageId in imageIds)
        {
            dataAdd.Add(imageId);
            AddPictureBoxDart(FormMainEffect.Instance.InfoImage[imageId]);
        }
    }

    private void AddPictureBoxDart(ImageInfo imageInfo)
    {
        AddPictureBoxDart(imageInfo.Image);
    }

    private void AddPictureBoxDart(Bitmap img)
    {
        var image = CreateImageChooser(img);
        DartWrapPanel.Children.Add(image);
        image.Selected += (sender, args) =>
        {
            if (sender is ImageChooser chooser)
                chooser.SetSelected(!chooser.IsSelected);
        };
    }

    private void AddPictureBoxDartHead(int id)
    {
        var image = CreateImageChooser(null)!;
        image.NumberPaint = id;
        DartWrapPanel.Children.Add(image);
        image.Selected += HandleDartHeadSelection;
    }

    private void HandleDartHeadSelection(object sender, EventArgs args)
    {
        if (sender is not ImageChooser selectedChooser) return;

        DeselectAllDartHeadImages();
        selectedChooser.SetSelected(true);
        _idHeadEdit = selectedChooser.NumberPaint;
        LoadDartHeadImages();
        selectedChooser.InvalidateVisual();
    }

    private void DeselectAllDartHeadImages()
    {
        foreach (var child in DartWrapPanel.Children)
            if (child is ImageChooser chooser && !ReferenceEquals(chooser, EffectSelected))
                chooser.SetSelected(false);
    }

    private void LoadDartHeadImages()
    {
        PanelImgHead.Children.Clear();

        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        var imageIds = GetDartHeadImageIds(dartInfo);
        if (imageIds == null) return;

        foreach (var idImg in imageIds)
            if (FormMainEffect.Instance.Images.TryGetValue(idImg, out var img))
                AddPictureBoxDartPanelHead(img);
    }

    private IEnumerable<short> GetDartHeadImageIds(DartInfo dartInfo)
    {
        return _typeDartEdit switch
        {
            TypeDartEdit.Head when _idHeadEdit < dartInfo.Head.Count =>
                dartInfo.Head[_idHeadEdit],
            TypeDartEdit.HeadBorder when _idHeadEdit < dartInfo.HeadBorder.Count =>
                dartInfo.HeadBorder[_idHeadEdit],
            _ => null
        };
    }

    private void AddPictureBoxDartPanelHead(Bitmap img)
    {
        AddPictureBoxDartPanelHead(new ImageInfo(img));
    }

    private void AddPictureBoxDartPanelHead(ImageInfo img)
    {
        var image = CreateImageChooser(img.Image);
        PanelImgHead.Children.Add(image);
        image.Selected += (sender, args) =>
        {
            if (sender is ImageChooser chooser)
                chooser.SetSelected(!chooser.IsSelected);
        };
    }

    #endregion

    #region Image Management - Skill

    public void AddPictureBoxSkill(int id)
    {
        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint))
            return;

        var image = CreateSkillImageChooser(id, skillPaint);
        SkillWrapPanel.Children.Add(image);
        image.Selected += HandleSkillImageSelection;
    }

    private ImageChooser CreateSkillImageChooser(int id, SkillPaint skillPaint)
    {
        return new ImageChooser
        {
            Width = StandardImageSize,
            Height = StandardImageSize,
            Margin = new Thickness(ImageMarginLeft, ImageMarginVertical, 0, ImageMarginVertical),
            NumberPaint = id,
            IdEffect =
            [
                skillPaint.SkillStand[id].EffS0Id,
                skillPaint.SkillStand[id].EffS1Id,
                skillPaint.SkillStand[id].EffS2Id
            ]
        };
    }

    private void HandleSkillImageSelection(object sender, EventArgs args)
    {
        if (sender is not ImageChooser selectedChooser) return;

        var wasSelected = selectedChooser.IsSelected;
        DeselectAllSkillImages();

        if (wasSelected)
        {
            IdSkillStandEdit = -1;
            ClearData(TypeDataEnum.Skill, false);
        }
        else
        {
            selectedChooser.SetSelected(true);
            LoadSkillData(selectedChooser.NumberPaint);
        }

        selectedChooser.InvalidateVisual();
    }

    private void DeselectAllSkillImages()
    {
        foreach (var child in SkillWrapPanel.Children)
            if (child is ImageChooser chooser)
                chooser.SetSelected(false);
    }

    private void LoadSkillData(int skillStandIndex)
    {
        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint))
            return;
        IdSkillStandEdit = skillStandIndex;
        var skillStand = skillPaint.SkillStand[IdSkillStandEdit];

        LoadSkillStandData(skillStand);
        LoadSkillEffectData(skillStand);
        LoadSkillDartData(skillStand);
    }

    private void LoadSkillStandData(SkillInfoPaint skillStand)
    {
        CharImageStand.Source = WindowExecution.Instance.ImageAction[skillStand.Status];
    }

    private void LoadSkillEffectData(SkillInfoPaint skillStand)
    {
        Effect1.SelectedIndex = skillStand.EffS0Id;
        Effect1Dx.Text = skillStand.E0dx.ToString();
        Effect1Dy.Text = skillStand.E0dy.ToString();

        Effect2.SelectedIndex = skillStand.EffS1Id;
        Effect2Dx.Text = skillStand.E1dx.ToString();
        Effect2Dy.Text = skillStand.E1dy.ToString();

        Effect3.SelectedIndex = skillStand.EffS2Id;
        Effect3Dx.Text = skillStand.E2dx.ToString();
        Effect3Dy.Text = skillStand.E2dy.ToString();
    }

    private void LoadSkillDartData(SkillInfoPaint skillStand)
    {
        var arrow = skillStand.ArrowId;
        DartId.SelectedIndex = arrow >= 100 ? arrow - 99 : 0;

        DartIdDx.Text = skillStand.Adx.ToString();
        DartIdDy.Text = skillStand.Ady.ToString();
    }

    private async void AddSkill_OnClick(object sender, RoutedEventArgs e)
    {
        if (SkillPanelFrame.IsPreview) return;
        if (!await IsLoadedData()) return;

        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint))
            return;

        var dataAdd = skillPaint.SkillStand;
        if (!await ValidateSkillCount(dataAdd.Count + 1))
            return;
        var count = dataAdd.Count;
        skillPaint.SkillStand.Add(new SkillInfoPaint());
        AddPictureBoxSkill(count);
    }

    private async Task<bool> ValidateSkillCount(int newCount)
    {
        if (newCount <= MaxSkillStands) return true;

        await ShowMaxSkillsError();
        return false;
    }

    #endregion

    #region Helper Methods

    private ImageChooser CreateImageChooser(Bitmap img)
    {
        return new ImageChooser
        {
            Source = img,
            Width = StandardImageSize,
            Height = StandardImageSize,
            Margin = new Thickness(ImageMarginLeft, ImageMarginVertical, 0, ImageMarginVertical)
        };
    }

    private List<ImageChooser> GetSelectedImageChoosers(Panel panel)
    {
        return panel.Children
            .OfType<ImageChooser>()
            .Where(chooser => chooser.IsSelected)
            .ToList();
    }

    private List<short> GetSelectedIndices(Panel panel)
    {
        var indices = new List<short>();
        for (short i = 0; i < panel.Children.Count; i++)
            if (panel.Children[i] is ImageChooser { IsSelected: true })
                indices.Add(i);
        return indices;
    }

    private static void RemoveItemsFromCollection<T>(IList<T> collection, List<short> indicesToRemove)
    {
        for (var i = indicesToRemove.Count - 1; i >= 0; i--)
        {
            var index = indicesToRemove[i];
            if (index >= 0 && index < collection.Count)
                collection.RemoveAt(index);
        }
    }

    private static void RemoveImagesFromPanel(Panel panel, List<ImageChooser> imagesToRemove)
    {
        foreach (var image in imagesToRemove)
            panel.Children.Remove(image);
    }

    private async Task<bool> IsLoadedData()
    {
        var instance = FormMainEffect.Instance;
        var hasAllData = !string.IsNullOrEmpty(instance.PathFileNrDart) &&
                         !string.IsNullOrEmpty(instance.PathFileNrEffect) &&
                         !string.IsNullOrEmpty(instance.PathFileNrSkill);

        if (hasAllData) return true;

        await ShowErrorAsync("Thông Báo", "Vui Lòng Load Dữ Liệu");
        return false;
    }

    private async Task ShowErrorAsync(string title, string message)
    {
        try
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                title, message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await messageBox.ShowAsync();
        }
        catch
        {
        }
    }

    private async Task ShowMaxImagesError(int maxCount)
    {
        await MessageBoxManager.GetMessageBoxStandard(
            "Lỗi", $"Chỉ có thể thêm tối đa {maxCount} ảnh",
            ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
    }

    private async Task ShowMaxSkillsError()
    {
        await MessageBoxManager.GetMessageBoxStandard(
            "Lỗi", $"Chỉ có thể thêm tối đa {MaxSkillStands}",
            ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
    }

    #endregion

    #region Event Handlers - Input

    private void EffectMob_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var chill);
        if (chill == null) return;
        chill.EffectHappenOnMob = (short)EffectMob.SelectedIndex;
    }

    private void InputElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.C:
                HandleCharacterDisplay();
                break;
            case Key.F5:
                HandlePreviewToggle();
                break;
        }
    }

    private void HandleCharacterDisplay()
    {
        switch (_typeEditEnum)
        {
            case TypeDataEnum.Skill:
                SkillPanelFrame.ShowChar();
                break;
            case TypeDataEnum.Effect:
                EffectPanelFrame.ShowChar();
                break;
        }
    }

    private void HandlePreviewToggle()
    {
        switch (_typeEditEnum)
        {
            case TypeDataEnum.Skill:
                SkillPanelFrame.StartPreview();
                break;
            case TypeDataEnum.Dart:
                HandleDartPreview();
                break;
            case TypeDataEnum.Effect:
                EffectPanelFrame.StartPreview();
                break;
        }
    }

    private void HandleDartPreview()
    {
        DartPanelFrame.StartPreview();

        if (DartPanelFrame.IsPreview)
        {
            _lastVisibleDartForm = HeadFramesTabContent.IsVisible;
            SetHeadFramesVisibility(false);
        }
        else
        {
            SetHeadFramesVisibility(_lastVisibleDartForm);
        }
    }

    private void NumericTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        var isNumeric = e.Key is >= Key.D0 and <= Key.D9 or
            >= Key.NumPad0 and <= Key.NumPad9 or
            Key.OemMinus or Key.Subtract;

        if (!isNumeric)
            e.Handled = true;
    }

    #endregion

    #region Event Handlers - Dart Controls

    private void ActiveDartButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DartPanelFrame.IsPreview) return;
        if (sender is not Button button || button.Tag == null) return;
        if (DartPanelFrame.IdData == -1) return;

        _typeDartEdit = (TypeDartEdit)button.Tag;
        PrepareForDartEdit();

        if (FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            LoadDartEditData(dartInfo);
    }

    private void PrepareForDartEdit()
    {
        DartWrapPanel.Children.Clear();
        SetHeadFramesVisibility(false);
    }

    private void LoadDartEditData(DartInfo dartInfo)
    {
        switch (_typeDartEdit)
        {
            case TypeDartEdit.HeadBorder:
                LoadDartHeadBorderData(dartInfo);
                break;
            case TypeDartEdit.Head:
                LoadDartHeadData(dartInfo);
                break;
            case TypeDartEdit.TailBorder:
                LoadDartTailImages(dartInfo.TailBorder);
                break;
            case TypeDartEdit.Tail:
                LoadDartTailImages(dartInfo.Tail);
                break;
            case TypeDartEdit.Xd1:
                LoadDartTailImages(dartInfo.Xd1);
                break;
            case TypeDartEdit.Xd2:
                LoadDartTailImages(dartInfo.Xd2);
                break;
        }
    }

    private void LoadDartHeadBorderData(DartInfo dartInfo)
    {
        LoadDartHeadFrames(dartInfo.HeadBorder.Count);
    }

    private void LoadDartHeadData(DartInfo dartInfo)
    {
        LoadDartHeadFrames(dartInfo.Head.Count);
    }

    private void LoadDartHeadFrames(int count)
    {
        PanelImgHead.Children.Clear();
        SetHeadFramesVisibility(true);
        for (var i = 0; i < count; i++)
            AddPictureBoxDartHead(i);
    }

    private void LoadDartTailImages(List<short> imageIds)
    {
        foreach (var idImg in imageIds)
            if (FormMainEffect.Instance.Images.TryGetValue(idImg, out var image))
                AddPictureBoxDart(image);
    }

    private void NUpdateTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        if (short.TryParse(NUpdateTextBox.Text, out var result))
            dartInfo.NUpdate = result;
    }

    private void AngleTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        if (int.TryParse(AngleTextBox.Text, out var result))
            dartInfo.Va = result * 256;
    }

    private void XdPercentTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        if (short.TryParse(XdPercentTextBox.Text, out var result))
            dartInfo.XdPercent = result;
    }

    #endregion

    #region Event Handlers - Dart Head Management

    private async void AddDartHead_OnClick(object sender, RoutedEventArgs e)
    {
        if (DartPanelFrame.IsPreview) return;
        if (!await IsLoadedData()) return;

        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        var result = await ShowAddFrameDialog();
        if (result == null) return;

        var dataAdd = GetCurrentDartHeadData(dartInfo);
        if (dataAdd == null) return;

        if (!await ValidateDartImageCount(dataAdd.Count + result.Count))
            return;

        AddDartHeadImages(dataAdd, result);
    }

    private List<short> GetCurrentDartHeadData(DartInfo dartInfo)
    {
        return _typeDartEdit switch
        {
            TypeDartEdit.Head when _idHeadEdit < dartInfo.Head.Count =>
                dartInfo.Head[_idHeadEdit],
            TypeDartEdit.HeadBorder when _idHeadEdit < dartInfo.HeadBorder.Count =>
                dartInfo.HeadBorder[_idHeadEdit],
            _ => null
        };
    }

    private void AddDartHeadImages(List<short> dataAdd, List<short> imageIds)
    {
        foreach (var imageId in imageIds)
        {
            dataAdd.Add(imageId);
            AddPictureBoxDartPanelHead(FormMainEffect.Instance.InfoImage[imageId]);
        }
    }

    private void RemoveDartHead_OnClick(object sender, RoutedEventArgs e)
    {
        if (DartPanelFrame.IsPreview) return;

        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        var dataRemove = GetCurrentDartHeadData(dartInfo);
        if (dataRemove == null) return;

        var selectedImages = GetSelectedImageChoosers(PanelImgHead);
        var indicesToRemove = GetSelectedIndices(PanelImgHead);

        RemoveItemsFromCollection(dataRemove, indicesToRemove);
        RemoveImagesFromPanel(PanelImgHead, selectedImages);
    }

    private void RemoveDart_OnClick(object sender, RoutedEventArgs e)
    {
        if (DartPanelFrame.IsPreview) return;

        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        if (IsDartHeadType())
            RemoveDartHeadType(dartInfo);
        else
            RemoveDartNonHeadType(dartInfo);
    }

    private void RemoveDartHeadType(DartInfo dartInfo)
    {
        var dataRemoveHead = GetDartHeadData(dartInfo);
        if (dataRemoveHead == null) return;

        DartWrapPanel.Children.RemoveAt(_idHeadEdit);
        PanelImgHead.Children.Clear();
        dataRemoveHead.RemoveAt(_idHeadEdit);
        UpdateDartHeadIndices();
        _idHeadEdit = -1;
    }

    private void UpdateDartHeadIndices()
    {
        for (short i = 0; i < DartWrapPanel.Children.Count; i++)
            if (DartWrapPanel.Children[i] is ImageChooser chooser)
                chooser.NumberPaint = i;
    }

    private void RemoveDartNonHeadType(DartInfo dartInfo)
    {
        var dataRemove = GetDartNonHeadData(dartInfo);
        if (dataRemove == null) return;

        var selectedImages = GetSelectedImageChoosers(DartWrapPanel);
        var indicesToRemove = GetSelectedIndices(DartWrapPanel);

        RemoveItemsFromCollection(dataRemove, indicesToRemove);
        RemoveImagesFromPanel(DartWrapPanel, selectedImages);
    }

    #endregion

    #region Event Handlers - Skill Controls

    private void RemoveSkill_OnClick(object sender, RoutedEventArgs e)
    {
        if (SkillPanelFrame.IsPreview) return;

        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint))
            return;
        SkillWrapPanel.Children.RemoveAt(IdSkillStandEdit);
        skillPaint.SkillStand.RemoveAt(IdSkillStandEdit);
        UpdateSkillIndices();
        IdSkillStandEdit = -1;
    }

    private void UpdateSkillIndices()
    {
        for (short i = 0; i < SkillWrapPanel.Children.Count; i++)
            if (SkillWrapPanel.Children[i] is ImageChooser chooser)
                chooser.NumberPaint = i;
    }

    private void Effect1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSkillEffect(skillStand => skillStand.EffS0Id = (short)Effect1.SelectedIndex);
    }

    private void Effect2_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSkillEffect(skillStand => skillStand.EffS1Id = (short)Effect2.SelectedIndex);
    }

    private void Effect3_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSkillEffect(skillStand => skillStand.EffS2Id = (short)Effect3.SelectedIndex);
    }

    private void UpdateSkillEffect(Action<SkillInfoPaint> updateAction)
    {
        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint))
            return;

        if (IdSkillStandEdit == -1 || IdSkillStandEdit >= skillPaint.SkillStand.Count)
            return;
        updateAction(skillPaint.SkillStand[IdSkillStandEdit]);
        UpdateEffectIdImage(skillPaint);
    }

    private void UpdateEffectIdImage(SkillPaint skillPaint)
    {
        foreach (var image in SkillWrapPanel.Children)
            if (image is ImageChooser chooser && chooser.NumberPaint == IdSkillStandEdit)
            {
                chooser.SetIdEffect([
                    skillPaint.SkillStand[IdSkillStandEdit].EffS0Id, skillPaint.SkillStand[IdSkillStandEdit].EffS1Id,
                    skillPaint.SkillStand[IdSkillStandEdit].EffS2Id
                ]);
                chooser.InvalidateVisual();
            }
    }

    private void DartId_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSkillEffect(skillStand =>
            skillStand.ArrowId = (short)(short.Parse(DartId.SelectedItem?.ToString() ?? "0") + 100));
    }

    private void Effect1Dx_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSkillCoordinate(Effect1Dx.Text, (skillStand, value) => skillStand.E0dx = value);
    }

    private void Effect1Dy_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSkillCoordinate(Effect1Dy.Text, (skillStand, value) => skillStand.E0dy = value);
    }

    private void Effect2Dx_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSkillCoordinate(Effect2Dx.Text, (skillStand, value) => skillStand.E1dx = value);
    }

    private void Effect2Dy_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSkillCoordinate(Effect2Dy.Text, (skillStand, value) => { skillStand.E1dy = value; });
    }

    private void Effect3Dx_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSkillCoordinate(Effect3Dx.Text, (skillStand, value) => skillStand.E2dx = value);
    }

    private void Effect3Dy_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSkillCoordinate(Effect3Dy.Text, (skillStand, value) => skillStand.E2dy = value);
    }

    private void UpdateSkillCoordinate(string text, Action<SkillInfoPaint, short> updateAction)
    {
        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint))
            return;
        if (IdSkillStandEdit == -1 || IdSkillStandEdit >= skillPaint.SkillStand.Count)
            return;
        if (short.TryParse(text, out var result))
            updateAction(skillPaint.SkillStand[IdSkillStandEdit], result);
        
    }

    private void DartIdDx_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSkillDartPosition(DartIdDx.Text, (skillStand, value) => skillStand.Adx = value);
    }

    private void DartIdDy_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSkillDartPosition(DartIdDy.Text, (skillStand, value) => skillStand.Ady = value);
    }

    private void UpdateSkillDartPosition(string text, Action<SkillInfoPaint, short> updateAction)
    {
        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint))
            return;

        if (IdSkillStandEdit == -1 || IdSkillStandEdit >= skillPaint.SkillStand.Count)
            return;

        if (!short.TryParse(text, out var result))
            return;

        updateAction(skillPaint.SkillStand[IdSkillStandEdit], result);
        UpdateDartPreviewPosition(skillPaint.SkillStand[IdSkillStandEdit]);
    }

    private void UpdateDartPreviewPosition(SkillInfoPaint skillStand)
    {
        var x = (short)(SkillPanelFrame.Bounds.Width / 2 + skillStand.Adx);
        var y = (short)(SkillPanelFrame.Bounds.Height / 1.2 + skillStand.Ady * 8.5);
        SkillPanelFrame.SkillRender?.SkillDartPreview?.UpdatePos(x, y);
    }

    #endregion

    #region Menu Events & Movement Operations

    private void EffectMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        var hasSelection = EffectSelected.Count > 0;
        MoveUpEffect.IsVisible = hasSelection;
        MoveDownEffect.IsVisible = hasSelection;
        RemoveImageEffect.IsVisible = hasSelection;
    }

    private void MoveUpEffect_OnClick(object sender, RoutedEventArgs e)
    {
        if (EffectPanelFrame.IsPreview) return;

        var validIndices = EffectSelected.Where(index => index > 0).OrderBy(x => x).ToList();
        MoveEffectItems(validIndices, -1);
    }

    private void MoveDownEffect_OnClick(object sender, RoutedEventArgs e)
    {
        if (EffectPanelFrame.IsPreview) return;

        var validIndices = EffectSelected
            .Where(index => index < EffectWrapPanel.Children.Count - 1)
            .OrderByDescending(x => x).ToList();
        MoveEffectItems(validIndices, 1);
    }

    private void MoveEffectItems(List<short> indices, int direction)
    {
        foreach (var index in indices)
        {
            SwapEffect(index, index + direction);
            EffectSelected.Remove(index);
            EffectSelected.Add((short)(index + direction));
        }
    }

    private void SwapEffect(int from, int to)
    {
        if (!IsValidSwapRange(from, to, EffectWrapPanel.Children.Count))
            return;

        if (!FormMainEffect.Instance.EffectCharPaints.TryGetValue(EffectPanelFrame.IdData, out var effectCharPaint))
            return;

        SwapCollectionItems(effectCharPaint.ArrEffInfo, from, to);
        SwapPanelChildren(EffectWrapPanel, from, to);
    }

    private void SkillMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        var hasSelection = IdSkillStandEdit != -1;
        CloneSkill.IsVisible = hasSelection;
        MoveUpSkill.IsVisible = hasSelection;
        MoveDownSkill.IsVisible = hasSelection;
        RemoveSkill.IsVisible = hasSelection;
    }

    private void MoveUpSkill_OnClick(object sender, RoutedEventArgs e)
    {
        if (CanMoveSkill(-1))
        {
            SwapSkill(IdSkillStandEdit, IdSkillStandEdit - 1);
            IdSkillStandEdit -= 1;
        }
    }

    private void MoveDownSkill_OnClick(object sender, RoutedEventArgs e)
    {
        if (CanMoveSkill(1))
        {
            SwapSkill(IdSkillStandEdit, IdSkillStandEdit + 1);
            IdSkillStandEdit += 1;
        }
    }

    private bool CanMoveSkill(int direction)
    {
        if (SkillPanelFrame.IsPreview || IdSkillStandEdit == -1)
            return false;

        var newIndex = IdSkillStandEdit + direction;
        return newIndex >= 0 && newIndex < SkillWrapPanel.Children.Count;
    }

    private void SwapSkill(int from, int to)
    {
        if (!IsValidSwapRange(from, to, SkillWrapPanel.Children.Count))
            return;

        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint))
            return;

        SwapCollectionItems(skillPaint.SkillStand, from, to);
        SwapSkillImageNumbers(from, to);
        SwapPanelChildren(SkillWrapPanel, from, to);
    }

    private void SwapSkillImageNumbers(int from, int to)
    {
        var children = SkillWrapPanel.Children.OfType<ImageChooser>().ToList();
        if (from < children.Count && to < children.Count)
            (children[from].NumberPaint, children[to].NumberPaint) =
                (children[to].NumberPaint, children[from].NumberPaint);
    }

    private static bool IsValidSwapRange(int from, int to, int maxCount)
    {
        return from >= 0 && from < maxCount &&
               to >= 0 && to < maxCount &&
               from != to;
    }

    private static void SwapCollectionItems<T>(IList<T> collection, int from, int to)
    {
        (collection[from], collection[to]) = (collection[to], collection[from]);
    }

    private static void SwapPanelChildren(Panel panel, int from, int to)
    {
        var children = panel.Children.ToList();
        (children[from], children[to]) = (children[to], children[from]);

        panel.Children.Clear();
        foreach (var child in children)
            panel.Children.Add(child);
    }

    private void DartHeadMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        var hasSelection = PanelImgHead.Children.OfType<ImageChooser>().Any(it => it.IsSelected);
        MoveUpDartHead.IsVisible = hasSelection;
        MoveDownDartHead.IsVisible = hasSelection;
        RemoveDartHead.IsVisible = hasSelection;
    }

    private void MoveUpDartHead_OnClick(object sender, RoutedEventArgs e)
    {
        MoveDartHeadItems(-1);
    }

    private void MoveDownDartHead_OnClick(object sender, RoutedEventArgs e)
    {
        MoveDartHeadItems(1);
    }

    private void MoveDartHeadItems(int direction)
    {
        if (DartPanelFrame.IsPreview) return;

        var selectedImages = PanelImgHead.Children.OfType<ImageChooser>().Where(img => img.IsSelected).ToList();
        var indices = selectedImages.Select(image => PanelImgHead.Children.IndexOf(image)).ToList();

        var validIndices = direction < 0
            ? indices.Where(index => index > 0).OrderBy(index => index).ToList()
            : indices.Where(index => index < PanelImgHead.Children.Count - 1).OrderByDescending(index => index)
                .ToList();

        foreach (var index in validIndices)
            SwapDartHead(index, index + direction);
    }

    private void SwapDartHead(int from, int to)
    {
        if (!IsValidSwapRange(from, to, PanelImgHead.Children.Count))
            return;

        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        SwapDartHeadData(dartInfo, from, to);
        SwapPanelChildren(PanelImgHead, from, to);
    }

    private void SwapDartHeadData(DartInfo dartInfo, int from, int to)
    {
        if (_typeDartEdit == TypeDartEdit.Head)
        {
            var headData = dartInfo.Head;
            if (_idHeadEdit >= 0 && _idHeadEdit < headData.Count &&
                from < headData[_idHeadEdit].Count && to < headData[_idHeadEdit].Count)
                SwapCollectionItems(headData[_idHeadEdit], from, to);
        }
        else if (_typeDartEdit == TypeDartEdit.HeadBorder)
        {
            var headBorderData = dartInfo.HeadBorder;
            if (_idHeadEdit >= 0 && _idHeadEdit < headBorderData.Count &&
                from < headBorderData[_idHeadEdit].Count && to < headBorderData[_idHeadEdit].Count)
                SwapCollectionItems(headBorderData[_idHeadEdit], from, to);
        }
    }

    private void DartMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        var hasSelection = DartWrapPanel.Children.OfType<ImageChooser>().Any(it => it.IsSelected);
        MoveUpDart.IsVisible = hasSelection;
        MoveDownDart.IsVisible = hasSelection;
        RemoveDart.IsVisible = hasSelection;
    }

    private void MoveUpDart_OnClick(object sender, RoutedEventArgs e)
    {
        MoveDartItems(-1);
    }

    private void MoveDownDart_OnClick(object sender, RoutedEventArgs e)
    {
        MoveDartItems(1);
    }

    private void MoveDartItems(int direction)
    {
        if (DartPanelFrame.IsPreview) return;

        var selectedImages = DartWrapPanel.Children.OfType<ImageChooser>().Where(img => img.IsSelected).ToList();
        var indices = selectedImages.Select(image => DartWrapPanel.Children.IndexOf(image)).ToList();

        var validIndices = direction < 0
            ? indices.Where(index => index > 0).OrderBy(index => index).ToList()
            : indices.Where(index => index < DartWrapPanel.Children.Count - 1).OrderByDescending(index => index)
                .ToList();

        foreach (var index in validIndices)
            SwapDart(index, index + direction);
    }

    private void SwapDart(int from, int to)
    {
        if (!IsValidSwapRange(from, to, DartWrapPanel.Children.Count))
            return;

        if (!FormMainEffect.Instance.DartInfos.TryGetValue(DartPanelFrame.IdData, out var dartInfo))
            return;

        SwapDartData(dartInfo, from, to);
        SwapPanelChildren(DartWrapPanel, from, to);
    }

    private void SwapDartData(DartInfo dartInfo, int from, int to)
    {
        switch (_typeDartEdit)
        {
            case TypeDartEdit.TailBorder:
                SwapCollectionItems(dartInfo.TailBorder, from, to);
                break;
            case TypeDartEdit.Tail:
                SwapCollectionItems(dartInfo.Tail, from, to);
                break;
            case TypeDartEdit.Xd1:
                SwapCollectionItems(dartInfo.Xd1, from, to);
                break;
            case TypeDartEdit.Xd2:
                SwapCollectionItems(dartInfo.Xd2, from, to);
                break;
        }
    }

    #endregion

    #region Special Event Handlers

    private async void CharStandBorder_DoubleTapped(object sender, RoutedEventArgs e)
    {
        var selectionWindow = new ImageSelectionWindow(WindowExecution.Instance.ImageAction, "CharStand");
        selectionWindow.ImageSelected += HandleCharStandImageSelected;

        if (WindowExecution.Instance is Window parentWindow)
            await selectionWindow.ShowDialog(parentWindow);
    }

    private void HandleCharStandImageSelected(object sender, ImageSelectedEventArgs e)
    {
        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint) ||
            IdSkillStandEdit == -1)
            return;

        skillPaint.SkillStand[IdSkillStandEdit].Status = e.Key;
        CharImageStand.Source = e.Image;
    }

    private void CloneSkill_OnClick(object sender, RoutedEventArgs e)
    {
        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SkillPanelFrame.IdData, out var skillPaint) ||
            IdSkillStandEdit == -1)
            return;
        skillPaint.SkillStand.Add(skillPaint.SkillStand[IdSkillStandEdit].Clone());
        AddPictureBoxSkill(skillPaint.SkillStand.Count - 1);
    }

    #endregion
}