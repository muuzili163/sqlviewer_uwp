using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQLViewer
{
    public partial class FormLogin : Form
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public FormLogin()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent; // 居中
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Username = txtUsername.Text.Trim();
            Password = txtPwd.Text;

            button1.Enabled = false;
            button1.Text = "正在登录...";

            bool success = HttpUtils.login(Username, Password);

            if (success)
            {
                this.DialogResult = DialogResult.OK; // 返回成功
                this.Close(); // 关闭窗口
                button1.Enabled = true;
                button1.Text = "登录";
            }
            else
            {
                MessageBox.Show("登录失败，请检查账号密码！");
                button1.Enabled = true;
                button1.Text = "登录";
            }

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
