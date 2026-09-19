using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace CreateSkillNro.Classes;

public static partial class Function
{
    private static readonly Random Random = new();

    public static int[] ArrowIndex =
    [
        0, 15, 37, 52, 75, 105, 127, 142, 165, 195,
        217, 232, 255, 285, 307, 322, 345, 370
    ];

    private static readonly short[] Sinz =
    [
        0, 18, 36, 54, 71, 89, 107, 125, 143, 160,
        178, 195, 213, 230, 248, 265, 282, 299, 316, 333,
        350, 367, 384, 400, 416, 433, 449, 465, 481, 496,
        512, 527, 543, 558, 573, 587, 602, 616, 630, 644,
        658, 672, 685, 698, 711, 724, 737, 749, 761, 773,
        784, 796, 807, 818, 828, 839, 849, 859, 868, 878,
        887, 896, 904, 912, 920, 928, 935, 943, 949, 956,
        962, 968, 974, 979, 984, 989, 994, 998, 1002, 1005,
        1008, 1011, 1014, 1016, 1018, 1020, 1022, 1023, 1023, 1024,
        1024
    ];

    private static readonly short[] Cosz;
    private static readonly int[] Tanz;

    public static readonly sbyte[] Frame =
    [
        0, 1, 2, 1, 0, 1, 2, 1, 0, 1,
        2, 1, 0, 1, 2, 1, 0, 1, 2, 1,
        0, 1, 2, 1, 0
    ];

    static Function()
    {
        Cosz = new short[91];
        Tanz = new int[91];
        for (var i = 0; i <= 90; i++)
        {
            Cosz[i] = Sinz[90 - i];
            if (Cosz[i] == 0)
                Tanz[i] = int.MaxValue;
            else
                Tanz[i] = (Sinz[i] << 10) / Cosz[i];
        }
    }

    public static void DrawSimple(DrawingContext context, Bitmap image, int dx, int dy, bool isEditor, Size size,
        double scale, bool isPreviewAll)
    {
        var srcRect = new Rect(image.Size);
        var destRect = isEditor
            ? new Rect(
                dx * 4 + size.Width / 2 - image.Size.Width / 2,
                dy * 4 + size.Height / 1.2 - image.Size.Height / 2,
                image.PixelSize.Width,
                image.PixelSize.Height)
            : isPreviewAll
                ? new Rect(
                    dx + size.Width / 4 - image.Size.Width / 2,
                    dy + size.Height / 1.2 - image.Size.Height,
                    image.PixelSize.Width,
                    image.PixelSize.Height)
                : new Rect(
                    (size.Width - image.Size.Width * scale) / 2 + dx,
                    (size.Height - image.Size.Height * scale) / 2 + dy,
                    image.Size.Width * scale,
                    image.Size.Height * scale);
        context.DrawImage(image, srcRect, destRect);
    }

    public static bool Contains(this Bitmap bitmap, int x0, int y0, Point point, Size size)
    {
        var left = x0 * 4 + size.Width / 2 - bitmap.Size.Width / 2;
        var top = y0 * 4 + size.Height / 1.2 - bitmap.Size.Height / 2;
        var right = left + bitmap.Size.Width;
        var bottom = top + bitmap.Size.Height;

        return point.X >= left && point.X <= right &&
               point.Y >= top && point.Y <= bottom;
    }

    public static int FindDirIndexFromAngle(int angle)
    {
        for (var i = 0; i < ArrowIndex.Length - 1; i++)
            if (angle >= ArrowIndex[i] && angle <= ArrowIndex[i + 1])
            {
                if (i >= 16) return 0;
                return i;
            }

        return 0;
    }

    public static int Sin(int a)
    {
        a = FixAngle(a);
        return a switch
        {
            >= 0 and < 90 => Sinz[a],
            >= 90 and < 180 => Sinz[180 - a],
            >= 180 and < 270 => -Sinz[a - 180],
            _ => -Sinz[360 - a]
        };
    }

    public static int Cos(int a)
    {
        a = FixAngle(a);
        return a switch
        {
            >= 0 and < 90 => Cosz[a],
            >= 90 and < 180 => -Cosz[180 - a],
            >= 180 and < 270 => -Cosz[a - 180],
            _ => Cosz[360 - a]
        };
    }

    public static string ExtractNumbers(string input)
    {
        return MyRegex().Replace(input, "");
    }

    public static int Angle(int dx, int dy)
    {
        int num;
        if (dx != 0)
        {
            var a = Math.Abs((dy << 10) / dx);
            num = Atan(a);
            if (dy >= 0 && dx < 0) num = 180 - num;
            if (dy < 0 && dx < 0) num = 180 + num;
            if (dy < 0 && dx >= 0) num = 360 - num;
        }
        else
        {
            num = dy <= 0 ? 270 : 90;
        }

        return num;
    }

    public static int Atan(int a)
    {
        for (var i = 0; i <= 90; i++)
            if (Tanz[i] >= a)
                return i;

        return 0;
    }

    public static int FixAngle(int angle)
    {
        if (angle >= 360) angle -= 360;
        if (angle < 0) angle += 360;
        return angle;
    }

    public static short? TryParseId(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        try
        {
            return short.Parse(ExtractNumbers(name));
        }
        catch
        {
            return null;
        }
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex MyRegex();

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

    public static int NextInt(int b)
    {
        return Random.Next(b);
    }

    public static sbyte[] ImageToSByteArray(Bitmap image)
    {
        using var ms = new MemoryStream();
        image.Save(ms);
        var byteArray = ms.ToArray();
        var sbyteArray = new sbyte[byteArray.Length];
        for (var i = 0; i < byteArray.Length; i++) sbyteArray[i] = (sbyte)byteArray[i];
        return sbyteArray;
    }

    public static void InvalidateAll(this StackPanel panel, bool recursive = true)
    {
        InvalidateAll(panel as Panel, recursive);
    }

    public static void InvalidateAll(this WrapPanel panel, bool recursive = true)
    {
        InvalidateAll(panel as Panel, recursive);
    }

    public static int FindRangeInArray(ref byte[] arr, ref byte[] template, int offset, bool for_end = false)
    {
        var found = false;
        var res = -1;
        if (arr.Length < template.Length) return res;
        for (var i = offset; i <= arr.Length - template.Length; i++)
        {
            if (arr[i] == template[0])
                for (var j = 1; j < template.Length; j++)
                {
                    if (arr[i + j] != template[j])
                        break;
                    if (j == template.Length - 1)
                    {
                        if (!for_end)
                            res = i;
                        else
                            res = i + j + 1;
                        found = true;
                        break;
                    }
                }

            if (found)
                break;
        }

        return res;
    }

    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }
}