using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ProjectLauncher.Core.Projects.Commands;
using ProjectLauncher.Core.Projects.Queries;
using ProjectLauncher.Core.GitHubRepositories.Queries;
using ProjectLauncher.Core.Streaks.Queries;
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
        services.AddSingleton(TimeProvider.System);
        services.AddTransient<AddProjectCommandHandler>();
        services.AddTransient<GetProjectQueryHandler>();
        services.AddTransient<GetProjectsQueryHandler>();
        services.AddTransient<GetArchivedProjectsQueryHandler>();
        services.AddTransient<GetProjectIncludingDeletedQueryHandler>();
        services.AddTransient<UpdateProjectCommandHandler>();
        services.AddTransient<ArchiveProjectCommandHandler>();
        services.AddTransient<RestoreProjectCommandHandler>();
        services.AddTransient<GetGitHubRepositoryQueryHandler>();
        services.AddTransient<GetProjectStreakQueryHandler>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>(provider => new MainWindow
        {
            DataContext = provider.GetRequiredService<MainViewModel>(),
        });

        return services.BuildServiceProvider();
    }
}
