using Create_Monster.Class;
using Create_Monster.Form_Mob;
using System.Linq;

namespace Create_Monster
{
    public partial class AddFrame : Form
    {
        private readonly FormCreateMob formFrame;
        public readonly List<byte> idImages = new();

        public AddFrame(FormCreateMob formFrame)
        {
            InitializeComponent();
            this.formFrame = formFrame;
        }

        private void InitPicture()
        {
            var imageDict = formFrame?.mainForm.InfoImage;
            foreach (var kvp in imageDict)
            {
                AddPictureBox(kvp.Key, kvp.Value);
            }
        }

        private void AddPictureBox(byte key, ImageInfo imageInfo)
        {
            var pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.AutoSize,
                Tag = key,
                Image = imageInfo.Image
            };
            pictureBox.Paint += PictureBox_Paint;
            pictureBox.Click += PictureBox_Click;
            flowLayoutPanel1.Controls.Add(pictureBox);
            flowLayoutPanel1.Controls.SetChildIndex(pictureBox, key);
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox pic && byte.TryParse(pic.Tag.ToString(), out byte key))
            {
                if (idImages.Contains(key))
                {
                    idImages.Remove(key);
                }
                else
                {
                    idImages.Add(key);
                }
                flowLayoutPanel1.Invalidate(true);
            }
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (sender is PictureBox pictureBox && byte.TryParse(pictureBox.Tag.ToString(), out byte key))
            {
                Color borderColor = idImages.Contains(key) ? Color.Blue : Color.FromArgb(255, 196, 142, 164);
                ControlPaint.DrawBorder(e.Graphics, pictureBox.ClientRectangle, borderColor, ButtonBorderStyle.Solid);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void AddFrame_Load(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();
            GC.Collect();
            InitPicture();
        }
    }
}
