using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectLauncher.Core.Projects;
using ProjectLauncher.Core.GitHubRepositories;
using ProjectLauncher.Core.Streaks;
using ProjectLauncher.Data.EF.GitHubRepositories;
using ProjectLauncher.Data.EF.Projects;
using ProjectLauncher.Data.EF.Streaks;

namespace ProjectLauncher.Data.EF;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectLauncherData(this IServiceCollection services) =>
        services.AddProjectLauncherData(ProjectLauncherDatabase.CreateConnectionString());

    public static IServiceCollection AddProjectLauncherData(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        services.AddTransient<IProjectStore, ProjectStore>();
        services.AddTransient<IGitHubRepositoryStore, GitHubRepositoryStore>();
        services.AddTransient<IProjectStreakStore, ProjectStreakStore>();
        return services;
    }
}
