using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectLauncher.Data.EF;

public static class DatabaseInitialization
{
    public static void InitializeProjectLauncherDatabase(this IServiceProvider services)
    {
        Directory.CreateDirectory(ProjectLauncherDatabase.GetDataDirectory());

        var factory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var dbContext = factory.CreateDbContext();
        dbContext.Database.Migrate();
    }
}
