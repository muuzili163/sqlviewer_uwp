using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

namespace SQLViewerUWP
{
    public partial class SqlEditorControl : UserControl
    {
        public string CurrentServer { get; private set; }
        public string? CurrentDatabase { get; private set; }
        public string? CurrentTable { get; private set; }
        public TabItem CurrentTabItem { get; private set; }

        private List<string> databases = new List<string>();
        private List<string> tables = new List<string>();
        private bool _isQuerying = false;

        public SqlEditorControl(string serverName, TabItem tabItem)
        {
            InitializeComponent();
            CurrentServer = serverName;
            CurrentTabItem = tabItem;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await InitDatabasesAsync();
        }

        private async Task InitDatabasesAsync()
        {
            databases = await HttpUtils.QueryDbAsync(CurrentServer);
            foreach (string item in databases)
            {
                CbDatabase.Items.Add(item);
            }
        }

        private async void CbDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbDatabase.SelectedItem == null)
                return;

            string dbName = CbDatabase.SelectedItem.ToString()!;
            if (!string.IsNullOrEmpty(dbName))
            {
                CurrentDatabase = dbName;
                await InitTablesAsync();
            }
        }

        private async Task InitTablesAsync()
        {
            if (CurrentDatabase == null)
                return;

            CbTable.Items.Clear();
            CbTable.SelectedIndex = -1;
            CurrentTable = null;

            tables = await HttpUtils.QueryTableAsync(CurrentDatabase);
            foreach (string item in tables)
            {
                CbTable.Items.Add(item);
            }
        }

        private void CbTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbTable.SelectedItem == null)
                return;

            string tableName = CbTable.SelectedItem.ToString()!;
            if (!string.IsNullOrEmpty(tableName))
            {
                CurrentTable = tableName;
                
                // Update tab header
                if (CurrentTabItem.Header is StackPanel panel && 
                    panel.Children[0] is TextBlock textBlock)
                {
                    textBlock.Text = $"查询{tableName}";
                }
            }
        }

        private async void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentDatabase == null || CurrentTable == null)
            {
                MessageBox.Show("请选择表", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await HandleQueryAsync();
        }

        private async Task HandleQueryAsync()
        {
            if (_isQuerying)
                return;

            _isQuerying = true;
            BtnExecute.IsEnabled = false;
            BtnExecute.Content = "查询中...";

            try
            {
                string sql = TxtSqlEditor.Text;

                if (string.IsNullOrWhiteSpace(sql))
                {
                    MessageBox.Show("请输入SQL语句", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                JObject jo = await HttpUtils.QuerySqlAsync(CurrentServer, CurrentDatabase!, CurrentTable!, sql);

                int status = jo["status"]?.Value<int>() ?? -1;
                JObject? data = jo["data"] as JObject;
                string? msg = jo["msg"]?.ToString();

                if (status != 0)
                {
                    MessageBox.Show(msg ?? "Unknown error", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (data != null)
                {
                    BindJsonToDataGrid(data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isQuerying = false;
                BtnExecute.IsEnabled = true;
                BtnExecute.Content = "查询";
            }
        }

        private void BindJsonToDataGrid(JObject data)
        {
            double queryTime = data["query_time"]?.Value<double>() ?? 0;
            int affectedRows = data["affected_rows"]?.Value<int>() ?? 0;

            LblQueryTime.Text = $"查询时间：{queryTime}";
            LblCount.Text = $"条数：{affectedRows}";

            JArray? columnList = data["column_list"] as JArray;
            JArray? rows = data["rows"] as JArray;

            if (columnList == null || rows == null)
                return;

            // Create DataTable
            DataTable dt = new DataTable();

            foreach (var col in columnList)
            {
                dt.Columns.Add(col.ToString());
            }

            if (affectedRows > 0)
            {
                foreach (JArray row in rows)
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < columnList.Count; i++)
                    {
                        dr[i] = row[i]?.ToString() ?? "";
                    }
                    dt.Rows.Add(dr);
                }
            }

            DgResults.ItemsSource = dt.DefaultView;
            DgResults.Visibility = Visibility.Visible;
        }

        private void TxtSqlEditor_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+Enter to execute
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                _ = HandleQueryAsync();
            }
        }
    }
}
