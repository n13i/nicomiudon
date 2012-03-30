using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Xml;
using Microsoft.WindowsCE.Forms;

namespace NiCoMiudon
{
    // 参考: http://dev.activebasic.com/egtra/?p=210
    //       http://dic.nicovideo.jp/a/%E3%83%8B%E3%82%B3%E7%94%9F%E3%82%A2%E3%83%A9%E3%83%BC%E3%83%88%28%E6%9C%AC%E5%AE%B6%29%E3%81%AE%E4%BB%95%E6%A7%98
    public partial class MainForm : Form
    {
        private Client client;

        private InputPanel inputPanel;

        private MenuItem miLeftSoftKey;
        private MenuItem miRightSoftKey;
        private MenuItem miCommentMenu;
        private MenuItem miSendComment;

        private System.Windows.Forms.Timer commentTimeoutTimer;

        public MainForm()
        {
            InitializeComponent();
            client = new Client();

            this.inputPanel = new InputPanel();
            inputPanel.EnabledChanged += new EventHandler(inputPanel_EnabledChanged);

            this.Menu = new MainMenu();

            BuildMenuItems();

            SetCommentPanelVisibility(false);

            client.CommentSent = (status => { this.BeginInvoke(new Action<bool>(CommentSent), status); });
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.commentPanel.Height = this.commandTextBox.Height;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            this.Activated -= new EventHandler(MainForm_Activated);

            Cursor.Current = Cursors.WaitCursor;

            Settings.Load();
            this.commentViewer.Fps = Settings.CommentFps;

            while (!Login())
            {
                Cursor.Current = Cursors.Default;

                DialogResult r = MessageBox.Show("ログインに失敗しました。設定画面を表示しますか？", "ログイン失敗", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                if (r == DialogResult.Yes)
                {
                    SettingsForm f = new SettingsForm();
                    f.ShowDialog();
                    f.Dispose();
                }
                else
                {
                    Shutdown();
                    break;
                }

                Cursor.Current = Cursors.WaitCursor;
            }

            Cursor.Current = Cursors.Default;

            if (client.IsLoggedIn)
            {
                ChannelSelect();
            }
        }

        private void BuildMenuItems()
        {
            // コメント入力ボックス非表示中
            this.miLeftSoftKey = new MenuItem { Text = "チャンネル", Enabled = true };
            this.miLeftSoftKey.Click += new EventHandler(miLeftSoftKey_Click);
            this.miRightSoftKey = new MenuItem { Text = "メニュー", Enabled = true };

            MenuItem miSetting = new MenuItem { Text = "設定" };
            MenuItem miVersion = new MenuItem { Text = "バージョン情報" };
            MenuItem miExit = new MenuItem { Text = "終了" };
            miSetting.Click += new EventHandler(miSetting_Click);
            miVersion.Click += new EventHandler(miVersion_Click);
            miExit.Click += new EventHandler(miExit_Click);

            this.miRightSoftKey.MenuItems.Add(miSetting);
            this.miRightSoftKey.MenuItems.Add(miVersion);
            this.miRightSoftKey.MenuItems.Add(new MenuItem { Text = "-" });
            this.miRightSoftKey.MenuItems.Add(miExit);

            // コメント入力ボックス表示中
            this.miCommentMenu = new MenuItem { Text = "メニュー", Enabled = true };
            this.miSendComment = new MenuItem { Text = "送信", Enabled = false };
            this.miSendComment.Click += new EventHandler(miSendComment_Click);

            MenuItem miCancelComment = new MenuItem { Text = "キャンセル" };
            miCancelComment.Click += new EventHandler((sender, e) => { SetCommentPanelVisibility(false); });

            this.miCommentMenu.MenuItems.Add(miCancelComment);
        }

        void inputPanel_EnabledChanged(object sender, EventArgs e)
        {
            if (inputPanel.Enabled)
            {
                this.dummyInputPanel.Height = inputPanel.Bounds.Height;
            }
            else
            {
                this.dummyInputPanel.Height = 0;
            }
        }

        void miLeftSoftKey_Click(object sender, EventArgs e)
        {
            ChannelSelect();
        }

        void miVersion_Click(object sender, EventArgs e)
        {
            MessageBox.Show("実況！ニコみうどん Ver." + Utils.GetExecutingAssemblyVersion() + "\nCopyright (C) 2010 n13i@m2hq.net", "バージョン情報");
        }

        void miSetting_Click(object sender, EventArgs e)
        {
            SettingsForm f = new SettingsForm();
            f.ShowDialog();
            f.Dispose();
            this.commentViewer.Fps = Settings.CommentFps;
        }

        void miExit_Click(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void CommentReceived(Comment comment)
        {
            Utils.DebugLog(comment);
            try
            {
                this.commentViewer.Add(comment);
            }
            catch (Exception e)
            {
                Utils.DebugLog(e);
            }
        }

        private void channelButton_Click(object sender, EventArgs e)
        {
            ChannelSelect();
        }

        void miSendComment_Click(object sender, EventArgs e)
        {
            if (this.commentTextBox.Text != null && this.commentTextBox.Text != "")
            {
                // CommentSentが呼ばれるまでは送信できないようにする
                this.miSendComment.Enabled = false;
                this.commentTextBox.Enabled = false;
                this.commandTextBox.Enabled = false;
                client.SendComment(this.commandTextBox.Text, this.commentTextBox.Text);

                // コメントが多い時(?)にCommentSentが呼ばれないことがあるみたい
                // <chat_result>を取りこぼしてる？
                // なのでタイマで復活させてやる
                this.commentTimeoutTimer = new System.Windows.Forms.Timer { Interval = 3000, Enabled = false };
                this.commentTimeoutTimer.Tick += new EventHandler((s, ev) => { CommentSent(false); });
                this.commentTimeoutTimer.Enabled = true;
            }
        }

        private void CommentSent(bool status)
        {
            // 自分の投げたコメントを表示させるのはClient側で処理

            Utils.DebugLog("CommentSent: " + status.ToString());
            this.commentTextBox.Text = "";
            this.commentTextBox.Enabled = true;
            this.commandTextBox.Enabled = true;

            if (this.commentTimeoutTimer != null)
            {
                this.commentTimeoutTimer.Enabled = false;
                this.commentTimeoutTimer.Dispose();
                this.commentTimeoutTimer = null;
            }
        }

        private bool Login()
        {
            return client.Login(Settings.Email, Settings.Password);
        }

        private void ChannelSelect()
        {
            client.StopReceiveComments();
            commentViewer.Stop();

            ChannelSelectForm f = new ChannelSelectForm(this.client);
            DialogResult r = f.ShowDialog();
            if (r == DialogResult.OK)
            {
                Channel c = f.SelectedChannel;

                Dictionary<string, string> gf;
                gf = client.GetFLV(c.Video);
                client.StartReceiveComments(gf["ms"], int.Parse(gf["ms_port"]), gf["thread_id"], comment => { this.BeginInvoke(new Action<Comment>(CommentReceived), comment); });

                this.headerPanel.Text = c.Name;
                this.headerPanel.Description = c.ProgTitle;
                this.commentViewer.ChannelText = c.Name;
            }
            f.Dispose();

            commentViewer.Clear();
            commentViewer.Start();
        }

        private void Shutdown()
        {
            commentViewer.Stop();
            client.StopReceiveComments();
            Application.Exit();
        }

        private void commentTextBox_Focused(object sender, EventArgs e)
        {
            Utils.DebugLog("Focused");
        }

        private void commentViewer1_Click(object sender, EventArgs e)
        {
            Utils.DebugLog("click");
            ToggleCommentPanel();
        }

        private void commentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.commentTextBox.Text != "")
            {
                this.miSendComment.Enabled = true;
            }
            else
            {
                this.miSendComment.Enabled = false;
            }
        }

        private void ToggleCommentPanel()
        {
            if (this.commentPanel.Visible)
            {
                SetCommentPanelVisibility(false);
            }
            else
            {
                SetCommentPanelVisibility(true);
            }
        }

        /// <summary>
        /// コメント入力ボックスの表示・非表示とメニュー内容の変更を行う
        /// </summary>
        /// <param name="show"></param>
        private void SetCommentPanelVisibility(bool show)
        {
            this.Menu.MenuItems.Clear();

            if (show)
            {
                this.Menu.MenuItems.Add(this.miSendComment);
                this.Menu.MenuItems.Add(this.miCommentMenu);
                this.commentTextBox.Focus();
            }
            else
            {
                this.Menu.MenuItems.Add(this.miLeftSoftKey);
                this.Menu.MenuItems.Add(this.miRightSoftKey);
            }

            this.commentPanel.Visible = show;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    ToggleCommentPanel();
                    break;
            }
        }
    }
}
