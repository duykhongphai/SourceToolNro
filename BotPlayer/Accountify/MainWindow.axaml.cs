using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using MySqlConnector;

namespace Accountify;

public partial class MainWindow : Window
{
    private MySqlConnection _connection;
    private string _currentDatabase;
    private string _currentTable;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void CloseButton_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        Close();
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        var server = ServerTextBox.Text;
        var username = UsernameTextBox.Text;
        var password = PasswordTextBox.Text;

        if (string.IsNullOrWhiteSpace(server))
        {
            ShowStatus("Vui lòng nhập địa chỉ server", true);
            return;
        }

        ConnectButton.IsEnabled = false;
        ConnectButton.Content = "Đang kết nối...";
        try
        {
            if (_connection is { State: ConnectionState.Open }) _connection.Close();
            var connectionString = $"Server={server};User ID={username};Password={password};";
            _connection = new MySqlConnection(connectionString);
            await _connection.OpenAsync();
            ShowStatus("Kết nối thành công!", false);
            await LoadDatabases();
        }
        catch (MySqlException ex)
        {
            ShowStatus($"Lỗi kết nối: {ex.Message}", true);
        }
        finally
        {
            ConnectButton.IsEnabled = true;
            ConnectButton.Content = "Kết nối";
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground =
            isError ? new SolidColorBrush(Color.Parse("#e74c3c")) : new SolidColorBrush(Color.Parse("#4267B2"));
        StatusTextBlock.IsVisible = true;
    }

    private Task LoadDatabases()
    {
        try
        {
            var dataTable = new DataTable();
            using (var command = new MySqlCommand("SHOW DATABASES;", _connection))
            using (var adapter = new MySqlDataAdapter(command))
            {
                adapter.Fill(dataTable);
            }

            var rootItems = new List<TreeViewItem>();
            foreach (DataRow row in dataTable.Rows)
            {
                var dbName = row[0].ToString();
                var dbItem = new TreeViewItem
                {
                    Header = new TextBlock
                    {
                        Text = dbName,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Tag = new ItemTag { Type = "database", Name = dbName }
                };
                var placeholderItem = new TreeViewItem
                {
                    Header = new TextBlock
                    {
                        Text = "Đang tải...",
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Classes = { "loadingItem" }
                };
                dbItem.ItemsSource = new List<TreeViewItem> { placeholderItem };
                dbItem.PropertyChanged += DbItem_PropertyChanged;
                rootItems.Add(dbItem);
            }

            DatabaseTreeView.ItemsSource = rootItems;
        }
        catch (Exception ex)
        {
            ShowStatus($"Không thể tải cơ sở dữ liệu: {ex.Message}", true);
        }

        return Task.CompletedTask;
    }

    private async void DbItem_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != "IsExpanded") return;
        var isExpanded = e.NewValue != null && (bool)e.NewValue;
        if (!isExpanded) return;
        if (sender is not TreeViewItem dbItem)
            return;
        if (dbItem.ItemsSource == null) return;
        var items = dbItem.ItemsSource.Cast<object>().ToList();
        if (items.Count != 1) return;
        if (items[0] is not TreeViewItem { Header: TextBlock { Text: "Đang tải..." } })
            return;
        if (dbItem.Tag is not ItemTag { Type: "database" } tag)
            return;
        try
        {
            var dbName = tag.Name;
            dbItem.ItemsSource = null;
            var tables = await LoadTables(dbName);
            var tableItems = tables.Select(tableName => new TreeViewItem
            {
                Header = new TextBlock { Text = tableName, VerticalAlignment = VerticalAlignment.Center },
                Tag = new ItemTag { Type = "table", Name = tableName, Database = dbName }
            }).ToList();
            if (tableItems.Count == 0)
            {
                var emptyItem = new TreeViewItem
                {
                    Header = new TextBlock
                    {
                        Text = "Không có bảng nào",
                        Foreground = new SolidColorBrush(Color.Parse("#757575")),
                        VerticalAlignment = VerticalAlignment.Center,
                        FontStyle = FontStyle.Italic
                    }
                };
                tableItems.Add(emptyItem);
            }

            dbItem.ItemsSource = tableItems;
        }
        catch (Exception ex)
        {
            dbItem.ItemsSource = new List<TreeViewItem>
            {
                new()
                {
                    Header = new TextBlock
                    {
                        Text = $"Lỗi: {ex.Message}",
                        Foreground = new SolidColorBrush(Color.Parse("#e74c3c")),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            };
        }
    }

    private async Task<List<string>> LoadTables(string dbName)
    {
        await using (var cmd = new MySqlCommand($"USE `{dbName}`;", _connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        var dataTable = new DataTable();
        await using (var cmd = new MySqlCommand("SHOW TABLES;", _connection))
        using (var adapter = new MySqlDataAdapter(cmd))
        {
            adapter.Fill(dataTable);
        }

        return (from DataRow row in dataTable.Rows select row[0].ToString()).ToList();
    }

    private async void DatabaseTreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DatabaseTreeView.SelectedItem is not TreeViewItem selectedItem) return;
        if (selectedItem.Tag is not ItemTag tag) return;
        if (tag.Type != "table") return;
        _currentDatabase = tag.Database;
        _currentTable = tag.Name;

        await LoadTableFields(_currentDatabase, _currentTable);
    }

    private async Task LoadTableFields(string dbName, string tableName)
    {
        try
        {
            await using (var cmd = new MySqlCommand($"USE `{dbName}`;", _connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            var dataTable = new DataTable();
            await using (var cmd = new MySqlCommand($"SHOW COLUMNS FROM `{tableName}`;", _connection))
            using (var adapter = new MySqlDataAdapter(cmd))
            {
                adapter.Fill(dataTable);
            }

            var fieldNames = (from DataRow row in dataTable.Rows
                let fieldName = row["Field"].ToString()
                let fieldType = row["Type"].ToString()
                select fieldName).ToList();
            UsernameFieldComboBox.ItemsSource = fieldNames;
            PasswordFieldComboBox.ItemsSource = fieldNames;
            if (fieldNames.Count > 0)
            {
                UsernameFieldComboBox.SelectedIndex = 0;
                PasswordFieldComboBox.SelectedIndex = fieldNames.Count > 1 ? 1 : 0;
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Không thể tải trường: {ex.Message}", true);
        }
    }

    private async void QueryButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentDatabase) || string.IsNullOrEmpty(_currentTable))
        {
            ShowStatus("Vui lòng chọn bảng trước khi truy vấn", true);
            return;
        }

        var fromValue = FromTextBox.Text;
        var toValue = ToTextBox.Text;

        if (string.IsNullOrWhiteSpace(fromValue) || string.IsNullOrWhiteSpace(toValue))
        {
            ShowStatus("Vui lòng nhập giá trị 'Từ' và 'Tới'", true);
            return;
        }

        var usernameField = UsernameFieldComboBox.SelectedItem?.ToString();
        var passwordField = PasswordFieldComboBox.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(usernameField) || string.IsNullOrEmpty(passwordField))
        {
            ShowStatus("Vui lòng chọn trường tài khoản và mật khẩu", true);
            return;
        }

        try
        {
            if (!int.TryParse(fromValue, out var from) || !int.TryParse(toValue, out var to))
            {
                ShowStatus("Giá trị 'Từ' và 'Tới' phải là số nguyên", true);
                return;
            }

            if (from > to)
            {
                ShowStatus("Giá trị 'Từ' phải nhỏ hơn hoặc bằng 'Tới'", true);
                return;
            }

            var values = new List<string>();
            StringBuilder builder = new();
            for (var i = from; i <= to; i++)
            {
                var account = $"{CustomUserTextBox.Text?.Replace("X", i.ToString())}";
                var password = EncryptPassword(Function.GenerateRandomString(12));
                values.Add($"('{account}', '{password}')");
                builder.Append(account).Append('|').Append(password).Append('\n');
            }

            builder.Remove(builder.Length - 1, 1);
            File.WriteAllText("accounts.txt", builder.ToString());
            var query = $@"
                    INSERT INTO `{_currentDatabase}`.`{_currentTable}` (`{usernameField}`, `{passwordField}`)
                    VALUES {string.Join(", ", values)};
                ";
            await using var cmd = new MySqlCommand(query, _connection);
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            ShowStatus($"Đã thêm {rowsAffected} tài khoản thành công!", false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Lỗi truy vấn: {ex.Message}", true);
        }
    }

    private string EncryptPassword(string password)
    {
        if (EncryptionComboBox == null || EncryptionComboBox.SelectedIndex < 0)
            return password;

        switch (EncryptionComboBox.SelectedIndex)
        {
            case 0:
                return password;
            case 1:
                try
                {
                    var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(password));
                    return Convert.ToHexStringLower(hashBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MD5 hashing error: {ex.Message}");
                    return password;
                }

            case 2:
                try
                {
                    var hashBytes = SHA1.HashData(Encoding.UTF8.GetBytes(password));
                    return Convert.ToHexStringLower(hashBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SHA1 hashing error: {ex.Message}");
                    return password;
                }

            case 3:
                try
                {
                    var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
                    return Convert.ToHexStringLower(hashBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SHA256 hashing error: {ex.Message}");
                    return password;
                }

            case 4:
                try
                {
                    var hashBytes = SHA384.HashData(Encoding.UTF8.GetBytes(password));
                    return Convert.ToHexStringLower(hashBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SHA384 hashing error: {ex.Message}");
                    return password;
                }

            case 5:
                try
                {
                    var hashBytes = SHA512.HashData(Encoding.UTF8.GetBytes(password));
                    return Convert.ToHexStringLower(hashBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SHA512 hashing error: {ex.Message}");
                    return password;
                }

            case 6:
                try
                {
                    var firstHash = SHA1.HashData(Encoding.UTF8.GetBytes(password));
                    var secondHash = SHA1.HashData(firstHash);
                    return "*" + Convert.ToHexString(secondHash).ToUpper();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MySQL PASSWORD() error: {ex.Message}");
                    return password;
                }

            default:
                return password;
        }
    }

    public class ItemTag
    {
        public string Type { get; init; }
        public string Name { get; init; }
        public string Database { get; init; }
        public string Table { get; set; }
    }
}