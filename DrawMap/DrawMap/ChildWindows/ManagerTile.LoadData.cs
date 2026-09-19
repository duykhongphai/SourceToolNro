using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DrawMap.Classes;
using DrawMap.Options;
using Enum = DrawMap.Options.Enum;

namespace DrawMap.ChildWindows;

public partial class ManagerTile
{
    private async Task LoadItemBackgroundFromDatabase()
    {
        if (!ConnectSQL.Instance.IsConnected())
        {
            await ShowErrorMessage(DatabaseConnectionRequiredMessage);
            return;
        }

        var instance = ConnectSQL.Instance;
        if (new[]
            {
                instance.IdImageBgItemFieldComboBox?.SelectedItem,
                instance.LayerBgItemFieldComboBox?.SelectedItem,
                instance.DxBgItemFieldComboBox?.SelectedItem,
                instance.DyBgItemFieldComboBox?.SelectedItem
            }.Any(x => x == null)) return;

        ClearExistingItemBackgrounds();

        var results = await instance.SelectAsync($"SELECT * FROM `{instance.TableBgItemFieldComboBox.SelectedItem}`");

        for (var i = 0; i < results.Count; i++)
        {
            var keyValuePairs = results[i];
            var idImage = short.Parse(keyValuePairs[instance.IdImageBgItemFieldComboBox.SelectedItem.ToString()]
                .ToString());

            if (!Fields.ResourceImageItemBackground.TryGetValue(idImage, out var img)) continue;

            var layer = sbyte.Parse(keyValuePairs[instance.LayerBgItemFieldComboBox.SelectedItem.ToString()]
                .ToString());
            var dx = short.Parse(keyValuePairs[instance.DxBgItemFieldComboBox.SelectedItem.ToString()].ToString());
            var dy = short.Parse(keyValuePairs[instance.DyBgItemFieldComboBox.SelectedItem.ToString()].ToString());

            InitPictureItemBg(i, img, layer, dx, dy, idImage);
        }
    }

    private async Task LoadItemBackgroundFromFile()
    {
        var file = await OpenSingleFileDialog("Open Data File");
        if (file == null) return;

        await using var stream = await file.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        ClearExistingItemBackgrounds();

        var reader = new BinaryDataReader(ByteHelper.AsSBytes(fileBytes));
        var count = reader.readShort();

        for (var i = 0; i < count; i++)
        {
            var idImage = reader.readShort();
            if (!Fields.ResourceImageItemBackground.TryGetValue(idImage, out var img)) continue;

            var layer = reader.readByte();
            var dx = reader.readShort();
            var dy = reader.readShort();
            var a = reader.readByte();

            for (var j = 0; j < a; j++)
            {
                reader.readByte();
                reader.readByte();
            }

            InitPictureItemBg(i, img, layer, dx, dy, idImage);
        }

        reader.Close();
    }

    private void ClearExistingItemBackgrounds()
    {
        if (Fields.ResourceItemBackground == null) return;
        foreach (var pic in Fields.ResourceItemBackground.Values)
            pic.Source = null;
        Fields.ResourceItemBackground.Clear();
    }

    private void InitPictureItemBg(int i, Bitmap img, sbyte layer, short dx, short dy, short idImage)
    {
        var w = (int)(img.Size.Width * ScalePercent / 100);
        var h = (int)(img.Size.Height * ScalePercent / 100);

        var pic = new PictureCustom
        {
            DataPicture = new FieldsData
            {
                Id = i,
                Layer = layer,
                Dx = dx,
                Dy = dy,
                IdImage = idImage
            },
            Width = w,
            Height = h,
            Source = img,
            TypePicture = Enum.ItemBackground
        };

        pic.PointerPressed += DrawMap.Instance.PicOnPointerPressed;
        Fields.ResourceItemBackground.TryAdd(i, pic);
    }

    private async Task ImportItemBackground(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        var bitmap = new Bitmap(stream);
        var idImage = (short)(Fields.ResourceImageItemBackground.Max(a => a.Key) + 1);
        var id = (short)(Fields.ResourceItemBackground.Max(a => a.Key) + 1);

        bitmap.Save($"{Settings.folderItemBackground}//{idImage}.png");
        Fields.ResourceImageItemBackground.TryAdd(idImage, bitmap);
        InitPictureItemBg(id, bitmap, 0, 0, 0, idImage);
    }

    private async Task DeleteItemBackground()
    {
        var idImage = (short)_pictureCustomSelected.GetIdImage();
        Fields.ResourceItemBackground.TryRemove(_pictureCustomSelected.GetId(), out _);

        if (Fields.ResourceItemBackground.Values.All(a => a.GetIdImage() != idImage))
        {
            Fields.ResourceImageItemBackground.TryRemove(idImage, out _);
            var filePath = Path.Combine(Settings.folderItemBackground, $"{idImage}.png");
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        _pictureCustomSelected = null;
        PanelViewItemBg.Image = null;
        PanelViewItemBg.InvalidateVisual();
    }

    private void ClearExistingEffects()
    {
        foreach (var eff in Fields.ResourceEffect.Values)
            eff.Dispose();
        Fields.ResourceEffect.Clear();
    }

    private async Task LoadEffectsFromImageAndData()
    {
        var imagePath = EffectImageFolderTextBox.Text;
        var dataPath = EffectDataFolderTextBox.Text;

        if (!ValidateFolderPaths(imagePath, dataPath)) return;

        var imageFiles = Directory.GetFiles(imagePath);
        var imageLookup = imageFiles.ToDictionary(
            file => Function.ExtractNumbers(Path.GetFileNameWithoutExtension(file)),
            file => file);

        var hasError = false;
        foreach (var file in Directory.GetFiles(dataPath))
            try
            {
                var idEffect = short.Parse(Function.ExtractNumbers(Path.GetFileNameWithoutExtension(file)));
                if (idEffect is >= EffectSkipRangeStart and <= EffectSkipRangeEnd) continue;

                await ProcessEffectFile(file, idEffect, imageLookup);
            }
            catch
            {
                hasError = true;
                await ShowErrorMessage("Có Lỗi Xảy Ra Khi Load Data Effect");
                break;
            }

        if (!hasError)
            await ShowSuccessMessage("Load Effect Data Thành Công");
    }

    private async Task ProcessEffectFile(string file, short idEffect, Dictionary<string, string> imageLookup)
    {
        var data = ByteHelper.AsSBytes(await File.ReadAllBytesAsync(file));
        var effectData = new EffectData { Id = idEffect };

        try
        {
            var dataInputStream = new BinaryDataReader(data);
            dataInputStream.readShort();
            var length = dataInputStream.readInt();
            var data2 = new sbyte[length];
            dataInputStream.read(ref data2);
            LoadEffectData(effectData, data2);
            dataInputStream.Close();
        }
        catch
        {
            LoadEffectData(effectData, data);
        }

        if (imageLookup.TryGetValue(idEffect.ToString(), out var imageFile))
            try
            {
                var imageBytes = ByteHelper.AsSBytes(await File.ReadAllBytesAsync(imageFile));
                InitImageEffect(effectData, imageBytes, Enum.Effect);
            }
            catch
            {
            }

        Fields.ResourceEffect.TryAdd(idEffect, effectData);
    }

    private async Task LoadEffectsFromDataOnly()
    {
        var dataPath = EffectDataOnlyFolderTextBox.Text;
        if (!ValidateFolderPath(dataPath)) return;

        var hasError = false;
        var files = Directory.GetFiles(dataPath);

        foreach (var file in files)
            try
            {
                var idEffect = short.Parse(Function.ExtractNumbers(Path.GetFileNameWithoutExtension(file)));
                if (idEffect is >= EffectSkipRangeStart and <= EffectSkipRangeEnd) continue;

                var fileData = ByteHelper.AsSBytes(await File.ReadAllBytesAsync(file));
                if (TryProcessEffectData(fileData, idEffect, true)) continue;

                TryProcessEffectData(fileData, idEffect, false);
            }
            catch
            {
                hasError = true;
                await ShowErrorMessage("Có Lỗi Xảy Ra Khi Load Data Effect");
                break;
            }

        if (!hasError)
            await ShowSuccessMessage("Load Effect Data Thành Công");
    }

    private bool TryProcessEffectData(sbyte[] fileData, short idEffect, bool withShort)
    {
        BinaryDataReader dataInputStream = null;
        try
        {
            dataInputStream = new BinaryDataReader(fileData);
            if (withShort)
                dataInputStream.readShort();

            var length = dataInputStream.readInt();
            var data = new sbyte[length];
            dataInputStream.read(ref data);
            dataInputStream.readByte();
            length = dataInputStream.readInt();
            var image = new sbyte[length];
            dataInputStream.read(ref image);

            var effectData = new EffectData { Id = idEffect };
            LoadEffectData(effectData, data);

            try
            {
                InitImageEffect(effectData, image, Enum.Effect);
            }
            catch
            {
            }

            Fields.ResourceEffect.TryAdd(idEffect, effectData);
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

    private void InitImageEffect(EffectData effectData, sbyte[] image, Enum type, string name = "")
    {
        using var ms = new MemoryStream(ByteHelper.AsBytes(image));
        using var img = new Bitmap(ms);
        var w = (int)(img.Size.Width * ScalePercent / 100);
        var h = (int)(img.Size.Height * ScalePercent / 100);

        var pic = new PictureCustom
        {
            DataPicture = new FieldsData { Id = effectData.Id },
            Tag = name,
            Width = effectData.Width,
            Height = effectData.Height,
            Source = img.CreateScaledBitmap(new PixelSize(w, h)),
            TypePicture = type,
            Margin = new Avalonia.Thickness(EffectMargin)
        };

        pic.PointerPressed += DrawMap.Instance.PicOnPointerPressed;
        effectData.Image = pic;
        pic.EffectData = effectData;
    }

    private void LoadEffectData(EffectData effectData, sbyte[] data)
    {
        var dataInputStream = new BinaryDataReader(data);
        try
        {
            var b = dataInputStream.readByte();
            effectData.ImgInfo = new ImageInfo[b];
            for (var i = 0; i < b; i++)
                effectData.ImgInfo[i] = new ImageInfo
                {
                    Id = dataInputStream.readByte(),
                    X0 = dataInputStream.readUnsignedByte(),
                    Y0 = dataInputStream.readUnsignedByte(),
                    W = dataInputStream.readUnsignedByte(),
                    H = dataInputStream.readUnsignedByte()
                };

            var num5 = dataInputStream.readShort();
            effectData.Frame = new Frame[num5];
            for (var j = 0; j < num5; j++)
            {
                effectData.Frame[j] = new Frame();
                var b2 = dataInputStream.readByte();
                effectData.Frame[j].Dx = new short[b2];
                effectData.Frame[j].Dy = new short[b2];
                effectData.Frame[j].IdImg = new sbyte[b2];
                for (var k = 0; k < b2; k++)
                {
                    effectData.Frame[j].Dx[k] = dataInputStream.readShort();
                    effectData.Frame[j].Dy[k] = dataInputStream.readShort();
                    effectData.Frame[j].IdImg[k] = dataInputStream.readByte();
                }
            }

            CalculateEffectDimensions(effectData);

            var num6 = dataInputStream.readShort();
            effectData.ArrFrame = new short[num6];
            for (var n = 0; n < num6; n++)
                effectData.ArrFrame[n] = dataInputStream.readShort();
        }
        catch
        {
        }

        dataInputStream.Close();
    }

    private void CalculateEffectDimensions(EffectData effectData)
    {
        var totalWidth = 0;
        var totalHeight = 0;
        var validFrames = 0;

        foreach (var frame in effectData.Frame)
        {
            if (frame.Dx.Length == 0) continue;

            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;

            for (var k = 0; k < frame.Dx.Length; k++)
            {
                var imgInfo = effectData.ImgInfo[frame.IdImg[k]];

                var left = frame.Dx[k];
                var top = frame.Dy[k];
                var right = frame.Dx[k] + imgInfo.W;
                var bottom = frame.Dy[k] + imgInfo.H;

                if (left < minX) minX = left;
                if (top < minY) minY = top;
                if (right > maxX) maxX = right;
                if (bottom > maxY) maxY = bottom;
            }

            var frameWidth = maxX - minX;
            var frameHeight = maxY - minY;

            totalWidth += frameWidth;
            totalHeight += frameHeight;
            validFrames++;
        }

        if (validFrames > 0)
        {
            effectData.Width = totalWidth / validFrames;
            effectData.Height = totalHeight / validFrames;
        }
        else
        {
            effectData.Width = 0;
            effectData.Height = 0;
        }
    }
}
