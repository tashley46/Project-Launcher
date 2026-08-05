using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectLauncher.Data.EF;

public static class DatabaseInitialization
{
    public static void InitializeProjectLauncherDatabase(this IServiceProvider services)
    {
        Directory.CreateDirectory(ProjectLauncherDatabase.GetDataDirectory());

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
}

