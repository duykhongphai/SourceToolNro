using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace ToolEffectNro.Classes;

public static class Function
{
    private static readonly Random _random = new();

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

    public static string ToJsonString(this Dictionary<byte, ImageInfo> images)
    {
        return $"[{string.Join(",", images.Values.Select(v => v.ToJsonString()))}]";
    }

    public static string ToJsonString(this Dictionary<short, List<ChildFrame>> frames)
    {
        return $"[{string.Join(",",
            frames.Values.Select(list =>
                $"[{string.Join(",", list.Select(cf => cf.ToJsonString()))}]"
            )
        )}]";
    }

    public static string ToJsonArray(this List<short> list)
    {
        return $"[{string.Join(",", list)}]";
    }

    public static int NextInt(int a, int b)
    {
        return a == b ? a : a + _random.Next(b - a);
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