using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SQLViewerUWP;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public bool IsLogin { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize page size
        ConfigHelper.Set("pageSize", "100");

        // Check if logged in
        var servers = await HttpUtils.QueryServerAsync();
        if (servers == null)
        {
            IsLogin = await HandleLoginAsync();
        }
        else
        {
            IsLogin = true;
        }

        if (IsLogin)
        {
            await InitDataAsync(servers);
        }
    }

    private async Task InitDataAsync(List<string>? servers)
    {
        string? username = ConfigHelper.Get("username");
        if (!string.IsNullOrEmpty(username))
        {
            BtnLogin.Content = $"用户：{username}";
        }

        if (servers == null)
        {
            servers = await HttpUtils.QueryServerAsync();
        }

        if (servers != null)
        {
            foreach (string item in servers)
            {
                CbServer.Items.Add(item);
            }

            string? instanceName = ConfigHelper.Get("instance_name");
            if (!string.IsNullOrEmpty(instanceName))
            {
                CbServer.SelectedItem = instanceName;
            }
        }
    }

    private async Task<bool> HandleLoginAsync()
    {
        var loginDialog = new LoginDialog { Owner = this };
        bool? result = loginDialog.ShowDialog();

        if (result == true)
        {
            string username = loginDialog.Username;
            string password = loginDialog.Password;
            ConfigHelper.Set("username", username);
            BtnLogin.Content = $"用户：{username}";

            MessageBox.Show($"登录成功！账号：{username}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }
        else
        {
            return false;
        }
    }

    private async void CbServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TvDatabases.Items.Clear();
        
        if (CbServer.SelectedItem == null)
            return;

        string instanceName = CbServer.SelectedItem.ToString()!;
        ConfigHelper.Set("instance_name", instanceName);

        var dbs = await HttpUtils.QueryDbAsync(instanceName);
        foreach (string item in dbs)
        {
            var dbNode = new TreeViewItem
            {
                Header = item,
                Tag = "database"
            };
            // Add dummy child to show expand arrow
            dbNode.Items.Add("Loading...");
            TvDatabases.Items.Add(dbNode);
        }
    }

    private async void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        var treeViewItem = (TreeViewItem)sender;
        
        // Only load if we have the dummy item
        if (treeViewItem.Items.Count == 1 && treeViewItem.Items[0] is string)
        {
            treeViewItem.Items.Clear();
            
            string dbName = treeViewItem.Header.ToString()!;
            var tables = await HttpUtils.QueryTableAsync(dbName);
            
            foreach (string item in tables)
            {
                var tableNode = new TreeViewItem
                {
                    Header = item,
                    Tag = "table"
                };
                treeViewItem.Items.Add(tableNode);
            }
        }
    }

    private void TvDatabases_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (TvDatabases.SelectedItem is TreeViewItem selectedNode)
        {
            if (selectedNode.Tag?.ToString() != "table")
                return;

            var parentNode = selectedNode.Parent as TreeViewItem;
            if (parentNode == null)
                return;

            string currentDatabase = parentNode.Header.ToString()!;
            string currentTable = selectedNode.Header.ToString()!;
            string? instanceName = ConfigHelper.Get("instance_name");

            if (string.IsNullOrEmpty(instanceName))
                return;

            // Create new tab with TableViewer
            var tabItem = new TabItem
            {
                Header = CreateTabHeader(currentTable)
            };

            var tableViewer = new TableViewerControl(instanceName, currentDatabase, currentTable);
            tabItem.Content = tableViewer;

            TabMain.Items.Add(tabItem);
            TabMain.SelectedItem = tabItem;
        }
    }

    private StackPanel CreateTabHeader(string title)
    {
        var header = new StackPanel { Orientation = Orientation.Horizontal };
        
        header.Children.Add(new TextBlock 
        { 
            Text = title,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        });

        var closeButton = new Button
        {
            Content = "✕",
            Width = 20,
            Height = 20,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Cursor = Cursors.Hand
        };

        closeButton.Click += (s, e) =>
        {
            // Find and remove the tab
            for (int i = 0; i < TabMain.Items.Count; i++)
            {
                if (TabMain.Items[i] is TabItem tab && tab.Header == header)
                {
                    TabMain.Items.RemoveAt(i);
                    break;
                }
            }
            e.Handled = true;
        };

        header.Children.Add(closeButton);
        return header;
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        bool flag = await HandleLoginAsync();
        if (!IsLogin)
        {
            IsLogin = flag;
            if (flag)
            {
                await InitDataAsync(null);
            }
        }
    }

    private void CbPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CbPageSize.SelectedItem is ComboBoxItem item)
        {
            string pageSize = item.Content.ToString()!;
            ConfigHelper.Set("pageSize", pageSize);
        }
    }

    private void BtnNewQuery_Click(object sender, RoutedEventArgs e)
    {
        string? instanceName = ConfigHelper.Get("instance_name");
        if (string.IsNullOrEmpty(instanceName))
        {
            MessageBox.Show("请先选择服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var tabItem = new TabItem();
        var header = CreateTabHeader("新建查询");
        tabItem.Header = header;

        var sqlEditor = new SqlEditorControl(instanceName, tabItem);
        tabItem.Content = sqlEditor;

        TabMain.Items.Add(tabItem);
        TabMain.SelectedItem = tabItem;
    }
}
