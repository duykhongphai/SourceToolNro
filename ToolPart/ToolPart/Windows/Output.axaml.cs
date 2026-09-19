using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ToolPart.Classes;
using ToolPart.Options;

namespace ToolPart.Windows;

public partial class Output : Window
{
    public Output()
    {
        InitializeComponent();
        LayoutUpdated += (_, _) => InitData();
        Instance = this;
    }

    public static Output Instance { get; private set; }

    private void DragPanel_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void MinimumButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    public void InitData()
    {
        if (JsonHead == null || JsonBody == null || JsonLeg == null) return;
        var head = Function.ConvertArrayToString(WindowExecution.Instance.PartHead);
        var body = Function.ConvertArrayToString(WindowExecution.Instance.PartBody);
        var leg = Function.ConvertArrayToString(WindowExecution.Instance.PartLeg);
        if (JsonArray?.IsChecked == true)
        {
            head = Function.CleanJson(head, "\"dx\"", "\"dy\"", "\"id\"", "{", "}", ":").Trim(' ');
            body = Function.CleanJson(body, "\"dx\"", "\"dy\"", "\"id\"", "{", "}", ":").Trim(' ');
            leg = Function.CleanJson(leg, "\"dx\"", "\"dy\"", "\"id\"", "{", "}", ":").Trim(' ');
        }

        JsonHead.Text = head;
        JsonBody.Text = body;
        JsonLeg.Text = leg;
    }

    private void Json_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        InitData();
    }

    private void IdDxButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (IdDxButton?.IsChecked == false) return;
        WindowExecution.Instance.TypeSort = BodyEnum.IdDxDy;
        InitData();
    }

    private void DxIdButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (DxIdButton?.IsChecked == false) return;
        WindowExecution.Instance.TypeSort = BodyEnum.DxDyId;
        InitData();
    }
}