using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ToolEffectNro.ChildWindows;
using ToolEffectNro.Classes;

namespace ToolEffectNro.Windows;

public partial class WindowExecution : Window
{
    public static WindowExecution Instance;
    public static int XOriginal;
    public static int YOriginal;

    private readonly HttpClient _httpClient;
    public readonly FormFilmEffect FilmEffect;
    public readonly FormMainEffect MainEffect;
    public readonly ManagerCrop ManagerCrop;

    public readonly ManagerEffect ManagerEffect;
    public readonly SideForm SideForm;
    public readonly FormSkillEffect SkillEffect;
    private Window _activeForm;
    private Button _currentSelectedButton;
    public FormArrFrame ArrayFrame;
    public Bitmap ImageChar = ImageHelper.LoadFromResource(new Uri("avares://ToolEffectNro/Assets/Resources/Char.png"));

    public WindowExecution()
    {
        _httpClient = new HttpClient();
        InitializeComponent();
        SideForm = new SideForm();
        MainEffect = new FormMainEffect();
        FilmEffect = new FormFilmEffect();
        SkillEffect = new FormSkillEffect();
        ManagerEffect = new ManagerEffect();
        ManagerCrop = new ManagerCrop();
        ArrayFrame = new FormArrFrame();
        PositionChanged += MainForm_PositionChanged;
        PropertyChanged += (_, args) =>
        {
            if (args.Property == WindowStateProperty || args.Property == IsActiveProperty)
                OnWindowStateChanged(WindowState, GetValue(IsActiveProperty));
        };
        Instance = this;
        ContentPanel.LayoutUpdated += ContentPanel_LayoutUpdated;
    }

    private void ContentPanel_LayoutUpdated(object sender, EventArgs e)
    {
        ContentPanel.LayoutUpdated -= ContentPanel_LayoutUpdated;
        UpdateWindow();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        MainEffect.Close();
        FilmEffect.Close();
        ManagerCrop.Close();
        ManagerEffect.Close();
        SideForm.Close();
        SkillEffect.Close();
        ArrayFrame.Close();
        ImportData.Instance?.Close();
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

    private void UpdateWindow()
    {
        var width = ContentPanel.Bounds.Width;
        var height = ContentPanel.Bounds.Height;
        XOriginal = (int)Math.Round(height * 0.85);
        YOriginal = (int)(width / 2);
        FormSkillEffect.Instance.PanelMain.InvalidateVisual();
        SideForm.Instance.PanelParentFrame.InvalidateAll();
        FormFilmEffect.Instance.MainPanel.InvalidateAll();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ButtonHome_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonHome);
        AddChildForm(MainEffect);
    }

    private void ButtonCropManager_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonCropManager);
        AddChildForm(ManagerCrop);
    }

    private void ButtonFilmEffect_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonFilmEffect);
        AddChildForm(FilmEffect);
    }

    private void ButtonEffectForm_OnClick(object sender, RoutedEventArgs e)
    {
        SetButtonSelected(ButtonEffectForm);
        AddChildForm(SkillEffect);
    }

    private void MainForm_PositionChanged(object sender, EventArgs e)
    {
        if (_activeForm is not FormFilmEffect) return;
        FilmEffect.UpdateLeftDockPosition();
    }


    private void OnWindowStateChanged(WindowState state, bool isActive)
    {
        if (_activeForm is not FormFilmEffect) return;
        if (isActive && SideForm.Instance.IsVisible)
        {
            SideForm.Instance.Topmost = true;
            SideForm.Instance.Topmost = false;
        }

        switch (state)
        {
            case WindowState.Minimized:
                SideForm.Instance.Hide();
                break;
            case WindowState.Normal or WindowState.Maximized:
                if (SideForm.Instance.IsVisible) SideForm.Instance.Show();
                FilmEffect.UpdateLeftDockPosition();
                break;
        }
    }

    #region Functions

    private void AddChildForm(Window user)
    {
        ContentPanel.Children.Clear();
        _activeForm = user;
        var content = user.Content;
        if (content is Control control)
        {
            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            control.VerticalAlignment = VerticalAlignment.Stretch;
            ContentPanel.Children.Add(control);
            if (user is ManagerCrop managerCrop) managerCrop.VisibleChange();
        }

        if (_activeForm is not FormFilmEffect)
            if (SideForm.IsVisible)
                SideForm.Hide();
        if (_activeForm is FormSkillEffect)
        {
            if (!ManagerEffect.IsVisible) ManagerEffect.Show();
        }
        else
        {
            if (ManagerEffect.IsVisible) ManagerEffect.Hide();
            if (ArrayFrame.IsVisible) ArrayFrame.Hide();
        }
    }

    private void SetButtonSelected(Button selectedButton)
    {
        if (_currentSelectedButton != null)
        {
            var previousImage = (Image)_currentSelectedButton.Content!;
            var previousImageName = GetBaseImageName(previousImage.Name!);
            previousImage.Source =
                ImageHelper.LoadFromResource(
                    new Uri($"avares://ToolEffectNro/Assets/Resources/{previousImageName}.png"));
        }

        if (selectedButton == null) return;
        var selectedImage = (Image)selectedButton.Content!;
        var baseImageName = GetBaseImageName(selectedImage.Name!);
        selectedImage.Source =
            ImageHelper.LoadFromResource(
                new Uri($"avares://ToolEffectNro/Assets/Resources/{baseImageName}Selected.png"));
        _currentSelectedButton = selectedButton;
    }

    private static string GetBaseImageName(string imagePath)
    {
        var filename = Path.GetFileNameWithoutExtension(imagePath);
        return filename.EndsWith("Selected") ? filename[..^8] : filename;
    }

    #endregion
}