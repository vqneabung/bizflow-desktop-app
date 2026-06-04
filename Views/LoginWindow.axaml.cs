using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using bizflow_desktop_app.ViewModels;

namespace bizflow_desktop_app.Views;

/// <summary>
/// LoginWindow — form đăng nhập.
/// Khi login thành công, đóng window và mở MainWindow (dashboard).
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is LoginViewModel vm)
        {
            vm.LoginSucceeded += OnLoginSucceeded;
        }
    }

    private void OnLoginSucceeded()
    {
        // Chạy trên UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var lifetime = Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime;

            if (lifetime is not null)
            {
                // Mở MainWindow (dashboard)
                var mainWindow = new MainWindow();
                lifetime.MainWindow = mainWindow;
                mainWindow.Show();

                // Đóng LoginWindow
                Close();
            }
        });
    }
}
