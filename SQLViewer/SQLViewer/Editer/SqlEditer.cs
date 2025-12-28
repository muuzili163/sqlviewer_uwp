using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SQLViewer.Editer
{
    public partial class SqlEditer : UserControl
    {

        public String CurrentServer { get; private set; }
        public String CurrentDatabase { get; private set; }
        public String CurrentTable { get; private set; }
        public TabPage CurrentTabPage { get; private set; }

        public List<string> databases = new List<string>();
        public List<string> tables = new List<string>();

        private List<string> allFields = new List<string>();
        private bool _isQuerying = false;
        private SqlContext _currentContext;
        private SqlContextAnalyzer analyzer;
        private SqlSuggestResolver resolver;

        public SqlEditer(string serverName, TabPage tabPage)
        {
            InitializeComponent();

            CurrentServer = serverName;
            CurrentTabPage = tabPage;

            richTextBox1.KeyDown += richTextBox1_KeyDown;
            listBoxSuggest.KeyDown += listBoxSuggest_KeyDown;
            listBoxSuggest.DoubleClick += listBoxSuggest_DoubleClick;
            richTextBox1.KeyUp += richTextBox1_KeyUp;
            richTextBox1.LostFocus += (_, __) =>
            {
                if (!listBoxSuggest.Focused)
                    listBoxSuggest.Visible = false;
            };
        }

        private void SqlEditer_Load(object sender, EventArgs e)
        {
            initDataGridView();
            initDatebases();
            splitContainer1.Panel2Collapsed = true;
            analyzer = new SqlContextAnalyzer();
        }

        private async void initDatebases()
        {
            databases = await Task.Run(() =>
            {
                return HttpUtils.queryDb(CurrentServer);
            });
            foreach (string item in databases)
            {
                comboBox1.Items.Add(item);
            }
        }

        private void initTables()
        {
            if (CurrentDatabase == null)
            {
                return;
            }
            comboBox2.Items.Clear();
            comboBox2.SelectedIndex = -1; // 清除选中项
            comboBox2.Text = "";          // 清除显示文字
            CurrentTable = null;
            tables = HttpUtils.queryTable(CurrentDatabase);
            foreach (string item in tables)
            {
                comboBox2.Items.Add(item);
            }
        }

        private void initDataGridView()
        {
            //优化dataGridView
            dataGridView1.GetType().InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null,
                dataGridView1,
                new object[] { true }
            );
            dataGridView1.AlternatingRowsDefaultCellStyle = null;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //dataGridView1.AllowUserToResizeRows = false;
            //dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.ReadOnly = true; // 如果不编辑数据
            dataGridView1.VirtualMode = true; // 如果数据大量（>1000）
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Font = new Font("Microsoft YaHei UI", 12);
            dataGridView1.ColumnHeadersHeight = 36;
            dataGridView1.RowTemplate.Height = 30;

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CurrentDatabase == null || CurrentTable == null)
            {
                MessageBox.Show("请选择表", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            handleQuery();
        }


        public async void handleQuery()
        {
            if (_isQuerying)
            {
                return;
            }
            _isQuerying = true;
            button1.Enabled = false;
            button1.Text = "查询中...";

            try
            {
                //组装sql

                string sql = buildSql();

                JObject jo = await Task.Run(() =>
                {
                    return HttpUtils.querySql(CurrentServer, CurrentDatabase, CurrentTable, sql);
                });

                int status = (int)jo["status"];
                JObject data = (JObject)jo["data"];
                string msg = (string)jo["msg"];
                if (status != 0)
                {
                    MessageBox.Show(msg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    bindJsonToDataGridView(data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isQuerying = false;
                button1.Enabled = true;
                button1.Text = "查询";
            }

        }

        public string buildSql()
        {
            return richTextBox1.Text;
        }

        public void bindJsonToDataGridView(JObject data)
        {
            double query_time = (double)data["query_time"];
            int affected_rows = (int)data["affected_rows"];

            label2.Text = "查询时间：" + query_time;
            label3.Text = "条数：" + affected_rows;

            JArray column_list = (JArray)data["column_list"];
            JArray rows = (JArray)data["rows"];

            // 创建 DataTable
            DataTable dt = new DataTable();
            allFields.Clear();
            foreach (var col in column_list)
            {
                dt.Columns.Add(col.ToString());
                allFields.Add(col.ToString());
            }

            if (affected_rows > 0)
            {
                // 遍历每行数据
                foreach (JArray row in rows)
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < column_list.Count; i++)
                    {
                        dr[i] = row[i].ToString();
                    }
                    dt.Rows.Add(dr);
                }
            }

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = dt;
            splitContainer1.Panel2Collapsed = false;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem==null)
            {
                return;
            }
            string dbName = comboBox1.SelectedItem.ToString();
            if (dbName != null && dbName.Length > 0)
            {
                CurrentDatabase = dbName;
                resolver = new SqlSuggestResolver(DefaultSqlMetadataProvider.getProviderInstance(), CurrentServer, CurrentDatabase);
                initTables();
            }

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == null)
            {
                return;
            }
            string tableName = comboBox2.SelectedItem.ToString();
            if (tableName != null && tableName.Length > 0)
            {
                CurrentTable = tableName;
                CurrentTabPage.Text = "查询" + tableName;
            }
        }

        private async Task ShowSuggestionsAsync()
        {
            int caretIndex = richTextBox1.SelectionStart;
            string sqlText = richTextBox1.Text;

            SqlContext context = analyzer.Analyze(sqlText, caretIndex);
            _currentContext = context;

            IReadOnlyList<SuggestItem> suggestions = await resolver.ResolveAsync(context);

            if (suggestions == null || suggestions.Count == 0)
            {
                listBoxSuggest.Visible = false;
                return;
            }

            listBoxSuggest.BeginUpdate();
            listBoxSuggest.DataSource = suggestions;
            listBoxSuggest.SelectedIndex = 0;
            listBoxSuggest.DisplayMember = nameof(SuggestItem.Text);
            listBoxSuggest.EndUpdate();

            PositionSuggestList();
            listBoxSuggest.Visible = true;
            listBoxSuggest.BringToFront();
        }

        private void ApplySuggestion()
        {
            if (listBoxSuggest.SelectedItem is not SuggestItem item)
                return;

            var ctx = _currentContext;

            // 关键字后自动加空格（体验优化）
            string insertText = item.Type == SuggestType.Keyword
                ? item.Text + " "
                : item.Text;

            richTextBox1.ReplaceToken(
                ctx.TokenStartIndex,
                ctx.TokenLength,
                insertText
            );

            listBoxSuggest.Visible = false;
            richTextBox1.Focus();
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!listBoxSuggest.Visible)
                return;

            if (e.KeyCode == Keys.Down)
            {
                listBoxSuggest.Focus();
                if (listBoxSuggest.Items.Count > 0)
                    listBoxSuggest.SelectedIndex = 0;

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplySuggestion();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                listBoxSuggest.Visible = false;
                e.SuppressKeyPress = true;
            }
        }

        private void listBoxSuggest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplySuggestion();
                e.Handled = true;
            }
        }

        private void listBoxSuggest_DoubleClick(object sender, EventArgs e)
        {
            ApplySuggestion();
        }

        private async void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (resolver == null)
            {
                return;
            }
            if (e.KeyCode == Keys.Space && richTextBox1.SelectionStart > 1 &&
            char.IsWhiteSpace(richTextBox1.Text[
            richTextBox1.SelectionStart - 2]))
            {
                return;
            }
            if (char.IsLetterOrDigit((char)e.KeyCode) ||
                e.KeyCode == Keys.OemPeriod ||
                e.KeyCode == Keys.Back ||
                e.KeyCode == Keys.Space)
            {
                await ShowSuggestionsAsync();
            }
        }

        private void PositionSuggestList()
        {
            var pos = richTextBox1.GetPositionFromCharIndex(
                richTextBox1.SelectionStart);

            listBoxSuggest.Left = richTextBox1.Left + pos.X;
            listBoxSuggest.Top = richTextBox1.Top + pos.Y + 20;
        }

    }
}
