using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using DrawMap.Classes;
using DrawMap.Options;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Enum = DrawMap.Options.Enum;

namespace DrawMap.ChildWindows;

public partial class ManagerTile
{
    public async Task LoadPartNpc(string iconPath, bool isJsonArray, bool isIdDxDyOrder)
    {
        var instance = ConnectSQL.Instance;
        if (new[]
            {
                instance.IdPartFieldComboBox?.SelectedItem,
                instance.DataPartFieldComboBox?.SelectedItem,
                instance.TypePartFieldComboBox?.SelectedItem
            }.Any(x => x == null))
            return;

        try
        {
            ClearExistingPartData();

            var resultsNpc =
                await instance.SelectAsync($"SELECT * FROM `{instance.TableNpcFieldComboBox.SelectedItem}`");

            for (var i = 0; i < resultsNpc.Count; i++)
            {
                var keyValuePairs = resultsNpc[i];
                var idNpc = short.Parse(keyValuePairs[instance.IdNpcFieldComboBox.SelectedItem.ToString()].ToString());
                var nameNpc = keyValuePairs[instance.NameNpcFieldComboBox.SelectedItem.ToString()].ToString();
                var headId =
                    short.Parse(keyValuePairs[instance.HeadNpcFieldComboBox.SelectedItem.ToString()].ToString());
                var bodyId =
                    short.Parse(keyValuePairs[instance.BodyNpcFieldComboBox.SelectedItem.ToString()].ToString());
                var legId = short.Parse(keyValuePairs[instance.LegNpcFieldComboBox.SelectedItem.ToString()].ToString());

                await LoadPartData(iconPath, isJsonArray, isIdDxDyOrder, headId, bodyId, legId);

                var pic = new PictureCustom
                {
                    Tag = nameNpc,
                    DataPicture = new FieldsData
                    {
                        Id = idNpc,
                        HeadId = headId,
                        BodyId = bodyId,
                        LegId = legId
                    },
                    Width = NpcWidth,
                    Height = NpcHeight,
                    TypePicture = Enum.Npc,
                    Margin = new Avalonia.Thickness(NpcMargin)
                };

                pic.PointerPressed += DrawMap.Instance.PicOnPointerPressed;
                Fields.ResourceNpc.TryAdd(idNpc, pic);
            }

            await ShowSuccessMessage(LoadSuccessMessage);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Có lỗi xảy ra: {ex.Message}");
        }
    }

    private void ClearExistingPartData()
    {
        foreach (var pic in Fields.ResourceImagePart.Values)
            pic?.Dispose();
        Fields.ResourceImagePart.Clear();

        foreach (var part in Fields.ResourcePartData.Values)
            part?.Dispose();
        Fields.ResourcePartData.Clear();

        Fields.ResourceNpc.Clear();
    }

    private async Task LoadPartData(string iconPath, bool isJsonArray, bool isIdDxDyOrder, params int[] ids)
    {
        var instance = ConnectSQL.Instance;
        var idsString = string.Join(",", ids.Select(id => $"'{id}'"));

        var databaseTask = Task.Run(async () =>
            await instance.SelectAsync(
                $"SELECT * FROM `{instance.TablePartFieldComboBox.SelectedItem}` WHERE `{instance.IdPartFieldComboBox.SelectedItem}` IN ({idsString})"));

        var imageFilesTask = GetAvailableImages(iconPath);

        var results = await databaseTask;
        var availableImages = await imageFilesTask;

        if (results?.Count == 0 || results == null) return;

        var imagesToLoad = new ConcurrentBag<short>();

        await ProcessPartResults(results, instance, isJsonArray, isIdDxDyOrder, availableImages, imagesToLoad);
        await LoadPartImages(iconPath, imagesToLoad);
    }

    private async Task<HashSet<short>> GetAvailableImages(string iconPath)
    {
        return await Task.Run(() =>
        {
            var availableImages = new HashSet<short>();
            if (!Directory.Exists(iconPath)) return availableImages;

            var pngFiles = Directory.GetFiles(iconPath, "*.png", SearchOption.TopDirectoryOnly);
            Parallel.ForEach(pngFiles, file =>
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (!short.TryParse(fileName, out var imageId)) return;
                lock (availableImages)
                {
                    availableImages.Add(imageId);
                }
            });
            return availableImages;
        });
    }

    private async Task ProcessPartResults(List<Dictionary<string, object>> results, ConnectSQL instance,
        bool isJsonArray, bool isIdDxDyOrder, HashSet<short> availableImages, ConcurrentBag<short> imagesToLoad)
    {
        await Task.Run(() =>
        {
            Parallel.ForEach(results, row =>
            {
                try
                {
                    ProcessSinglePartRow(row, instance, isJsonArray, isIdDxDyOrder, availableImages, imagesToLoad);
                }
                catch (Exception rowEx)
                {
                    Console.WriteLine($"Error processing row: {rowEx.Message}");
                }
            });
        });
    }

    private void ProcessSinglePartRow(Dictionary<string, object> row, ConnectSQL instance,
        bool isJsonArray, bool isIdDxDyOrder, HashSet<short> availableImages, ConcurrentBag<short> imagesToLoad)
    {
        if (!short.TryParse(row[instance.IdPartFieldComboBox.SelectedItem.ToString()]?.ToString(), out var idPart) ||
            !int.TryParse(row[instance.TypePartFieldComboBox.SelectedItem.ToString()]?.ToString(), out var type))
            return;

        var jsonData = row[instance.DataPartFieldComboBox.SelectedItem.ToString()]?.ToString();
        if (string.IsNullOrWhiteSpace(jsonData)) return;

        var result1 = JsonParser.ParseArray(jsonData);
        if (result1?.Count == 0 || result1 == null) return;

        var part = new Part(type);

        for (var j = 0; j < 2; j++)
        {
            var partImage = ParsePartImage(result1[j], isJsonArray, isIdDxDyOrder);
            if (partImage == null) continue;

            part.Pi[j] = partImage;
            if (availableImages.Contains(partImage.Id) && !Fields.ResourceImagePart.ContainsKey(partImage.Id))
                imagesToLoad.Add(partImage.Id);
        }

        Fields.ResourcePartData[idPart] = part;
    }

    private PartImage ParsePartImage(object data, bool isJsonArray, bool isIdDxDyOrder)
    {
        if (isJsonArray)
        {
            if (data is List<object> arr && arr.Count >= 3 &&
                short.TryParse(arr[0]?.ToString(), out var arrId) &&
                short.TryParse(arr[1]?.ToString(), out var arrDx) &&
                short.TryParse(arr[2]?.ToString(), out var arrDy))
                return new PartImage
                {
                    Id = isIdDxDyOrder ? arrId : arrDy,
                    Dx = (sbyte)(isIdDxDyOrder ? arrDx : arrId),
                    Dy = (sbyte)(isIdDxDyOrder ? arrDy : arrDx)
                };
        }
        else
        {
            if (data is Dictionary<string, object> obj &&
                obj.TryGetValue("id", out var value) &&
                obj.TryGetValue("dx", out var value2) &&
                obj.TryGetValue("dy", out var value3) &&
                short.TryParse(value?.ToString(), out var objId) &&
                sbyte.TryParse(value2?.ToString(), out var objDx) &&
                sbyte.TryParse(value3?.ToString(), out var objDy))
                return new PartImage { Id = objId, Dx = objDx, Dy = objDy };
        }

        return null;
    }

    private async Task LoadPartImages(string iconPath, ConcurrentBag<short> imagesToLoad)
    {
        var uniqueImageIds = imagesToLoad.Distinct().ToList();
        if (uniqueImageIds.Count == 0) return;

        await Task.Run(() =>
        {
            Parallel.ForEach(uniqueImageIds,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                imageId =>
                {
                    if (Fields.ResourceImagePart.ContainsKey(imageId)) return;
                    try
                    {
                        var imagePath = Path.Combine(iconPath, $"{imageId}.png");
                        using var bitmap = new Bitmap(imagePath);
                        Fields.ResourceImagePart[imageId] = bitmap.CreateScaledBitmap(
                            new PixelSize((int)(bitmap.Size.Width * ScalePercent / 100),
                                (int)(bitmap.Size.Height * ScalePercent / 100)));
                    }
                    catch (Exception imgEx)
                    {
                        Console.WriteLine($"Failed to load image {imageId}.png: {imgEx.Message}");
                    }
                });
        });
    }

    private async Task LoadMonsters()
    {
        ClearExistingMonsters();

        var instance = ConnectSQL.Instance;
        var databaseMonster =
            await instance.SelectAsync($"SELECT * FROM `{instance.TableMonsterFieldComboBox.SelectedItem}`");

        var totalCount = databaseMonster.Count;
        var successCount = 0;
        var errorCount = 0;

        foreach (var keyValuePairs in databaseMonster)
        {
            var idMonster =
                byte.Parse(keyValuePairs[instance.IdMonsterFieldComboBox.SelectedItem.ToString()].ToString());
            var nameMonster = keyValuePairs[instance.NameMonsterFieldComboBox.SelectedItem.ToString()].ToString();

            try
            {
                if (MonsterFromImageDataRadio.IsChecked == true)
                    await LoadMonstersFromImageAndData(idMonster, nameMonster);
                else if (MonsterFromDataRadio.IsChecked == true)
                    await LoadMonstersFromDataOnly(idMonster, nameMonster);

                successCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                Console.WriteLine($"Lỗi load monster {idMonster} ({nameMonster}): {ex.Message}");
            }
        }

        var resultMessage = $"Hoàn thành load {totalCount} monsters\nThành công: {successCount}\nLỗi: {errorCount}";
        var icon = errorCount == 0 ? MsBox.Avalonia.Enums.Icon.Success : MsBox.Avalonia.Enums.Icon.Warning;
        await MessageBoxManager.GetMessageBoxStandard("Kết Quả", resultMessage, ButtonEnum.Ok, icon).ShowAsync();
    }

    private void ClearExistingMonsters()
    {
        foreach (var eff in Fields.ResourceMonster.Values)
            eff.Dispose();
        Fields.ResourceMonster.Clear();
    }

    private async Task LoadMonstersFromImageAndData(short idm, string name)
    {
        var imagePath = MonsterImageFolderTextBox.Text;
        var dataPath = MonsterDataFolderTextBox.Text;

        var data = ByteHelper.AsSBytes(await File.ReadAllBytesAsync(Path.Combine(dataPath, idm.ToString())));
        var effectData = new EffectData { Id = idm };

        try
        {
            var dataInputStream = new BinaryDataReader(data);
            dataInputStream.readByte();
            var typeRead = dataInputStream.readByte();
            if (typeRead == 0)
            {
                var length = dataInputStream.readInt();
                var data2 = new sbyte[length];
                dataInputStream.read(ref data2);
                LoadEffectData(effectData, data2);
            }

            dataInputStream.Close();
        }
        catch
        {
            LoadEffectData(effectData, data);
        }

        try
        {
            var imageBytes = ByteHelper.AsSBytes(await File.ReadAllBytesAsync(Path.Combine(imagePath, $"{idm}.png")));
            InitImageEffect(effectData, imageBytes, Enum.Monster, name);
        }
        catch
        {
        }

        Fields.ResourceMonster.TryAdd(idm, effectData);
    }

    private async Task LoadMonstersFromDataOnly(short idm, string name)
    {
        var dataPath = MonsterDataOnlyFolderTextBox.Text;
        var fileData = ByteHelper.AsSBytes(await File.ReadAllBytesAsync(Path.Combine(dataPath, idm.ToString())));

        if (TryProcessMonsterData(fileData, idm, name, true))
            return;

        TryProcessMonsterData(fileData, idm, name, false);
    }

    private bool TryProcessMonsterData(sbyte[] fileData, short idm, string name, bool readFirstByte)
    {
        BinaryDataReader dataInputStream = null;
        try
        {
            dataInputStream = new BinaryDataReader(fileData);

            if (readFirstByte)
                dataInputStream.readByte();

            var type = dataInputStream.readByte();
            if (type != 0) return false;

            var length = dataInputStream.readInt();
            var data = new sbyte[length];
            dataInputStream.read(ref data);
            var effectData = new EffectData { Id = idm };
            LoadEffectData(effectData, data);

            length = dataInputStream.readInt();
            var image = new sbyte[length];
            dataInputStream.read(ref image);

            try
            {
                dataInputStream.readByte();
            }
            catch
            {
            }

            try
            {
                InitImageEffect(effectData, image, Enum.Monster, name);
            }
            catch
            {
            }

            Fields.ResourceMonster.TryAdd(idm, effectData);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            dataInputStream?.Close();
        }
    }
}
