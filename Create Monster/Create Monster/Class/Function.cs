using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Create_Monster.Class
{
    public class Function
    {
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening URL: {ex.Message}");
            }
        }


        public static string ConvertArrayToString<T>(List<T> array)
        {
            StringBuilder sb = new StringBuilder("[");
            int count = array.Count;
            for (int i = 0; i < count; i++)
            {
                var item = array[i];
                if (item != null)
                {
                    sb.Append(item.GetType().IsPrimitive ? item : item.ToString());
                    if (i < count - 1)
                    {
                        sb.Append(", ");
                    }
                }
            }
            if (sb.Length > 2)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static string ConvertDictionaryToString<K, T>(Dictionary<K, List<T>> dicrionary)
        {
            StringBuilder sb = new StringBuilder("[");
            foreach (var key in dicrionary.Keys)
            {
                sb.Append(ConvertArrayToString(dicrionary[key].ToList()));
                sb.Append(", ");
            }
            if (sb.Length > 2)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static (List<System.Drawing.Image>, List<Rectangle>) FastCrop(System.Drawing.Image img)
        {
            List<System.Drawing.Image> images = new List<System.Drawing.Image>();
            List<Rectangle> Rectangles = new List<Rectangle>();
            Bitmap largeImage = new Bitmap(img);
            List<Rectangle> elementRectangles = FindElements(largeImage);
            for (int i = 0; i < elementRectangles.Count; i++)
            {
                Rectangle elementRectangle = elementRectangles[i];
                Bitmap subImage = largeImage.Clone(elementRectangle, largeImage.PixelFormat);
                bool isTransparent = IsSubImageTransparent(subImage);
                if (!isTransparent)
                {
                    images.Add(subImage);
                    Rectangles.Add(elementRectangle);
                }
            }
            return (images, Rectangles);
        }

        private static List<Rectangle> FindElements(Bitmap largeImage)
        {
            List<Rectangle> elementRectangles = new List<Rectangle>();
            Bitmap grayImage = Grayscale.CommonAlgorithms.RMY.Apply(largeImage);
            SobelEdgeDetector edgeDetector = new SobelEdgeDetector();
            Bitmap edgeImage = edgeDetector.Apply(grayImage);
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(edgeImage);
            AForge.Imaging.Blob[] blobs = blobCounter.GetObjectsInformation();
            foreach (var blob in blobs)
            {
                Rectangle elementRectangle = blob.Rectangle;
                if (blob.Area > 100)
                {
                    elementRectangles.Add(elementRectangle);
                }
            }
            return elementRectangles;
        }

        private static bool IsSubImageTransparent(Bitmap subImage)
        {
            for (int y = 0; y < subImage.Height; y++)
            {
                for (int x = 0; x < subImage.Width; x++)
                {
                    int alphaValue = subImage.GetPixel(x, y).A;

                    if (alphaValue != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static System.Drawing.Image ResizeImage(System.Drawing.Image img, int w, int h)
        {
            System.Drawing.Image sc = new Bitmap(img);
            System.Drawing.Image newImage = ScaleImage(sc, w, h);
            sc.Dispose();
            return newImage;
        }

        private static Bitmap ScaleImage(System.Drawing.Image image, int width, int height)
        {
            try
            {
                var destRect = new Rectangle(0, 0, width, height);
                var destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                    {
                        wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }
                return destImage;
            }
            catch
            {
                return null;
            }
        }

        public static sbyte[] ImageToSByteArray(System.Drawing.Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteArray = ms.ToArray();
                sbyte[] sbyteArray = new sbyte[byteArray.Length];
                for (int i = 0; i < byteArray.Length; i++)
                {
                    sbyteArray[i] = (sbyte)byteArray[i];
                }
                return sbyteArray;
            }
        }

        private static System.Drawing.Image createImage(sbyte[] imageData, int offset, int lenght)
        {
            if (offset + lenght > imageData.Length)
            {
                return null;
            }
            byte[] array = new byte[lenght];
            for (int i = 0; i < lenght; i++)
            {
                array[i] = convertSbyteToByte(imageData[i + offset]);
            }
            return ByteArrayToImage(array);
        }

        public static System.Drawing.Image ByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                return new Bitmap(image);
            }
        }

        private static byte convertSbyteToByte(sbyte var)
        {
            if ((int)var > 0)
            {
                return (byte)var;
            }
            return (byte)((int)var + 256);
        }

        public static System.Drawing.Image createImage(sbyte[] arr)
        {
            return createImage(arr, 0, arr.Length);
        }
    }
}
