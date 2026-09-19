using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ToolPart.Options;

namespace ToolPart.Classes;

public static partial class Function
{
    private static readonly Random Random = new();

    public static string ExtractNumbers(string input)
    {
        return MyRegex().Replace(input, "");
    }

    public static int? TryParseId(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        return int.TryParse(ExtractNumbers(name), out var id) ? id : null;
    }

    public static Bitmap CloneBitmap(this Bitmap original)
    {
        using var ms = new MemoryStream();
        original.Save(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return new Bitmap(ms);
    }

    public static void PutItem(this Dictionary<sbyte, List<PanelFrame>> dictionary, sbyte key, PanelFrame panel)
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = [];
            dictionary.Add(key, value);
        }

        value.Add(panel);
        panel.Tag = value.IndexOf(panel);
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
        return a == b ? a : a + Random.Next(b - a);
    }

    public static int CountNotNull(List<PartImage> a)
    {
        return a == null ? 0 : a.Count(p => p != null);
    }

    public static void InvalidateAll(this StackPanel panel, bool recursive = true)
    {
        InvalidateAll(panel as Panel, recursive);
    }

    public static void InvalidateAll(this WrapPanel panel, bool recursive = true)
    {
        InvalidateAll(panel as Panel, recursive);
    }

    public static string ConvertArrayToString(List<PartImage> array)
    {
        if (array == null) return "[]";
        return "[" + string.Join(", ", array.Select(item => item?.ToString() ?? "null")) + "]";
    }

    public static string CleanJson(string serializedJson, params string[] replacements)
    {
        serializedJson = replacements.Aggregate(serializedJson, (current, replacement) => current.Replace(replacement,
            replacement switch
            {
                "{" => "[",
                "}" => "]",
                _ => ""
            }));
        serializedJson = serializedJson.Replace("\n", "").Replace("\r", "");
        return serializedJson;
    }

    public static string ConvertDaysToYearsMonthsDays(int inputDays)
    {
        var years = inputDays / 365;
        var remainingDays = inputDays % 365;
        var months = remainingDays / 30;
        var days = remainingDays % 30;
        List<string> parts = [];
        if (years > 0)
            parts.Add($"{years} Năm");
        if (months > 0)
            parts.Add($"{months} Tháng");
        if (days > 0)
            parts.Add($"{days} Ngày");
        return parts.Count == 0 ? "0 Ngày" : string.Join(" ", parts);
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex MyRegex();
}