using Create_Monster.Form_Mob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Create_Monster.Class
{
    public class ChildFrame
    {
        public byte ID;
        public short IDParent;
        public ImageInfo ImageInfo { get; set; }
        public Rectangle Bounds { get; set; }

        public Pen BorderPen = Pens.Black;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            sb.Append($"\"image_id\": {ImageInfo.ID}, ");
            sb.Append($"\"dx\": {(Bounds.X - FormCreateMob.YOriginal) / 4}, ");
            sb.Append($"\"dy\": {(Bounds.Y - FormCreateMob.XOriginal) / 4}, ");
            sb.Append('}');
            return sb.ToString();
        }

        public ChildFrame(byte id, short IDParent, byte IdImg, FormCreateMob form)
        {
            this.ID = id;
            this.IDParent = IDParent;
            this.ImageInfo = form.getImageInfo(IdImg);
            this.Bounds = new Rectangle(0, 0, ImageInfo.Image.Width, ImageInfo.Image.Height);
        }

        public ChildFrame(byte id, short IDParent, byte IdImg, short x, short y, FormCreateMob form)
        {
            this.ID = id;
            this.IDParent = IDParent;
            this.ImageInfo = form.getImageInfo(IdImg);
            this.Bounds = new Rectangle(x, y, ImageInfo.Image.Width, ImageInfo.Image.Height);
        }
    }
}
