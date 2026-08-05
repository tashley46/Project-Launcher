using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.Infrastructure.Git;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Commands;

public sealed record RelocateProjectCommand(int ProjectId, string FolderPath)
    : ICommand<Result<ProjectResponse>>;

public sealed class RelocateProjectCommandHandler(
    IProjectStore projectStore,
    IGitRepositoryReader gitRepositoryReader,
    TimeProvider timeProvider)
{
    public async Task<Result<ProjectResponse>> HandleAsync(
        RelocateProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ProjectId <= 0)
            return Result<ProjectResponse>.Failure(ProjectErrors.IdMustBePositive);
        if (string.IsNullOrWhiteSpace(command.FolderPath))
            return Result<ProjectResponse>.Failure(ProjectErrors.FolderRequired);
        if (!Path.IsPathFullyQualified(command.FolderPath))
            return Result<ProjectResponse>.Failure(ProjectErrors.FolderMustBeAbsolute);

        string path;
        try { path = Path.TrimEndingDirectorySeparator(Path.GetFullPath(command.FolderPath)); }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        { return Result<ProjectResponse>.Failure(ProjectErrors.FolderPathInvalid); }

        if (!Directory.Exists(path))
            return Result<ProjectResponse>.Failure(ProjectErrors.FolderNotFound);
        if (!CanAccess(path))
            return Result<ProjectResponse>.Failure(ProjectErrors.FolderNotAccessible);

        try
        {
            if (await projectStore.ExistsByFolderPathAsync(path, command.ProjectId, cancellationToken))
                return Result<ProjectResponse>.Failure(ProjectErrors.AlreadyExists(path));

            var project = await projectStore.GetByIdAsync(command.ProjectId, cancellationToken);
            if (project is null)
                return Result<ProjectResponse>.Failure(ProjectErrors.NotFound(command.ProjectId));

            var git = await gitRepositoryReader.ReadAsync(command.ProjectId, path, cancellationToken);
            var repository = project.GitHubRepository;
            if (git.GitHubUrl is not null && git.GitHubOwner is not null && git.GitHubRepositoryName is not null)
            {
                var dto = new GitHubRepositoryDto
                {
                    ProjectId = project.Id,
                    Project = project,
                    Owner = git.GitHubOwner,
                    Name = git.GitHubRepositoryName,
                    WebUrl = git.GitHubUrl,
                    OriginalRemoteUrl = git.PreferredRemoteUrl,
                    DefaultBranch = git.DefaultBranch,
                };
                if (repository is null) repository = GitHubRepository.Create(dto, timeProvider.GetUtcNow());
                else repository.Update(dto, timeProvider.GetUtcNow());
            }

            project.Update(new ProjectDto
            {
                Name = project.Name,
                Description = project.Description,
                Folder = new ProjectFolder(path),
                GitRootPath = git.RepositoryRoot,
                GitHubRepository = repository,
                Streak = project.Streak,
                Lifecycle = project.Lifecycle,
                IsFavorite = project.IsFavorite,
                LastOpenedAt = project.LastOpenedAt,
            }, timeProvider.GetUtcNow());
            await projectStore.UpdateAsync(project, cancellationToken);
            return Result<ProjectResponse>.Success(ProjectResponse.FromDomain(project));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        { return Result<ProjectResponse>.Failure(ProjectErrors.CouldNotSave(exception.Message)); }
    }

    private static bool CanAccess(string path)
    {
        try
        {
            using var entries = Directory.EnumerateFileSystemEntries(path).GetEnumerator();
            _ = entries.MoveNext();
            return true;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException or System.Security.SecurityException)
        { return false; }
    }
}
