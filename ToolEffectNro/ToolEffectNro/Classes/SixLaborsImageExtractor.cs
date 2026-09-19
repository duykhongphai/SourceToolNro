using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ToolEffectNro.Options;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace ToolEffectNro.Classes;

public class SixLaborsImageExtractor
{
    public static Bitmap CropBitmap(Bitmap sourceBitmap, int x, int y, int width, int height)
    {
        using var sourceStream = new MemoryStream();
        sourceBitmap.Save(sourceStream);
        sourceStream.Position = 0;

        using var image = Image.Load<Rgba32>(sourceStream);

        var actualX = Math.Max(0, x);
        var actualY = Math.Max(0, y);
        var actualWidth = Math.Min(width, image.Width - actualX);
        var actualHeight = Math.Min(height, image.Height - actualY);

        if (actualWidth <= 0 || actualHeight <= 0)
            throw new ArgumentException("Invalid crop dimensions");

        var cropRect = new Rectangle(actualX, actualY, actualWidth, actualHeight);
        image.Mutate(ctx => ctx.Crop(cropRect));

        using var resultStream = new MemoryStream();
        image.SaveAsPng(resultStream);
        resultStream.Position = 0;

        return new Bitmap(resultStream);
    }

    public static async Task<List<ExtractedObject>> ProcessAndExtractAsync(string imagePath,
        bool useContrast = true,
        int threshold = 127,
        int minArea = 1000)
    {
        try
        {
            using var image = await Task.Run(() => Image.Load<Rgba32>(imagePath));
            using var mat = ImageToMat(image);
            using var gray = new Mat();
            Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new Size(5, 5), 0);
            List<Rectangle> regions;
            if (useContrast)
            {
                using var enhanced = new Mat();
                Cv2.EqualizeHist(gray, enhanced);
                using var edges = new Mat();
                Cv2.Canny(enhanced, edges, 30, 150);
                var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
                Cv2.Dilate(edges, edges, kernel);
                Cv2.FindContours(
                    edges,
                    out var contours,
                    out var hierarchy,
                    RetrievalModes.External,
                    ContourApproximationModes.ApproxSimple
                );

                regions = FilterContours(contours, minArea);
            }
            else
            {
                using var binary = new Mat();
                Cv2.Threshold(gray, binary, threshold, 255, ThresholdTypes.Binary);
                Cv2.FindContours(
                    binary,
                    out var contours,
                    out var hierarchy,
                    RetrievalModes.External,
                    ContourApproximationModes.ApproxSimple
                );
                regions = FilterContours(contours, minArea);
            }

            return await ExtractObjectsAsync(image, regions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi xử lý ảnh: {ex.Message}");
            throw;
        }
    }

    public static async Task<List<ExtractedObject>> CutImageByGridAsync(string imagePath, int tileWidth, int tileHeight)
    {
        try
        {
            using var image = await Task.Run(() => Image.Load<Rgba32>(imagePath));
            var imageWidth = image.Width;
            var imageHeight = image.Height;

            var extractedObjects = new List<ExtractedObject>();

            var tilesX = (int)Math.Ceiling((double)imageWidth / tileWidth);
            var tilesY = (int)Math.Ceiling((double)imageHeight / tileHeight);

            for (var row = 0; row < tilesY; row++)
            for (var col = 0; col < tilesX; col++)
            {
                var x = col * tileWidth;
                var y = row * tileHeight;

                var actualWidth = Math.Min(tileWidth, imageWidth - x);
                var actualHeight = Math.Min(tileHeight, imageHeight - y);

                var region = new Rectangle(x, y, actualWidth, actualHeight);

                var extractedObject = await Task.Run(() =>
                {
                    using var tileImage = image.Clone();
                    tileImage.Mutate(ctx => ctx.Crop(region));
                    var bitmap = ImageHelper.ImageSixLaborsToBitmap(tileImage);

                    return new ExtractedObject(
                        bitmap,
                        x,
                        y,
                        actualWidth,
                        actualHeight
                    );
                });

                extractedObjects.Add(extractedObject);
            }

            return extractedObjects;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi cắt ảnh: {ex.Message}");
            throw;
        }
    }

    #region Helper methods

    private static (int rows, int cols) FindOptimalRowsAndCols(int count)
    {
        var cols = (int)Math.Ceiling(Math.Sqrt(count));
        var rows = (int)Math.Ceiling((double)count / cols);
        return (rows, cols);
    }

    private static Mat ImageToMat(Image<Rgba32> image)
    {
        using var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);
        memoryStream.Position = 0;
        return Mat.FromImageData(memoryStream.ToArray());
    }

    private static List<Rectangle> FilterContours(
        Point[][] contours,
        int minArea)
    {
        var rectangles = (from contour in contours
            let area = Cv2.ContourArea(contour)
            where area > minArea
            select Cv2.BoundingRect(contour)
            into rect
            select new Rectangle(rect.X, rect.Y, rect.Width, rect.Height)).ToList();

        rectangles.Sort((r1, r2) => (r2.Width * r2.Height).CompareTo(r1.Width * r1.Height));
        return rectangles;
    }

    private static async Task<List<ExtractedObject>> ExtractObjectsAsync(
        Image<Rgba32> image,
        List<Rectangle> regions)
    {
        var extractedObjects = new List<ExtractedObject>();

        foreach (var region in regions)
        {
            var extractedObject = await Task.Run(() =>
            {
                using var objectImage = image.Clone();
                objectImage.Mutate(ctx => ctx.Crop(region));
                var bitmap = ImageHelper.ImageSixLaborsToBitmap(objectImage);

                return new ExtractedObject(
                    bitmap,
                    region.X,
                    region.Y,
                    region.Width,
                    region.Height
                );
            });

            extractedObjects.Add(extractedObject);
        }

        return extractedObjects;
    }

    #endregion
}