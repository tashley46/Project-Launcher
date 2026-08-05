using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Core.Streaks;

public interface IProjectStreakStore
{
    Task<ProjectStreak?> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken);
}
