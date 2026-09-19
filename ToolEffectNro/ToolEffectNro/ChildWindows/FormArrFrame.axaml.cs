using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace ToolEffectNro.ChildWindows;

public partial class FormArrFrame : Window
{
    public bool JustView;

    public FormArrFrame()
    {
        InitializeComponent();
        UpdateComboBoxAndGrid();
    }

    public void UpdateComboBoxAndGrid()
    {
        ParentId.Items.Clear();
        foreach (var key in FormMainEffect.Instance.ParentFrame.Keys) ParentId.Items.Add(key);
        ParentId.Items.Add(-501);
        ParentId.Items.Add(-502);
        ParentId.Items.Add(-503);
        ParentId.Items.Add(-504);
        ParentId.Items.Add(-510);
        ListBox.Items.Clear();
        foreach (var id in FormMainEffect.Instance.ArrayFrame) ListBox.Items.Add(id.ToString());
    }

    private void AddFrame_OnClick(object? sender, RoutedEventArgs e)
    {
        if (JustView) return;
        if (ParentId.SelectedIndex == -1) return;
        if (FormMainEffect.Instance.ArrayFrame.Count + 1 > short.MaxValue)
        {
            MessageBoxManager.GetMessageBoxStandard("Lỗi", $"Chỉ có thể thêm tối đa {short.MaxValue} array frame",
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
            return;
        }

        var idSeletect = ParentId.SelectedItem!.ToString();
        ListBoxItem item = new()
        {
            Content = idSeletect
        };
        ListBox.Items.Add(item);
        if (idSeletect != null) FormMainEffect.Instance.ArrayFrame.Add(short.Parse(idSeletect));
    }

    private void ListBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;
        var selectedItem = ListBox.SelectedItem;
        if (selectedItem == null) return;
        if (JustView) return;
        if (ListBox.Items.Count <= 0) return;
        var selectedItems = ListBox.SelectedItems;
        if (selectedItems is not { Count: > 0 }) return;
        var itemsToRemove = selectedItems.Cast<object>().ToList();
        foreach (var item in itemsToRemove) ListBox.Items.Remove(item);
    }
}