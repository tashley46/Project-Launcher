using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Commands;

public sealed record AddProjectCommand(string FolderPath) : ICommand<Result<ProjectResponse>>;

public sealed class AddProjectCommandHandler(IProjectStore projectStore, TimeProvider timeProvider)
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
        var streak = ProjectStreak.Create(new ProjectStreakDto(), createdDateTime);
        var project = Project.Create(
            new ProjectDto
            {
                Name = projectName,
                Folder = new ProjectFolder(normalizedPath),
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
