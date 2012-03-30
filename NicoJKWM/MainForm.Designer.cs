namespace NiCoMiudon
{
    partial class MainForm
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
            this.commentPanel = new System.Windows.Forms.Panel();
            this.commentTextBox = new System.Windows.Forms.TextBox();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.commentViewer = new NiCoMiudon.CommentViewer();
            this.headerPanel = new NiCoMiudon.HeaderPanel();
            this.dummyInputPanel = new System.Windows.Forms.Panel();
            this.commentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // commentPanel
            // 
            this.commentPanel.Controls.Add(this.commentTextBox);
            this.commentPanel.Controls.Add(this.commandTextBox);
            this.commentPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commentPanel.Location = new System.Drawing.Point(0, 247);
            this.commentPanel.Name = "commentPanel";
            this.commentPanel.Size = new System.Drawing.Size(238, 19);
            this.commentPanel.TabIndex = 5;
            // 
            // commentTextBox
            // 
            this.commentTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commentTextBox.Location = new System.Drawing.Point(60, 0);
            this.commentTextBox.Name = "commentTextBox";
            this.commentTextBox.Size = new System.Drawing.Size(178, 19);
            this.commentTextBox.TabIndex = 2;
            this.commentTextBox.TextChanged += new System.EventHandler(this.commentTextBox_TextChanged);
            // 
            // commandTextBox
            // 
            this.commandTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.commandTextBox.Location = new System.Drawing.Point(0, 0);
            this.commandTextBox.Multiline = true;
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(60, 19);
            this.commandTextBox.TabIndex = 1;
            // 
            // commentViewer
            // 
            this.commentViewer.BackColor = System.Drawing.Color.Black;
            this.commentViewer.ChannelText = null;
            this.commentViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commentViewer.ForeColor = System.Drawing.Color.White;
            this.commentViewer.Fps = 10;
            this.commentViewer.Location = new System.Drawing.Point(0, 32);
            this.commentViewer.Name = "commentViewer";
            this.commentViewer.Size = new System.Drawing.Size(238, 215);
            this.commentViewer.TabIndex = 0;
            this.commentViewer.Click += new System.EventHandler(this.commentViewer1_Click);
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
            this.headerPanel.TabIndex = 6;
            // 
            // dummyInputPanel
            // 
            this.dummyInputPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dummyInputPanel.Location = new System.Drawing.Point(0, 266);
            this.dummyInputPanel.Name = "dummyInputPanel";
            this.dummyInputPanel.Size = new System.Drawing.Size(238, 0);
            this.dummyInputPanel.TabIndex = 7;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(238, 266);
            this.Controls.Add(this.commentViewer);
            this.Controls.Add(this.commentPanel);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.dummyInputPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "実況！ニコみうどん";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.commentPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CommentViewer commentViewer;
        private System.Windows.Forms.Panel commentPanel;
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.TextBox commentTextBox;
        private HeaderPanel headerPanel;
        private System.Windows.Forms.Panel dummyInputPanel;
    }
}

