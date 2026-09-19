using Create_Monster.Class;
using Create_Monster.Form_Mob;

namespace Create_Monster
{
    public partial class HDSD : Form
    {
        private MobForm mobForm;

        public HDSD(MobForm form)
        {
            InitializeComponent();
            this.Size = new Size(297, 443);
            guna2ControlBox1.Location = new Point(249, 9);
            guna2TextBox2.Text = "∞";
            mobForm = form;
            guna2TextBox1.Text = "Create Monster [Premium]";
            guna2TextBox1.Size = new Size(210, 26);
            guna2TextBox2.Size = new Size(210, 26);
            panel1.Size = new Size(256, 2);
            panel2.Size = new Size(256, 2);
            guna2TextBox1.Enabled = false;
            guna2TextBox2.Enabled = false;
            label3.Text = "Hướng Dẫn Sử Dụng\r\n\r\n*****Main Form*****\r\n- Ctrl+O: Mở Lại Dữ Liệu Trước Lúc Đóng \r\n- Có Thể Di Chuyển Crop Zone\r\n\r\n*****Form Mob*****\r\n- F5: Build Data\r\n- F6: View Data Mob\r\n- Chuột Trái 2 Lần: Add Frame\r\n- Chuột Trái 2 Lần(Child Frame): Edit Or Move\r\n\r\nLưu Ý:\n- Chọn Ảnh Có Zoom Level Là 4\n-Chọn Đúng Các Chức Năng Của Mob Ở Các Ô\nBên Phải";
        }

        private void HDSD_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mobForm != null)
            {
                mobForm.isOpenFormHDSD = false;
            }
        }
    }
}
