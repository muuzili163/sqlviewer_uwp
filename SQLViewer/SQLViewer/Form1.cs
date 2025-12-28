using Newtonsoft.Json.Linq;
using System.Data;
using System.Net;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using SQLViewer.Editer;

namespace SQLViewer
{
    public partial class Form1 : Form
    {

        public bool IsLogin { get; private set; }


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            initTab();

            ConfigHelper.Set("pageSize", "100");

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.SelectedIndex = 1;

            //检查是否登录
            List<string> servers = HttpUtils.queryServer();
            if (servers == null)
            {
                IsLogin = handleLogin();
            }
            else
            {
                IsLogin = true;
            }

            if (IsLogin)
            {
                initData(servers);
            }
        }

        private void initData(List<string> servers)
        {
            string username = ConfigHelper.Get("username");
            if (username != null)
            {
                button1.Text = "用户：" + username;
            }


            if (servers == null)
            {
                servers = HttpUtils.queryServer();
            }

            foreach (string item in servers)
            {
                comboBox1.Items.Add(item);
            }

            string instanceName = ConfigHelper.Get("instance_name");
            if (instanceName != null && !instanceName.Equals(""))
            {
                comboBox1.SelectedItem = instanceName;
            }

        }

        private void initTab()
        {
            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.Padding = new Point(20, 4); // 给关闭按钮留空间
            tabControl1.DrawItem += TabControl1_DrawItem;
            tabControl1.MouseDown += TabControl1_MouseDown;

            /*TabPage newPage = new TabPage("测试");
            tabControl1.TabPages.Add(newPage);
            tabControl1.SelectedTab = newPage;*/

        }

        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage tabPage = tabControl1.TabPages[e.Index];
            Rectangle tabRect = tabControl1.GetTabRect(e.Index);
            tabRect.Inflate(-2, -2);

            // 绘制标签文本
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font,
                tabRect, tabPage.ForeColor, TextFormatFlags.Left);

            // 计算关闭按钮位置（右侧小方块）
            Rectangle closeRect = new Rectangle(
                tabRect.Right - 30, tabRect.Top + (tabRect.Height - 26) / 2, 20, 20);

            // 绘制关闭按钮“X”
            e.Graphics.DrawString("×", e.Font, Brushes.Black, closeRect.Location);
        }

        private void TabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                Rectangle tabRect = tabControl1.GetTabRect(i);
                Rectangle closeRect = new Rectangle(
                tabRect.Right - 30, tabRect.Top + (tabRect.Height - 26) / 2, 20, 20);

                if (closeRect.Contains(e.Location))
                {
                    tabControl1.TabPages.RemoveAt(i);
                    break;
                }
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            string instanceName = comboBox1.SelectedItem.ToString();
            ConfigHelper.Set("instance_name", instanceName);
            List<string> dbs = HttpUtils.queryDb(instanceName);
            foreach (string item in dbs)
            {
                TreeNode root = new TreeNode(item);
                root.Nodes.Add("default");
                treeView1.Nodes.Add(root);

            }
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode root = e.Node;
            root.Nodes.Clear();
            List<string> tables = HttpUtils.queryTable(root.Text);
            foreach (string item in tables)
            {
                TreeNode tableNode = new TreeNode(item);
                tableNode.Tag = "table";
                root.Nodes.Add(tableNode);
            }
        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode == null || !"table".Equals(selectedNode.Tag))
            {
                return;
            }
            string currentDatabase = selectedNode.Parent.Text;
            string currentTable = selectedNode.Text;

            // 1. 创建一个新的 TabPage
            TabPage newPage = new TabPage(currentTable);

            // 2. 创建你自定义的用户控件
            TableViewer uc = new TableViewer(ConfigHelper.Get("instance_name"), currentDatabase, currentTable);
            uc.Dock = DockStyle.Fill; // 自动填满整个 TabPage

            // 3. 把用户控件放入 TabPage
            newPage.Controls.Add(uc);

            // 4. 把 TabPage 加入 TabControl
            tabControl1.TabPages.Add(newPage);

            // 5. 切换到新创建的页签（可选）
            tabControl1.SelectedTab = newPage;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool flag = handleLogin();
            if (!IsLogin)
            {
                IsLogin = flag;
            }
        }

        public bool handleLogin()
        {
            using (FormLogin loginForm = new FormLogin())
            {
                var result = loginForm.ShowDialog(); // 弹出窗口

                if (result == DialogResult.OK)
                {
                    string username = loginForm.Username;
                    string password = loginForm.Password;
                    ConfigHelper.Set("username", username);
                    button1.Text = "用户：" + username;

                    MessageBox.Show($"登录成功！账号：{username}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string pageSize = comboBox2.SelectedItem.ToString();
            ConfigHelper.Set("pageSize", pageSize);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 1. 创建一个新的 TabPage
            TabPage newPage = new TabPage("新建查询");

            // 2. 创建你自定义的用户控件
            SqlEditer sqlEditer = new SqlEditer(ConfigHelper.Get("instance_name"), newPage);
            sqlEditer.Dock = DockStyle.Fill; // 自动填满整个 TabPage

            // 3. 把用户控件放入 TabPage
            newPage.Controls.Add(sqlEditer);

            // 4. 把 TabPage 加入 TabControl
            tabControl1.TabPages.Add(newPage);

            // 5. 切换到新创建的页签（可选）
            tabControl1.SelectedTab = newPage;
        }
    }
}
