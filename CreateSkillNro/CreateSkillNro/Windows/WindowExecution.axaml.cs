using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CreateSkillNro.ChildWindows;
using CreateSkillNro.Classes;
using MsBox.Avalonia.Enums;

namespace CreateSkillNro.Windows;

public partial class WindowExecution : Window
{
    public static WindowExecution Instance;

    private readonly sbyte[] _actionCharLoad =
    [
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 26, 27, 28, 29, 30,
        31, 32
    ];

    private readonly FormMainEffect _mainEffect;
    private readonly FormSkillEffect _skillEffect;
    private readonly ViewEffect _viewEffect;
    private readonly ViewSkill _viewSkill;
    private Button _currentSelectedButton;
    public Dictionary<sbyte, Bitmap> ImageAction = [];

    public Bitmap ImageChar =
        ImageHelper.LoadFromResource(new Uri("avares://CreateSkillNro/Assets/Resources/ActionChar/0.png"));

    public Bitmap ImageCharInjured =
        ImageHelper.LoadFromResource(new Uri("avares://CreateSkillNro/Assets/Resources/ActionChar/CharInjured.png"));

    public WindowExecution()
    {
        InitializeComponent();
        foreach (var action in _actionCharLoad)
            ImageAction.Add(action,
                ImageHelper.LoadFromResource(
                    new Uri($"avares://CreateSkillNro/Assets/Resources/ActionChar/{action}.png")));
        _mainEffect = new FormMainEffect();
        _skillEffect = new FormSkillEffect();
        _viewEffect = new ViewEffect();
        _viewSkill = new ViewSkill();
        Instance = this;
        ContentPanel.LayoutUpdated += ContentPanel_LayoutUpdated;
    }

    private void ContentPanel_LayoutUpdated(object sender, EventArgs e)
    {
        ContentPanel.LayoutUpdated -= ContentPanel_LayoutUpdated;
        UpdateWindow();
    }

    private void UpdateWindow()
    {
        FormSkillEffect.Instance.EffectPanelFrame.InvalidateVisual();
        FormSkillEffect.Instance.SkillPanelFrame.InvalidateVisual();
        FormSkillEffect.Instance.DartPanelFrame.InvalidateVisual();
        FormSkillEffect.Instance.DartPanelFrame.DartPreview?.Renew(FormSkillEffect.Instance.DartPanelFrame.Bounds);
        FormSkillEffect.Instance.SkillPanelFrame.SkillRender?.Renew(
            FormSkillEffect.Instance.SkillPanelFrame.Bounds.Size);
    }


    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _mainEffect.Close();
        _skillEffect.Close();
        _viewEffect.Close();
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
        UpdateWindow();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ButtonHome_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonHome);
        AddChildForm(_mainEffect);
    }

    private void ButtonEffectForm_OnClick(object sender, RoutedEventArgs e)
    {
        HandleEffectForm();
    }

    public void HandleEffectForm()
    {
        SetButtonSelected(ButtonEffectForm);
        AddChildForm(_skillEffect);
    }

    private void ButtonView_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonView);
        AddChildForm(_viewEffect);
    }

    private void ButtonFilm_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonFilmForm);
        AddChildForm(_viewSkill);
    }

    private void ThemeToggle_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
    }

    #region Functions

    private void AddChildForm(Window user)
    {
        ContentPanel.Children.Clear();
        var content = user.Content;
        if (content is not Control control) return;
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        control.VerticalAlignment = VerticalAlignment.Stretch;
        control.Focusable = true;
        control.IsTabStop = true;
        ContentPanel.Children.Add(control);
        Dispatcher.UIThread.Post(() => control.Focus());
    }

    private void SetButtonSelected(Button selectedButton)
    {
        if (_currentSelectedButton != null)
        {
            var previousImage = (Image)_currentSelectedButton.Content!;
            var previousImageName = GetBaseImageName(previousImage.Name!);
            previousImage.Source =
                ImageHelper.LoadFromResource(
                    new Uri($"avares://CreateSkillNro/Assets/Resources/{previousImageName}.png"));
        }

        if (selectedButton == null) return;
        var selectedImage = (Image)selectedButton.Content!;
        var baseImageName = GetBaseImageName(selectedImage.Name!);
        selectedImage.Source =
            ImageHelper.LoadFromResource(
                new Uri($"avares://CreateSkillNro/Assets/Resources/{baseImageName}Selected.png"));
        _currentSelectedButton = selectedButton;
    }

    private static string GetBaseImageName(string imagePath)
    {
        var filename = Path.GetFileNameWithoutExtension(imagePath);
        return filename.EndsWith("Selected") ? filename[..^8] : filename;
    }

    #endregion
}