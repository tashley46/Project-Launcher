using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.Infrastructure.Git;
using ProjectLauncher.Core.Projects;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Streaks.Commands;

public sealed record RefreshProjectStreakCommand(int ProjectId)
    : ICommand<Result<ProjectStreakResponse>>;

public sealed class RefreshProjectStreakCommandHandler(
    IProjectStore projectStore,
    IGitRepositoryReader gitRepositoryReader,
    TimeProvider timeProvider)
{
    public async Task<Result<ProjectStreakResponse>> HandleAsync(
        RefreshProjectStreakCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ProjectId <= 0)
            return Result<ProjectStreakResponse>.Failure(ProjectStreakErrors.ProjectIdMustBePositive);

        try
        {
            var project = await projectStore.GetByIdAsync(command.ProjectId, cancellationToken);
            if (project is null)
                return Result<ProjectStreakResponse>.Failure(ProjectStreakErrors.NotFound(command.ProjectId));
            var history = await gitRepositoryReader.ReadCommitHistoryAsync(
                project.Folder.Path,
                cancellationToken);
            if (history.Error is not null)
                return Result<ProjectStreakResponse>.Failure(
                    ProjectStreakErrors.GitHistoryUnavailable(history.Error));
            if (!history.IsGitRepository)
                return Result<ProjectStreakResponse>.Failure(ProjectStreakErrors.NotGitRepository);
            if (history.IdentityEmails.Count == 0)
                return Result<ProjectStreakResponse>.Failure(ProjectStreakErrors.IdentityNotConfigured);

            var calculatedAt = timeProvider.GetUtcNow();
            var calculation = ProjectStreakCalculator.Calculate(
                history.Commits,
                history.IdentityEmails,
                DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
            project.Streak.Update(new ProjectStreakDto
            {
                ProjectId = project.Id,
                Project = project,
                CurrentDays = calculation.CurrentDays,
                LongestDays = calculation.LongestDays,
                LastCommitByUserAt = calculation.LastCommitByUserAt,
                ActiveCommitDaysLast30 = calculation.ActiveCommitDaysLast30,
                CalculatedAt = calculatedAt,
            }, calculatedAt);
            await projectStore.UpdateAsync(project, cancellationToken);
            return Result<ProjectStreakResponse>.Success(
                ProjectStreakResponse.FromDomain(project.Streak));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            return Result<ProjectStreakResponse>.Failure(
                ProjectStreakErrors.CouldNotSave(exception.Message));
        }
    }
}
