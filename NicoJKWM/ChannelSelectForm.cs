using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace NiCoMiudon
{
    public partial class ChannelSelectForm : Form
    {
        private Client client;

        private SizeF scaleFactor;

        private List<Channel> chList;
        private List<TVProgram> tvProgs;

        private MenuItem miLeftSoftKey;
        private MenuItem miRightSoftKey;

        public Channel SelectedChannel;

        public ChannelSelectForm(Client client)
        {
            InitializeComponent();

            this.Closing += new CancelEventHandler(ChannelSelectForm_Closing);
            this.client = client;

            this.Menu = new MainMenu();

            miLeftSoftKey = new MenuItem { Text = "更新", Enabled = true };
            miLeftSoftKey.Click += new EventHandler(miLeftSoftKey_Click);
            miRightSoftKey = new MenuItem { Text = "終了", Enabled = true };
            miRightSoftKey.Click += new EventHandler(miRightSoftKey_Click);

            this.Menu.MenuItems.Add(miLeftSoftKey);
            this.Menu.MenuItems.Add(miRightSoftKey);

            this.channelList1.SelectedIndexChanged += new EventHandler(channelList1_SelectedIndexChanged);
        }

        private void ChannelSelectForm_Load(object sender, EventArgs e)
        {
            Utils.DebugLog("Load");

            GetChannels();
        }

        void ChannelSelectForm_Closing(object sender, CancelEventArgs e)
        {
            Utils.DebugLog("Closing");

            if (this.SelectedChannel == null)
            {
                this.DialogResult = DialogResult.Cancel;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            Utils.DebugLog("ScaleControl: " + factor.Width.ToString() + "x" + factor.Height.ToString());
            this.scaleFactor = factor;
            base.ScaleControl(factor, specified);
        }

        private void miRightSoftKey_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void GetChannels()
        {
            this.headerPanel.Text = "お待ちください…";

            ThreadPool.QueueUserWorkItem(GetChannelsAsync, this);
        }

        private void GetChannelsAsync(object o)
        {
            this.chList = client.GetChannels();
            this.tvProgs = TVProgram.GetPrograms();
            this.BeginInvoke(new Action<object>(GetChannelsDone), this);
        }

        private void GetChannelsDone(object o)
        {
            this.headerPanel.Text = "チャンネルを選択してください";

            int[] chCount = { 0, 0, 0, 0 };
            foreach (var ch in this.chList)
            {
                if (this.tvProgs != null)
                {
                    foreach (var p in this.tvProgs)
                    {
                        if (p.ChannelNo == ch.No)
                        {
                            ch.ProgTitle = p.Title;
                            break;
                        }
                    }
                }

                switch (ch.Type)
                {
                    case Channel.ChannelType.Television:
                        chCount[0]++;
                        break;
                    case Channel.ChannelType.Radio:
                        chCount[1]++;
                        break;
                    case Channel.ChannelType.Event:
                        chCount[2]++;
                        break;
                    case Channel.ChannelType.User:
                        chCount[3]++;
                        break;
                }
            }

            this.tvTabButton.Text = "テレビ(" + chCount[0].ToString() + ")";
            this.radioTabButton.Text = "ラジオ(" + chCount[1].ToString() + ")";
            this.eventTabButton.Text = "イベント(" + chCount[2].ToString() + ")";
            this.userTabButton.Text = "ユーザー(" + chCount[3].ToString() + ")";

            SetChannels();
        }

        private void SetChannels()
        {
            if (this.chList == null)
            {
                return;
            }

            this.channelList1.Clear();

            Channel.ChannelType selectedType = GetSelectedChannelGroup();

            foreach (var ch in this.chList)
            {
                if(ch.Type == selectedType)
                {
                    this.channelList1.Add(new ChannelListItem
                    {
                        Width = this.Width,
                        Height = (int)(48 * this.scaleFactor.Height),
                        ScaleFactor = this.scaleFactor,
                        Channel = ch
                    });
                }
            }
        }

        private Channel.ChannelType GetSelectedChannelGroup()
        {
            if (!tvTabButton.Enabled)
            {
                return Channel.ChannelType.Television;
            }
            if (!radioTabButton.Enabled)
            {
                return Channel.ChannelType.Radio;
            }
            if (!eventTabButton.Enabled)
            {
                return Channel.ChannelType.Event;
            }
            if (!userTabButton.Enabled)
            {
                return Channel.ChannelType.User;
            }

            Utils.DebugLog("WARNING GetSelectedChannelGroup");
            return Channel.ChannelType.Television;
        }

        private void channelGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetChannels();
        }

        private void miLeftSoftKey_Click(object sender, EventArgs e)
        {
            // refresh
            GetChannels();
        }

        private void channelList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = channelList1.SelectedIndex;
            Channel.ChannelType selectedType = GetSelectedChannelGroup();

            int count = 0;
            foreach (var ch in chList)
            {
                if (ch.Type == selectedType)
                {
                    if (count == idx)
                    {
                        this.SelectedChannel = ch;
                        break;
                    }
                    count++;
                }
            }
            this.channelList1.Dispose();
            this.Close();
        }

        private void tvTabButton_Click(object sender, EventArgs e)
        {
            tvTabButton.Enabled = false;
            radioTabButton.Enabled = true;
            eventTabButton.Enabled = true;
            userTabButton.Enabled = true;
            SetChannels();
        }

        private void radioTabButton_Click(object sender, EventArgs e)
        {
            tvTabButton.Enabled = true;
            radioTabButton.Enabled = false;
            eventTabButton.Enabled = true;
            userTabButton.Enabled = true;
            SetChannels();
        }

        private void eventTabButton_Click(object sender, EventArgs e)
        {
            tvTabButton.Enabled = true;
            radioTabButton.Enabled = true;
            eventTabButton.Enabled = false;
            userTabButton.Enabled = true;
            SetChannels();
        }

        private void userTabButton_Click(object sender, EventArgs e)
        {
            tvTabButton.Enabled = true;
            radioTabButton.Enabled = true;
            eventTabButton.Enabled = true;
            userTabButton.Enabled = false;
            SetChannels();
        }

        private void ChannelSelectForm_Resize(object sender, EventArgs e)
        {
            SetChannels();
        }
    }
}
