using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace NiCoMiudon
{
    public partial class ChannelList : UserControl
    {
        private List<ChannelListItem> items;
        private Scroller scroller;
        private int positionY = 0;

        private SizeF scaleFactor;
        private Image offImg;

        public int SelectedIndex = -1;
        public event EventHandler SelectedIndexChanged;

        public ChannelList()
        {
            InitializeComponent();
            this.items = new List<ChannelListItem>();
            scroller = new Scroller(this);
            scroller.Scroll += new MouseEventHandler(scroller_Scroll);
            scroller.ScrollStopped += new EventHandler(scroller_ScrollStopped);
            scroller.Tapped += new MouseEventHandler(scroller_Tapped);
        }

        void scroller_Tapped(object sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
            Utils.DebugLog("Tapped " + e.X + ", " + e.Y);

            int idx = 0;
            int y = this.positionY;
            foreach (var item in this.items)
            {
                if (y <= e.Y && e.Y < y + item.Height)
                {
                    if (item.IsSelected)
                    {
                        this.SelectedIndex = idx;
                        if (this.SelectedIndexChanged != null)
                        {
                            this.SelectedIndexChanged(this, new EventArgs());
                        }
                    }
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
                y += item.Height;
                idx++;
            }
            Invalidate();
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            this.scaleFactor = factor;
            base.ScaleControl(factor, specified);
        }

        void scroller_Scroll(object sender, MouseEventArgs e)
        {
            if(this.items == null)
            {
                return;
            }

            if (this.items.Count <= 0)
            {
                return;
            }

            this.positionY += e.Y;
            int listHeight = this.items.Count * this.items[0].Height;

            //Utils.DebugLog("positionY = " + this.positionY);

            if (this.positionY > 0)
            {
                this.scroller.StopScroll();
                this.positionY = 0;
            }
            if(this.positionY < -listHeight + this.Height)
            {
                this.scroller.StopScroll();
                this.positionY = -listHeight + this.Height;
            }
            if (listHeight < this.Height)
            {
                this.positionY = 0;
            }

            Invalidate();
        }

        void scroller_ScrollStopped(object sender, EventArgs e)
        {
        }

        public void Clear()
        {
            foreach (var item in this.items)
            {
                item.Dispose();
            }
            this.items.Clear();
            this.positionY = 0;
            Invalidate();
        }

        public void Add(ChannelListItem item)
        {
            this.items.Add(item);
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 何もしない
            //base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (offImg == null)
            {
                this.offImg = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
            }

            using (Graphics g = Graphics.FromImage(this.offImg))
            {
                using (SolidBrush b = new SolidBrush(this.BackColor))
                {
                    g.FillRectangle(b, 0, 0, this.Width, this.Height);
                }

                int y = this.positionY;
                foreach (var item in this.items)
                {
                    // visibleなものだけ描画させたい
                    if (y >= -item.Height && y < this.Height)
                    {
                        item.Draw(g, 0, y);
                    }
                    y += item.Height;
                }
            }
            e.Graphics.DrawImage(offImg, 0, 0);
        }

        protected override void OnResize(EventArgs e)
        {
            if (offImg != null)
            {
                offImg.Dispose();
                offImg = null;
            }
            base.OnResize(e);
        }

        public new void Dispose()
        {
            Utils.DebugLog("ChannelList: Dispose");
            if (this.scroller != null)
            {
                this.scroller.Dispose();
            }
            base.Dispose();
        }
    }

    public class ChannelListItem : IDisposable
    {
        public Channel Channel;
        public int Width
        {
            get { return this.size.Width; }
            set
            {
                if (this.size.Width != value)
                {
                    this.size.Width = value;
                    ClearImage();
                }
            }
        }
        public int Height
        {
            get { return this.size.Height; }
            set
            {
                if (this.size.Height != value)
                {
                    this.size.Height = value;
                    ClearImage();
                }
            }
        }
        public SizeF ScaleFactor;
        public bool IsSelected
        {
            get { return this.selected; }
            set
            {
                if (this.selected != value)
                {
                    this.selected = value;
                    ClearImage();
                }
            }
        }

        private bool selected = false;
        private Size size;
        private Image img;

        public ChannelListItem()
        {
            this.size = new Size();
        }

        private void ClearImage()
        {
            if (this.img != null)
            {
                this.img.Dispose();
                this.img = null;
            }
        }

        public void Draw(Graphics g, int x, int y)
        {
            if (img == null)
            {
                img = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);

                using(Graphics gr = Graphics.FromImage(img))
                {
                    SolidBrush b;
                    Color bgColor;
                    Color channelColor;

                    if (this.IsSelected)
                    {
                        bgColor = Color.FromArgb(248, 193, 143);
                        channelColor = Color.FromArgb(219, 69, 13);
                    }
                    else
                    {
                        bgColor = Color.FromArgb(255, 255, 255);
                        channelColor = Color.Black;
                    }

                    using(b = new SolidBrush(bgColor))
                    {
                      gr.FillRectangle(b, 0, 0, this.Width, this.Height);
                    }

                    string channelName = this.Channel.Name;
                    if (this.Channel.Type == Channel.ChannelType.Television)
                    {
                        channelName = this.Channel.Id.ToString() + " " + channelName;
                    }

                    using(b = new SolidBrush(channelColor))
                    {
                        Font f = new Font("Tahoma", 10, FontStyle.Bold);
                        gr.DrawString(channelName, f, b, 4 * ScaleFactor.Width, 0);
                        f.Dispose();
                    }

                    using (b = new SolidBrush(Color.FromArgb(96, 96, 96)))
                    {
                        Font f = new Font("Tahoma", 8, FontStyle.Regular);
                        gr.DrawString(this.Channel.ProgTitle, f, b, 4 * ScaleFactor.Width, 16 * ScaleFactor.Height);
                        f.Dispose();
                    }

                    using (b = new SolidBrush(Color.Black))
                    {
                        Font f = new Font("Tahoma", 9, FontStyle.Regular);
                        gr.DrawString(this.Channel.LastRes, f, b, 4 * ScaleFactor.Width, 32 * ScaleFactor.Height);
                        f.Dispose();
                    }

                    // 勢いの描画
                    Color forceColor;
                    if (this.Channel.Force > 200)
                    {
                        forceColor = Color.FromArgb(232, 0, 0);
                    }
                    else if (this.Channel.Force > 100)
                    {
                        forceColor = Color.FromArgb(208, 112, 23);
                    }
                    else if (this.Channel.Force > 50)
                    {
                        forceColor = Color.FromArgb(133, 99, 35);
                    }
                    else if (this.Channel.Force > 15)
                    {
                        forceColor = Color.FromArgb(48, 120, 57);
                    }
                    else
                    {
                        forceColor = Color.FromArgb(65, 92, 103);
                    }

                    Font fFont = new Font("Tahoma", 10, FontStyle.Regular);
                    string fStr = String.Format("{0,3}", this.Channel.Force);
                    SizeF fSize = gr.MeasureString("000", fFont);

                    using (b = new SolidBrush(forceColor))
                    {
                        SolidBrush fBrush = new SolidBrush(Color.White);

                        Rectangle rText = new Rectangle(0, 0, (int)fSize.Width, (int)fSize.Height);
                        Rectangle rBack = new Rectangle(0, 0, rText.Width + (int)(4 * ScaleFactor.Width), rText.Height + (int)(4 * ScaleFactor.Height));
                        rBack.X = this.Width - rBack.Width - (int)(8 * ScaleFactor.Width);
                        rBack.Y = (int)(4 * ScaleFactor.Height);
                        rText.X = rBack.X + (int)(2 * ScaleFactor.Width);
                        rText.Y = rBack.Y + (int)(2 * ScaleFactor.Height);
                        
                        gr.FillRectangle(b, rBack);
                        gr.DrawString(fStr, fFont, fBrush, rText.X, rText.Y);

                        fBrush.Dispose();
                    }

                    fFont.Dispose();

                    using (Pen p = new Pen(Color.FromArgb(192, 192, 192)))
                    {
                        gr.DrawLine(p, (int)(8 * ScaleFactor.Width), this.Height - 1, (int)(this.Width - 16 * ScaleFactor.Width), this.Height - 1);
                    }
                }
            }

            g.DrawImage(img, x, y);
        }

        public void Dispose()
        {
            if (img != null)
            {
                img.Dispose();
                img = null;
            }
        }
    }
}
