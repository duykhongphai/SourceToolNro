using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace CreateSkillNro.Classes;

public class ImageInfo
{
    public ImageInfo(short id, Border container, Bitmap image)
    {
        Id = id;
        Image = image;
        Container = container;
    }

    public ImageInfo(Bitmap image)
    {
        Image = image;
    }

    public short Id { get; set; }
    public Bitmap Image { get; set; }
    public Border Container { get; set; }

    public void Dispose()
    {
        Container = null;
        Image = null;
    }
}