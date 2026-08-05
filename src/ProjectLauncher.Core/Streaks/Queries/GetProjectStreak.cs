using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Streaks.Queries;

public sealed record GetProjectStreakQuery(int ProjectId)
    : IQuery<Result<ProjectStreakResponse>>;

public sealed class GetProjectStreakQueryHandler(IProjectStreakStore streakStore)
{
    public async Task<Result<ProjectStreakResponse>> HandleAsync(
        GetProjectStreakQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ProjectId <= 0)
        {
            return Result<ProjectStreakResponse>.Failure(
                ProjectStreakErrors.ProjectIdMustBePositive);
        }

        try
        {
            var streak = await streakStore.GetByProjectIdAsync(query.ProjectId, cancellationToken);
            return streak is null
                ? Result<ProjectStreakResponse>.Failure(ProjectStreakErrors.NotFound(query.ProjectId))
                : Result<ProjectStreakResponse>.Success(ProjectStreakResponse.FromDomain(streak));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<ProjectStreakResponse>.Failure(
                ProjectStreakErrors.CouldNotLoad(exception.Message));
        }
    }
}
