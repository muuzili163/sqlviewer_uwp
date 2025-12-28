using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using SQLViewerUWP.Editor;

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
        private SqlContext? _currentContext;
        private SqlContextAnalyzer? analyzer;
        private SqlSuggestResolver? resolver;

        public SqlEditorControl(string serverName, TabItem tabItem)
        {
            InitializeComponent();
            CurrentServer = serverName;
            CurrentTabItem = tabItem;
            analyzer = new SqlContextAnalyzer();
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
                resolver = new SqlSuggestResolver(
                    DefaultSqlMetadataProvider.GetProviderInstance(), 
                    CurrentServer, 
                    CurrentDatabase);
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
            if (LstSqlSuggestions.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Down)
                {
                    LstSqlSuggestions.SelectedIndex = Math.Min(
                        LstSqlSuggestions.SelectedIndex + 1, 
                        LstSqlSuggestions.Items.Count - 1);
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    LstSqlSuggestions.SelectedIndex = Math.Max(
                        LstSqlSuggestions.SelectedIndex - 1, 
                        0);
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    ApplySuggestion();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    LstSqlSuggestions.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Ctrl+Enter to execute
                if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = true;
                    _ = HandleQueryAsync();
                }
                // Space to show suggestions
                else if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = true;
                    _ = ShowSuggestionsAsync();
                }
            }
        }

        private async void TxtSqlEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (resolver == null || analyzer == null)
                return;

            // Auto-show suggestions on certain characters
            if (TxtSqlEditor.Text.Length > 0)
            {
                char lastChar = TxtSqlEditor.Text[Math.Max(0, TxtSqlEditor.CaretIndex - 1)];
                if (lastChar == '.' || char.IsWhiteSpace(lastChar))
                {
                    await ShowSuggestionsAsync();
                }
            }
        }

        private async Task ShowSuggestionsAsync()
        {
            if (resolver == null || analyzer == null)
            {
                LstSqlSuggestions.Visibility = Visibility.Collapsed;
                return;
            }

            int caretIndex = TxtSqlEditor.CaretIndex;
            string sqlText = TxtSqlEditor.Text;

            SqlContext context = analyzer.Analyze(sqlText, caretIndex);
            _currentContext = context;

            IReadOnlyList<SuggestItem> suggestions = await resolver.ResolveAsync(context);

            if (suggestions == null || suggestions.Count == 0)
            {
                LstSqlSuggestions.Visibility = Visibility.Collapsed;
                return;
            }

            LstSqlSuggestions.ItemsSource = suggestions;
            LstSqlSuggestions.SelectedIndex = 0;

            // Position the suggestions list box
            var rect = TxtSqlEditor.GetRectFromCharacterIndex(caretIndex);
            LstSqlSuggestions.Margin = new Thickness(rect.Left, rect.Bottom + 5, 0, 0);

            LstSqlSuggestions.Visibility = Visibility.Visible;
        }

        private void ApplySuggestion()
        {
            if (LstSqlSuggestions.SelectedItem is not SuggestItem item)
                return;

            var ctx = _currentContext;
            if (ctx == null)
                return;

            // Add space after keyword for better UX
            string insertText = item.Type == SuggestType.Keyword
                ? item.Text + " "
                : item.Text;

            // Replace the token
            int start = ctx.TokenStartIndex;
            int length = ctx.TokenLength;
            
            string text = TxtSqlEditor.Text;
            TxtSqlEditor.Text = text.Remove(start, length).Insert(start, insertText);
            TxtSqlEditor.CaretIndex = start + insertText.Length;

            LstSqlSuggestions.Visibility = Visibility.Collapsed;
        }

        private void LstSqlSuggestions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ApplySuggestion();
        }

        private void LstSqlSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySuggestion();
                e.Handled = true;
            }
        }
    }
}
