using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Commands;

public sealed record RestoreProjectCommand(int ProjectId) : ICommand<Result<ProjectResponse>>;

public sealed class RestoreProjectCommandHandler(IProjectStore projectStore, TimeProvider timeProvider)
{
    public async Task<Result<ProjectResponse>> HandleAsync(
        RestoreProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ProjectId <= 0)
            return Result<ProjectResponse>.Failure(ProjectErrors.IdMustBePositive);

        try
        {
            var project = await projectStore.GetByIdIncludingDeletedAsync(command.ProjectId, cancellationToken);
            if (project is null)
                return Result<ProjectResponse>.Failure(ProjectErrors.NotFound(command.ProjectId));
            if (!project.IsDeleted)
                return Result<ProjectResponse>.Failure(ProjectErrors.NotArchived(command.ProjectId));

            var modifiedDateTime = timeProvider.GetUtcNow();
            project.Update(new ProjectLaunch.Core.Domain.ProjectDto
            {
                Name = project.Name,
                Description = project.Description,
                Folder = project.Folder,
                GitRootPath = project.GitRootPath,
                GitHubRepository = project.GitHubRepository,
                Streak = project.Streak,
                Lifecycle = ProjectLaunch.Core.Domain.ProjectLifecycle.Active,
                IsFavorite = project.IsFavorite,
                LastOpenedAt = project.LastOpenedAt,
            }, modifiedDateTime);
            project.Restore();
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
