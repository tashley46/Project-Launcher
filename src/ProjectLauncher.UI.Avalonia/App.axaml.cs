using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ProjectLauncher.Core.Projects.Commands;
using ProjectLauncher.Core.Projects.Queries;
using ProjectLauncher.Core.GitHubRepositories.Queries;
using ProjectLauncher.Core.GitHubRepositories.Commands;
using ProjectLauncher.Core.Streaks.Queries;
using ProjectLauncher.Core.Streaks.Commands;
using ProjectLauncher.Core.Infrastructure.Git;
using ProjectLauncher.Data.EF;
using ProjectLauncher.ViewModels;
using ProjectLauncher.Views;
using ProjectLauncher.Services;

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
        services.AddSingleton<IGitRepositoryReader, GitRepositoryReader>();
        services.AddTransient<AddProjectCommandHandler>();
        services.AddTransient<GetProjectQueryHandler>();
        services.AddTransient<GetProjectsQueryHandler>();
        services.AddTransient<GetArchivedProjectsQueryHandler>();
        services.AddTransient<GetFavoriteProjectsQueryHandler>();
        services.AddTransient<GetProjectIncludingDeletedQueryHandler>();
        services.AddTransient<UpdateProjectCommandHandler>();
        services.AddTransient<ArchiveProjectCommandHandler>();
        services.AddTransient<RestoreProjectCommandHandler>();
        services.AddTransient<SetProjectFavoriteCommandHandler>();
        services.AddTransient<GetGitHubRepositoryQueryHandler>();
        services.AddTransient<ConnectGitHubRepositoryCommandHandler>();
        services.AddTransient<GetProjectStreakQueryHandler>();
        services.AddTransient<RefreshProjectStreakCommandHandler>();
        services.AddTransient<GetOverallStreakQueryHandler>();
        services.AddTransient<GetProjectGitStatusQueryHandler>();
        services.AddTransient<RelocateProjectCommandHandler>();
        services.AddSingleton<ProjectFolderPicker>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>(provider =>
        {
            var window = new MainWindow { DataContext = provider.GetRequiredService<MainViewModel>() };
            provider.GetRequiredService<ProjectFolderPicker>().SetOwner(window);
            return window;
        });

        return services.BuildServiceProvider();
    }
}
