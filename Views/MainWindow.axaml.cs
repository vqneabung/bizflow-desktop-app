using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using bizflow_desktop_app.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace bizflow_desktop_app.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainWindowViewModel vm)
        {
            vm.LogoutSucceeded += OnLogoutSucceeded;
        }
    }

    private void OnLogoutSucceeded()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var lifetime = Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime;

            if (lifetime is null) return;

            var services = (Application.Current as App)?.Services;
            if (services is null) return;

            var loginWindow = new LoginWindow
            {
                DataContext = services.GetRequiredService<LoginViewModel>()
            };
            lifetime.MainWindow = loginWindow;
            loginWindow.Show();
            Close();
        });
    }
}