using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DrawMap.ChildWindows;
using DrawMap.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Enum = DrawMap.Options.Enum;
using Image = SixLabors.ImageSharp.Image;
using Point = SixLabors.ImageSharp.Point;

namespace DrawMap.Classes;

public static partial class Function
{
    private static readonly Random Random = new();

    public static string ExtractNumbers(string input)
    {
        return MyRegex().Replace(input, "");
    }

    public static BgItem FindItemUnderMouse(int mouseX, int mouseY)
    {
        return (from item in ManagerDrawMap.Instance.BgItem
            let imageScale = Fields.ResourceItemBackground[item.Id]
            where mouseX >= item.X && mouseX <= item.X + imageScale.Width && mouseY >= item.Y &&
                  mouseY <= item.Y + imageScale.Height
            select item).FirstOrDefault();
    }

    public static ActorMap FindWaypointMapUnderMouse(int mouseX, int mouseY)
    {
        return (from item in ManagerDrawMap.Instance.WaypointMap
            let imageScale = Fields.PictureWaypoint.Source
            where mouseX >= item.X && mouseX <= item.X + imageScale.Size.Width && mouseY >= item.Y &&
                  mouseY <= item.Y + imageScale.Size.Height
            select item).FirstOrDefault();
    }

    public static ActorMap FindActorMapUnderMouse(int mouseX, int mouseY)
    {
        foreach (var item in from item in ManagerDrawMap.Instance.MonsterMap
                 let imageScale = Fields.ResourceMonster[item.Id]
                 where mouseX >= item.X && mouseX <= item.X + imageScale.Width &&
                       mouseY >= item.Y && mouseY <= item.Y + imageScale.Height
                 select item)
            return item;

        return (from item in ManagerDrawMap.Instance.NpcMap
            let imageScale = Fields.ResourceNpc[item.Id]
            where mouseX >= item.X && mouseX <= item.X + imageScale.Width && mouseY >= item.Y &&
                  mouseY <= item.Y + imageScale.Height
            select item).FirstOrDefault();
    }

    public static EffectMap FindItemEffectUnderMouse(int mouseX, int mouseY)
    {
        return (from item in ManagerDrawMap.Instance.EffectMap
            let imageScale = Fields.ResourceEffect[item.Id]
            where mouseX >= item.X && mouseX <= item.X + imageScale.Width && mouseY >= item.Y &&
                  mouseY <= item.Y + imageScale.Height
            select item).FirstOrDefault();
    }

    public static ConcurrentDictionary<int, List<PictureCustom>> GetAllImageBackground()
    {
        var groupedImageResources = new ConcurrentDictionary<int, List<PictureCustom>>();
        if (!Directory.Exists(Settings.folderBackground))
            return groupedImageResources;
        try
        {
            foreach (var file in Directory.GetFiles(Settings.folderBackground))
                try
                {
                    var img = new Bitmap(file);
                    var name = Path.GetFileNameWithoutExtension(file);
                    var pic = new PictureCustom
                    {
                        DataPicture = new FieldsData
                        {
                            BackgroundId = GetBgId(name),
                            BackgroundType = GetBgType(name)
                        },
                        Width = 100, Height = 50,
                        Source = img,
                        TypeBlock = null,
                        TypePicture = Enum.Background
                    };
                    int bgType = pic.GetBgType();
                    if (!groupedImageResources.TryGetValue(bgType, out var value))
                    {
                        value = [];
                        groupedImageResources[bgType] = value;
                    }

                    value.Add(pic);
                }
                catch
                {
                }

            foreach (var list in groupedImageResources.Values) list.Sort();
        }
        catch
        {
        }

        return groupedImageResources;
    }

    private static byte GetBgType(string name)
    {
        var showName = name.Replace("b", "");
        showName = showName.Contains('-')
            ? showName[..(showName.IndexOf('-') - 1)]
            : showName.Remove(showName.Length - 1);
        return byte.Parse(showName);
    }

    private static byte GetBgId(string name)
    {
        var showName = name.Replace("b", "");
        showName = showName.Contains('-') ? showName[showName.IndexOf('-') - 1].ToString() : showName[^1].ToString();
        return byte.Parse(showName);
    }

    public static ConcurrentDictionary<short, Bitmap> GetAllImageItemBackground()
    {
        var groupedImageResources = new ConcurrentDictionary<short, Bitmap>();
        if (!Directory.Exists(Settings.folderItemBackground))
            return groupedImageResources;
        try
        {
            foreach (var file in Directory.GetFiles(Settings.folderItemBackground))
                try
                {
                    var img = new Bitmap(file);
                    var name = Path.GetFileNameWithoutExtension(file);

                    if (short.TryParse(name, out var id)) groupedImageResources[id] = img;
                }
                catch
                {
                }

            var sortedDict = groupedImageResources.OrderBy(a => a.Key).ToDictionary(a => a.Key, a => a.Value);
            return new ConcurrentDictionary<short, Bitmap>(sortedDict);
        }
        catch
        {
            return groupedImageResources;
        }
    }

    public static ConcurrentDictionary<int, List<PictureCustom>> GetAllImageTile()
    {
        var groupedImageResources = new ConcurrentDictionary<int, List<PictureCustom>>();

        if (!Directory.Exists(Settings.folderTileMap))
            return groupedImageResources;

        try
        {
            foreach (var file in Directory.GetFiles(Settings.folderTileMap))
                try
                {
                    var img = new Bitmap(file);
                    var name = Path.GetFileNameWithoutExtension(file);
                    var parts = name.Split('$');

                    if (parts.Length != 2 ||
                        !int.TryParse(parts[0], out var idParent) ||
                        !int.TryParse(parts[1], out var id)) continue;
                    var pic = new PictureCustom
                    {
                        DataPicture = new FieldsData
                        {
                            Id = id,
                            IdParent = idParent
                        },
                        Width = 24, Height = 24,
                        Source = img,
                        TypeBlock = Fields.FindTileType(id, idParent),
                        TypePicture = Enum.TileMap
                    };
                    pic.PointerPressed += ChildWindows.DrawMap.Instance.PicOnPointerPressed;
                    if (!groupedImageResources.TryGetValue(idParent, out var value))
                    {
                        value = [];
                        groupedImageResources[idParent] = value;
                    }

                    value.Add(pic);
                }
                catch
                {
                }

            foreach (var list in groupedImageResources.Values) list.Sort();
        }
        catch
        {
        }

        return groupedImageResources;
    }

    public static int? TryParseId(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        try
        {
            return int.Parse(ExtractNumbers(name));
        }
        catch
        {
            return null;
        }
    }

    public static void ClearFolder(string folderPath)
    {
        DirectoryInfo directory = new(folderPath);
        foreach (var file in directory.GetFiles()) file.Delete();
        foreach (var subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
    }

    public static void ClearFolder(string folderPath, string startWith)
    {
        DirectoryInfo directory = new(folderPath);
        foreach (var file in directory.GetFiles())
            if (file.Name.StartsWith(startWith))
                file.Delete();
    }

    public static void InvalidateAll(this Panel panel, bool recursive = true)
    {
        if (panel == null)
            return;

        panel.InvalidateVisual();

        foreach (var child in panel.Children.Where(child => child != null))
        {
            child.InvalidateVisual();

            if (recursive && child is Panel childPanel) InvalidateAll(childPanel, recursive);
        }
    }

    public static int NextInt(int a, int b)
    {
        int result;
        if (a == b)
            result = a;
        else
            result = a + Random.Next(b - a);
        return result;
    }

    public static void InvalidateAll(this StackPanel panel, bool recursive = true)
    {
        InvalidateAll(panel as Panel, recursive);
    }

    public static void InvalidateAll(this WrapPanel panel, bool recursive = true)
    {
        InvalidateAll(panel as Panel, recursive);
    }

    public static async Task<Bitmap> CombineImageAsync(List<PictureCustom> images, int maxWidth, int maxHeight)
    {
        var imageData = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            return images.OrderBy(obj => obj.GetId())
                .Where(img => img.Source != null)
                .Select(img => new
                {
                    Source = (Bitmap)img.Source,
                    Id = img.GetId()
                })
                .ToList();
        });
        return await Task.Run(() =>
        {
            using var combinedImage = new Image<Rgba32>(maxWidth, maxHeight);
            var y = 0;

            foreach (var item in imageData)
            {
                using var stream = new MemoryStream();
                item.Source?.Save(stream);
                stream.Position = 0;
                using var sourceImage = Image.Load<Rgba32>(stream);
                sourceImage.Mutate(x => x.Resize(24, 24));
                combinedImage.Mutate(ctx => ctx.DrawImage(sourceImage, new Point(0, y), 1f));
                y += 24;
            }

            using var resultStream = new MemoryStream();
            combinedImage.SaveAsPng(resultStream);
            resultStream.Position = 0;
            return new Bitmap(resultStream);
        });
    }

    private static Bitmap TransformBitmap(Bitmap img, Action<IImageProcessingContext> transform)
    {
        if (img == null) return null;
        using var stream = new MemoryStream();
        img.Save(stream);
        stream.Position = 0;
        using var image = Image.Load<Rgba32>(stream);
        image.Mutate(transform);
        using var resultStream = new MemoryStream();
        image.SaveAsPng(resultStream);
        resultStream.Position = 0;
        return new Bitmap(resultStream);
    }

    public static Bitmap RotateImage(this Bitmap img, float rotationAngle)
        => TransformBitmap(img, x => x.Rotate(rotationAngle));

    public static Bitmap FlipVertical(this Bitmap image)
        => TransformBitmap(image, x => x.Flip(FlipMode.Vertical));

    public static Bitmap FlipHorizontal(this Bitmap image)
        => TransformBitmap(image, x => x.Flip(FlipMode.Horizontal));

    public static Bitmap CloneBitmap(this Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream);
        stream.Position = 0;
        var data = stream.ToArray();
        return new Bitmap(new MemoryStream(data));
    }

    public static sbyte[] ImageToSByteArray(Bitmap image)
    {
        using MemoryStream ms = new();
        image.Save(ms);
        return ByteHelper.AsSBytes(ms.ToArray());
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex MyRegex();
}