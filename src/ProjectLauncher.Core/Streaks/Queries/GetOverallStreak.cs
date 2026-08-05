using ProjectLauncher.Core.Infrastructure.Git;
using ProjectLauncher.Core.Projects;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Streaks.Queries;

public sealed record GetOverallStreakQuery : IQuery<Result<OverallStreakResponse>>;

public sealed record OverallStreakResponse(
    int CurrentDays,
    int LongestDays,
    int ActiveDaysLast30,
    DateTimeOffset CalculatedAt);

public sealed class GetOverallStreakQueryHandler(
    IProjectStore projectStore,
    IGitRepositoryReader gitRepositoryReader,
    TimeProvider timeProvider)
{
    public async Task<Result<OverallStreakResponse>> HandleAsync(
        GetOverallStreakQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dates = new HashSet<DateOnly>();
            foreach (var project in await projectStore.GetAllAsync(cancellationToken))
            {
                if (!Directory.Exists(project.Folder.Path)) continue;
                var history = await gitRepositoryReader.ReadCommitHistoryAsync(project.Folder.Path, cancellationToken);
                if (!history.IsGitRepository || history.Error is not null) continue;
                dates.UnionWith(ProjectStreakCalculator.GetActivityDates(history.Commits, history.IdentityEmails));
            }

            var now = timeProvider.GetUtcNow();
            var calculation = ProjectStreakCalculator.CalculateFromActivityDates(
                dates,
                DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
            return Result<OverallStreakResponse>.Success(new(
                calculation.CurrentDays,
                calculation.LongestDays,
                calculation.ActiveDaysLast30,
                now));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        { return Result<OverallStreakResponse>.Failure(ProjectStreakErrors.CouldNotLoad(exception.Message)); }
    }
}
