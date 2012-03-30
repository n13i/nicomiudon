using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.IO;

namespace NiCoMiudon
{
    public partial class DebugForm : Form
    {
        private List<Comment> comments;

        public DebugForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = this.textBox1.Text;
            string mail = this.textBox2.Text;
            string xml = "<chat no=\"0\" mail=\"" + mail + "\">" + text + "</chat>";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            commentViewer1.Add(new Comment(xmlDoc.DocumentElement));
        }

        private void DebugForm_Load(object sender, EventArgs e)
        {
            commentViewer1.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.comments = new List<Comment>();

            XmlDocument xmlComment = new XmlDocument();
            xmlComment.Load(Path.Combine(Settings.ExePath, "comment.xml"));

            foreach (XmlNode node in xmlComment.DocumentElement.ChildNodes)
            {
                Utils.DebugLog(node.Name);
                if (node.Name != "chat")
                {
                    continue;
                }
                this.comments.Add(new Comment(node));
            }

            this.comments.Sort((a, b) => { return a.VPos - b.VPos; });

            Thread t = new Thread(new ThreadStart(Run));
            t.IsBackground = true;
            t.Start();

            Utils.DebugLog("run...");
        }

        private void Run()
        {
            long vpos = 0;

            while (this.comments.Count > 1)
            {
                int i;
                for (i = 0; i < this.comments.Count && this.comments[i].VPos <= vpos; i++)
                {
                    Utils.DebugLog("add: " + this.comments[i].VPos);
                    commentViewer1.Add(this.comments[i]);
                }
                if (i > 0)
                {
                    Utils.DebugLog("remove 0 to " + i.ToString());
                    this.comments.RemoveRange(0, i);
                }

                Thread.Sleep(10);
                vpos += 1;
            }
        }
    }
}
