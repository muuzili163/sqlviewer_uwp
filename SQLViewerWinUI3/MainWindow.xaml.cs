using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using SQLViewerWinUI3.Views;
using SQLViewerWinUI3.Helpers;

namespace SQLViewerWinUI3
{
    public sealed partial class MainWindow : Window
    {
        public bool IsLogin { get; private set; }

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Archery互联网SQL审计";
            
            // Set window size
            var appWindow = this.AppWindow;
            if (appWindow != null)
            {
                var displayArea = Microsoft.UI.Windowing.DisplayArea.Primary;
                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(
                    displayArea.WorkArea.X,
                    displayArea.WorkArea.Y,
                    displayArea.WorkArea.Width,
                    displayArea.WorkArea.Height
                ));
            }

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            ConfigHelper.Set("pageSize", "100");

            comboBox2.SelectedIndex = 1;

            // Check if logged in
            List<string>? servers = HttpUtils.QueryServer();
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
                InitData(servers);
            }
        }

        private void InitData(List<string>? servers)
        {
            string? username = ConfigHelper.Get("username");
            if (username != null)
            {
                button1.Content = "用户：" + username;
            }

            if (servers == null)
            {
                servers = HttpUtils.QueryServer();
            }

            if (servers != null)
            {
                foreach (string item in servers)
                {
                    comboBox1.Items.Add(item);
                }

                string? instanceName = ConfigHelper.Get("instance_name");
                if (!string.IsNullOrEmpty(instanceName))
                {
                    comboBox1.SelectedItem = instanceName;
                }
            }
        }

        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            treeView1.RootNodes.Clear();
            
            if (comboBox1.SelectedItem == null)
                return;

            string instanceName = comboBox1.SelectedItem.ToString()!;
            ConfigHelper.Set("instance_name", instanceName);
            
            List<string> dbs = HttpUtils.QueryDb(instanceName);
            foreach (string item in dbs)
            {
                var rootNode = new TreeViewNode
                {
                    Content = item,
                    HasUnrealizedChildren = true
                };
                treeView1.RootNodes.Add(rootNode);
            }
        }

        private void TreeView1_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            if (args.Node.HasUnrealizedChildren)
            {
                args.Node.Children.Clear();
                string dbName = args.Node.Content.ToString()!;
                List<string> tables = HttpUtils.QueryTable(dbName);
                
                foreach (string item in tables)
                {
                    var tableNode = new TreeViewNode
                    {
                        Content = item
                    };
                    tableNode.SetValue(FrameworkElement.TagProperty, "table");
                    args.Node.Children.Add(tableNode);
                }
                
                args.Node.HasUnrealizedChildren = false;
            }
        }

        private void TreeView1_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            // Check if double-click simulation is needed
            // For now, we'll use single click to open tables
            var selectedNode = args.InvokedItem as TreeViewNode;
            if (selectedNode == null)
                return;

            var tag = selectedNode.GetValue(FrameworkElement.TagProperty) as string;
            if (tag != "table")
                return;

            string currentTable = selectedNode.Content.ToString()!;
            string currentDatabase = selectedNode.Parent?.Content?.ToString() ?? "";

            // Create new tab with TableViewer
            var tabItem = new TabViewItem
            {
                Header = currentTable,
                IconSource = new SymbolIconSource { Symbol = Symbol.Document }
            };

            var tableViewer = new TableViewer(
                ConfigHelper.Get("instance_name") ?? "",
                currentDatabase,
                currentTable
            );

            tabItem.Content = tableViewer;
            tabControl1.TabItems.Add(tabItem);
            tabControl1.SelectedItem = tabItem;
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            bool flag = await HandleLoginAsync();
            if (!IsLogin)
            {
                IsLogin = flag;
            }
        }

        private async System.Threading.Tasks.Task<bool> HandleLoginAsync()
        {
            var loginDialog = new LoginDialog
            {
                XamlRoot = this.Content.XamlRoot
            };

            var result = await loginDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string username = loginDialog.Username;
                string password = loginDialog.Password;
                
                ConfigHelper.Set("username", username);
                button1.Content = "用户：" + username;

                var messageDialog = new ContentDialog
                {
                    Title = "登录成功",
                    Content = $"登录成功，账号：{username}",
                    CloseButtonText = "确定",
                    XamlRoot = this.Content.XamlRoot
                };
                await messageDialog.ShowAsync();
                
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox2.SelectedItem != null)
            {
                string pageSize = comboBox2.SelectedItem.ToString()!;
                ConfigHelper.Set("pageSize", pageSize);
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            // Create new SQL query tab
            var tabItem = new TabViewItem
            {
                Header = "新建查询",
                IconSource = new SymbolIconSource { Symbol = Symbol.Edit }
            };

            var sqlEditor = new SqlEditor(ConfigHelper.Get("instance_name") ?? "", tabItem);
            tabItem.Content = sqlEditor;
            
            tabControl1.TabItems.Add(tabItem);
            tabControl1.SelectedItem = tabItem;
        }

        private void TabControl1_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
        }
    }
}
