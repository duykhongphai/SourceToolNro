using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ToolEffectNro.Class;
using ToolEffectNro.Classes;
using ToolEffectNro.Windows;

namespace ToolEffectNro.ChildWindows;

public partial class ManagerEffect : Window
{
    public static ManagerEffect Instance;

    public ManagerEffect()
    {
        InitializeComponent();
        Instance = this;

        var pathEff = Settings.localLowPath + "_1";
        var pathFlag = Settings.localLowPath + "_2";
        var pathSeparate = Settings.localLowPath + "_3";

        if (DataHasEffectId != null) DataHasEffectId.IsChecked = File.Exists(pathEff);
        if (FlagBag != null) FlagBag.IsChecked = File.Exists(pathFlag);
        if (SeparateImageAndData != null) SeparateImageAndData.IsChecked = File.Exists(pathSeparate);
    }

    private void EditArrayItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (WindowExecution.Instance.ArrayFrame.IsVisible)
            WindowExecution.Instance.ArrayFrame.Hide();
        else
            WindowExecution.Instance.ArrayFrame.Show();
    }

    private async void BuildEffect_OnClick(object sender, RoutedEventArgs e)
    {
        await BuildData();
    }

    private void ViewChar_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        FormSkillEffect.Instance.PanelMain.InvalidateVisual();
    }

    private async Task BuildData()
    {
        try
        {
            _ = short.TryParse(FormMainEffect.Instance.IdEffectTextBox.Text, out var idEff);
            if (FlagBag.IsChecked == true && idEff >= 255)
            {
                await MessageBoxManager.GetMessageBoxStandard("Thông Báo", "Id Flag Bag Tối Đa Là 254",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                return;
            }

            for (var i = 1; i <= 4; i++)
            {
                string[] subdirectories = ["All", "Data", "Image"];
                foreach (var subdirectory in subdirectories)
                {
                    var path = $"Effect//x{i}//{subdirectory}";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                }
            }

            if (!Directory.Exists("Effect//Json")) Directory.CreateDirectory("Effect//Json");

            myWriter dataEffect = new();
            dataEffect.writeByte((sbyte)FormMainEffect.Instance.InfoImage.Keys.Count);
            for (byte i = 0; i < FormMainEffect.Instance.InfoImage.Keys.Count; i++)
            {
                var info = FormMainEffect.Instance.InfoImage[i];
                if (info.X0 / 4 > byte.MaxValue || info.Y0 / 4 > byte.MaxValue ||
                    info.Image.PixelSize.Width / 4 > byte.MaxValue || info.Image.PixelSize.Height / 4 > byte.MaxValue)
                {
                    await MessageBoxManager.GetMessageBoxStandard("Lỗi", "Kích Thước Ảnh Gốc Quá Dài Hoặc Quá Rộng",
                        ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
                    return;
                }

                dataEffect.writeByte((sbyte)info.ID);
                dataEffect.writeByte((sbyte)(info.X0 / 4));
                dataEffect.writeByte((sbyte)(info.Y0 / 4));
                dataEffect.writeByte((sbyte)(info.Image.PixelSize.Width / 4));
                dataEffect.writeByte((sbyte)(info.Image.PixelSize.Height / 4));
            }

            dataEffect.writeShort((short)FormMainEffect.Instance.ParentFrame.Keys.Count);
            for (short i = 0; i < FormMainEffect.Instance.ParentFrame.Keys.Count; i++)
            {
                var childFrames = FormMainEffect.Instance.ParentFrame[i];
                dataEffect.writeByte((sbyte)childFrames.Count);
                foreach (var chill in childFrames)
                {
                    dataEffect.writeShort((short)(chill.Bounds.X / 4));
                    dataEffect.writeShort((short)(chill.Bounds.Y / 4));
                    dataEffect.writeByte((sbyte)chill.ImageInfo.ID);
                }
            }

            dataEffect.writeShort((short)FormMainEffect.Instance.ArrayFrame.Count);
            foreach (var t in FormMainEffect.Instance.ArrayFrame) dataEffect.writeShort(t);
            var dataEff = dataEffect.getData();
            for (var i = 1; i <= 4; i++)
            {
                var percent = 25 * i;
                var imgOrg = FormMainEffect.Instance.ImageCanvas.Source;
                if (imgOrg == null) continue;
                var img = ((Bitmap)imgOrg).CreateScaledBitmap(new PixelSize((int)(imgOrg.Size.Width * percent / 100),
                    (int)(imgOrg.Size.Height * percent / 100)));


                myWriter imageOutPut = new();
                if (DataHasEffectId.IsChecked == true) imageOutPut.writeShort(idEff);
                if (SeparateImageAndData.IsChecked != true) imageOutPut.writeInt(dataEff.Length);
                imageOutPut.write(dataEff);
                if (SeparateImageAndData.IsChecked != true)
                {
                    var image = Function.ImageToSByteArray(img);
                    imageOutPut.writeByte(0);
                    imageOutPut.writeInt(image.Length);
                    imageOutPut.write(image);
                }
                else
                {
                    img.Save($"Effect//x{i}//Image//{idEff}.png");
                }

                await File.WriteAllBytesAsync($"Effect//x{i}//Data//{idEff}",
                    Array.ConvertAll(imageOutPut.getData(), a => (byte)a));
                imageOutPut.Close();
            }

            await File.WriteAllTextAsync($"Effect//Json//{idEff}.txt",
                $"Images:\n{FormMainEffect.Instance.InfoImage.ToJsonString()}\nFrames:\n{FormMainEffect.Instance.ParentFrame.ToJsonString()}\nRun:\n{FormMainEffect.Instance.ArrayFrame.ToJsonArray()}");
            await MessageBoxManager.GetMessageBoxStandard("Thông Báo", "Thành Công",
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Success).ShowAsync();
            dataEffect.Close();
        }
        catch (Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandard("Lỗi", $"Xảy Ra Lỗi \n{e}",
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
        }
    }

    private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private static void SyncFlagFile(bool isChecked, string path)
    {
        if (isChecked)
            File.WriteAllText(path, "");
        else if (File.Exists(path)) File.Delete(path);
    }

    private void DataHasEffectId_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        SyncFlagFile(DataHasEffectId.IsChecked == true, Settings.localLowPath + "_1");
    }

    private void FlagBag_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        SyncFlagFile(FlagBag.IsChecked == true, Settings.localLowPath + "_2");
    }

    private void SeparateImageAndData_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        SyncFlagFile(SeparateImageAndData.IsChecked == true, Settings.localLowPath + "_3");
    }
}