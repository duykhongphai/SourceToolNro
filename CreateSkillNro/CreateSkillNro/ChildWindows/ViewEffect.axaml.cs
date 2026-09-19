using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CreateSkillNro.Classes.Enums;
using CreateSkillNro.Options;
using CreateSkillNro.Skills;
using CreateSkillNro.Windows;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CreateSkillNro.ChildWindows;

public partial class ViewEffect : Window
{
    private const int UpdateIntervalMs = 200;

    private readonly DispatcherTimer _updateTimer;

    public ViewEffect()
    {
        InitializeComponent();
        DataContext = this;
        Instance = this;
        _updateTimer = CreateUpdateTimer();
        _updateTimer.Tick += UpdateEffect_Tick;
        _updateTimer.Start();
    }

    public PanelView SelectedItem { get; set; }
    public static ViewEffect Instance { get; private set; }

    #region Timer and Update Logic

    private DispatcherTimer CreateUpdateTimer()
    {
        return new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(UpdateIntervalMs)
        };
    }

    private void UpdateEffect_Tick(object sender, EventArgs e)
    {
        var activeTabUpdater = GetActiveTabUpdater();
        activeTabUpdater?.Invoke();
    }

    private Action GetActiveTabUpdater()
    {
        if (ViewEffectTabContent.Classes.Contains("active"))
            return () => UpdatePanelViews(EffectWrapPanel);

        if (ViewSkillTabContent.Classes.Contains("active"))
            return () => UpdatePanelViews(SkillWrapPanel);

        if (ViewDartTabContent.Classes.Contains("active"))
            return () => UpdatePanelViews(DartWrapPanel);

        return null;
    }

    private static void UpdatePanelViews(Panel wrapPanel)
    {
        foreach (var child in wrapPanel.Children.OfType<PanelView>()) child.Update();
    }

    #endregion

    #region Tab Management

    private void OnTabClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button clickedButton) return;

        var tabName = clickedButton.Tag?.ToString();
        SwitchToTab(clickedButton, tabName);
    }

    private void SwitchToTab(Button clickedButton, string tabName)
    {
        ResetAllTabClasses();
        clickedButton.Classes.Add("active");

        var tabContent = GetTabContentByName(tabName);
        tabContent?.Classes.Add("active");
    }

    private Control GetTabContentByName(string tabName)
    {
        return tabName switch
        {
            "ViewEffect" => ViewEffectTabContent,
            "ViewDart" => ViewDartTabContent,
            "ViewSkill" => ViewSkillTabContent,
            _ => null
        };
    }

    private void ResetAllTabClasses()
    {
        var tabButtons = new[] { ViewEffectTabBtn, ViewSkillTabBtn, ViewDartTabBtn };
        var tabContents = new[] { ViewEffectTabContent, ViewSkillTabContent, ViewDartTabContent };

        foreach (var button in tabButtons)
            button.Classes.Remove("active");

        foreach (var content in tabContents)
            content.Classes.Remove("active");
    }

    #endregion

    #region Effect Management

    private async void AddEffect_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await IsLoadedData()) return;
        var effectId = GetNextAvailableId(FormMainEffect.Instance.EffectCharPaints);
        var effectCharPaint = CreateNewEffect(effectId);

        FormMainEffect.Instance.EffectCharPaints.TryAdd(effectId, effectCharPaint);
        EffectWrapPanel.Children.Add(effectCharPaint.PanelView);
    }

    private EffectCharPaint CreateNewEffect(short effectId)
    {
        return new EffectCharPaint
        {
            PanelView = FormMainEffect.Instance.CreateEffectPanelView(effectId),
            ArrEffInfo = []
        };
    }

    private EffectCharPaint CloneEffect(short effectId)
    {
        if (!FormMainEffect.Instance.EffectCharPaints.TryGetValue(SelectedItem.IdData, out var effect)) return null;
        return new EffectCharPaint
        {
            PanelView = FormMainEffect.Instance.CreateEffectPanelView(effectId),
            ArrEffInfo = effect.ArrEffInfo.Select(p => p.Clone()).ToList()
        };
    }

    private async void RemoveEffect_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await IsLoadedData()) return;
        if (!IsValidEffectSelection()) return;

        if (await IsEffectInUseBySkills())
        {
            await ShowErrorAsync("Thông Báo", "Không thể xóa effect vì có skill đang sử dụng");
            return;
        }

        await RemoveEffectAndReorganize();
        FormSkillEffect.Instance.ClearData(TypeDataEnum.Effect);
    }

    private bool IsValidEffectSelection()
    {
        return SelectedItem is { TypeData: TypeDataEnum.Effect };
    }

    private Task<bool> IsEffectInUseBySkills()
    {
        return Task.FromResult(FormMainEffect.Instance.SkillPaints.Values
            .Where(skillPaint => skillPaint.SkillStand != null)
            .SelectMany(skillPaint => skillPaint.SkillStand)
            .Any(skill => IsEffectUsedInSkill(skill, SelectedItem.IdData)));
    }

    private static bool IsEffectUsedInSkill(SkillInfoPaint skill, short effectId)
    {
        return skill.EffS0Id == effectId ||
               skill.EffS1Id == effectId ||
               skill.EffS2Id == effectId;
    }

    private Task RemoveEffectAndReorganize()
    {
        EffectWrapPanel.Children.Remove(SelectedItem);
        if (FormMainEffect.Instance.EffectCharPaints.TryGetValue(SelectedItem.IdData, out var effect))
            effect.Dispose();
        FormMainEffect.Instance.EffectCharPaints.TryRemove(SelectedItem.IdData, out _);
        ReorganizeEffects();
        SelectedItem = null;
        return Task.CompletedTask;
    }

    private void ReorganizeEffects()
    {
        var tempEffects = FormMainEffect.Instance.EffectCharPaints.Values.ToList();
        FormMainEffect.Instance.EffectCharPaints.Clear();
        FormMainEffect.Instance.EffectKeys.Clear();
        FormMainEffect.Instance.EffectKeys.Add(-1);
        for (short i = 0; i < tempEffects.Count; i++)
            FormMainEffect.Instance.EffectCharPaints.TryAdd(i, tempEffects[i]);

        UpdateEffectPanelIds();
    }

    private void UpdateEffectPanelIds()
    {
        for (short i = 0; i < EffectWrapPanel.Children.Count; i++)
            if (EffectWrapPanel.Children[i] is PanelView { TypeData: TypeDataEnum.Effect } panel)
                panel.IdData = i;
    }

    private void EditEffect_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsValidEffectSelection()) return;

        EditEffect(SelectedItem.IdData);
        WindowExecution.Instance.HandleEffectForm();
    }

    private static void EditEffect(short effectId)
    {
        var effectFrame = FormSkillEffect.Instance.EffectPanelFrame;
        effectFrame.IdData = effectId;
        effectFrame.InvalidateVisual();
        FormSkillEffect.Instance.SwitchTab(effectFrame.TypeData);
    }

    private async void CloneEffect_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await IsLoadedData()) return;
        if (!IsValidEffectSelection()) return;
        var effectId = GetNextAvailableId(FormMainEffect.Instance.EffectCharPaints);
        var effectCharPaint = CloneEffect(effectId);
        if (effectCharPaint == null) return;
        FormMainEffect.Instance.EffectCharPaints.TryAdd(effectId, effectCharPaint);
        EffectWrapPanel.Children.Add(effectCharPaint.PanelView);
    }

    private void MenuEffect_OnOpened(object sender, RoutedEventArgs e)
    {
        RemoveEffectMenuItem.IsVisible =
            EditEffectMenuItem.IsVisible = CloneEffectMenuItem.IsVisible = IsValidEffectSelection();
    }

    #endregion

    #region Skill Management

    private async void AddSkill_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await IsLoadedData()) return;
        var skillIdString = await ShowCustomInputDialog("Skill ID", "Nhập ID skill:",
            GetNextAvailableId(FormMainEffect.Instance.SkillPaints).ToString());
        if (string.IsNullOrWhiteSpace(skillIdString)) return;
        if (!short.TryParse(skillIdString, out var skillId)) return;
        if (FormMainEffect.Instance.SkillPaints.ContainsKey(skillId))
        {
            await ShowErrorAsync("Thông báo", $"ID '{skillId}' đã tồn tại!");
            return;
        }

        try
        {
            var skillPaint = CreateNewSkill(skillId);
            FormMainEffect.Instance.SkillPaints.TryAdd(skillId, skillPaint);
            SkillWrapPanel.Children.Add(skillPaint.PanelView);
        }
        catch
        {
        }
    }

    private SkillPaint CreateNewSkill(short skillId)
    {
        var skillPaint = new SkillPaint
        {
            Id = skillId,
            SkillStand = [],
            SkillFly = [],
            PanelView = FormMainEffect.Instance.CreateSkillPanelView(skillId)
        };
        skillPaint.SkillRender = new SkillRender(skillPaint, 0.45, false,
            new Size(skillPaint.PanelView.Width, skillPaint.PanelView.Height));
        return skillPaint;
    }

    private SkillPaint CloneNewSkill(short skillId)
    {
        if (!FormMainEffect.Instance.SkillPaints.TryGetValue(SelectedItem.IdData, out var skill)) return null;
        var skillPaint = new SkillPaint
        {
            Id = skillId,
            SkillStand = skill.SkillStand.Select(p => p.Clone()).ToList(),
            SkillFly = [],
            PanelView = FormMainEffect.Instance.CreateSkillPanelView(skillId)
        };
        skillPaint.SkillRender =
            new SkillRender(skillPaint, 0.45, false, new Size(skillPaint.PanelView.Width, skillPaint.PanelView.Height));
        return skillPaint;
    }

    private bool IsValidSkillSelection()
    {
        return SelectedItem is { TypeData: TypeDataEnum.Skill };
    }

    private async void EditSkill_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsValidSkillSelection()) return;
        EditSkill(SelectedItem.IdData);
        WindowExecution.Instance.HandleEffectForm();
        await ViewSkill.Instance.LoadData();
    }

    private static void EditSkill(short skillId)
    {
        var skillFrame = FormSkillEffect.Instance.SkillPanelFrame;
        skillFrame.IdData = skillId;
        skillFrame.InvalidateVisual();
        FormSkillEffect.Instance.SwitchTab(skillFrame.TypeData);
    }

    private void RemoveSkill_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsValidSkillSelection()) return;
        RemoveSkill();
        FormSkillEffect.Instance.ClearData(TypeDataEnum.Skill);
    }

    private void RemoveSkill()
    {
        SkillWrapPanel.Children.Remove(SelectedItem);
        if (FormMainEffect.Instance.SkillPaints.TryGetValue(SelectedItem.IdData, out var skill))
            skill.Dispose();
        FormMainEffect.Instance.SkillPaints.TryRemove(SelectedItem.IdData, out _);
        SelectedItem = null;
    }

    private async void CloneSkill_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await IsLoadedData()) return;
        if (!IsValidSkillSelection()) return;
        var skillIdString = await ShowCustomInputDialog("Skill ID", "Nhập ID skill:",
            GetNextAvailableId(FormMainEffect.Instance.SkillPaints).ToString());
        if (string.IsNullOrWhiteSpace(skillIdString)) return;
        if (!short.TryParse(skillIdString, out var skillId)) return;
        if (FormMainEffect.Instance.SkillPaints.ContainsKey(skillId))
        {
            await ShowErrorAsync("Thông báo", $"ID '{skillId}' đã tồn tại!");
            return;
        }

        var skillPaint = CloneNewSkill(skillId);
        if (skillPaint == null) return;
        FormMainEffect.Instance.SkillPaints.TryAdd(skillId, skillPaint);
        SkillWrapPanel.Children.Add(skillPaint.PanelView);
    }

    private void MenuSkill_OnOpened(object sender, RoutedEventArgs e)
    {
        CloneSkillMenuItem.IsVisible =
            EditSkillMenuItem.IsVisible = RemoveSkillMenuItem.IsVisible = IsValidSkillSelection();
    }

    #endregion

    #region Dart Management

    private async void AddDart_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await IsLoadedData()) return;
        var dartId = GetNextAvailableId(FormMainEffect.Instance.DartInfos);
        var dartInfo = CreateNewDart(dartId);

        FormMainEffect.Instance.DartInfos.TryAdd(dartId, dartInfo);
        DartWrapPanel.Children.Add(dartInfo.PanelView);
    }

    private bool IsValidDartSelection()
    {
        return SelectedItem is { TypeData: TypeDataEnum.Dart };
    }

    private DartInfo CreateNewDart(short dartId)
    {
        var dartInfo = new DartInfo
        {
            PanelView = FormMainEffect.Instance.CreateDartPanelView(dartId),
            Tail = [],
            TailBorder = [],
            Xd1 = [],
            Xd2 = [],
            Head = [],
            HeadBorder = []
        };
        dartInfo.PlayerDart = new PlayerDart(dartInfo, 0.45, 10, (short)(dartInfo.PanelView.Height / 2.0),
            (short)(dartInfo.PanelView.Width - dartInfo.PanelView.Width * 0.28),
            (short)(dartInfo.PanelView.Height / 2));
        return dartInfo;
    }

    private DartInfo CloneNewDart(short dartId)
    {
        if (!FormMainEffect.Instance.DartInfos.TryGetValue(SelectedItem.IdData, out var dart)) return null;
        return new DartInfo
        {
            PanelView = FormMainEffect.Instance.CreateDartPanelView(dartId),
            Tail = [..dart.Tail],
            TailBorder = [..dart.TailBorder],
            Xd1 = [..dart.Xd1],
            Xd2 = [..dart.Xd2],
            Head = [..dart.Head],
            HeadBorder = [..dart.HeadBorder],
            Va = dart.Va,
            NUpdate = dart.NUpdate,
            XdPercent = dart.XdPercent
        };
    }

    private void EditDart_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsValidDartSelection()) return;

        EditDart(SelectedItem.IdData);
        WindowExecution.Instance.HandleEffectForm();
    }

    private static void EditDart(short dartId)
    {
        var dartFrame = FormSkillEffect.Instance.DartPanelFrame;
        dartFrame.IdData = dartId;
        dartFrame.InvalidateVisual();
        FormSkillEffect.Instance.SwitchTab(dartFrame.TypeData);
    }

    private async void RemoveDart_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await IsLoadedData()) return;
        if (!IsValidDartSelection()) return;

        if (await IsDartInUseBySkills())
        {
            await ShowErrorAsync("Thông Báo", "Không thể xóa dart vì có skill đang sử dụng");
            return;
        }

        await RemoveDartAndReorganize();
        FormSkillEffect.Instance.ClearData(TypeDataEnum.Dart);
    }

    private Task<bool> IsDartInUseBySkills()
    {
        return Task.FromResult(FormMainEffect.Instance.SkillPaints.Values
            .Where(skillPaint => skillPaint.SkillStand != null)
            .SelectMany(skillPaint => skillPaint.SkillStand)
            .Any(skill => skill.ArrowId - 100 == SelectedItem.IdData));
    }

    private Task RemoveDartAndReorganize()
    {
        DartWrapPanel.Children.Remove(SelectedItem);

        if (FormMainEffect.Instance.DartInfos.TryGetValue(SelectedItem.IdData, out var dart))
            dart.Dispose();

        FormMainEffect.Instance.DartInfos.TryRemove(SelectedItem.IdData, out _);

        ReorganizeDarts();
        SelectedItem = null;
        return Task.CompletedTask;
    }

    private void ReorganizeDarts()
    {
        var tempDarts = FormMainEffect.Instance.DartInfos.Values.ToList();
        FormMainEffect.Instance.DartInfos.Clear();
        FormMainEffect.Instance.DartKeys.Clear();
        FormMainEffect.Instance.DartKeys.Add(-1);
        for (short i = 0; i < tempDarts.Count; i++)
            FormMainEffect.Instance.DartInfos.TryAdd(i, tempDarts[i]);

        UpdateDartPanelIds();
    }

    private void UpdateDartPanelIds()
    {
        for (short i = 0; i < DartWrapPanel.Children.Count; i++)
            if (DartWrapPanel.Children[i] is PanelView { TypeData: TypeDataEnum.Dart } panel)
                panel.IdData = i;
    }

    private async void CloneDart_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await IsLoadedData()) return;
        if (!IsValidDartSelection()) return;
        var dartId = GetNextAvailableId(FormMainEffect.Instance.DartInfos);
        var dartInfo = CloneNewDart(dartId);
        if (dartInfo == null) return;
        FormMainEffect.Instance.DartInfos.TryAdd(dartId, dartInfo);
        DartWrapPanel.Children.Add(dartInfo.PanelView);
    }

    private void MenuDart_OnOpened(object sender, RoutedEventArgs e)
    {
        CloneDartMenuItem.IsVisible =
            EditDartMenuItem.IsVisible = RemoveDartMenuItem.IsVisible = IsValidDartSelection();
    }

    #endregion

    #region Helper Methods

    private async Task<string> ShowCustomInputDialog(string title = "Nhập dữ liệu", string message = "Nhập giá trị:",
        string defaultValue = "")
    {
        string result = null;
        var dialog = new Window
        {
            Title = title,
            Width = 300,
            Height = 140,
            MinWidth = 280,
            MinHeight = 130,
            MaxWidth = 350,
            MaxHeight = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SystemDecorations = SystemDecorations.BorderOnly,
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            Classes = { "compact-input-dialog" }
        };
        var mainBorder = new Border
        {
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(8),
            BoxShadow = new BoxShadows(
                new BoxShadow
                {
                    Color = Color.FromArgb(20, 0, 0, 0),
                    OffsetX = 0,
                    OffsetY = 2,
                    Blur = 8,
                    Spread = 0
                }
            )
        };
        var innerGrid = new Grid
        {
            Margin = new Thickness(16, 12),
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(8, GridUnitType.Pixel)),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(12, GridUnitType.Pixel)),
                new RowDefinition(GridLength.Auto)
            }
        };
        var messagePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6
        };
        var iconBorder = new Border
        {
            Width = 14,
            Height = 14,
            Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
            CornerRadius = new CornerRadius(7),
            VerticalAlignment = VerticalAlignment.Center
        };
        var iconText = new TextBlock
        {
            Text = "i",
            FontSize = 9,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        iconBorder.Child = iconText;
        var messageLabel = new TextBlock
        {
            Text = message,
            FontSize = 12,
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81)),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        messagePanel.Children.Add(iconBorder);
        messagePanel.Children.Add(messageLabel);
        Grid.SetRow(messagePanel, 0);
        innerGrid.Children.Add(messagePanel);
        var textBoxBorder = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Colors.White)
        };
        var textBox = new TextBox
        {
            Text = defaultValue,
            FontSize = 12,
            Height = 28,
            Padding = new Thickness(10, 6),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Watermark = "Nhập...",
            Classes = { "compact-textbox" }
        };
        textBox.GotFocus += (s, e) =>
        {
            textBoxBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(99, 102, 241));
            textBoxBorder.BorderThickness = new Thickness(1.5);
        };
        textBox.LostFocus += (s, e) =>
        {
            textBoxBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219));
            textBoxBorder.BorderThickness = new Thickness(1);
        };
        textBoxBorder.Child = textBox;
        Grid.SetRow(textBoxBorder, 2);
        innerGrid.Children.Add(textBoxBorder);
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };
        var cancelButton = new Button
        {
            Content = "Hủy",
            Width = 50,
            Height = 24,
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Background = new SolidColorBrush(Colors.White),
            Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            IsCancel = true,
            Classes = { "compact-secondary-button" }
        };
        var okButton = new Button
        {
            Content = "OK",
            Width = 50,
            Height = 24,
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Background = new SolidColorBrush(Color.FromRgb(99, 102, 241)),
            Foreground = new SolidColorBrush(Colors.White),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(4),
            IsDefault = true,
            Classes = { "compact-primary-button" }
        };
        okButton.PointerEntered += (s, e) => { okButton.Background = new SolidColorBrush(Color.FromRgb(79, 70, 229)); };
        okButton.PointerExited += (s, e) => { okButton.Background = new SolidColorBrush(Color.FromRgb(99, 102, 241)); };
        cancelButton.PointerEntered += (s, e) =>
        {
            cancelButton.Background = new SolidColorBrush(Color.FromRgb(249, 250, 251));
        };
        cancelButton.PointerExited += (s, e) => { cancelButton.Background = new SolidColorBrush(Colors.White); };
        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(okButton);
        Grid.SetRow(buttonPanel, 4);
        innerGrid.Children.Add(buttonPanel);
        mainBorder.Child = innerGrid;
        dialog.Content = mainBorder;
        okButton.Click += (s, e) =>
        {
            var input = textBox.Text?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                var animation = new Animation
                {
                    Duration = TimeSpan.FromMilliseconds(200),
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(0.0),
                            Setters = { new Setter(MarginProperty, new Thickness(8)) }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(0.5),
                            Setters = { new Setter(MarginProperty, new Thickness(10, 8, 6, 8)) }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(1.0),
                            Setters = { new Setter(MarginProperty, new Thickness(8)) }
                        }
                    }
                };

                animation.RunAsync(mainBorder);
                textBox.Focus();
                return;
            }

            result = input;
            dialog.Close();
        };

        cancelButton.Click += (s, e) => dialog.Close();

        textBox.KeyDown += (s, e) =>
        {
            switch (e.Key)
            {
                case Key.Enter when !string.IsNullOrWhiteSpace(textBox.Text):
                    result = textBox.Text.Trim();
                    dialog.Close();
                    break;
                case Key.Escape:
                    dialog.Close();
                    break;
            }
        };
        dialog.Opened += (s, e) =>
        {
            textBox.Focus();
            textBox.SelectAll();
        };

        Window ownerWindow = WindowExecution.Instance;
        await dialog.ShowDialog(ownerWindow);
        return result;
    }


    private async Task<bool> IsLoadedData()
    {
        if (!string.IsNullOrEmpty(FormMainEffect.Instance.PathFileNrDart) &&
            !string.IsNullOrEmpty(FormMainEffect.Instance.PathFileNrEffect) &&
            !string.IsNullOrEmpty(FormMainEffect.Instance.PathFileNrSkill)) return true;
        await ShowErrorAsync("Thông Báo", "Vui Lòng Load Dữ Liệu");
        return false;
    }

    private static short GetNextAvailableId<T>(ConcurrentDictionary<short, T> dictionary)
    {
        return (short)(dictionary.IsEmpty ? 0 : dictionary.Keys.Max() + 1);
    }

    private async Task ShowErrorAsync(string title, string message)
    {
        try
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                title,
                message,
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error);
            await messageBox.ShowAsync();
        }
        catch
        {
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _updateTimer?.Stop();
        base.OnClosed(e);
    }

    #endregion
}