using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Create_Monster.Class
{
    public class ImageInfo
    {
        public byte ID { get; set; }
        public short X0 { get; set; }
        public short Y0 { get; set; }
        public Image Image { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public ImageInfo(byte id, short x0, short y0, Image image)
        {
            ID = id;
            X0 = x0;
            Y0 = y0;
            Image = image;
            W = image.Width;
            H = image.Height;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            sb.Append($"\"id\": {ID}, ");
            sb.Append($"\"x\": {X0 / 4}, ");
            sb.Append($"\"y\": {Y0 / 4}, ");
            sb.Append($"\"w\": {W / 4}, ");
            sb.Append($"\"h\": {H / 4}, ");
            sb.Append('}');
            return sb.ToString();
        }

        public ImageInfo Clone()
        {
            return new ImageInfo(ID, X0, Y0, Image);
        }
    }
}
