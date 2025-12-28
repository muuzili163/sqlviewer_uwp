using System.Windows;

namespace SQLViewerUWP
{
    public partial class LoginDialog : Window
    {
        public string Username { get; private set; } = "";
        public string Password { get; private set; } = "";

        public LoginDialog()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            Username = TxtUsername.Text.Trim();
            Password = TxtPassword.Password;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("请输入用户名和密码", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnLogin.IsEnabled = false;
            BtnLogin.Content = "正在登录...";

            bool success = await HttpUtils.LoginAsync(Username, Password);

            if (success)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("登录失败，请检查账号密码！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnLogin.IsEnabled = true;
                BtnLogin.Content = "登录";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TxtPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }
    }
}
