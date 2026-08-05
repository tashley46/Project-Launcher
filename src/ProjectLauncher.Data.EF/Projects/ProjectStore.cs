using Microsoft.EntityFrameworkCore;
using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.Projects;

namespace ProjectLauncher.Data.EF.Projects;

public sealed class ProjectStore(IDbContextFactory<ApplicationDbContext> contextFactory) : IProjectStore
{
    public async Task<bool> ExistsByFolderPathAsync(
        string normalizedPath,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var paths = await context.Projects
            .AsNoTracking()
            .Where(project => !project.IsDeleted)
            .Select(project => project.Folder.Path)
            .ToListAsync(cancellationToken);

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return paths.Any(path => string.Equals(path, normalizedPath, comparison));
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Projects.Add(project);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Project?> GetByIdAsync(int projectId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Projects
            .AsNoTracking()
            .Include(project => project.Streak)
            .Include(project => project.GitHubRepository)
            .SingleOrDefaultAsync(
                project => project.Id == projectId && !project.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var projects = await context.Projects
            .AsNoTracking()
            .Where(project => !project.IsDeleted)
            .Include(project => project.Streak)
            .Include(project => project.GitHubRepository)
            .ToListAsync(cancellationToken);

        return projects
            .OrderByDescending(project => project.ModifiedDateTime)
            .ToArray();
    }
}
