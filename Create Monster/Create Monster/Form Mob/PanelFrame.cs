using Create_Monster.Class;
using Create_Monster.Form_Mob;

namespace Create_Monster
{
    public partial class PanelFrame : PictureBox
    {
        public bool isClone;
        public bool isSelected;
        public FormCreateMob mobForm;
        public PanelFrame clone;

        public PanelFrame(FormCreateMob mob = null)
        {
            InitializeComponent();
            mobForm = mob;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawCrossLinesAndBorder(e.Graphics);
            DrawFrames(e.Graphics);
        }

        private void DrawCrossLinesAndBorder(Graphics graphics)
        {
            using Pen borderPen = new(isClone ? Color.Blue : (isSelected ? Color.Green : Color.OrangeRed), 1);
            graphics.DrawLine(Pens.Black, 0, isClone ? FormCreateMob.XOriginal : 0, this.Width, isClone ? FormCreateMob.XOriginal : 0);
            graphics.DrawLine(Pens.Black, isClone ? FormCreateMob.YOriginal : 0, 0, isClone ? FormCreateMob.YOriginal : 0, this.Height);
            graphics.DrawRectangle(borderPen, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
        }

        private void DrawFrames(Graphics graphics)
        {
            short key = short.Parse(this.Tag.ToString());
            if (mobForm != null)
            {
                DrawFrameChildren(graphics, mobForm.mainForm.ParentFrame[key], mobForm.scaleX, mobForm.scaleY);
            }
        }

        private void DrawFrameChildren(Graphics graphics, List<ChildFrame> frameChildren, float scaleX, float scaleY)
        {
            foreach (var frameChild in frameChildren)
            {
                if (isClone)
                {
                    graphics.DrawImage(frameChild.ImageInfo.Image, frameChild.Bounds);
                    graphics.DrawRectangle(frameChild.BorderPen, frameChild.Bounds);
                }
                else
                {
                    Image img = Function.ResizeImage(frameChild.ImageInfo.Image, (int)(frameChild.ImageInfo.Image.Width * scaleX), (int)(frameChild.ImageInfo.Image.Height * scaleY));
                    graphics.DrawImage(img, frameChild.Bounds.X * scaleX, frameChild.Bounds.Y * scaleY);
                }
            }
        }
    }
}
