using System.Text;
using Avalonia;
using Avalonia.Media;
using ToolEffectNro.ChildWindows;
using ToolEffectNro.Windows;

namespace ToolEffectNro.Classes;

public class ChildFrame
{
    public readonly byte Id;
    public readonly short IdParent;
    public IPen BorderPen = new Pen(Brushes.Black);

    public ChildFrame(byte id, short idParent, byte idImg)
    {
        Id = id;
        IdParent = idParent;
        ImageInfo = FormMainEffect.Instance.GetImageInfo(idImg);
        Bounds = new Rect(0, 0, ImageInfo.Image.PixelSize.Width, ImageInfo.Image.PixelSize.Height);
    }

    public ChildFrame(byte id, short idParent, byte idImg, short x, short y)
    {
        Id = id;
        IdParent = idParent;
        ImageInfo = FormMainEffect.Instance.GetImageInfo(idImg);
        Bounds = new Rect(x, y, ImageInfo.Image.PixelSize.Width, ImageInfo.Image.PixelSize.Height);
    }

    public ImageInfo ImageInfo { get; set; }

    public Rect Bounds { get; set; }

    public bool Contains(Point point)
    {
        var left = Bounds.X + WindowExecution.YOriginal;
        var top = Bounds.Y + WindowExecution.XOriginal;
        var right = left + Bounds.Width;
        var bottom = top + Bounds.Height;

        return point.X >= left && point.X <= right &&
               point.Y >= top && point.Y <= bottom;
    }

    public void Dispose()
    {
        ImageInfo?.Dispose();
    }

    public string ToJsonString()
    {
        var sb = new StringBuilder("{");
        sb.Append($"\"image_id\": {ImageInfo.ID}, ");
        sb.Append($"\"dx\": {(int)Bounds.X / 4}, ");
        sb.Append($"\"dy\": {(int)Bounds.Y / 4}");
        sb.Append('}');
        return sb.ToString();
    }
}