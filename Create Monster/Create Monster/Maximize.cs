using Guna.UI2.Material.Animation;
using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Enums;
using Guna.UI2.WinForms.Helpers;
using Guna.UI2.WinForms.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheArtOfDevHtmlRenderer.Adapters.Entities;

namespace Create_Monster
{
    public partial class Maximize : Control, IControl
    {
        public Maximize()
        {
            base.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.UserMouse | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;
            base.SuspendLayout();
            this.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
            base.ResumeLayout(false);
            base.Size = new Size(45, 29);
        }

        private Color fillColor = Color.FromArgb(139, 152, 166);
        private Color iconColor = Color.White;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.Invalidate();
            base.OnResize(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.MouseState = MouseState.DOWN;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.MouseState = (this.isEnter ? MouseState.HOVER : MouseState.OUT);
            base.OnMouseUp(e);
            base.Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.isEnter = true;
            this.MouseState = MouseState.HOVER;
            base.OnMouseEnter(e);
            base.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.isEnter = false;
            this.MouseState = MouseState.OUT;
            base.OnMouseLeave(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            this.isEnter = false;
            this.MouseState = MouseState.OUT;
            base.OnLostFocus(e);
        }

        protected override void OnClick(EventArgs param_2866)
        {
            base.Focus();
            Form form = base.FindForm();
            if (form.WindowState != FormWindowState.Maximized)
            {
                form.WindowState = FormWindowState.Maximized;
            }
            else
            {
                form.WindowState = FormWindowState.Normal;
            }
            base.Invalidate();
            base.OnClick(param_2866);
        }

        public void PerformClick()
        {
            this.OnClick(EventArgs.Empty);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space | e.KeyData == Keys.Return)
            {
                this.PerformClick();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private Image CreateImage(Color color, Color color2)
        {
            Bitmap bitmap = new Bitmap(10, 10);
            Graphics graphics = Graphics.FromImage(bitmap);
            if (base.FindForm().WindowState != FormWindowState.Maximized)
            {
                graphics.DrawRectangle(new Pen(color, 1f), new Rectangle(0, 0, 9, 9));
            }
            else
            {
                graphics.DrawRectangle(new Pen(color, 1f), new Rectangle(2, 0, 7, 7));
                graphics.FillRectangle(new SolidBrush(color2), new Rectangle(0, 2, 7, 7));
                graphics.DrawRectangle(new Pen(color, 1f), new Rectangle(0, 2, 7, 7));
            }
            return bitmap;
        }

        internal static Color Interpolate(Color color1, Color color2, int percent)
        {
            if (color1 == Color.Transparent)
            {
                color1 = Color.Empty;
            }
            if (color2 == Color.Transparent)
            {
                color2 = Color.Empty;
            }
            return GraphicsHelper.BlendColorARGB(color1, color2, (double)(255 - percent * 255 / 100));
        }

        internal static Color BandingColor(Color param_4733, Color param_4734, Color param_4735, int depth)
        {
            Color result;
            if ((int)(checked(param_4733.R + param_4733.G + param_4733.B)) >= 382)
            {
                result = Interpolate(param_4733, param_4734, depth);
            }
            else
            {
                result = Interpolate(param_4733, param_4735, depth);
            }
            return result;
        }

        private Color FillColort()
        {
            return BandingColor(this.fillColor, Color.Black, Color.White, 15);
        }

        private void CustomPaint(Graphics G22)
        {
            Color color = this.fillColor;
            Color color2 = this.iconColor;
            Rectangle rect = new Rectangle(base.Width / 2 - 5, base.Height / 2 - 5, 10, 10);
            if (this.MouseState == MouseState.HOVER | this.MouseState == MouseState.DOWN)
            {
                color = this.FillColort();
                color2 = this.iconColor;
            }
            if (this.MouseState == MouseState.DOWN)
            {
                color = Interpolate(this.fillColor, Color.Black, 30);
            }
            using (Image image = this.CreateImage(color2, color))
            {
                G22.DrawImage(image, rect);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (!base.Enabled)
                {
                    using (Bitmap bitmap = new Bitmap(base.Width, base.Height))
                    {
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            this.CustomPaint(graphics);
                        }
                        ControlPaint.DrawImageDisabled(e.Graphics, bitmap, 0, 0, Color.White);
                        goto IL_5F;
                    }
                }
                this.CustomPaint(e.Graphics);
            IL_5F:;
            }
            catch
            {
            }
            base.OnPaint(e);
        }

        protected MouseState MouseState = MouseState.OUT;
        private bool isEnter;
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        public virtual Color FillColor
        {
            get
            {
                return this.fillColor;
            }
            set
            {
                this.fillColor = value;
                base.Invalidate();
            }
        }

        public virtual Color IconColor
        {
            get
            {
                return this.iconColor;
            }
            set
            {
                this.iconColor = value;
                base.Invalidate();
            }
        }

        public virtual bool IsDesignMode
        {
            get
            {
                return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            }
        }
    }
}
