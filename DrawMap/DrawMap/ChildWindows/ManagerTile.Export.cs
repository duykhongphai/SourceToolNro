using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using DrawMap.Classes;
using DrawMap.Options;

namespace DrawMap.ChildWindows;

public partial class ManagerTile
{
    private async void ExportDataTileMap()
    {
        if (!Directory.Exists($"{Settings.folderOutputTileMap}x1"))
            Directory.CreateDirectory($"{Settings.folderOutputTileMap}x1");
        Function.ClearFolder($"{Settings.folderOutputTileMap}x1");
        ExportBtn.IsEnabled = false;
        try
        {
            await ExportTileMaps();
            await ExportScaledTileMaps();
            ExportTileInfo();

            await ShowSuccessMessage("Xuất Thành Công Tile Map Và Tile Info");
        }
        finally
        {
            ExportBtn.IsEnabled = true;
        }
    }

    private async Task ExportTileMaps()
    {
        foreach (var id in Fields.ResourceTitleMap.Keys)
        {
            var info = Fields.ResourceTitleMap[id];
            var maxHeight = TileSize * info.Count;
            var bitmap = await Function.CombineImageAsync(info, TileSize, maxHeight);
            var writer = new BinaryDataWriter();
            writer.write(Function.ImageToSByteArray(bitmap));
            await File.WriteAllBytesAsync($"{Settings.folderOutputTileMap}x1//{id}",
                ByteHelper.AsBytes(writer.getData()));
            writer.Close();
        }
    }

    private async Task ExportScaledTileMaps()
    {
        var imageDataList = await GetImageDataList();

        for (var i = 2; i <= 4; i++) await ExportScaledTileMapAtScale(imageDataList, i);
    }

    private async Task<List<PictureCustom>> GetImageDataList()
    {
        return await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            return Fields.ResourceTitleMap.Values
                .SelectMany(info => info.Where(info2 => info2?.Source != null))
                .ToList();
        });
    }

    private async Task ExportScaledTileMapAtScale(List<PictureCustom> imageDataList, int scale)
    {
        var folderPath = $"{Settings.folderOutputTileMap}x{scale}";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        Function.ClearFolder(folderPath);
        var percent = ScalePercent * scale;
        var processedData = imageDataList.Select(imageData =>
        {
            var bitmap = (Bitmap)imageData.Source;
            var newWidth = 96 * percent / 100;
            var newHeight = 96 * percent / 100;

            return new
            {
                ImageData = imageData,
                ScaledBitmap = bitmap.CreateScaledBitmap(new PixelSize(newWidth, newHeight)),
                FileName = $"{imageData.GetIdParent()}${imageData.GetId()}"
            };
        }).ToList();
        await Task.Run(async () =>
        {
            foreach (var item in processedData)
            {
                using var img = item.ScaledBitmap;
                await File.WriteAllBytesAsync($"{folderPath}//{item.FileName}",
                    ByteHelper.AsBytes(Function.ImageToSByteArray(img)));
            }
        });
    }

    public void ExportTileInfo()
    {
        try
        {
            var writer = new BinaryDataWriter();
            writer.writeByte((sbyte)Fields.TileIndex.Count);

            for (var i = 0; i < Fields.TileIndex.Count; i++)
            {
                writer.writeByte((sbyte)Fields.TileIndex[i].Count);
                for (var k = 0; k < Fields.TileIndex[i].Count; k++)
                {
                    writer.writeInt(Fields.TileType[i][k]);
                    writer.writeByte((sbyte)Fields.TileIndex[i][k].Count);
                    for (var j = 0; j < Fields.TileIndex[i][k].Count; j++)
                        writer.writeByte((sbyte)Fields.TileIndex[i][k][j]);
                }
            }

            File.WriteAllBytes(Settings.folderOutputTileInfo, ByteHelper.AsBytes(writer.getData()));
            writer.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task ExportDataBackground()
    {
        for (var i = 1; i <= 4; i++)
        {
            var outputFolder = $"{Settings.folderOutputBackground}//x{i}";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            Function.ClearFolder(outputFolder);
            var percent = ScalePercent * i;

            foreach (var info2 in Fields.ResourceBackground.Values.SelectMany(info =>
                         info.Where(info2 => info2.Source != null)))
            {
                using var img = ((Bitmap)info2.Source!).CreateScaledBitmap(new PixelSize(
                    (int)(info2.Source!.Size.Width * percent / 100),
                    (int)info2.Source.Size.Height * percent / 100));

                var writer = new BinaryDataWriter();
                writer.write(Function.ImageToSByteArray(img));
                await File.WriteAllBytesAsync(
                    $"{outputFolder}//b{info2.GetBgType()}-{info2.GetBgId()}",
                    ByteHelper.AsBytes(writer.getData()));
                writer.Close();
            }
        }
    }

    private async Task ExportDataItemBg()
    {
        for (var i = 1; i <= 4; i++)
        {
            var outputFolder = $"{Settings.folderOutputImageItemBg}//x{i}";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            Function.ClearFolder(outputFolder);
            var percent = ScalePercent * i;

            foreach (var key in Fields.ResourceImageItemBackground.Keys)
            {
                var image = Fields.ResourceImageItemBackground[key];
                using var img = image.CreateScaledBitmap(new PixelSize(
                    (int)(image.Size.Width * percent / 100),
                    (int)(image.Size.Height * percent / 100)));

                var writer = new BinaryDataWriter();
                writer.write(Function.ImageToSByteArray(img));
                await File.WriteAllBytesAsync($"{outputFolder}//{key}",
                    ByteHelper.AsBytes(writer.getData()));
                writer.Close();
            }
        }
    }

    private async Task ExportItemBackgroundTemplate(bool[] result)
    {
        try
        {
            if (result[0])
                await ExportItemBackgroundToDatabase();
            else
                await ExportItemBackgroundToFile();
        }
        catch (Exception e)
        {
            await ShowErrorMessage($"Có Lỗi Xảy Ra Trong Quá Trình Ghi Item Background Template\n{e}");
        }
    }

    private async Task ExportItemBackgroundToDatabase()
    {
        if (!ConnectSQL.Instance.IsConnected())
        {
            await ShowErrorMessage(DatabaseConnectionRequiredMessage);
            return;
        }

        var tableName = ConnectSQL.Instance.TableBgItemFieldComboBox.SelectedItem.ToString();
        await ConnectSQL.Instance.ClearTable(tableName);

        foreach (var key in Fields.ResourceItemBackground.Keys)
        {
            var img = Fields.ResourceItemBackground[key];
            await ConnectSQL.Instance.InsertAsync(
                $"INSERT INTO `{tableName}`(`{ConnectSQL.Instance.IdBgItemFieldComboBox.SelectedItem}`,`{ConnectSQL.Instance.IdImageBgItemFieldComboBox.SelectedItem}`, `{ConnectSQL.Instance.LayerBgItemFieldComboBox.SelectedItem}`, `{ConnectSQL.Instance.DxBgItemFieldComboBox.SelectedItem}`, `{ConnectSQL.Instance.DyBgItemFieldComboBox.SelectedItem}`) VALUES ('{key}','{img.GetIdImage()}','{img.GetLayer()}','{img.GetDx()}','{img.GetDy()}')");
        }

        await ShowSuccessMessage("Xuất Thành Công Item Background Template (Database) Và Image Item Background");
    }

    private async Task ExportItemBackgroundToFile()
    {
        var writer = new BinaryDataWriter();
        writer.writeShort((short)Fields.ResourceItemBackground.Count);

        foreach (var key in Fields.ResourceItemBackground.Keys)
            if (Fields.ResourceItemBackground.TryGetValue(key, out var img))
            {
                writer.writeShort((short)img.GetIdImage());
                writer.writeByte((sbyte)img.GetLayer());
                writer.writeShort((short)img.GetDx());
                writer.writeShort((short)img.GetDy());
                writer.writeByte(0);
            }

        await File.WriteAllBytesAsync("Output//BgItemTemplate", ByteHelper.AsBytes(writer.getData()));
        writer.Close();

        await ShowSuccessMessage("Xuất Thành Công Item Background Template (Data File) Và Image Item Background");
    }
}
