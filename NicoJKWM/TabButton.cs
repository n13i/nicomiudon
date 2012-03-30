using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace NiCoMiudon
{
    public class TabButton : UserControl
    {
        public override string Text
        {
            get
            {
                return this.buttonText;
            }
            set
            {
                this.buttonText = value;
                this.Invalidate();
            }
        }
        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                this.Invalidate();
            }
        }

        private string buttonText;
        private SizeF scaleFactor;

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            this.scaleFactor = factor;
            base.ScaleControl(factor, specified);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            //base.OnPaint(e);
            Font font;
            SolidBrush textBrush;
            Pen borderPen = new Pen(Color.FromArgb(128, 128, 128));

            if (this.Enabled)
            {
                // 選択されてない状態
                using (SolidBrush b = new SolidBrush(Color.Black))
                {
                    g.FillRectangle(b, 0, 0, this.Width, this.Height);
                }
                using (SolidBrush b = new SolidBrush(Color.DarkGray))
                {
                    g.FillRectangle(b, 0, 0, this.Width, (int)(4 * scaleFactor.Height));
                }

                //g.DrawLine(borderPen, 0, 0, this.Width - 1, 0);

                font = new Font("Tahoma", 8, FontStyle.Regular);
                textBrush = new SolidBrush(Color.White);
            }
            else
            {
                using (SolidBrush b = new SolidBrush(Color.White))
                {
                    g.FillRectangle(b, 0, 0, this.Width, this.Height);
                }
                using (SolidBrush b = new SolidBrush(Color.FromArgb(219, 69, 13)))
                {
                    g.FillRectangle(b, 0, 0, this.Width, (int)(4 * scaleFactor.Height));
                }

                //g.DrawLine(borderPen, 0, 0, this.Width - 1, 0);
                //g.DrawLine(borderPen, 0, 0, 0, this.Height - 1);
                //g.DrawLine(borderPen, this.Width - 1, 0, this.Width - 1, this.Height - 1);

                font = new Font("Tahoma", 8, FontStyle.Bold);
                textBrush = new SolidBrush(Color.FromArgb(219, 69, 13));
            }

            //Utils.DebugLog(this.buttonText);
            SizeF s = g.MeasureString(this.buttonText, font);
            g.DrawString(this.buttonText, font, textBrush, (int)(this.Width - s.Width) / 2, (int)(this.Height - (4 * scaleFactor.Height) - s.Height) / 2 + (4 * scaleFactor.Height));

            font.Dispose();
            textBrush.Dispose();
            borderPen.Dispose();
        }
    }
}
