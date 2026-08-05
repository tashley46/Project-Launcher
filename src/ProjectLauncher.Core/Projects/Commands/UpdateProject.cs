using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Commands;

public sealed record UpdateProjectCommand(
    int ProjectId,
    string Name,
    string? Description,
    string Lifecycle) : ICommand<Result<ProjectResponse>>;

public sealed class UpdateProjectCommandHandler(IProjectStore projectStore, TimeProvider timeProvider)
{
    public async Task<Result<ProjectResponse>> HandleAsync(
        UpdateProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ProjectId <= 0)
            return Result<ProjectResponse>.Failure(ProjectErrors.IdMustBePositive);
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<ProjectResponse>.Failure(ProjectErrors.NameRequired);

        var name = command.Name.Trim();
        if (name.Length > 120)
            return Result<ProjectResponse>.Failure(ProjectErrors.NameTooLong);

        var description = string.IsNullOrWhiteSpace(command.Description)
            ? null
            : command.Description.Trim();
        if (description?.Length > 1000)
            return Result<ProjectResponse>.Failure(ProjectErrors.DescriptionTooLong);

        if (!Enum.TryParse<ProjectLifecycle>(command.Lifecycle, true, out var lifecycle) ||
            lifecycle == ProjectLifecycle.Archived)
            return Result<ProjectResponse>.Failure(ProjectErrors.LifecycleInvalid);

        try
        {
            var project = await projectStore.GetByIdAsync(command.ProjectId, cancellationToken);
            if (project is null)
                return Result<ProjectResponse>.Failure(ProjectErrors.NotFound(command.ProjectId));

            project.Update(new ProjectDto
            {
                Name = name,
                Description = description,
                Folder = project.Folder,
                GitRootPath = project.GitRootPath,
                GitHubRepository = project.GitHubRepository,
                Streak = project.Streak,
                Lifecycle = lifecycle,
                IsFavorite = project.IsFavorite,
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
