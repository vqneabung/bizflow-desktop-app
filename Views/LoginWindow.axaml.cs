using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using bizflow_desktop_app.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is LoginViewModel vm)
        {
            vm.LoginSucceeded += OnLoginSucceeded;
        }
    }

    private void OnLoginSucceeded()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var lifetime = Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime;

            if (lifetime is not null)
            {
                var services = (Application.Current as App)?.Services;
                if (services is null) return;

                var mainWindow = new MainWindow
                {
                    DataContext = services.GetRequiredService<MainWindowViewModel>()
                };
                lifetime.MainWindow = mainWindow;
                mainWindow.Show();
                Close();
            }
        });
    }
}
