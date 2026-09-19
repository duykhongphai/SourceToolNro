using System;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DrawMap.Classes;

namespace DrawMap.Options;

public class Frame
{
    public short[] Dx { get; set; }
    public short[] Dy { get; set; }
    public sbyte[] IdImg { get; set; }
}

public class ImageInfo
{
    public int Id { get; set; }
    public int X0 { get; set; }
    public int Y0 { get; set; }
    public int W { get; set; }
    public int H { get; set; }
}

public class EffectData
{
    private short _currFrame = -1;
    private bool _isGetTime;
    private int _t;
    public PictureCustom Image { get; set; }
    public ImageInfo[] ImgInfo { get; set; }
    public Frame[] Frame { get; set; }
    public short[] ArrFrame { get; set; }
    public int Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    private ImageInfo GetImageInfo(sbyte id)
    {
        return ImgInfo[id];
    }

    public void Paint(DrawingContext g)
    {
        Paint(g, _currFrame, Image.Width / 2, Image.Height);
    }

    public void Paint(DrawingContext g, int currFrame, double x, double y)
    {
        if (currFrame < 0 || currFrame >= Frame.Length || Image.Source == null) return;

        try
        {
            var frame = Frame[currFrame];
            var imageSource = Image.Source;
            var imageWidth = imageSource.Size.Width;
            var imageHeight = imageSource.Size.Height;

            for (var i = 0; i < frame.Dx.Length; i++)
            {
                var imageInfo = GetImageInfo(frame.IdImg[i]);
                if (imageInfo.X0 < 0 || imageInfo.Y0 < 0 ||
                    imageInfo.X0 + imageInfo.W > imageWidth ||
                    imageInfo.Y0 + imageInfo.H > imageHeight)
                    continue;
                var sourceRect = new Rect(
                    Math.Max(0, imageInfo.X0),
                    Math.Max(0, imageInfo.Y0),
                    Math.Min(imageInfo.W, imageWidth - imageInfo.X0),
                    Math.Min(imageInfo.H, imageHeight - imageInfo.Y0)
                );
                if (sourceRect.Width <= 0 || sourceRect.Height <= 0)
                    continue;
                var destRect = new Rect(
                    x + frame.Dx[i],
                    y + frame.Dy[i],
                    sourceRect.Width,
                    sourceRect.Height
                );
                g.DrawImage(imageSource, sourceRect, destRect);
            }
        }
        catch
        {
        }
    }

    public void UpdateEffect()
    {
        if (ArrFrame != null)
        {
            if (!_isGetTime)
            {
                _isGetTime = true;
                var num = ArrFrame.Length - 1;
                if (num > 0) _t = Function.NextInt(0, num);
            }

            if (_t < ArrFrame.Length) _t++;
            if (_t <= ArrFrame.Length - 1) _currFrame = ArrFrame[_t];
            if (_t >= ArrFrame.Length - 1) _t = 0;
        }

        Image.InvalidateVisual();
    }

    public void Dispose()
    {
        if (Image != null)
        {
            ((Bitmap)Image.Source)?.Dispose();
            Image.Source = null;
        }

        Image = null;
        ImgInfo = null;
        Frame = null;
    }
}