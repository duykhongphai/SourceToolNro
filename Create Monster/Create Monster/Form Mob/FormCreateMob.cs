using Create_Monster.Class;
using Guna.UI2.WinForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Create_Monster.Form_Mob
{
    public partial class FormCreateMob : Form
    {
        public FormCreateMob(MobForm mobForm)
        {
            InitializeComponent();
            this.mainForm = mobForm;
            addframeForm = new AddFrame(this);
        }

        #region Variable
        public MobForm mainForm;
        private ChildFrame selectedImage = null;
        private ChildFrame ImageHover = null;
        public AddFrame addframeForm;
        public PanelFrame selectParentFrame;

        public Image Char;
        private Point offset;
        public int MouseX;
        public int MouseY;
        public bool isOpenPointForm;

        public static int XOriginal;
        public static int YOriginal;

        public float scaleX;
        public float scaleY;

        private string[] toolTip = { "Stand Frame", "Stand Frame", "Move Frame", "Move Frame", "Punch Frame", "Punch Frame", "Punch Frame", "Kame Frame", "Kame Frame", "Kame Frame", "Die Frame", "Die Frame" };
        private string[] name = { "stand1", "stand2", "move1", "move2", "punch1", "punch2", "punch3", "kame1", "kame2", "kame3", "die1", "die2" };
        #endregion

        #region Function
        private bool buildData()
        {
            try
            {
                int idMob = 0;
                int.TryParse(mainForm.txtIdMob.Text, out idMob);
                for (int i = 1; i <= 4; i++)
                {
                    if (!Directory.Exists($"Mob//x{i}"))
                    {
                        Directory.CreateDirectory($"Mob//x{i}");
                    }
                }
                DataOutputStream toByteArray = new DataOutputStream();
                toByteArray.writeByte((sbyte)mainForm.InfoImage.Keys.Count);
                for (byte i = 0; i < mainForm.InfoImage.Keys.Count; i++)
                {
                    ImageInfo info = mainForm.InfoImage[i];
                    toByteArray.writeByte((sbyte)info.ID);
                    toByteArray.writeByte((sbyte)(info.X0 / 4));
                    toByteArray.writeByte((sbyte)(info.Y0 / 4));
                    toByteArray.writeByte((sbyte)(info.Image.Width / 4));
                    toByteArray.writeByte((sbyte)(info.Image.Height / 4));
                }
                toByteArray.writeShort((short)mainForm.ParentFrame.Keys.Count);
                for (short i = 0; i < mainForm.ParentFrame.Keys.Count; i++)
                {
                    List<ChildFrame> childFrames = mainForm.ParentFrame[i];
                    toByteArray.writeByte((sbyte)childFrames.Count);
                    for (int j = 0; j < childFrames.Count; j++)
                    {
                        ChildFrame chill = childFrames[j];
                        toByteArray.writeShort((short)((chill.Bounds.Location.X - YOriginal) / 4));
                        toByteArray.writeShort((short)((chill.Bounds.Location.Y - XOriginal) / 4));
                        toByteArray.writeByte((sbyte)chill.ImageInfo.ID);
                    }
                }
                toByteArray.writeShort(0);
                sbyte[] dataMob = toByteArray.toByteArray();
                for (int i = 1; i <= 4; i++)
                {
                    int percent = 25 * i;
                    Image img = Function.ResizeImage(mainForm.picDefault.Image, mainForm.picDefault.Image.Width * percent / 100, mainForm.picDefault.Image.Height * percent / 100);
                    DataOutputStream dataOutputStream = new DataOutputStream();
                    sbyte[] image = Function.ImageToSByteArray(img);
                    if (mainForm.cbHasMobId.Checked)
                    {
                        dataOutputStream.writeByte((sbyte)idMob);
                    }
                    dataOutputStream.writeByte(0);
                    dataOutputStream.writeInt(dataMob.Length);
                    dataOutputStream.write(dataMob);
                    dataOutputStream.writeInt(image.Length);
                    dataOutputStream.write(image);
                    dataOutputStream.writeByte(0);
                    File.WriteAllBytes($"Mob//x{i}//{idMob}", Array.ConvertAll(dataOutputStream.toByteArray(), a => (byte)a));
                    dataOutputStream.close();
                }
                toByteArray.close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void CreatePictureBox()
        {
            for (short i = 0; i < 12; i++)
            {
                PanelFrame pic = new(this)
                {
                    Size = new Size(154, 109),
                    Location = new Point(14, 6 + (115 * i)),
                    Name = name[i],
                    Tag = i
                };
                pic.Click += pic_Click;
                PanelFrame clone = new(this)
                {
                    isClone = true,
                    Dock = DockStyle.Fill
                };
                clone.BringToFront();
                clone.Tag = pic.Tag;
                clone.MouseClick += panelFrame_MouseClick;
                clone.MouseLeave += panelFrame_MouseLeave;
                clone.MouseDown += panelFrame_MouseDown;
                clone.MouseUp += panelFrame_MouseUp;
                clone.MouseMove += panelFrame_MouseMove;
                pic.clone = clone;
                toolTip1.SetToolTip(pic, toolTip[i]);
                guna2Panel3.Controls.Add(pic);
                mainForm.ParentFrame.TryAdd(i, new List<ChildFrame>());
            }
        }

        public ImageInfo getImageInfo(byte id)
        {
            ImageInfo info = null;
            mainForm.InfoImage.TryGetValue(id, out info);
            return info;
        }

        public ChildFrame getChildFrame(Point e)
        {
            ChildFrame childFrame = null;
            foreach (var value in mainForm.ParentFrame[short.Parse(selectParentFrame.Tag.ToString())])
            {
                if (value.Bounds.Contains(e))
                {
                    childFrame = value;
                    break;
                }
            }
            return childFrame;
        }

        private void unSelect(PanelFrame frame)
        {
            foreach (var control in guna2Panel3.Controls)
            {
                PanelFrame panel = control as PanelFrame;
                if (panel != null && panel.isSelected && panel != frame)
                {
                    panel.isSelected = false;
                    panel.Invalidate();
                }
            }
        }

        #endregion

        #region Event

        private void pic_Click(object sender, EventArgs e)
        {
            PanelFrame pictureBox = sender as PanelFrame;
            pictureBox.isSelected = true;
            if (selectParentFrame != null)
            {
                selectParentFrame.isSelected = false;
                selectParentFrame.Invalidate();
            }
            guna2Panel4.Controls.Clear();
            selectParentFrame = pictureBox;
            guna2Panel4.Controls.Add(pictureBox.clone);
            pictureBox.Invalidate();
        }

        private void panelFrame_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PanelFrame panelFrame = sender as PanelFrame;
                var frameChild = this.getChildFrame(e.Location);
                if (frameChild != null)
                {
                    ContextMenuStrip contextMenu = new ContextMenuStrip();
                    ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("Delete");
                    deleteMenuItem.Tag = frameChild.ID;
                    deleteMenuItem.Click += DeleteMenuItem_Click;
                    contextMenu.Items.Add(deleteMenuItem);
                    panelFrame.ContextMenuStrip = contextMenu;
                    return;
                }
                ContextMenuStrip contextFrame = new ContextMenuStrip();
                ToolStripMenuItem addF = new ToolStripMenuItem("Add Image");
                addF.Click += AddFrame_Click;
                contextFrame.Items.Add(addF);
                panelFrame.ContextMenuStrip = contextFrame;
            }
        }

        private void panelFrame_MouseLeave(object sender, EventArgs e)
        {
            PanelFrame panelFrame = sender as PanelFrame;
            panelFrame.ContextMenuStrip = null;
        }

        private void panelFrame_MouseDown(object sender, MouseEventArgs e)
        {
            if (selectedImage != null) return;
            ChildFrame selectedFrame = getChildFrame(e.Location);
            if (selectedFrame != null)
            {
                mainForm.isStopWrite = true;
                selectedImage = selectedFrame;
                offset = new Point(e.X - selectedFrame.Bounds.Left, e.Y - selectedFrame.Bounds.Top);
                selectedFrame.BorderPen = Pens.Red;
                guna2Panel4.Invalidate();
                guna2Panel3.Invalidate(true);
            }
        }

        private void panelFrame_MouseMove(object sender, MouseEventArgs e)
        {
            ImageHover = null;
            PanelFrame panelFrame = sender as PanelFrame;
            if (selectedImage == null)
            {
                var frameChild = this.getChildFrame(panelFrame.PointToClient(Control.MousePosition));
                if (frameChild != null)
                {
                    ImageHover = frameChild;
                }
            }
            else
            {
                ImageHover = selectedImage;
            }
            MouseX = e.X - YOriginal;
            MouseY = e.Y - XOriginal;
            txtXY.Text = $"X: {MouseX} - Y: {MouseY}" + (ImageHover != null ? $"    ID: {ImageHover.ID} X: {ImageHover.Bounds.X - YOriginal} - Y: {ImageHover.Bounds.Y - XOriginal}" : "");
            if (e.Button == MouseButtons.Left && selectedImage != null)
            {
                selectedImage.Bounds = new Rectangle(e.X - offset.X, e.Y - offset.Y, selectedImage.Bounds.Width, selectedImage.Bounds.Height);
                panelFrame.Invalidate();
            }
        }

        private void panelFrame_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedImage != null)
            {
                PanelFrame panelFrame = sender as PanelFrame;
                mainForm.isStopWrite = false;
                selectedImage.BorderPen = Pens.Black;
                selectedImage = null;
                panelFrame.Invalidate();
                guna2Panel3.Invalidate(true);
            }
        }

        private void AddFrame_Click(object sender, EventArgs e)
        {
            if (addframeForm.ShowDialog() == DialogResult.OK)
            {
                if (selectParentFrame != null)
                {
                    short idParent = short.Parse(selectParentFrame.Tag.ToString());
                    mainForm.ParentFrame.TryGetValue(idParent, out List<ChildFrame> chill);
                    if (chill.Count + 1 > byte.MaxValue)
                    {
                        MessageBox.Show("Chỉ có thể thêm tối đa " + byte.MaxValue + " ảnh", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    for (int i = 0; i < addframeForm.idImages.Count; i++)
                    {
                        mainForm.ParentFrame[idParent].Add(new ChildFrame((byte)chill.Count, idParent, addframeForm.idImages[i], this));
                    }
                    selectParentFrame.Invalidate();
                    guna2Panel4.Invalidate(true);
                    guna2Panel3.Invalidate(true);
                }
            }
        }

        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag != null)
            {
                byte idChild = byte.Parse(menuItem.Tag.ToString());
                short idParent = short.Parse(selectParentFrame.Tag.ToString());
                if (mainForm.ParentFrame.TryGetValue(idParent, out List<ChildFrame> value))
                {
                    ChildFrame frame = value.FirstOrDefault(a => a.ID == idChild);
                    mainForm.deletedImagesStack.Push(frame);
                    value.Remove(frame);
                    selectParentFrame.Invalidate();
                    guna2Panel3.Invalidate(true);
                }
            }
        }

        private void FormCreateMob_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                if (mainForm.deletedImagesStack.Count > 0)
                {
                    ChildFrame frame = mainForm.deletedImagesStack.Pop();
                    frame.ID = (byte)mainForm.ParentFrame[frame.IDParent].Count;
                    mainForm.ParentFrame[frame.IDParent].Add(frame);
                    selectParentFrame.Invalidate();
                    guna2Panel3.Invalidate(true);
                }
            }
            switch (e.KeyCode)
            {
                case Keys.F5:
                    if (buildData())
                    {
                        MessageBox.Show("Thành Công", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    break;
            }
        }

        private void FormCreateMob_Resize(object sender, EventArgs e)
        {
            scaleX = (float)154 / guna2Panel4.Width;
            scaleY = (float)109 / guna2Panel4.Height;
            YOriginal = guna2Panel4.Width / 2;
            XOriginal = (int)Math.Round(guna2Panel4.Height * 0.87);
            guna2Panel3.Invalidate(true);
        }

        private void FormCreateMob_Load(object sender, EventArgs e)
        {
            CreatePictureBox();
        }

        private void FormCreateMob_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.mainForm.isOpenFormMob = false;
            this.mainForm.formMob = new FormCreateMob(mainForm);
        }

        private void FormCreateMob_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        #endregion
    }
}
