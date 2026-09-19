using System.Text;
using Avalonia.Media.Imaging;

namespace ToolEffectNro.Classes;

public class ImageInfo
{
    public ImageInfo(byte id, short x0, short y0, Bitmap image)
    {
        ID = id;
        X0 = x0;
        Y0 = y0;
        Image = image;
        W = (int)image.Size.Width;
        H = (int)image.Size.Height;
    }

    public byte ID { get; set; }
    public short X0 { get; set; }
    public short Y0 { get; set; }
    public Bitmap Image { get; set; }
    public int W { get; set; }
    public int H { get; set; }

    public void Dispose()
    {
        Image?.Dispose();
        Image = null;
    }

    public string ToJsonString()
    {
        var sb = new StringBuilder("{");
        sb.Append($"\"id\": {ID}, ");
        sb.Append($"\"x\": {X0 / 4}, ");
        sb.Append($"\"y\": {Y0 / 4}, ");
        sb.Append($"\"w\": {Image.PixelSize.Width / 4}, ");
        sb.Append($"\"h\": {Image.PixelSize.Height / 4}");
        sb.Append('}');
        return sb.ToString();
    }
}