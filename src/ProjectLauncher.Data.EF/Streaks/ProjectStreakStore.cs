using Microsoft.EntityFrameworkCore;
using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.Streaks;

namespace ProjectLauncher.Data.EF.Streaks;

public sealed class ProjectStreakStore(
    IDbContextFactory<ApplicationDbContext> contextFactory) : IProjectStreakStore
{
    public async Task<ProjectStreak?> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ProjectStreaks
            .AsNoTracking()
            .SingleOrDefaultAsync(
                streak => streak.ProjectId == projectId && !streak.IsDeleted,
                cancellationToken);
    }
}
