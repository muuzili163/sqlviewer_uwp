using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

namespace SQLViewerUWP
{
    public partial class TableViewerControl : UserControl
    {
        public string CurrentServer { get; private set; }
        public string CurrentDatabase { get; private set; }
        public string CurrentTable { get; private set; }

        private List<string> allFields = new List<string>();
        private bool _isQuerying = false;
        private string lastSortColumn = "";
        private ListSortDirection lastSortDirection = ListSortDirection.Ascending;

        public TableViewerControl(string serverName, string dbName, string tbName)
        {
            InitializeComponent();
            CurrentServer = serverName;
            CurrentDatabase = dbName;
            CurrentTable = tbName;

            LblServer.Text = $"服务:{CurrentServer}";
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Load table DDL
            await InitTableDDLAsync();

            // Query data
            await HandleQueryAsync();
        }

        private async Task InitTableDDLAsync()
        {
            try
            {
                string tableDDL = await HttpUtils.ShowTableAsync(CurrentServer, CurrentDatabase, CurrentTable);
                TxtDDL.Text = tableDDL;
            }
            catch (Exception ex)
            {
                TxtDDL.Text = $"Error loading table structure: {ex.Message}";
            }
        }

        private async Task HandleQueryAsync()
        {
            if (_isQuerying)
                return;

            _isQuerying = true;
            BtnQuery.IsEnabled = false;
            BtnQuery.Content = "查询中...";

            try
            {
                // Build SQL
                string orderBy = "";
                if (!string.IsNullOrEmpty(lastSortColumn))
                {
                    orderBy = lastSortColumn + (lastSortDirection == ListSortDirection.Descending ? " DESC" : "");
                }
                string sql = BuildSql(TxtCondition.Text, orderBy);
                LblSql.Text = sql;

                JObject jo = await HttpUtils.QuerySqlAsync(CurrentServer, CurrentDatabase, CurrentTable, sql);

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
                BtnQuery.IsEnabled = true;
                BtnQuery.Content = "查询";
            }
        }

        private string BuildSql(string condition, string orderBy)
        {
            string sql = $"select * from {CurrentTable}";
            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += $" where {condition}";
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                sql += $" order by {orderBy}";
            }
            return sql;
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
            allFields.Clear();

            foreach (var col in columnList)
            {
                string colName = col.ToString();
                dt.Columns.Add(colName);
                allFields.Add(colName);
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

            DgData.ItemsSource = dt.DefaultView;
        }

        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            _ = HandleQueryAsync();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            lastSortColumn = "";
            lastSortDirection = ListSortDirection.Ascending;
            _ = HandleQueryAsync();
        }

        private async void BtnCount_Click(object sender, RoutedEventArgs e)
        {
            BtnCount.IsEnabled = false;
            BtnCount.Content = "查询中...";

            try
            {
                int count = await HttpUtils.CountBySqlAsync(CurrentServer, CurrentDatabase, CurrentTable);
                LblTotal.Text = $"总数：{count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnCount.IsEnabled = true;
                BtnCount.Content = "计算总数";
            }
        }

        private void BtnToggleDDL_Click(object sender, RoutedEventArgs e)
        {
            TxtDDL.Visibility = TxtDDL.Visibility == Visibility.Visible 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private void BtnCopySql_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LblSql.Text))
            {
                Clipboard.SetText(LblSql.Text);
            }
        }

        private void DgData_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            string colName = e.Column.SortMemberPath;

            // Toggle sort direction
            if (colName == lastSortColumn)
            {
                lastSortDirection = lastSortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                lastSortColumn = colName;
                lastSortDirection = ListSortDirection.Descending;
            }

            e.Column.SortDirection = lastSortDirection;

            _ = HandleQueryAsync();
        }

        // Auto-complete functionality
        private void TxtCondition_TextChanged(object sender, TextChangedEventArgs e)
        {
            string word = GetCurrentWord();

            if (string.IsNullOrWhiteSpace(word))
            {
                LstSuggestions.Visibility = Visibility.Collapsed;
                return;
            }

            var matches = allFields
                .Where(f => f.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
            {
                LstSuggestions.Visibility = Visibility.Collapsed;
                return;
            }

            LstSuggestions.ItemsSource = matches;
            LstSuggestions.SelectedIndex = 0;
            LstSuggestions.Visibility = Visibility.Visible;
        }

        private string GetCurrentWord()
        {
            int pos = TxtCondition.CaretIndex;
            string text = TxtCondition.Text.Substring(0, pos);

            int lastSpace = text.LastIndexOfAny(new[] { ' ', ',', '\n', '\t' });
            return lastSpace >= 0 ? text.Substring(lastSpace + 1) : text;
        }

        private void TxtCondition_KeyDown(object sender, KeyEventArgs e)
        {
            if (LstSuggestions.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Down)
                {
                    LstSuggestions.SelectedIndex = Math.Min(LstSuggestions.SelectedIndex + 1, LstSuggestions.Items.Count - 1);
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    LstSuggestions.SelectedIndex = Math.Max(LstSuggestions.SelectedIndex - 1, 0);
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    InsertSelected();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    LstSuggestions.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    _ = HandleQueryAsync();
                }
            }
        }

        private void InsertSelected()
        {
            if (LstSuggestions.SelectedItem == null)
                return;

            string selected = LstSuggestions.SelectedItem.ToString()!;

            int pos = TxtCondition.CaretIndex;
            string text = TxtCondition.Text;

            int start = pos - GetCurrentWord().Length;
            TxtCondition.Text = text.Remove(start, pos - start).Insert(start, selected);
            TxtCondition.CaretIndex = start + selected.Length;

            LstSuggestions.Visibility = Visibility.Collapsed;
        }

        private void LstSuggestions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InsertSelected();
        }

        private void LstSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InsertSelected();
                e.Handled = true;
            }
        }
    }
}
