namespace NiCoMiudon
{
    partial class DebugForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.tabButton1 = new NiCoMiudon.TabButton();
            this.commentViewer1 = new NiCoMiudon.CommentViewer();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(272, 243);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(48, 25);
            this.button1.TabIndex = 3;
            this.button1.Text = "Post";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(89, 246);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(177, 19);
            this.textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(0, 246);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(83, 19);
            this.textBox2.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(230, 277);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(80, 25);
            this.button2.TabIndex = 4;
            this.button2.Text = "Load XML";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tabButton1
            // 
            this.tabButton1.BackColor = System.Drawing.Color.Black;
            this.tabButton1.ForeColor = System.Drawing.Color.White;
            this.tabButton1.Location = new System.Drawing.Point(19, 276);
            this.tabButton1.Name = "tabButton1";
            this.tabButton1.Size = new System.Drawing.Size(54, 25);
            this.tabButton1.TabIndex = 5;
            // 
            // commentViewer1
            // 
            this.commentViewer1.BackColor = System.Drawing.Color.Black;
            this.commentViewer1.ChannelText = null;
            this.commentViewer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.commentViewer1.Location = new System.Drawing.Point(0, 0);
            this.commentViewer1.Name = "commentViewer1";
            this.commentViewer1.Size = new System.Drawing.Size(320, 240);
            this.commentViewer1.TabIndex = 0;
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(320, 305);
            this.Controls.Add(this.tabButton1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.commentViewer1);
            this.Name = "DebugForm";
            this.Text = "DebugForm";
            this.Load += new System.EventHandler(this.DebugForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private CommentViewer commentViewer1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button2;
        private TabButton tabButton1;
    }
}