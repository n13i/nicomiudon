using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NiCoMiudon
{
    public partial class SettingsForm : Form
    {
        private MenuItem miLeft;
        private MenuItem miRight;

        public SettingsForm()
        {
            InitializeComponent();

            this.Closing += new CancelEventHandler(SettingsForm_Closing);

            this.Menu = new MainMenu();

            this.miLeft = new MenuItem { Text = "OK" };
            this.miLeft.Click += new EventHandler(miLeft_Click);

            this.miRight = new MenuItem { Text = "キャンセル" };
            this.miRight.Click += new EventHandler(miRight_Click);

            this.Menu.MenuItems.Add(this.miLeft);
            this.Menu.MenuItems.Add(this.miRight);

            this.headerPanel.Text = "設定";

            this.DialogResult = DialogResult.OK;
        }

        void miLeft_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        void miRight_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            this.mailTextBox.Text = Settings.Email;
            this.passwordTextBox.Text = Settings.Password;
            this.commentAnonymizeCheckBox.Checked = Settings.AlwaysAnonymize;

            int[] fps = { 5, 8, 10, 12, 15 };
            foreach (int n in fps)
            {
                int idx = this.commentFpsComboBox.Items.Add(n);
                if (Settings.CommentFps == n)
                {
                    this.commentFpsComboBox.SelectedIndex = idx;
                }
            }
        }

        void SettingsForm_Closing(object sender, CancelEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                Settings.Email = this.mailTextBox.Text;
                Settings.Password = this.passwordTextBox.Text;
                Settings.AlwaysAnonymize = this.commentAnonymizeCheckBox.Checked;

                int idx = this.commentFpsComboBox.SelectedIndex;
                Settings.CommentFps = (int)this.commentFpsComboBox.Items[idx];

                Settings.Save();
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
