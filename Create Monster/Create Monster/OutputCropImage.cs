using Create_Monster.Class;
using System.Windows.Forms;

namespace Create_Monster
{
    public partial class OutputCropImage : Form
    {
        public OutputCropImage(Image img)
        {
            InitializeComponent();
            Image = img;
        }

        private Image Image;
        public List<Image> images;
        public List<Rectangle> rects;
        public PictureBox pictureBoxSelect;
        public LinkedList<PictureBox> pictureBoxDeletes = new LinkedList<PictureBox>();
        private int indexChildDelete;
        private void OutputCropImage_Load(object sender, EventArgs e)
        {
            var listItem = Function.FastCrop(Image);
            images = listItem.Item1;
            rects = listItem.Item2;
            for (int i = 0; i < images.Count; i++)
            {
                Image img = images[i];
                PictureBox pictureBox = new();
                PictureBox pictureBox1 = new()
                {
                    SizeMode = PictureBoxSizeMode.AutoSize
                };
                pictureBox1.Tag = i;
                pictureBox1.Paint += PictureBox1_Paint;
                pictureBox1.Click += PictureBox1_Click;
                pictureBox1.Image = img;
                flowLayoutPanel1.Controls.Add(pictureBox1);
                flowLayoutPanel1.Controls.SetChildIndex(pictureBox1, i);
            }
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            pictureBoxSelect = sender as PictureBox;
            flowLayoutPanel1.Invalidate(true);
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            if (pictureBoxSelect == pictureBox)
            {
                ControlPaint.DrawBorder(e.Graphics, pictureBox.ClientRectangle, Color.Blue, ButtonBorderStyle.Solid);
            }
            else
            {
                ControlPaint.DrawBorder(e.Graphics, pictureBox.ClientRectangle, Color.FromArgb(255, 196, 142, 164), ButtonBorderStyle.Solid);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            if (pictureBoxSelect == null)
            {
                MessageBox.Show("Vui Lòng Chọn Ảnh Để Di Chuyển", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int indexChild = flowLayoutPanel1.Controls.GetChildIndex(pictureBoxSelect);
            if (indexChild <= 0)
            {
                return;
            }
            int previous = indexChild - 1;
            PictureBox picPrevious = flowLayoutPanel1.Controls[previous] as PictureBox;
            images[indexChild] = picPrevious.Image;
            images[previous] = pictureBoxSelect.Image;
            flowLayoutPanel1.Controls.SetChildIndex(flowLayoutPanel1.Controls[previous], indexChild);
            flowLayoutPanel1.Controls.SetChildIndex(pictureBoxSelect, previous);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            if (pictureBoxSelect == null)
            {
                MessageBox.Show("Vui Lòng Chọn Ảnh Để Di Chuyển", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int indexChild = flowLayoutPanel1.Controls.GetChildIndex(pictureBoxSelect);
            if (indexChild >= flowLayoutPanel1.Controls.Count - 1)
            {
                return;
            }
            int behind = indexChild + 1;
            PictureBox picBehind = flowLayoutPanel1.Controls[behind] as PictureBox;
            images[indexChild] = picBehind.Image;
            images[behind] = pictureBoxSelect.Image;
            flowLayoutPanel1.Controls.SetChildIndex(picBehind, indexChild);
            flowLayoutPanel1.Controls.SetChildIndex(pictureBoxSelect, behind);
        }

        private void btnDeleteImage_Click(object sender, EventArgs e)
        {
            if (pictureBoxSelect == null)
            {
                MessageBox.Show("Vui Lòng Chọn Ảnh Để Xóa", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            indexChildDelete = flowLayoutPanel1.Controls.GetChildIndex(pictureBoxSelect);
            pictureBoxDeletes.AddLast(pictureBoxSelect);
            flowLayoutPanel1.Controls.Remove(pictureBoxSelect);
            images.RemoveAt(int.Parse(pictureBoxSelect.Tag.ToString()));
            for (int i = 0; i < flowLayoutPanel1.Controls.Count; i++)
            {
                PictureBox pic = flowLayoutPanel1.Controls[i] as PictureBox;
                pic.Tag = i;
            }
            pictureBoxSelect = null;
        }

        private void restoreImage_Click(object sender, EventArgs e)
        {
            if (pictureBoxDeletes.Count <= 0)
            {
                return;
            }
            PictureBox picDel = pictureBoxDeletes.Last();
            flowLayoutPanel1.Controls.Add(picDel);
            flowLayoutPanel1.Controls.SetChildIndex(picDel, indexChildDelete);
            images.Insert(int.Parse(picDel.Tag.ToString()), picDel.Image);
            pictureBoxDeletes.RemoveLast();
        }
    }
}
