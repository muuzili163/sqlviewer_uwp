using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLViewerWinUI3.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Microsoft.UI;

namespace SQLViewerWinUI3.Views
{
    public sealed partial class TableViewer : UserControl
    {
        public string CurrentServer { get; private set; }
        public string CurrentDatabase { get; private set; }
        public string CurrentTable { get; private set; }

        private List<string> allFields = new List<string>();
        private bool _isQuerying = false;
        private string lastSortColumn = "";
        private bool lastSortAsc = true;

        public TableViewer(string serverName, string dbName, string tbName)
        {
            this.InitializeComponent();
            CurrentServer = serverName;
            CurrentDatabase = dbName;
            CurrentTable = tbName;

            this.Loaded += TableViewer_Loaded;
        }

        private async void TableViewer_Loaded(object sender, RoutedEventArgs e)
        {
            await InitTableDDLAsync();

            // Initialize sorting and query condition
            lastSortColumn = "";
            lastSortAsc = true;
            textBox1.Text = "";
            label5.Text = "服务:" + CurrentServer;

            // Query data
            await HandleQueryAsync();
        }

        private async Task InitTableDDLAsync()
        {
            // Query table structure
            string tableDDL = await Task.Run(() =>
            {
                return HttpUtils.ShowTable(CurrentServer, CurrentDatabase, CurrentTable);
            });

            var rtb = richTextBox1;
            rtb.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, tableDDL);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            richTextBox1.Visibility = richTextBox1.Visibility == Visibility.Visible 
                ? Visibility.Collapsed 
                : Visibility.Visible;
            
            scrollViewer1.Visibility = richTextBox1.Visibility == Visibility.Visible 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private async void Button3_Click(object sender, RoutedEventArgs e)
        {
            await HandleQueryAsync();
        }

        private async Task HandleQueryAsync()
        {
            if (_isQuerying)
            {
                return;
            }
            _isQuerying = true;
            button3.IsEnabled = false;
            button3.Content = "查询中...";

            try
            {
                // Build SQL
                string orderBy = "";
                if (!string.IsNullOrWhiteSpace(lastSortColumn))
                {
                    orderBy = lastSortColumn + (lastSortAsc ? " DESC" : "");
                }
                string sql = BuildSql(textBox1.Text, orderBy);
                label1.Text = sql;

                JObject jo = await Task.Run(() =>
                {
                    return HttpUtils.QuerySql(CurrentServer, CurrentDatabase, CurrentTable, sql);
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
                button3.IsEnabled = true;
                button3.Content = "查询";
            }
        }

        private string BuildSql(string condition, string orderBy)
        {
            string sql = "select * from " + CurrentTable;
            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += " where " + condition;
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                sql += " order by " + orderBy;
            }
            return sql;
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
                    var headerButton = new Button
                    {
                        Content = columnList[i].ToString(),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Tag = columnList[i].ToString(),
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                        Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.LightGray)
                    };
                    headerButton.Click += DataGridColumnHeader_Click;
                    Grid.SetRow(headerButton, 0);
                    Grid.SetColumn(headerButton, i);
                    grid.Children.Add(headerButton);
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

        private async void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string colName)
            {
                // Toggle sort direction
                if (colName == lastSortColumn)
                    lastSortAsc = !lastSortAsc;
                else
                {
                    lastSortColumn = colName;
                    lastSortAsc = true;
                }

                await HandleQueryAsync();
            }
        }

        private async void Button5_Click(object sender, RoutedEventArgs e)
        {
            button5.IsEnabled = false;
            button5.Content = "查询中...";
            
            int count = await Task.Run(() =>
            {
                return HttpUtils.CountBySql(CurrentServer, CurrentDatabase, CurrentTable);
            });

            label4.Text = "总数：" + count;
            button5.IsEnabled = true;
            button5.Content = "计算总数";
        }

        private async void Button4_Click(object sender, RoutedEventArgs e)
        {
            lastSortColumn = "";
            lastSortAsc = true;
            await HandleQueryAsync();
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(label1.Text))
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(label1.Text);
                Clipboard.SetContent(dataPackage);
            }
        }

        // Autocomplete
        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            string word = GetCurrentWord();

            if (string.IsNullOrWhiteSpace(word))
            {
                listBoxSuggest.Visibility = Visibility.Collapsed;
                return;
            }

            var matches = allFields
                .Where(f => f.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
            {
                listBoxSuggest.Visibility = Visibility.Collapsed;
                return;
            }

            listBoxSuggest.ItemsSource = matches;
            listBoxSuggest.SelectedIndex = 0;
            listBoxSuggest.Visibility = Visibility.Visible;
        }

        private string GetCurrentWord()
        {
            int pos = textBox1.SelectionStart;
            string text = textBox1.Text.Substring(0, pos);

            int lastSpace = text.LastIndexOfAny(new[] { ' ', ',', '\n', '\t' });
            return lastSpace >= 0 ? text.Substring(lastSpace + 1) : text;
        }

        private void TextBox1_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (listBoxSuggest.Visibility == Visibility.Visible)
            {
                if (e.Key == Windows.System.VirtualKey.Down)
                {
                    listBoxSuggest.SelectedIndex = Math.Min(listBoxSuggest.SelectedIndex + 1, listBoxSuggest.Items.Count - 1);
                    e.Handled = true;
                }
                else if (e.Key == Windows.System.VirtualKey.Up)
                {
                    listBoxSuggest.SelectedIndex = Math.Max(listBoxSuggest.SelectedIndex - 1, 0);
                    e.Handled = true;
                }
                else if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Tab)
                {
                    InsertSelected();
                    e.Handled = true;
                }
                else if (e.Key == Windows.System.VirtualKey.Escape)
                {
                    listBoxSuggest.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    _ = HandleQueryAsync();
                }
            }
        }

        private void InsertSelected()
        {
            if (listBoxSuggest.SelectedItem == null) return;

            string selected = listBoxSuggest.SelectedItem.ToString() ?? "";

            int pos = textBox1.SelectionStart;
            string text = textBox1.Text;

            int start = pos - GetCurrentWord().Length;
            textBox1.Text = text.Remove(start, pos - start).Insert(start, selected);

            textBox1.SelectionStart = start + selected.Length;
            listBoxSuggest.Visibility = Visibility.Collapsed;
        }

        private void ListBoxSuggest_ItemClick(object sender, ItemClickEventArgs e)
        {
            InsertSelected();
        }

        private void ListBoxSuggest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: handle selection changed
        }
    }
}
