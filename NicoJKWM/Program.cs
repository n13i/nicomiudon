using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NiCoMiudon
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        static void Main()
        {
            try
            {
                Application.Run(new MainForm());
                //Application.Run(new DebugForm());
            }
            catch (Exception e)
            {
                // FIXME ログ取る
                throw e;
            }
        }
    }
}
