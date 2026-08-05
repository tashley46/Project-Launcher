using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ProjectLauncher.Data.EF;
using ProjectLauncher.ViewModels;
using ProjectLauncher.Views;

namespace ProjectLauncher;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _serviceProvider = ConfigureServices();
            _serviceProvider.InitializeProjectLauncherDatabase();

            desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            desktop.Exit += (_, _) => _serviceProvider.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddProjectLauncherData();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>(provider => new MainWindow
        {
            DataContext = provider.GetRequiredService<MainViewModel>(),
        });

        return services.BuildServiceProvider();
    }
}
