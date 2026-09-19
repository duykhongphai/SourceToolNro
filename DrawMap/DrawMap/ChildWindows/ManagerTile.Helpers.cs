using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DrawMap.Options;

namespace DrawMap.ChildWindows;

public partial class ManagerTile
{
    private async Task<string> ShowFolderPickerAsync(string title = "Select Folder")
    {
        try
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return null;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

            return folders.Count > 0 ? folders[0].Path.LocalPath : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task BrowseFolderAndSetTextBox(string title, TextBox textBox)
    {
        var folderPath = await ShowFolderPickerAsync(title);
        if (!string.IsNullOrEmpty(folderPath))
            textBox.Text = folderPath;
    }

    private async Task<IStorageFile[]> OpenMultipleFileDialog(string title)
    {
        var topLevel = GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = true
        });
        return files.ToArray();
    }

    private async Task<IStorageFile> OpenSingleFileDialog(string title)
    {
        var topLevel = GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });
        return files.Count > 0 ? files[0] : null;
    }

    private bool ValidateItemBackground()
    {
        return Fields.ResourceItemBackground != null && Fields.ResourceItemBackground.Count > 0;
    }

    private async Task<bool> ValidateVipAccessAndItemBackground()
    {
        if (!ValidateItemBackground())
        {
            await ShowErrorMessage("Vui Lòng Load Data Item Background Để Thực Hiện");
            return false;
        }

        return true;
    }

    private bool ValidatePartLoading()
    {
        var iconsPath = IconsPathTextBox.Text;

        if (string.IsNullOrEmpty(iconsPath))
        {
            _ = ShowErrorMessage(SelectFolderMessage);
            return false;
        }

        if (!Directory.Exists(iconsPath))
        {
            _ = ShowErrorMessage(FolderNotExistMessage);
            return false;
        }

        if (!ConnectSQL.Instance.IsConnected())
        {
            _ = ShowErrorMessage(DatabaseConnectionRequiredMessage);
            return false;
        }

        if (ConnectSQL.Instance.TableNpcFieldComboBox.SelectedIndex == -1)
        {
            _ = ShowErrorMessage("Vui Lòng Chọn Bảng Npc");
            return false;
        }

        return true;
    }

    private bool ValidateMonsterLoading()
    {
        if (!ConnectSQL.Instance.IsConnected())
        {
            _ = ShowErrorMessage(DatabaseConnectionRequiredMessage);
            return false;
        }

        if (ConnectSQL.Instance.TableMonsterFieldComboBox.SelectedIndex == -1)
        {
            _ = ShowErrorMessage("Vui Lòng Chọn Bảng Monster");
            return false;
        }

        if (MonsterFromImageDataRadio.IsChecked == true)
            return ValidateFolderPaths(MonsterImageFolderTextBox.Text, MonsterDataFolderTextBox.Text);

        if (MonsterFromDataRadio.IsChecked == true) return ValidateFolderPath(MonsterDataOnlyFolderTextBox.Text);

        return false;
    }

    private bool ValidateFolderPaths(string imagePath, string dataPath)
    {
        if (string.IsNullOrEmpty(imagePath) || string.IsNullOrEmpty(dataPath))
        {
            _ = ShowErrorMessage(SelectFolderMessage);
            return false;
        }

        if (!Directory.Exists(dataPath) || !Directory.Exists(imagePath))
        {
            _ = ShowErrorMessage(FolderNotExistMessage);
            return false;
        }

        return true;
    }

    private bool ValidateFolderPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            _ = ShowErrorMessage(SelectFolderMessage);
            return false;
        }

        if (!Directory.Exists(path))
        {
            _ = ShowErrorMessage(FolderNotExistMessage);
            return false;
        }

        return true;
    }

    private async Task<bool> ConfirmDeletion(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Thông Báo", message, ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        return result == ButtonResult.Yes;
    }

    private async Task ShowErrorMessage(string message)
    {
        await MessageBoxManager.GetMessageBoxStandard("Thông Báo", message,
            ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
    }

    private async Task ShowSuccessMessage(string message)
    {
        await MessageBoxManager.GetMessageBoxStandard("Thông Báo", message,
            ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success).ShowAsync();
    }
}
