using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MySqlConnector;

namespace DrawMap.ChildWindows;

public partial class ConnectSQL : Window
{
    private const string ConnectingText = "Đang kết nối...";
    private const string ConnectText = "Kết nối";

    public static ConnectSQL Instance;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private readonly Dictionary<ComboBox, ComboBox[]> _tableFieldMapping;
    private MySqlConnection _connection;
    private string _currentDatabase;

    public ConnectSQL()
    {
        InitializeComponent();
        Instance = this;
        _tableFieldMapping = new Dictionary<ComboBox, ComboBox[]>
        {
            [TablePartFieldComboBox] = [IdPartFieldComboBox, TypePartFieldComboBox, DataPartFieldComboBox],
            [TableMonsterFieldComboBox] = [IdMonsterFieldComboBox, NameMonsterFieldComboBox],
            [TableNpcFieldComboBox] =
            [
                IdNpcFieldComboBox, NameNpcFieldComboBox, HeadNpcFieldComboBox, BodyNpcFieldComboBox,
                LegNpcFieldComboBox
            ],
            [TableBgItemFieldComboBox] =
            [
                IdBgItemFieldComboBox, IdImageBgItemFieldComboBox, LayerBgItemFieldComboBox, DxBgItemFieldComboBox,
                DyBgItemFieldComboBox
            ]
        };
        foreach (var tableComboBox in _tableFieldMapping.Keys)
            tableComboBox.SelectionChanged += OnTableSelectionChanged;
    }

    public bool IsConnected()
    {
        return _connection?.State == ConnectionState.Open;
    }

    private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        var server = ServerTextBox.Text?.Trim();
        var username = UsernameTextBox.Text?.Trim();
        var password = PasswordTextBox.Text;

        if (string.IsNullOrWhiteSpace(server))
        {
            ShowStatus("Vui lòng nhập địa chỉ server", true);
            return;
        }

        SetConnectButtonState(true);

        try
        {
            await ConnectToDatabase(server, username, password);
            ShowStatus("Kết nối thành công!", false);
            await LoadDatabases();
        }
        catch (MySqlException ex)
        {
            ShowStatus($"Lỗi kết nối: {ex.Message}", true);
        }
        catch (Exception ex)
        {
            ShowStatus($"Lỗi: {ex.Message}", true);
        }
        finally
        {
            SetConnectButtonState(false);
        }
    }

    private async Task ConnectToDatabase(string server, string username, string password)
    {
        await _connectionSemaphore.WaitAsync();
        try
        {
            await CloseExistingConnection();

            var connectionString = $"Server={server};User ID={username};Password={password};";
            _connection = new MySqlConnection(connectionString);
            await _connection.OpenAsync();
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    private async Task CloseExistingConnection()
    {
        if (_connection?.State == ConnectionState.Open) await _connection.CloseAsync();
        _connection?.Dispose();
    }

    private void SetConnectButtonState(bool isConnecting)
    {
        ConnectButton.IsEnabled = !isConnecting;
        ConnectButton.Content = isConnecting ? ConnectingText : ConnectText;
    }

    private async Task LoadDatabases()
    {
        try
        {
            var databases = await ExecuteStringListQuery("SHOW DATABASES;");
            var treeViewItems = databases.Select(CreateDatabaseTreeViewItem).ToList();
            DatabaseTreeView.ItemsSource = treeViewItems;
        }
        catch (Exception ex)
        {
            ShowStatus($"Không thể tải cơ sở dữ liệu: {ex.Message}", true);
        }
    }

    private TreeViewItem CreateDatabaseTreeViewItem(string dbName)
    {
        var item = new TreeViewItem
        {
            Header = new TextBlock { Text = dbName, VerticalAlignment = VerticalAlignment.Center },
            Tag = new ItemTag { Type = "database", Name = dbName }
        };
        item.PropertyChanged += DbItemOnPropertyChanged;
        return item;
    }

    private async void DbItemOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != "IsSelected" || e.NewValue is not bool isSelected || !isSelected)
            return;

        if (sender is not TreeViewItem { Tag: ItemTag { Type: "database" } tag }) return;
        try
        {
            _currentDatabase = tag.Name;
            var tables = await LoadTables(tag.Name);
            var tableNames = tables.Count > 0 ? tables : new List<string> { "Không có bảng nào" };

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var tableComboBox in _tableFieldMapping.Keys) tableComboBox.ItemsSource = tableNames;
            });
        }
        catch (Exception ex)
        {
            ShowStatus($"Lỗi tải bảng: {ex.Message}", true);
        }
    }

    private async void OnTableSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox tableComboBox ||
            !_tableFieldMapping.TryGetValue(tableComboBox, out var fieldComboBoxes))
            return;

        var selectedTable = tableComboBox.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selectedTable)) return;

        try
        {
            var fields = await LoadTableFields(selectedTable);
            var fieldNames = fields.Count > 0 ? fields : new List<string> { "Không có dữ liệu" };

            foreach (var fieldComboBox in fieldComboBoxes) fieldComboBox.ItemsSource = fieldNames;
        }
        catch (Exception ex)
        {
            ShowStatus($"Không thể tải trường: {ex.Message}", true);
        }
    }

    private async Task<List<string>> LoadTables(string dbName)
    {
        if (!IsConnected()) return new List<string> { "Không có kết nối" };

        await _connectionSemaphore.WaitAsync();
        try
        {
            await ExecuteNonQuery($"USE `{dbName}`;");
            return await ExecuteStringListQuery("SHOW TABLES;");
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    private async Task<List<string>> LoadTableFields(string tableName)
    {
        if (!IsConnected()) return new List<string> { "Không có kết nối" };

        await _connectionSemaphore.WaitAsync();
        try
        {
            await ExecuteNonQuery($"USE `{_currentDatabase}`;");

            var fieldNames = new List<string>();
            await using var cmd = new MySqlCommand($"SHOW COLUMNS FROM `{tableName}`;", _connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) fieldNames.Add(reader["Field"].ToString());

            return fieldNames;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    private async Task<List<string>> ExecuteStringListQuery(string query)
    {
        await using var command = new MySqlCommand(query, _connection);
        await using var reader = await command.ExecuteReaderAsync();

        var results = new List<string>();
        while (await reader.ReadAsync()) results.Add(reader.GetString(0));
        return results;
    }

    private async Task ExecuteNonQuery(string query)
    {
        await using var command = new MySqlCommand(query, _connection);
        await command.ExecuteNonQueryAsync();
    }

    private void ShowStatus(string message, bool isError)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = isError
                ? new SolidColorBrush(Color.Parse("#e74c3c"))
                : new SolidColorBrush(Color.Parse("#4267B2"));
            StatusTextBlock.IsVisible = true;
        });
    }

    private void DatabaseTreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DatabaseTreeView.SelectedItem is not TreeViewItem selectedItem) return;
        if (selectedItem.Tag is not ItemTag tag) return;
        if (tag.Type != "database") return;
        _currentDatabase = tag.Name;
    }

    private async void TableMonsterFieldComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = TableMonsterFieldComboBox.SelectedItem;
        if (selectedItem == null) return;
        var selectedTable = selectedItem.ToString();
        var items = await LoadTableFields(selectedTable);
        IdMonsterFieldComboBox.ItemsSource = items;
        NameMonsterFieldComboBox.ItemsSource = items;
    }

    private async void TablePartFieldComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = TablePartFieldComboBox.SelectedItem;
        if (selectedItem == null) return;
        var selectedTable = selectedItem.ToString();
        var items = await LoadTableFields(selectedTable);
        IdPartFieldComboBox.ItemsSource = items;
        TypePartFieldComboBox.ItemsSource = items;
        DataPartFieldComboBox.ItemsSource = items;
    }

    private async void TableNpcFieldComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = TableNpcFieldComboBox.SelectedItem;
        if (selectedItem == null) return;
        var selectedTable = selectedItem.ToString();
        var items = await LoadTableFields(selectedTable);
        IdNpcFieldComboBox.ItemsSource = items;
        NameNpcFieldComboBox.ItemsSource = items;
        HeadNpcFieldComboBox.ItemsSource = items;
        BodyNpcFieldComboBox.ItemsSource = items;
        LegNpcFieldComboBox.ItemsSource = items;
    }

    private async void TableBgItemFieldComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = TableBgItemFieldComboBox.SelectedItem;
        if (selectedItem == null) return;
        var selectedTable = selectedItem.ToString();
        var items = await LoadTableFields(selectedTable);
        IdBgItemFieldComboBox.ItemsSource = items;
        IdImageBgItemFieldComboBox.ItemsSource = items;
        LayerBgItemFieldComboBox.ItemsSource = items;
        DxBgItemFieldComboBox.ItemsSource = items;
        DyBgItemFieldComboBox.ItemsSource = items;
    }

    protected override void OnClosed(EventArgs e)
    {
        _connection?.Dispose();
        _connectionSemaphore?.Dispose();
        base.OnClosed(e);
    }

    public class ItemTag
    {
        public string Type { get; init; }
        public string Name { get; init; }
    }

    #region Public API - giữ nguyên interface cũ

    public async Task<List<Dictionary<string, object>>> SelectAsync(string query)
    {
        if (!IsConnected()) throw new Exception("Null Connecting");

        await _connectionSemaphore.WaitAsync();
        try
        {
            await using var command = new MySqlCommand(query, _connection);
            await using var reader = await command.ExecuteReaderAsync();
            var results = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (var i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.GetValue(i);
                results.Add(row);
            }

            return results;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public async Task ClearTable(string table)
    {
        if (!IsConnected()) throw new Exception("Null Connecting");

        await _connectionSemaphore.WaitAsync();
        try
        {
            await using var command = new MySqlCommand($"TRUNCATE TABLE {table};", _connection);
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public async Task InsertAsync(string query)
    {
        if (!IsConnected()) throw new Exception("Null Connecting");

        await _connectionSemaphore.WaitAsync();
        try
        {
            await using var command = new MySqlCommand(query, _connection);
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    #endregion
}