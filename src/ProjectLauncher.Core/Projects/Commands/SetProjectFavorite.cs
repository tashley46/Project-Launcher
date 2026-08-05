using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Commands;

public sealed record SetProjectFavoriteCommand(int ProjectId, bool IsFavorite)
    : ICommand<Result<ProjectResponse>>;

public sealed class SetProjectFavoriteCommandHandler(
    IProjectStore projectStore,
    TimeProvider timeProvider)
{
    public async Task<Result<ProjectResponse>> HandleAsync(
        SetProjectFavoriteCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ProjectId <= 0)
            return Result<ProjectResponse>.Failure(ProjectErrors.IdMustBePositive);

        try
        {
            var project = await projectStore.GetByIdAsync(command.ProjectId, cancellationToken);
            if (project is null)
                return Result<ProjectResponse>.Failure(ProjectErrors.NotFound(command.ProjectId));
            if (project.IsFavorite == command.IsFavorite)
                return Result<ProjectResponse>.Success(ProjectResponse.FromDomain(project));

            project.Update(new ProjectDto
            {
                Name = project.Name,
                Description = project.Description,
                Folder = project.Folder,
                GitRootPath = project.GitRootPath,
                GitHubRepository = project.GitHubRepository,
                Streak = project.Streak,
                Lifecycle = project.Lifecycle,
                IsFavorite = command.IsFavorite,
                LastOpenedAt = project.LastOpenedAt,
            }, timeProvider.GetUtcNow());
            await projectStore.UpdateAsync(project, cancellationToken);
            return Result<ProjectResponse>.Success(ProjectResponse.FromDomain(project));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.CouldNotSave(exception.Message));
        }
    }
}
