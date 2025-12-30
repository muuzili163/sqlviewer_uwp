using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLViewerWinUI3.Helpers;
using Windows.UI;
using Microsoft.UI;

namespace SQLViewerWinUI3.Views
{
    public sealed partial class SqlEditor : UserControl
    {
        public string CurrentServer { get; private set; }
        public string? CurrentDatabase { get; private set; }
        public string? CurrentTable { get; private set; }
        public TabViewItem CurrentTabPage { get; private set; }

        public List<string> databases = new List<string>();
        public List<string> tables = new List<string>();

        private List<string> allFields = new List<string>();
        private bool _isQuerying = false;

        public SqlEditor(string serverName, TabViewItem tabPage)
        {
            this.InitializeComponent();
            
            CurrentServer = serverName;
            CurrentTabPage = tabPage;

            this.Loaded += SqlEditor_Loaded;
        }

        private async void SqlEditor_Loaded(object sender, RoutedEventArgs e)
        {
            await InitDatabasesAsync();
        }

        private async Task InitDatabasesAsync()
        {
            databases = await Task.Run(() =>
            {
                return HttpUtils.QueryDb(CurrentServer);
            });
            
            foreach (string item in databases)
            {
                comboBox1.Items.Add(item);
            }
        }

        private void InitTables()
        {
            if (CurrentDatabase == null)
            {
                return;
            }
            
            comboBox2.Items.Clear();
            comboBox2.SelectedIndex = -1;
            CurrentTable = null;
            
            tables = HttpUtils.QueryTable(CurrentDatabase);
            foreach (string item in tables)
            {
                comboBox2.Items.Add(item);
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                return;
            }
            
            string? dbName = comboBox1.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(dbName))
            {
                CurrentDatabase = dbName;
                InitTables();
            }
        }

        private void ComboBox2_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox2.SelectedItem == null)
            {
                return;
            }
            
            string? tableName = comboBox2.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(tableName))
            {
                CurrentTable = tableName;
                CurrentTabPage.Header = "查询" + tableName;
            }
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentDatabase == null || CurrentTable == null)
            {
                var messageDialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "请选择表",
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot
                };
                await messageDialog.ShowAsync();
                return;
            }
            
            await HandleQueryAsync();
        }

        private async Task HandleQueryAsync()
        {
            if (_isQuerying)
            {
                return;
            }
            _isQuerying = true;
            button1.IsEnabled = false;
            button1.Content = "查询中...";

            try
            {
                string sql = BuildSql();

                JObject jo = await Task.Run(() =>
                {
                    return HttpUtils.QuerySql(CurrentServer, CurrentDatabase ?? "", CurrentTable ?? "", sql);
                });

                int status = (int)(jo["status"] ?? -1);
                JObject? data = jo["data"] as JObject;
                string? msg = jo["msg"]?.ToString();

                if (status != 0)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "错误",
                        Content = msg ?? "查询失败",
                        CloseButtonText = "确定",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
                else if (data != null)
                {
                    BindJsonToDataGrid(data);
                }
            }
            catch (Exception ex)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "错误",
                    Content = ex.Message,
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
            finally
            {
                _isQuerying = false;
                button1.IsEnabled = true;
                button1.Content = "查询";
            }
        }

        private string BuildSql()
        {
            richTextBox1.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string text);
            return text.Trim();
        }

        private void BindJsonToDataGrid(JObject data)
        {
            double queryTime = (double)(data["query_time"] ?? 0.0);
            int affectedRows = (int)(data["affected_rows"] ?? 0);

            label2.Text = "查询时间：" + queryTime;
            label3.Text = "条数：" + affectedRows;

            JArray? columnList = data["column_list"] as JArray;
            JArray? rows = data["rows"] as JArray;

            allFields.Clear();

            // Create a simple grid-based data display
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            
            if (columnList != null)
            {
                // Add column definitions
                foreach (var col in columnList)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 100 });
                    allFields.Add(col.ToString());
                }

                // Add header row
                for (int i = 0; i < columnList.Count; i++)
                {
                    var headerText = new TextBlock
                    {
                        Text = columnList[i].ToString(),
                        Padding = new Thickness(8, 4, 8, 4),
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    
                    var headerBorder = new Border
                    {
                        BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.LightGray),
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.LightGray),
                        Child = headerText
                    };
                    
                    Grid.SetRow(headerBorder, 0);
                    Grid.SetColumn(headerBorder, i);
                    grid.Children.Add(headerBorder);
                }

                // Add data rows
                if (rows != null && affectedRows > 0)
                {
                    for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        
                        JArray? row = rows[rowIndex] as JArray;
                        if (row != null)
                        {
                            for (int colIndex = 0; colIndex < columnList.Count; colIndex++)
                            {
                                var cellText = new TextBlock
                                {
                                    Text = row[colIndex]?.ToString() ?? "",
                                    Padding = new Thickness(8, 4, 8, 4),
                                    TextWrapping = TextWrapping.NoWrap,
                                    VerticalAlignment = VerticalAlignment.Center
                                };
                                
                                var border = new Border
                                {
                                    BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.LightGray),
                                    BorderThickness = new Thickness(0, 0, 1, 1),
                                    Child = cellText
                                };

                                Grid.SetRow(border, rowIndex + 1);
                                Grid.SetColumn(border, colIndex);
                                grid.Children.Add(border);
                            }
                        }
                    }
                }
            }

            dataGridContainer.Children.Clear();
            dataGridContainer.Children.Add(grid);
        }

        private void ListBoxSuggest_ItemClick(object sender, ItemClickEventArgs e)
        {
            // TODO: Implement SQL autocomplete
        }

        private void ListBoxSuggest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: handle selection changed
        }
    }
}
