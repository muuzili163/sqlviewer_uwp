using Microsoft.UI.Xaml.Controls;
using System;
using SQLViewerWinUI3.Helpers;

namespace SQLViewerWinUI3.Views
{
    public sealed partial class LoginDialog : ContentDialog
    {
        public string Username { get; private set; } = "";
        public string Password { get; private set; } = "";

        public LoginDialog()
        {
            this.Title = "登录";
            this.PrimaryButtonText = "登录";
            this.CloseButtonText = "取消";
            this.DefaultButton = ContentDialogButton.Primary;

            // Create the content
            var stackPanel = new StackPanel
            {
                Spacing = 12
            };

            var usernameBox = new TextBox
            {
                Header = "用户名",
                PlaceholderText = "请输入用户名"
            };

            var passwordBox = new PasswordBox
            {
                Header = "密码",
                PlaceholderText = "请输入密码"
            };

            stackPanel.Children.Add(usernameBox);
            stackPanel.Children.Add(passwordBox);

            this.Content = stackPanel;

            this.PrimaryButtonClick += async (sender, args) =>
            {
                var deferral = args.GetDeferral();
                
                Username = usernameBox.Text.Trim();
                Password = passwordBox.Password;

                sender.IsPrimaryButtonEnabled = false;
                sender.PrimaryButtonText = "正在登录...";

                bool success = HttpUtils.Login(Username, Password);

                if (success)
                {
                    // Login successful, close dialog
                    deferral.Complete();
                }
                else
                {
                    // Login failed, show error and keep dialog open
                    args.Cancel = true;
                    
                    var errorDialog = new ContentDialog
                    {
                        Title = "登录失败",
                        Content = "登录失败，请检查账号密码！",
                        CloseButtonText = "确定",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();

                    sender.IsPrimaryButtonEnabled = true;
                    sender.PrimaryButtonText = "登录";
                    deferral.Complete();
                }
            };
        }
    }
}
