using Microsoft.EntityFrameworkCore;
using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Data.EF;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<GitHubRepository> GitHubRepositories => Set<GitHubRepository>();

    public DbSet<ProjectStreak> ProjectStreaks => Set<ProjectStreak>();

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public DbSet<ImportLog> ImportLogs => Set<ImportLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

