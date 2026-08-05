using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.Infrastructure.Git;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Commands;

public sealed record AddProjectCommand(string FolderPath) : ICommand<Result<ProjectResponse>>;

public sealed class AddProjectCommandHandler(
    IProjectStore projectStore,
    IGitRepositoryReader gitRepositoryReader,
    TimeProvider timeProvider)
{
    public async Task<Result<ProjectResponse>> HandleAsync(
        AddProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.FolderPath))
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.FolderRequired);
        }

        if (!Path.IsPathFullyQualified(command.FolderPath))
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.FolderMustBeAbsolute);
        }

        string normalizedPath;
        try
        {
            normalizedPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(command.FolderPath));
        }
        catch (Exception exception) when (
            exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.FolderPathInvalid);
        }

        if (!Directory.Exists(normalizedPath))
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.FolderNotFound);
        }

        if (!CanAccess(normalizedPath))
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.FolderNotAccessible);
        }

        var projectName = new DirectoryInfo(normalizedPath).Name;
        if (string.IsNullOrWhiteSpace(projectName))
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.NameCouldNotBeDerived);
        }

        bool alreadyExists;
        try
        {
            alreadyExists = await projectStore.ExistsByFolderPathAsync(
                normalizedPath,
                null,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<ProjectResponse>.Failure(
                ProjectErrors.CouldNotCheckForDuplicate(exception.Message));
        }

        if (alreadyExists)
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.AlreadyExists(normalizedPath));
        }

        var createdDateTime = timeProvider.GetUtcNow();
        var git = await gitRepositoryReader.ReadAsync(0, normalizedPath, cancellationToken);
        var streak = ProjectStreak.Create(new ProjectStreakDto(), createdDateTime);
        var gitHubRepository = git.GitHubUrl is not null &&
            git.GitHubOwner is not null &&
            git.GitHubRepositoryName is not null
            ? GitHubRepository.Create(new GitHubRepositoryDto
            {
                Owner = git.GitHubOwner,
                Name = git.GitHubRepositoryName,
                WebUrl = git.GitHubUrl,
                OriginalRemoteUrl = git.PreferredRemoteUrl,
                DefaultBranch = git.DefaultBranch,
            }, createdDateTime)
            : null;
        var project = Project.Create(
            new ProjectDto
            {
                Name = projectName,
                Folder = new ProjectFolder(normalizedPath),
                GitRootPath = git.RepositoryRoot,
                GitHubRepository = gitHubRepository,
                Lifecycle = ProjectLifecycle.Active,
                Streak = streak,
            },
            createdDateTime);

        try
        {
            await projectStore.AddAsync(project, cancellationToken);
            return Result<ProjectResponse>.Success(ProjectResponse.FromDomain(project));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.CouldNotSave(exception.Message));
        }
    }

    private static bool CanAccess(string path)
    {
        try
        {
            using var entries = Directory.EnumerateFileSystemEntries(path).GetEnumerator();
            _ = entries.MoveNext();
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (System.Security.SecurityException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
