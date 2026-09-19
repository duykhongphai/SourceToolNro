using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using BotPlayer.Classes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace BotPlayer.Windows;

public partial class WindowExecution : Window
{
    private static readonly SolidColorBrush SuccessColor = new(Color.Parse("#28a745"));
    private static readonly SolidColorBrush InfoColor = new(Color.Parse("#4267B2"));
    private static readonly SolidColorBrush WarningColor = new(Color.Parse("#ffc107"));
    private static readonly SolidColorBrush ErrorColor = new(Color.Parse("#dc3545"));
    public static string? IpConnect;
    public static int PortConnect;
    private bool _isConnected;
    private bool _isLoggedIn;
    public Account[] Accounts;
    public bool IsChangeMap;
    public bool IsChat;
    public bool IsChatGlobal;
    public bool IsMonster;
    public bool IsPvp;

    public WindowExecution()
    {
        InitializeComponent();
        LoginButton.IsEnabled = false;
        ImportButton.IsEnabled = false;
        Instance = this;
        Function.InitCharData();
    }

    public static WindowExecution Instance { get; private set; }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isLoggedIn)
        {
            ShowNotification("Vui lòng đăng xuất trước", "error");
            return;
        }

        var topLevel = GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Chọn file tài khoản",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt" } }
            }
        });

        if (files.Count <= 0) return;
        var file = files[0];

        try
        {
            var accountList = new List<Account>();
            using (var reader = new StreamReader(file.Path.LocalPath))
            {
                string line;
                var lineCount = 0;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var splitContent = line.Split('|');
                        if (splitContent.Length >= 2) accountList.Add(new Account(splitContent[0], splitContent[1]));
                        lineCount++;
                        if (lineCount % 100 == 0)
                        {
                            NumAccountText.Text = $"ℹ️ Đang đọc: {lineCount}";
                            await Task.Delay(1);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (accountList.Count > 0)
            {
                Accounts = accountList.ToArray();
                NumAccountText.Text = $"ℹ️ Số lượng: {Accounts.Length}";
                if (Accounts.Length > 1000)
                    ShowNotification($"Đã nhập {Accounts.Length} tài khoản thành công", "success");
            }
            else
            {
                ShowNotification("Không tìm thấy tài khoản", "error");
            }
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("Thông Báo", ex.ToString(),
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
            ShowNotification("Định dạng file tài khoản không phù hợp", "error");
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isConnected)
        {
            ShowNotification("Cần kiểm tra kết nối tới server trước khi thực hiện thao tác này", "error");
            return;
        }

        if (Accounts.Length == 0)
        {
            ShowNotification("Vui lòng nhập tài khoản trước", "error");
            return;
        }

        _isLoggedIn = true;
        LoginButton.IsEnabled = false;
        ShowNotification($"Đang đăng nhập: 0/{Accounts.Length}");
        var progress = new Progress<int>(value => { ShowNotification($"Đang đăng nhập: {value}/{Accounts.Length}"); });
        await SessionManager.Instance.InitSession(progress);
        await Task.Delay(5000);
        LogoutButton.IsEnabled = true;
        LogoutButton.Background = ErrorColor;
        ShowNotification("Đã đăng nhập thành công !", "success");
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _isLoggedIn = false;
        LogoutButton.IsEnabled = false;
        LogoutButton.Background = new SolidColorBrush(Color.Parse("#808080"));
        ShowNotification($"Đang đăng xuất: 0/{Accounts.Length}");
        var progress = new Progress<int>(value => { ShowNotification($"Đang đăng xuất: {value}/{Accounts.Length}"); });
        await SessionManager.Instance.Dispose(progress);
        await Task.Delay(5000);
        LoginButton.IsEnabled = true;
        ShowNotification("Đã đăng xuất. Vui lòng đăng nhập lại để tiếp tục.", "warning");
    }

    private void ShowNotification(string message, string type = "info")
    {
        var icon = "";
        SolidColorBrush color;

        switch (type.ToLower())
        {
            case "success":
                icon = "✓ ";
                color = SuccessColor;
                break;
            case "warning":
                icon = "⚠️ ";
                color = WarningColor;
                break;
            case "error":
                icon = "❌ ";
                color = ErrorColor;
                break;
            default:
                icon = "ℹ️ ";
                color = InfoColor;
                break;
        }

        NotificationText.Text = icon + message;
        NotificationText.Foreground = color;
    }

    private void Panel_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void CheckServer_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(IpAddressTextBox.Text) || string.IsNullOrEmpty(PortTextBox.Text))
        {
            ShowNotification("Vui lòng nhập đầy đủ thông tin", "error");
            return;
        }

        if (!IPAddress.TryParse(IpAddressTextBox.Text, out var ipAddress))
        {
            ShowNotification("Địa chỉ IP không hợp lệ", "error");
            return;
        }

        if (!int.TryParse(PortTextBox.Text, out var port) || port < 1 || port > 65535)
        {
            ShowNotification("Cổng không hợp lệ, phải là số từ 1 đến 65535", "error");
            return;
        }

        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var connectResult = socket.BeginConnect(ipAddress, port, null, null);
            var success = connectResult.AsyncWaitHandle.WaitOne(3000, true);
            if (success && socket.Connected)
            {
                IpConnect = IpAddressTextBox.Text;
                PortConnect = port;
                socket.EndConnect(connectResult);
                ShowNotification("Kết nối thành công", "success");
                _isConnected = true;
            }
            else
            {
                socket.Close();
                ShowNotification("Kết nối thất bại: Máy chủ không phản hồi", "error");
                _isConnected = false;
            }
        }
        catch (SocketException ex)
        {
            ShowNotification($"Lỗi kết nối: {ex.Message}", "error");
            _isConnected = false;
        }
        catch (Exception ex)
        {
            ShowNotification($"Lỗi không xác định: {ex.Message}", "error");
            _isConnected = false;
        }

        if (_isConnected)
        {
            LoginButton.IsEnabled = true;
            ImportButton.IsEnabled = true;
            CheckServer.IsEnabled = false;
        }
        else
        {
            LoginButton.IsEnabled = false;
            ImportButton.IsEnabled = false;
            await SessionManager.Instance.Dispose();
        }
    }

    private void MonsterCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        IsMonster = MonsterCheckBox.IsChecked == true;
    }

    private void MapCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        IsChangeMap = MapCheckBox.IsChecked == true;
    }

    private void ChatCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        IsChat = ChatCheckBox.IsChecked == true;
    }

    private void WorldChatCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        IsChatGlobal = WorldChatCheckBox.IsChecked == true;
    }

    private void PvpCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        IsPvp = PvpCheckBox.IsChecked == true;
    }
}