using System;
using System.IO;
using System.Threading;
using Avalonia;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace DrawMap;

internal class Program
{
    private static readonly Mutex Mutex = new(true, "{DrawMapNro}");

    [STAThread]
    public static void Main(string[] args)
    {
        var path = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                    @"\..\LocalLow\XTOOLS247\");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        if (Mutex.WaitOne(TimeSpan.Zero, true))
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        else
            MessageBoxManager.GetMessageBoxStandard("Thông Báo", "Ứng dụng đang chạy.",
                ButtonEnum.Ok,
                Icon.Error).ShowAsync();
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}