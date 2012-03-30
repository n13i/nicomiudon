using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace NiCoMiudon
{
    // TODO 普通のコメントリストへの切り替え
    public partial class CommentViewer : UserControl
    {
        private const int N_LANES = 10;
        private const int COMMENT_TTL = 4;
        private const int COMMENT_FIXED_TTL = 3;

        private SizeF scaleFactor;

        private List<CommentElement> comments;
        private Image offImg;
        private Image bgImg;

        private Font bgDateFont;
        private Font bgTimeFont;
        private SolidBrush bgBrush;

        // FIXME 設定可能にする
        private int fps = 10;

        public int Fps
        {
            get { return this.fps; }
            set
            {
                this.fps = value;
                RecalcPPF();
            }
        }

        private int posY = 0;

        //private Thread thread;
        private bool isRunning = true;

        // コメントカラーの初期化
        // 参考: http://nicowiki.com/command.html
        private static Dictionary<string, Color> commentColors = new Dictionary<string, Color>()
        {
            { "white",          Color.FromArgb(255, 255, 255) }, // #FFFFFF
            { "red",            Color.FromArgb(255,   0,   0) }, // #FF0000
            { "pink",           Color.FromArgb(255, 128, 128) }, // #FF8080
            { "orange",         Color.FromArgb(255, 204,   0) }, // #FFCC00
            { "yellow",         Color.FromArgb(255, 255,   0) }, // #FFFF00
            { "green",          Color.FromArgb(  0, 255,   0) }, // #00FF00
            { "cyan",           Color.FromArgb(  0, 255, 255) }, // #00FFFF
            { "blue",           Color.FromArgb(  0,   0, 255) }, // #0000FF
            { "purple",         Color.FromArgb(192,   0, 255) }, // #C000FF
            { "black",          Color.FromArgb(  0,   0,   0) }, // #000000
            { "niconicowhite",  Color.FromArgb(204, 204, 153) }, // #CCCC99
            { "white2",         Color.FromArgb(204, 204, 153) }, // #CCCC99
            { "truered",        Color.FromArgb(204,   0,  51) }, // #CC0033
            { "red2",           Color.FromArgb(204,   0,  51) }, // #CC0033
            { "passionorange",  Color.FromArgb(255, 102,   0) }, // #FF6600
            { "orange2",        Color.FromArgb(255, 102,   0) }, // #FF6600
            { "madyellow",      Color.FromArgb(153, 153,   0) }, // #999900
            { "yellow2",        Color.FromArgb(153, 153,   0) }, // #999900
            { "elementalgreen", Color.FromArgb(  0, 204, 102) }, // #00CC66
            { "green2",         Color.FromArgb(  0, 204, 102) }, // #00CC66
            { "marineblue",     Color.FromArgb( 51, 255, 252) }, // #33FFFC
            { "blue2",          Color.FromArgb( 51, 255, 252) }, // #33FFFC
            { "nobleviolet",    Color.FromArgb(102,  51, 204) }, // #6633CC
            { "purple2",        Color.FromArgb(102,  51, 204) }, // #6633CC
        };
        private static Dictionary<string, Font> commentFonts = new Dictionary<string, Font>()
        {
            { "big",    new Font("Tahoma", 16, FontStyle.Bold) },
            { "medium", new Font("Tahoma", 12, FontStyle.Bold) },
            { "small",  new Font("Tahoma",  9, FontStyle.Bold) },
        };

        private string channelText;
        public string ChannelText
        {
            get { return this.channelText; }
            set
            {
                if (this.bgImg != null)
                {
                    this.bgImg.Dispose();
                    this.bgImg = null;
                }
                this.channelText = value;
                Invalidate();
            }
        }

        private readonly object lockObject = new object();

        public CommentViewer()
        {
            InitializeComponent();
            this.comments = new List<CommentElement>();

            this.bgDateFont = new Font("Tahoma", 24, FontStyle.Regular);
            this.bgTimeFont = new Font("Tahoma", 16, FontStyle.Regular);
            this.bgBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
        }

        public void Clear()
        {
            this.comments.Clear();
        }

        public void Start()
        {
            this.isRunning = true;

            Thread t = new Thread(new ThreadStart(Run));
            t.IsBackground = true;
            t.Start();
        }

        public void Stop()
        {
            this.isRunning = false;
        }

        private void Run()
        {
            while (this.isRunning)
            {
                // コメントがないときはまったり再描画
                if (this.comments.Count > 0)
                {
                    ProcComments();
                    this.BeginInvoke(new Action<object>(Redraw), this);
                    Thread.Sleep(1000 / this.fps);
                }
                else
                {
                    this.BeginInvoke(new Action<object>(Redraw), this);
                    Thread.Sleep(500);
                }
            }
        }

        private void Redraw(object o)
        {
            Invalidate();
        }

        /// <summary>
        /// コメントを表示リストに追加
        /// </summary>
        /// <param name="comment"></param>
        public void Add(Comment comment)
        {
            if (comment.Text == null)
            {
                return;
            }

            CommentElement c = new CommentElement();
            c.Comment = comment;

            c.Y = posY;

            c.Color = CommentViewer.commentColors["white"];
            c.Font = CommentViewer.commentFonts["medium"];
            c.Lane = -1;
            foreach (var cmd in comment.Commands)
            {
                if (CommentViewer.commentColors.ContainsKey(cmd))
                {
                    Utils.DebugLog("color=" + cmd);
                    c.Color = CommentViewer.commentColors[cmd];
                    continue;
                }
                if (CommentViewer.commentFonts.ContainsKey(cmd))
                {
                    Utils.DebugLog("size=" + cmd);
                    c.Font = CommentViewer.commentFonts[cmd];
                    continue;
                }
                if (c.Lane == -1)
                {
                    if (cmd == "ue")
                    {
                        c.Lane = SetUeShitaLane(c, 10);
                    }
                    else if (cmd == "shita")
                    {
                        c.Lane = SetUeShitaLane(c, 20);
                    }
                }
            }

            // レーン決定前にPixelsPerFrameを決めておく必要がある
            c.Render();
            c.CalcPPF(this.Width, this.fps);

            if (c.Lane == -1)
            {
                c.Lane = SetNakaLane(c);
            }

            lock (lockObject)
            {
                this.comments.Add(c);
            }

            Invalidate();

            posY = (posY + c.Height) % (this.Height - c.Height);
        }

        private int SetUeShitaLane(CommentElement c, int offset)
        {
            for (int i = 0; i < N_LANES; i++)
            {
                bool isBlank = true;
                foreach (var comm in this.comments)
                {
                    if (comm.Lane == i + offset)
                    {
                        isBlank = false;
                        break;
                    }
                }
                if (isBlank)
                {
                    return i + offset;
                }
            }

            return offset;
        }

        private int SetNakaLane(CommentElement c)
        {
            // 流すレーンを決める
            int lane = -1;
            bool[] laneChecked = new bool[N_LANES];
            bool[] laneIsBlank = new bool[N_LANES];
            for (int i = 0; i < N_LANES; i++)
            {
                laneChecked[i] = false;
                laneIsBlank[i] = true;
            }
            for (int i = this.comments.Count - 1; i >= 0; i--)
            {
                CommentElement comm = this.comments[i];

                // ue,shitaなコメントとチェック済みレーンをスキップ
                if (comm.Lane >= N_LANES || laneChecked[this.comments[i].Lane])
                {
                    continue;
                }

                laneChecked[comm.Lane] = true;

                // 追加直後と先行するコメントが消える直前における
                // 流したいコメントとの衝突判定
                int t = this.fps * (COMMENT_TTL) - comm.ElapsedFrames;
                if (comm.ElapsedFrames * comm.PixelsPerFrame - comm.Width < 0 ||
                    t * c.PixelsPerFrame > (comm.ElapsedFrames + t) * comm.PixelsPerFrame - comm.Width)
                {
                    Utils.DebugLog("not blank, lane=" + comm.Lane);
                    laneIsBlank[comm.Lane] = false;
                }
            }
            for (int i = 0; i < N_LANES; i++)
            {
                if (laneIsBlank[i])
                {
                    lane = i;
                    break;
                }
            }
            if (lane == -1)
            {
                // FIXME
                lane = N_LANES-1;
            }

            return lane;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            this.scaleFactor = factor;
            base.ScaleControl(factor, specified);
        }

        /// <summary>
        /// コメントの座標を動かしたり消したり
        /// サスペンド中はOnPaintが呼ばれず山のようにコメントがたまるようなので移動
        /// </summary>
        private void ProcComments()
        {
            lock (lockObject)
            {
                int count = this.comments.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this.comments[i].ToBeDeleted)
                    {
                        //Utils.DebugLog("remove: " + this.comments[i].Text);
                        this.comments.RemoveAt(i);
                        i--;
                        count--;
                    }
                }

                foreach (var c in this.comments)
                {
                    c.ElapsedFrames++;
                    if (c.Lane < N_LANES)
                    {
                        if (c.ElapsedFrames > this.fps * COMMENT_TTL)
                        {
                            c.ToBeDeleted = true;
                        }
                    }
                    else
                    {
                        if (c.ElapsedFrames > this.fps * COMMENT_FIXED_TTL)
                        {
                            c.ToBeDeleted = true;
                        }
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (offImg == null)
            {
                offImg = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
            }
            if (bgImg == null)
            {
                bgImg = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);

                Graphics gr = Graphics.FromImage(bgImg);

                using (SolidBrush b = new SolidBrush(this.BackColor))
                {
                    gr.FillRectangle(b, 0, 0, this.Width, this.Height);
                }

                /*
                Font f = new Font("Tahoma", 14, FontStyle.Regular);
                SizeF size = gr.MeasureString(this.channelText, f);
                using (SolidBrush br = new SolidBrush(Color.Green))
                {
                    gr.DrawString(this.channelText, f, br, (this.Width - size.Width) / 2, (this.Height - size.Height) / 2);
                }
                f.Dispose();
                */
                gr.Dispose();
            }

            ImageAttributes ia = new ImageAttributes();
            ia.SetColorKey(CommentElement.TransparentColor, CommentElement.TransparentColor);

            Graphics g = Graphics.FromImage(offImg);

            g.DrawImage(this.bgImg, 0, 0);
            DrawCurrentDateTime(g);

            if (this.comments.Count > 0)
            {
                lock (lockObject)
                {
                    foreach (var c in this.comments)
                    {
                        if (c.Lane < N_LANES)
                        {
                            g.DrawImage(c.RenderedImage, new Rectangle(this.Width - c.X, this.Height * c.Lane / N_LANES, c.Width, c.Height), 0, 0, c.Width, c.Height, GraphicsUnit.Pixel, ia);
                        }
                        else
                        {
                            int w = c.Width;
                            int h = c.Height;
                            if (c.Width > this.Width)
                            {
                                w = this.Width;
                                h = c.Height * this.Width / c.Width;
                            }
                            int y = 0;
                            if (c.Lane >= 20)
                            {
                                // shita
                                y = this.Height - (this.Height * (c.Lane - 20) / N_LANES) - h;
                            }
                            else
                            {
                                // ue
                                y = this.Height * (c.Lane - 10) / N_LANES;
                            }
                            g.DrawImage(c.RenderedImage, new Rectangle((this.Width - w) / 2, y, w, h), 0, 0, c.Width, c.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                }
            }
            e.Graphics.DrawImage(offImg, 0, 0);
            g.Dispose();
        }

        private void DrawCurrentDateTime(Graphics g)
        {
            DateTime now = DateTime.Now;

            string date = now.ToString("M/d");
            string time = now.ToString("HH:mm:ss");
            SizeF s1 = g.MeasureString(date, this.bgDateFont);
            SizeF s2 = g.MeasureString(time, this.bgTimeFont);

            g.DrawString(date, this.bgDateFont, this.bgBrush, (this.Width - s1.Width) / 2, this.Height / 2 - s1.Height - 2 * this.scaleFactor.Height);
            g.DrawString(time, this.bgTimeFont, this.bgBrush, (this.Width - s2.Width) / 2, this.Height / 2 + 2 * this.scaleFactor.Height);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 何もしない
        }

        private void CommentViewer_Resize(object sender, EventArgs e)
        {
            if (this.offImg != null)
            {
                this.offImg.Dispose();
                this.offImg = null;
            }
            if (this.bgImg != null)
            {
                this.bgImg.Dispose();
                this.bgImg = null;
            }

            RecalcPPF();
        }

        private void RecalcPPF()
        {
            if (this.comments != null)
            {
                lock (lockObject)
                {
                    // commentsのPixelsPerFrameを再計算
                    foreach (var c in this.comments)
                    {
                        c.CalcPPF(this.Width, this.fps);
                    }
                }
            }
        }
    }

    class CommentElement
    {
        public Comment Comment;
        public int Width;
        public int Height;
        public int PixelsPerFrame;
        public int Y; // deprecated
        public bool ToBeDeleted;
        public Color Color;
        public Font Font;
        public int ElapsedFrames;
        public int Lane;

        public int X
        {
            get
            {
                return this.PixelsPerFrame * this.ElapsedFrames;
            }
        }

        public static Color TransparentColor = Color.FromArgb(30, 30, 30);

        public Image RenderedImage;

        public CommentElement()
        {
            this.Color = Color.White;
            this.ToBeDeleted = false;
            this.ElapsedFrames = 0;
            this.Lane = 0;
        }

        public void CalcPPF(int screenWidth, int fps)
        {
            this.PixelsPerFrame = (int)Math.Ceiling((double)(this.Width + screenWidth) / (double)(fps * 4));
        }

        public void Render()
        {
            this.RenderedImage = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            Graphics g = Graphics.FromImage(this.RenderedImage);
            SizeF s = g.MeasureString(this.Comment.Text, this.Font);
            g.Dispose();
            this.RenderedImage.Dispose();

            this.Width = (int)s.Width;
            this.Height = (int)s.Height;

            this.RenderedImage = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            g = Graphics.FromImage(this.RenderedImage);
            SolidBrush bgBrush = new SolidBrush(CommentElement.TransparentColor);
            SolidBrush brush = new SolidBrush(this.Color);
            g.FillRectangle(bgBrush, 0, 0, this.Width, this.Height);
            g.DrawString(this.Comment.Text, this.Font, brush, 0, 0);
            brush.Dispose();
            bgBrush.Dispose();
            g.Dispose();
        }
    }
}
