using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ToolEffectNro.Classes;

public class ImageHelper
{
    public static Bitmap LoadFromResource(Uri resourceUri)
    {
        return new Bitmap(AssetLoader.Open(resourceUri));
    }

    public static async Task<Bitmap?> LoadFromWeb(Uri url)
    {
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
            return null;
        }
    }

    public static Bitmap CropBitmap(Bitmap source, int x, int y, int width, int height)
    {
        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (x + width > source.PixelSize.Width) width = source.PixelSize.Width - x;
        if (y + height > source.PixelSize.Height) height = source.PixelSize.Height - y;
        
        var croppedRenderTarget = new RenderTargetBitmap(new PixelSize(width, height));
        var sourceRect = new Rect(x, y, width, height);
        var destRect = new Rect(0, 0, width, height);
        
        using var context = croppedRenderTarget.CreateDrawingContext();
        context.DrawImage(source, sourceRect, destRect);
        return ConvertRenderTargetToBitmap(croppedRenderTarget);
    }
    private static Bitmap ConvertRenderTargetToBitmap(RenderTargetBitmap renderTargetBitmap)
    {
        using var memoryStream = new MemoryStream();
        renderTargetBitmap.Save(memoryStream);
        memoryStream.Position = 0;
        return new Bitmap(memoryStream);
    }
    public static Bitmap ImageSixLaborsToBitmap(Image<Rgba32> image)
    {
        using var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);
        memoryStream.Position = 0;
        return new Bitmap(memoryStream);
    }
}