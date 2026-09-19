using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Media.Imaging;
using ToolPart.Windows;

namespace ToolPart.Options;

public class PartImage
{
    public bool IsSelected;

    public PartImage()
    {
    }

    public PartImage(short id, int dx, int dy, Bitmap image, BodyEnum partType)
    {
        Id = id;
        Dx = dx;
        Dy = dy;
        Image = image;
        Bounds = new Rect(new Point(-999, -999), Image.Size);
        PartType = partType;
    }

    public short Id { get; set; }

    public int Dx { get; set; }

    public int Dy { get; set; }

    public Bitmap Image { get; set; }

    public Rect Bounds { get; set; }

    public BodyEnum PartType { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder("{");
        var propertyValues = new Dictionary<string, int>
        {
            { "id", Id },
            { "dx", Dx },
            { "dy", Dy }
        };
        if (WindowExecution.Instance.TypeSort == BodyEnum.DxDyId)
            propertyValues = new Dictionary<string, int>
            {
                { "dx", Dx },
                { "dy", Dy },
                { "id", Id }
            };
        foreach (var kvp in propertyValues) sb.Append($"\"{kvp.Key}\": {kvp.Value}, ");
        if (sb.Length > 1) sb.Length -= 2;
        sb.Append('}');
        return sb.ToString();
    }

    public void UpdatePoint(int x, int y)
    {
        Bounds = new Rect(new Point(x, y), Image.Size);
    }
}