using Create_Monster.Class;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Create_Monster.Form_Mob
{
    public partial class MobForm : Form
    {
        public MobForm()
        {
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            InitializeComponent();
            formMob = new FormCreateMob(this);
        }

        #region Variable
        private Image imageCrop;
        private Point startPoint;
        public RectangleF cropRectangle;
        private ImageInfo DataPreventive;
        public FormCreateMob formMob;
        public bool isOpenFormMob;
        public bool isOpenFormHDSD;
        public Dictionary<short, List<ChildFrame>> ParentFrame = new Dictionary<short, List<ChildFrame>>();
        public Stack<ChildFrame> deletedImagesStack = new Stack<ChildFrame>();
        public Dictionary<byte, ImageInfo> InfoImage = new Dictionary<byte, ImageInfo>();
        private static string localLowPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "..\\LocalLow");
        private static string dataPath = Path.Combine(localLowPath, "CreateMob");
        private string dataFilePathCheck = Path.Combine(dataPath, "dontClose.mobNRO");
        private string dataFilePathSave = Path.Combine(dataPath, "dataCreateImage.mobNRO");
        private bool isCloseWinform;
        public bool isStopWrite = true;
        private byte[] key = new byte[32];
        private byte[] iv = new byte[16];
        private string filePathIV = Path.Combine(Path.GetTempPath(), "fsdfstertedfsdf");
        private string filePathKEY = Path.Combine(Path.GetTempPath(), "tewrturit");
        public string fileName;
        public bool isOpenPointForm;

        private PointF offset;

        private enum MouseOperation
        {
            None,
            Nwse,
            Ns,
            Nesw,
            Ew,
            Senw,
            Sn,
            Swne,
            We,
            Move,
            Crop
        }

        private enum AnchorEnum
        {
            None,
            Nwse,
            Ns,
            Nesw,
            Ew,
            Senw,
            Sn,
            Swne,
            We
        }

        private MouseOperation _eMouseOperation = MouseOperation.None;
        private RectangleF _rcMoveResizeInitRect;
        private Region[] _rgAnchors = new Region[(int)AnchorEnum.We + 1];
        private Size szAnhorSize = new Size(8, 8);
        #endregion

        #region Function
        private void DisposeRegions()
        {
            for (AnchorEnum eAnchor = AnchorEnum.None; eAnchor <= AnchorEnum.We; eAnchor++)
            {
                if (_rgAnchors[(int)eAnchor] != null)
                {
                    _rgAnchors[(int)eAnchor].Dispose();
                    _rgAnchors[(int)eAnchor] = null;
                }
            }
        }

        private PointF RotatePoint(PointF pointToRotate, PointF centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new PointF() { X = Convert.ToInt32(cosTheta * (pointToRotate.X - centerPoint.X) - sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X), Y = Convert.ToInt32(sinTheta * (pointToRotate.X - centerPoint.X) + cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y) };
        }

        private Cursor AnchorToCursor(AnchorEnum eAnchor)
        {
            float snAngle = 0.0f;
            switch (eAnchor)
            {
                case AnchorEnum.Nwse:
                case AnchorEnum.Senw:
                    snAngle += 45f;
                    break;
                case AnchorEnum.Ns:
                case AnchorEnum.Sn:
                    snAngle += 90f;
                    break;
                case AnchorEnum.Nesw:
                case AnchorEnum.Swne:
                    snAngle += 135f;
                    break;
                default:
                    break;
            }
            if (snAngle > 360)
            {
                snAngle -= 360;
            }
            if ((Convert.ToInt32(snAngle) >= 26 && Convert.ToInt32(snAngle) <= 68) || (Convert.ToInt32(snAngle) >= 204 && Convert.ToInt32(snAngle) <= 248))
            {
                return Cursors.SizeNWSE;
            }
            else if ((Convert.ToInt32(snAngle) >= 69 && Convert.ToInt32(snAngle) <= 113) || (Convert.ToInt32(snAngle) >= 249 && Convert.ToInt32(snAngle) <= 293))
            {
                return Cursors.SizeNS;
            }
            else if ((Convert.ToInt32(snAngle) >= 114 && Convert.ToInt32(snAngle) <= 158) || (Convert.ToInt32(snAngle) >= 294 && Convert.ToInt32(snAngle) <= 338))
            {
                return Cursors.SizeNESW;
            }
            else
            {
                return Cursors.SizeWE;
            }
        }

        private sbyte[] ImageToSByteArray(Image image)
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

        private Image createImage(sbyte[] imageData, int offset, int lenght)
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

        private Image ByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                Image image = Image.FromStream(stream);
                return new Bitmap(image);
            }
        }

        private byte convertSbyteToByte(sbyte var)
        {
            if ((int)var > 0)
            {
                return (byte)var;
            }
            return (byte)((int)var + 256);
        }

        private Image createImage(sbyte[] arr)
        {
            return createImage(arr, 0, arr.Length);
        }

        private void saveData()
        {
            try
            {
                DataOutputStream st = new(999999);
                st.writeUTF(txtIdMob.Text);
                st.writeBoolean(cbHasMobId.Checked);
                sbyte[] img = Array.Empty<sbyte>();
                if (picDefault.Image != null)
                {
                    img = ImageToSByteArray(picDefault.Image);
                }
                st.writeInt(img.Length);
                st.write(img);
                st.writeByte((sbyte)InfoImage.Keys.Count);
                List<byte> keysList = new List<byte>(InfoImage.Keys);
                for (int i = 0; i < InfoImage.Keys.Count; i++)
                {
                    st.writeByte((sbyte)keysList[i]);
                    ImageInfo info = InfoImage[keysList[i]];
                    sbyte[] image = ImageToSByteArray(info.Image);
                    st.writeByte((sbyte)info.ID);
                    st.writeShort(info.X0);
                    st.writeShort(info.Y0);
                    st.writeInt(image.Length);
                    st.write(image);
                }
                st.writeShort((short)ParentFrame.Keys.Count);
                List<short> keysListPr = new List<short>(ParentFrame.Keys);
                for (int i = 0; i < keysListPr.Count; i++)
                {
                    st.writeShort(keysListPr[i]);
                    List<ChildFrame> childs = ParentFrame[keysListPr[i]];
                    st.writeByte((sbyte)childs.Count);
                    for (int j = 0; j < childs.Count; j++)
                    {
                        st.writeByte((sbyte)childs[j].ID);
                        st.writeShort(childs[j].IDParent);
                        st.writeByte((sbyte)childs[j].ImageInfo.ID);
                        st.writeShort((short)childs[j].Bounds.X);
                        st.writeShort((short)childs[j].Bounds.Y);
                    }
                }
                List<ChildFrame> stackAsList = deletedImagesStack.ToList();
                st.writeByte((sbyte)stackAsList.Count);
                for (int i = 0; i < stackAsList.Count; i++)
                {
                    st.writeByte((sbyte)stackAsList[i].ID);
                    st.writeShort(stackAsList[i].IDParent);
                    st.writeByte((sbyte)stackAsList[i].ImageInfo.ID);
                    st.writeShort((short)stackAsList[i].Bounds.X);
                    st.writeShort((short)stackAsList[i].Bounds.Y);
                }
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(key);
                }
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(iv);
                }
                byte[] encryptedData = EncryptByteArray(Array.ConvertAll(st.toByteArray(), a => (byte)a));
                string keyS = Convert.ToBase64String(key);
                string IVS = Convert.ToBase64String(iv);
                File.WriteAllText(filePathKEY, keyS);
                File.WriteAllText(filePathIV, IVS);
                File.WriteAllBytes(dataFilePathSave, encryptedData);
            }
            catch
            {
            }
        }

        public void readData(byte[] data)
        {
            try
            {
                this.ParentFrame.Clear();
                this.InfoImage.Clear();
                this.deletedImagesStack.Clear();
                DataInputStream input = new DataInputStream(Array.ConvertAll(data, a => (sbyte)a));
                txtIdMob.Text = input.readUTF();
                cbHasMobId.Checked = input.readBoolean();
                int lengthImg = input.readInt();
                sbyte[] image = new sbyte[lengthImg];
                input.read(ref image);
                picDefault.Image = createImage(image);
                int countImageInfo = input.readByte();
                for (byte i = 0; i < countImageInfo; i++)
                {
                    byte key = (byte)input.readByte();
                    byte ID = (byte)input.readByte();
                    short X0 = input.readShort();
                    short Y0 = input.readShort();
                    image = new sbyte[input.readInt()];
                    input.read(ref image);
                    ImageInfo info = new ImageInfo(ID, X0, Y0, createImage(image));
                    this.InfoImage.Add(key, info);
                }
                short countParent = input.readShort();
                for (short i = 0; i < countParent; i++)
                {
                    short key = input.readShort();
                    byte count = (byte)input.readByte();
                    List<ChildFrame> children = new List<ChildFrame>();
                    for (byte j = 0; j < count; j++)
                    {
                        byte id = (byte)input.readByte();
                        short idParent = input.readShort();
                        byte idImg = (byte)input.readByte();
                        short x = input.readShort();
                        short y = input.readShort();
                        ChildFrame item = new ChildFrame(id, idParent, idImg, x, y, this.formMob);
                        children.Add(item);
                    }
                    this.ParentFrame.Add(key, children);
                }
                int countImageDelete = input.readByte();
                for (int i = 0; i < countImageDelete; i++)
                {
                    byte id = (byte)input.readByte();
                    short idParent = input.readShort();
                    byte idImg = (byte)input.readByte();
                    short x = input.readShort();
                    short y = input.readShort();
                    ChildFrame item = new ChildFrame(id, idParent, idImg, x, y, this.formMob);
                    this.deletedImagesStack.Push(item);
                }
                this.isStopWrite = false;
            }
            catch
            {
                isStopWrite = false;
                MessageBox.Show("Có lỗi xảy ra với dữ liệu được lưu trữ", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private byte[] EncryptByteArray(byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(data, 0, data.Length);
                        csEncrypt.Close();
                    }
                    return msEncrypt.ToArray();
                }
            }
        }

        private byte[] DecryptByteArray(byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(File.ReadAllText(filePathKEY));
                aesAlg.IV = Convert.FromBase64String(File.ReadAllText(filePathIV));
                using (MemoryStream msDecrypt = new MemoryStream(data))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream msPlainText = new MemoryStream())
                        {
                            csDecrypt.CopyTo(msPlainText);
                            return msPlainText.ToArray();
                        }
                    }
                }
            }
        }

        private void CropAndDisplayOnPanel()
        {
            try
            {
                if (cropRectangle.Width > 0 && cropRectangle.Height > 0)
                {
                    isStopWrite = true;
                    Bitmap croppedImage = new Bitmap((int)cropRectangle.Width + 1, (int)cropRectangle.Height + 1);
                    using (Graphics g = Graphics.FromImage(croppedImage))
                    {
                        g.DrawImage(picDefault.Image, new RectangleF(0, 0, cropRectangle.Width + 1, cropRectangle.Height + 1),
                                    cropRectangle, GraphicsUnit.Pixel);
                    }
                    if (imageCrop != null)
                    {
                        imageCrop.Dispose();
                    }
                    if (DataPreventive != null)
                    {
                        DataPreventive = null;
                    }
                    DataPreventive = new ImageInfo((byte)InfoImage.Keys.Count, (short)cropRectangle.X, (short)cropRectangle.Y, croppedImage);
                    imageCrop = croppedImage;
                    isStopWrite = false;
                }
            }
            catch
            {
                MessageBox.Show("Có lỗi xảy ra", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Event

        private void UpdateExpired_Tick(object sender, EventArgs e)
        {
        }

        private void MobForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                isCloseWinform = true;
                if (File.Exists(dataFilePathCheck))
                {
                    File.Delete(dataFilePathCheck);
                }
            }
            saveData();
        }

        private void btnImportImage_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                isStopWrite = false;
                this.ParentFrame.Clear();
                this.InfoImage.Clear();
                this.deletedImagesStack.Clear();
                fileName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                picDefault.Image = Image.FromFile(openFileDialog1.FileName);
            }
        }


        private void btnSaveCrop_Click(object sender, EventArgs e)
        {
            if (imageCrop == null) return;
            if (DataPreventive != null)
            {
                cropRectangle = new Rectangle();
                if (InfoImage.Keys.Count + 1 > byte.MaxValue)
                {
                    MessageBox.Show("Chỉ có thể thêm tối đa " + byte.MaxValue + " ảnh", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                InfoImage.Add((byte)InfoImage.Keys.Count, DataPreventive);
            }
            picDefault.Invalidate();
            imageCrop = null;
        }


        private void picDefault_MouseLeave(object sender, EventArgs e)
        {
            if (!Cursor.Equals(Cursors.Default))
            {
                Cursor = Cursors.Default;
            }
        }

        private void picDefault_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Aqua, 1))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, picDefault.Width - 1, picDefault.Height - 1));
            }
            using (Pen pen = new Pen(Color.Red, 1))
            {
                e.Graphics.DrawRectangle(pen, cropRectangle);
            }
            if (!cropRectangle.IsEmpty)
            {
                DisposeRegions();
                RectangleF[] rcAnchors = new RectangleF[(int)AnchorEnum.We + 1];
                rcAnchors[(int)AnchorEnum.Nwse] = new RectangleF(cropRectangle.Left - szAnhorSize.Width, cropRectangle.Top - szAnhorSize.Height, szAnhorSize.Width, szAnhorSize.Height);
                RectangleF tRcNs = new RectangleF((float)((cropRectangle.Left + (cropRectangle.Width / 2)) - ((double)szAnhorSize.Width / 2)), cropRectangle.Top - szAnhorSize.Height, szAnhorSize.Width, szAnhorSize.Height);
                if (cropRectangle.Width > (szAnhorSize.Width * 2))
                {
                    rcAnchors[(int)AnchorEnum.Ns] = tRcNs;
                    rcAnchors[(int)AnchorEnum.Sn] = new RectangleF((float)((cropRectangle.Left + (cropRectangle.Width / 2)) - ((double)szAnhorSize.Width / 2)), cropRectangle.Bottom, szAnhorSize.Width, szAnhorSize.Height);
                }
                if (cropRectangle.Height > (szAnhorSize.Height * 2))
                {
                    rcAnchors[(int)AnchorEnum.Ew] = new RectangleF(cropRectangle.Right, (float)((cropRectangle.Top + (cropRectangle.Height / 2)) - ((double)szAnhorSize.Height / 2)), szAnhorSize.Width, szAnhorSize.Height);
                    rcAnchors[(int)AnchorEnum.We] = new RectangleF(cropRectangle.Left - szAnhorSize.Width, (float)((cropRectangle.Top + (cropRectangle.Height / 2)) - ((double)szAnhorSize.Height / 2)), szAnhorSize.Width, szAnhorSize.Height);
                }
                rcAnchors[(int)AnchorEnum.Nesw] = new RectangleF(cropRectangle.Right, cropRectangle.Top - szAnhorSize.Height, szAnhorSize.Width, szAnhorSize.Height);
                rcAnchors[(int)AnchorEnum.Senw] = new RectangleF(cropRectangle.Right, cropRectangle.Bottom, szAnhorSize.Width, szAnhorSize.Height);
                rcAnchors[(int)AnchorEnum.Swne] = new RectangleF(cropRectangle.Left - szAnhorSize.Width, cropRectangle.Bottom, szAnhorSize.Width, szAnhorSize.Height);
                for (AnchorEnum eAnchor = AnchorEnum.Nwse; eAnchor <= AnchorEnum.We; eAnchor++)
                {
                    if (eAnchor == (AnchorEnum)_eMouseOperation)
                    {
                        e.Graphics.FillRectangle(Brushes.White, rcAnchors[(int)eAnchor]);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(Brushes.LightGray, rcAnchors[(int)eAnchor]);
                    }
                    e.Graphics.DrawRectangle(Pens.White, rcAnchors[(int)eAnchor].Left, rcAnchors[(int)eAnchor].Top, rcAnchors[(int)eAnchor].Width, rcAnchors[(int)eAnchor].Height);
                }
                for (AnchorEnum eAnchor = AnchorEnum.None; eAnchor <= AnchorEnum.We; eAnchor++)
                {
                    _rgAnchors[(int)eAnchor] = new Region(rcAnchors[(int)eAnchor]);
                }
            }
        }

        private void picDefault_MouseMove(object sender, MouseEventArgs e)
        {
            if (picDefault.Image == null)
            {
                return;
            }
            if (_eMouseOperation == MouseOperation.None)
            {
                Cursor eCursor = Cursors.Default;
                for (AnchorEnum eAnchor = AnchorEnum.Nwse; eAnchor <= AnchorEnum.We; eAnchor++)
                {
                    if ((_rgAnchors[(int)eAnchor] != null) && (_rgAnchors[(int)eAnchor].IsVisible(e.Location)))
                    {
                        eCursor = AnchorToCursor(eAnchor);
                        break;
                    }
                }
                if (eCursor == Cursors.Default)
                {
                    if (!cropRectangle.IsEmpty && cropRectangle.Contains(e.Location))
                    {
                        eCursor = Cursors.SizeAll;
                    }
                }
                if (!Cursor.Equals(eCursor))
                {
                    Cursor = eCursor;
                }
            }
            else if (_eMouseOperation == MouseOperation.Crop)
            {
                Cursor = Cursors.Cross;
                int x = e.X;
                int y = e.Y;
                x = Math.Max(Math.Min(x, picDefault.Width), 0);
                y = Math.Max(Math.Min(y, picDefault.Height), 0);
                int width = x - startPoint.X;
                int height = y - startPoint.Y;
                width = Math.Min(width, picDefault.Width - startPoint.X) - 1;
                height = Math.Min(height, picDefault.Height - startPoint.Y) - 1;
                cropRectangle = new RectangleF(startPoint.X, startPoint.Y, width, height);
                picDefault.Invalidate();
            }
            else if (_eMouseOperation == MouseOperation.Move)
            {
                if (_rcMoveResizeInitRect.IsEmpty)
                {
                    return;
                }
                cropRectangle = new RectangleF(Math.Max(0, Math.Min(e.X - offset.X, picDefault.Width - cropRectangle.Width) - 1), Math.Max(0, Math.Min(e.Y - offset.Y, picDefault.Height - cropRectangle.Height) - 1), cropRectangle.Width, cropRectangle.Height);
                picDefault.Invalidate();
            }
            else if (_eMouseOperation >= MouseOperation.Nwse && _eMouseOperation <= MouseOperation.We)
            {
                PointF tDest = e.Location;
                if (Convert.ToBoolean(0.0f))
                {
                    tDest = RotatePoint(tDest, startPoint, Convert.ToDouble(-0.0f));
                }
                PointF tPt = new PointF(tDest.X - startPoint.X, tDest.Y - startPoint.Y);
                RectangleF tRc = _rcMoveResizeInitRect;
                switch (_eMouseOperation)
                {
                    case MouseOperation.Nwse:
                        tRc.X += tPt.X;
                        tRc.Width -= tPt.X;
                        tRc.Y += tPt.Y;
                        tRc.Height -= tPt.Y;
                        break;
                    case MouseOperation.Ns:
                        tRc.Y += tPt.Y;
                        tRc.Height -= tPt.Y;
                        break;
                    case MouseOperation.Nesw:
                        tRc.Width += tPt.X;
                        tRc.Y += tPt.Y;
                        tRc.Height -= tPt.Y;
                        break;
                    case MouseOperation.Ew:
                        tRc.Width += tPt.X;
                        break;
                    case MouseOperation.Senw:
                        tRc.Width += tPt.X;
                        tRc.Height += tPt.Y;
                        break;
                    case MouseOperation.Sn:
                        tRc.Height += tPt.Y;
                        break;
                    case MouseOperation.Swne:
                        tRc.X += tPt.X;
                        tRc.Width -= tPt.X;
                        tRc.Height += tPt.Y;
                        break;
                    case MouseOperation.We:
                        tRc.X += tPt.X;
                        tRc.Width -= tPt.X;
                        break;
                }
                if (tRc.Width < 1)
                {
                    return;
                }
                if (tRc.Height < 1)
                {
                    return;
                }
                if (cropRectangle.Equals(tRc))
                {
                    return;
                }
                cropRectangle = tRc;
                picDefault.Invalidate();
            }
        }

        private void picDefault_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && picDefault.Image != null)
            {
                if (_eMouseOperation != MouseOperation.None)
                {
                    return;
                }
                if (cropRectangle.IsEmpty)
                {
                    _eMouseOperation = MouseOperation.Crop;
                    cropRectangle = new Rectangle();
                    startPoint = e.Location;
                    picDefault.Invalidate();
                    return;
                }
                if (!cropRectangle.IsEmpty)
                {
                    for (AnchorEnum eAnchor = AnchorEnum.Nwse; eAnchor <= AnchorEnum.We; eAnchor++)
                    {
                        if ((_rgAnchors[(int)eAnchor] != null) && (_rgAnchors[(int)eAnchor].IsVisible(e.Location)))
                        {
                            _eMouseOperation = (MouseOperation)eAnchor;
                            startPoint = e.Location;
                            _rcMoveResizeInitRect = cropRectangle;
                            return;
                        }
                    }
                    if (cropRectangle.Contains(e.Location))
                    {
                        Cursor = Cursors.SizeAll;
                        _eMouseOperation = MouseOperation.Move;
                        offset = new PointF(e.X - cropRectangle.Left, e.Y - cropRectangle.Top);
                        _rcMoveResizeInitRect = cropRectangle;
                        return;
                    }
                }
            }
        }

        private void picDefault_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            if (_eMouseOperation == MouseOperation.None)
            {
                return;
            }
            CropAndDisplayOnPanel();
            _eMouseOperation = MouseOperation.None;
        }

        private void MobForm_Load(object sender, EventArgs e)
        {
            TopMost = false;
            if (File.Exists(dataFilePathCheck) && File.Exists(dataFilePathSave))
            {
                if (MessageBox.Show("Dữ liệu được lưu: " + File.ReadAllText(dataFilePathCheck) + ".Có muốn khôi phục?", "Thông Báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        byte[] arr = File.ReadAllBytes(dataFilePathSave);
                        byte[] data = DecryptByteArray(arr);
                        this.readData(data);
                    }
                    catch
                    {
                        MessageBox.Show("Có lỗi xảy ra với dữ liệu được lưu trữ", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MobForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.O)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn nhập dữ liệu trước đó?", "Thông Báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        byte[] arr = File.ReadAllBytes(dataFilePathSave);
                        byte[] data = DecryptByteArray(arr);
                        this.readData(data);
                    }
                    catch
                    {
                        MessageBox.Show("Có lỗi xảy ra với dữ liệu được lưu trữ", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void openHDSD_Click(object sender, EventArgs e)
        {
            if (!isOpenFormHDSD)
            {
                isOpenFormHDSD = true;
                HDSD hDSD = new HDSD(this);
                hDSD.Show();
            }
        }

        private void btnEffectForm_Click(object sender, EventArgs e)
        {
            if (!isOpenFormMob)
            {
                if (cbHasMobId.Checked)
                {
                    if (string.IsNullOrEmpty(txtIdMob.Text))
                    {
                        MessageBox.Show("Vui Lòng Nhập Mob ID", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (!int.TryParse(txtIdMob.Text, out _))
                    {
                        MessageBox.Show("Vui Lòng Nhập Mob ID Là Số", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                formMob.Show();
                isOpenFormMob = true;
            }
        }

        private void txtIdMob_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void checkSaveData_Tick(object sender, EventArgs e)
        {
            if (!isCloseWinform)
            {
                TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTime currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                File.WriteAllText(dataFilePathCheck, currentDateTime.ToString());
            }
            if (!isStopWrite)
            {
                saveData();
            }
        }


        private void btnFastCrop_Click(object sender, EventArgs e)
        {
            if (picDefault.Image == null)
            {
                return;
            }
            this.ParentFrame.Clear();
            this.InfoImage.Clear();
            this.deletedImagesStack.Clear();
            OutputCropImage cropImage = new OutputCropImage(picDefault.Image);
            if (cropImage.ShowDialog() == DialogResult.OK)
            {
                if (cropImage.images.Count + 1 > byte.MaxValue)
                {
                    MessageBox.Show("Chỉ có thể thêm tối đa " + byte.MaxValue + " ảnh", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                for (int i = 0; i < cropImage.images.Count; i++)
                {
                    Image image = cropImage.images[i];
                    Rectangle rectangle = cropImage.rects[i];
                    InfoImage.Add((byte)InfoImage.Keys.Count, new ImageInfo((byte)InfoImage.Keys.Count, (short)rectangle.X, (short)rectangle.Y, image));
                }
            }
        }
        #endregion
    }
}
