using Avalonia.Media.Imaging;

namespace ToolEffectNro.Options;

public class ExtractedObject
{
    public ExtractedObject(Bitmap image, int x, int y, int width, int height)
    {
        Image = image;
        X = x;
        Y = y;
    }

    public Bitmap Image { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}