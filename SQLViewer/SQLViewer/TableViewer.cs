using Newtonsoft.Json.Linq;
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
    public partial class TableViewer : UserControl
    {
        public string CurrentServer { get; private set; }
        public string CurrentDatabase { get; private set; }
        public string CurrentTable { get; private set; }

        private List<string> allFields=new List<string>();
        private bool _isQuerying = false;

        public TableViewer(string serverName, string dbName, string tbName)
        {
            InitializeComponent();
            CurrentServer = serverName;
            CurrentDatabase = dbName;
            CurrentTable = tbName;
            listBoxSuggest.Visible = false;

            //字段名自动补全
            listBoxSuggest.Click += ListBoxSuggest_Click;
            textBox1.TextChanged += TextBox1_TextChanged;
            textBox1.KeyDown += TextBox1_KeyDown;

            //失去焦点时隐藏listBoxSuggest
            textBox1.LostFocus += Control_LostFocus;
            listBoxSuggest.LostFocus += Control_LostFocus;
        }

        private void UserControl1_Load(object sender, EventArgs e)
        {
            initDataGridView();

            initTableDDL();

            //初始化排序与查询条件
            lastSortColumn = "";
            lastSortAsc = true;
            textBox1.Text = "";
            label5.Text ="服务:"+CurrentServer;


            //查询数据
            handleQuery();

        }

        private async void initTableDDL()
        {
            //查询表结构
            string tableDDL = await Task.Run(() =>
            {
                return HttpUtils.showTable(CurrentServer, CurrentDatabase, CurrentTable);
            });
            richTextBox1.Text = tableDDL;
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

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Visible = !richTextBox1.Visible;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            handleQuery();
        }

        public async void handleQuery()
        {
            if (_isQuerying) {
                return;
            }
            _isQuerying = true;
            button3.Enabled = false;
            button3.Text = "查询中...";

            try
            {
                //组装sql
                string orderBy = "";
                if (lastSortColumn != null && !lastSortColumn.Trim().Equals(""))
                {
                    orderBy = lastSortColumn + (lastSortAsc ? " DESC" : "");
                }
                string sql = buildSql(textBox1.Text, orderBy);
                label1.Text = sql;

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
                button3.Enabled = true;
                button3.Text = "查询";
            }
            
        }

        public string buildSql(string condition, string orderBy)
        {
            string sql = "select * from " + CurrentTable;
            if (condition != null && !condition.Trim().Equals(""))
            {
                sql += (" where " + condition);
            }
            if (orderBy != null && !orderBy.Trim().Equals(""))
            {
                sql += (" order by " + orderBy);
            }
            return sql;
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

        }

        // 保存上一列的排序方向
        private string lastSortColumn = "";
        private bool lastSortAsc = true;

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;

            // 切换排序方向
            if (colName == lastSortColumn)
                lastSortAsc = !lastSortAsc;
            else
            {
                lastSortColumn = colName;
                lastSortAsc = true;
            }

            handleQuery();

            // 显示排序图标（三角形）
            dataGridView1.Columns[colName].HeaderCell.SortGlyphDirection =
                lastSortAsc ? SortOrder.Descending : SortOrder.Ascending;
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            button5.Enabled = false;
            button5.Text = "查询中...";
            int count = await Task.Run(() =>
            {
                return HttpUtils.countBySql(CurrentServer, CurrentDatabase, CurrentTable);
            });
             
            label4.Text = "总数：" + count;
            button5.Enabled = true;
            button5.Text = "计算总数";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            lastSortColumn = "";
            lastSortAsc = true;
            handleQuery();
        }

        private const int MAX_COLUMN_WIDTH = 500;

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (lastSortColumn != null && !lastSortColumn.Trim().Equals(""))
            {
                // 显示排序图标（三角形）
                dataGridView1.Columns[lastSortColumn].HeaderCell.SortGlyphDirection =
                lastSortAsc ? SortOrder.Descending : SortOrder.Ascending;
            }

            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
                if (col.Width > MAX_COLUMN_WIDTH)
                {
                    col.Width = MAX_COLUMN_WIDTH;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(label1.Text))
            {
                Clipboard.SetText(label1.Text); // 将内容复制到剪切板
            }
        }

        //自动补全
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            string word = GetCurrentWord();

            if (string.IsNullOrWhiteSpace(word))
            {
                listBoxSuggest.Visible = false;
                return;
            }

            var matches = allFields
                .Where(f => f.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
            {
                listBoxSuggest.Visible = false;
                return;
            }

            listBoxSuggest.BeginUpdate();
            listBoxSuggest.Items.Clear();
            matches.ForEach(m => listBoxSuggest.Items.Add(m));
            listBoxSuggest.SelectedIndex = 0;
            listBoxSuggest.EndUpdate();

            ShowSuggestBox();
        }

        private string GetCurrentWord()
        {
            int pos = textBox1.SelectionStart;
            string text = textBox1.Text.Substring(0, pos);

            int lastSpace = text.LastIndexOfAny(new[] { ' ', ',', '\n', '\t' });
            return lastSpace >= 0 ? text[(lastSpace + 1)..] : text;
        }

        private void ShowSuggestBox()
        {
            listBoxSuggest.Left = textBox1.Left;
            listBoxSuggest.Top = textBox1.Bottom + 2;
            listBoxSuggest.BringToFront();
            listBoxSuggest.Visible = true;
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (listBoxSuggest.Visible) { 

                if (e.KeyCode == Keys.Down)
                {
                    listBoxSuggest.SelectedIndex =
                        Math.Min(listBoxSuggest.SelectedIndex + 1, listBoxSuggest.Items.Count - 1);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    listBoxSuggest.SelectedIndex =
                        Math.Max(listBoxSuggest.SelectedIndex - 1, 0);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
                {
                    InsertSelected();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    listBoxSuggest.Visible = false;
                }
            }
            else
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    handleQuery();
                }
            }
        }

        private void InsertSelected()
        {
            if (listBoxSuggest.SelectedItem == null) return;

            string selected = listBoxSuggest.SelectedItem.ToString();

            int pos = textBox1.SelectionStart;
            string text = textBox1.Text;

            int start = pos - GetCurrentWord().Length;
            textBox1.Text = text.Remove(start, pos - start)
                                 .Insert(start, selected);

            textBox1.SelectionStart = start + selected.Length;
            listBoxSuggest.Visible = false;
        }

        private void ListBoxSuggest_Click(object sender, EventArgs e)
        {
            InsertSelected();
        }

        private async void Control_LostFocus(object sender, EventArgs e)
        {
            // 延迟一下，等点击事件完成
            await Task.Delay(50);

            if (!textBox1.Focused && !listBoxSuggest.Focused)
            {
                listBoxSuggest.Visible = false;
            }
        }

    }
}
