using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using M2HQ.Utils;

namespace M2HQ.Drawing
{
    // via http://gihyo.jp/dev/serial/01/windows-phone/0014
    public sealed class Gradient
    {
        public enum FillDirection
        {
            // 水平にグラデーションで塗りつぶします
            LeftToRight = Win32Helper.GRADIENT_FILL.GRADIENT_FILL_RECT_H,

            // 垂直にグラデーションで塗りつぶします
            TopToBottom = Win32Helper.GRADIENT_FILL.GRADIENT_FILL_RECT_V
        }

        public sealed class GradientFill
        {
            public static bool Fill(
                Graphics gr, Rectangle rc,
                Color startColor, Color endColor,
                FillDirection fillDir)
            {
                // 頂点の座標と色を指定
                Win32Helper.TRIVERTEX[] tva = new Win32Helper.TRIVERTEX[2];
                tva[0] = new Win32Helper.TRIVERTEX(rc.X, rc.Y, startColor);
                tva[1] = new Win32Helper.TRIVERTEX(rc.Right, rc.Bottom, endColor);

                // どのTRIVERTEXの値を使用するかインデックスを指定
                Win32Helper.GRADIENT_RECT[] gra
                    = new Win32Helper.GRADIENT_RECT[] { new Win32Helper.GRADIENT_RECT(0, 1) };

                // GradientFill関数の呼び出し
                IntPtr hdc = gr.GetHdc();
                bool b = Win32Helper.GradientFill(
                    hdc, tva, (uint)tva.Length,
                    gra, (uint)gra.Length, (uint)fillDir);
                gr.ReleaseHdc(hdc);
                return b;
            }
        }
    }
}
