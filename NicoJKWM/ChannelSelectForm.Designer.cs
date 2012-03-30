namespace NiCoMiudon
{
    partial class ChannelSelectForm
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.tabPanel = new System.Windows.Forms.Panel();
            this.channelList1 = new NiCoMiudon.ChannelList();
            this.userTabButton = new NiCoMiudon.TabButton();
            this.eventTabButton = new NiCoMiudon.TabButton();
            this.radioTabButton = new NiCoMiudon.TabButton();
            this.tvTabButton = new NiCoMiudon.TabButton();
            this.headerPanel = new NiCoMiudon.HeaderPanel();
            this.tabPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPanel
            // 
            this.tabPanel.BackColor = System.Drawing.Color.Black;
            this.tabPanel.Controls.Add(this.userTabButton);
            this.tabPanel.Controls.Add(this.eventTabButton);
            this.tabPanel.Controls.Add(this.radioTabButton);
            this.tabPanel.Controls.Add(this.tvTabButton);
            this.tabPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabPanel.Location = new System.Drawing.Point(0, 32);
            this.tabPanel.Name = "tabPanel";
            this.tabPanel.Size = new System.Drawing.Size(238, 32);
            this.tabPanel.TabIndex = 6;
            // 
            // channelList1
            // 
            this.channelList1.BackColor = System.Drawing.Color.LightGray;
            this.channelList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.channelList1.Location = new System.Drawing.Point(0, 64);
            this.channelList1.Name = "channelList1";
            this.channelList1.Size = new System.Drawing.Size(238, 202);
            this.channelList1.TabIndex = 5;
            // 
            // userTabButton
            // 
            this.userTabButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.userTabButton.Location = new System.Drawing.Point(180, 0);
            this.userTabButton.Name = "userTabButton";
            this.userTabButton.Size = new System.Drawing.Size(60, 32);
            this.userTabButton.TabIndex = 3;
            this.userTabButton.Click += new System.EventHandler(this.userTabButton_Click);
            // 
            // eventTabButton
            // 
            this.eventTabButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.eventTabButton.Location = new System.Drawing.Point(120, 0);
            this.eventTabButton.Name = "eventTabButton";
            this.eventTabButton.Size = new System.Drawing.Size(60, 32);
            this.eventTabButton.TabIndex = 2;
            this.eventTabButton.Click += new System.EventHandler(this.eventTabButton_Click);
            // 
            // radioTabButton
            // 
            this.radioTabButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.radioTabButton.Location = new System.Drawing.Point(60, 0);
            this.radioTabButton.Name = "radioTabButton";
            this.radioTabButton.Size = new System.Drawing.Size(60, 32);
            this.radioTabButton.TabIndex = 1;
            this.radioTabButton.Click += new System.EventHandler(this.radioTabButton_Click);
            // 
            // tvTabButton
            // 
            this.tvTabButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.tvTabButton.Enabled = false;
            this.tvTabButton.Location = new System.Drawing.Point(0, 0);
            this.tvTabButton.Name = "tvTabButton";
            this.tvTabButton.Size = new System.Drawing.Size(60, 32);
            this.tvTabButton.TabIndex = 0;
            this.tvTabButton.Click += new System.EventHandler(this.tvTabButton_Click);
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.DimGray;
            this.headerPanel.Description = null;
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.ForeColor = System.Drawing.Color.White;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(238, 32);
            this.headerPanel.TabIndex = 7;
            // 
            // ChannelSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(238, 266);
            this.Controls.Add(this.channelList1);
            this.Controls.Add(this.tabPanel);
            this.Controls.Add(this.headerPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ChannelSelectForm";
            this.Text = "実況！ニコみうどん";
            this.Load += new System.EventHandler(this.ChannelSelectForm_Load);
            this.Resize += new System.EventHandler(this.ChannelSelectForm_Resize);
            this.tabPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ChannelList channelList1;
        private System.Windows.Forms.Panel tabPanel;
        private TabButton eventTabButton;
        private TabButton radioTabButton;
        private TabButton tvTabButton;
        private TabButton userTabButton;
        private HeaderPanel headerPanel;
    }
}