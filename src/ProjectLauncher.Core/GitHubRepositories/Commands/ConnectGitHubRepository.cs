using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.Projects;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.GitHubRepositories.Commands;

public sealed record ConnectGitHubRepositoryCommand(
    int ProjectId,
    string Owner,
    string RepositoryName,
    string WebUrl,
    string? OriginalRemoteUrl,
    string? DefaultBranch) : ICommand<Result<GitHubRepositoryResponse>>;

public sealed class ConnectGitHubRepositoryCommandHandler(
    IProjectStore projectStore,
    TimeProvider timeProvider)
{
    public async Task<Result<GitHubRepositoryResponse>> HandleAsync(
        ConnectGitHubRepositoryCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ProjectId <= 0)
            return Result<GitHubRepositoryResponse>.Failure(GitHubRepositoryErrors.ProjectIdMustBePositive);
        if (string.IsNullOrWhiteSpace(command.Owner))
            return Result<GitHubRepositoryResponse>.Failure(GitHubRepositoryErrors.OwnerRequired);
        if (string.IsNullOrWhiteSpace(command.RepositoryName))
            return Result<GitHubRepositoryResponse>.Failure(GitHubRepositoryErrors.NameRequired);
        if (!Uri.TryCreate(command.WebUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
            return Result<GitHubRepositoryResponse>.Failure(GitHubRepositoryErrors.UrlInvalid);

        try
        {
            var project = await projectStore.GetByIdAsync(command.ProjectId, cancellationToken);
            if (project is null)
                return Result<GitHubRepositoryResponse>.Failure(
                    GitHubRepositoryErrors.ProjectNotFound(command.ProjectId));

            var owner = command.Owner.Trim();
            var name = command.RepositoryName.Trim();
            var defaultBranch = string.IsNullOrWhiteSpace(command.DefaultBranch)
                ? null
                : command.DefaultBranch.Trim();
            var existing = project.GitHubRepository;
            if (existing is not null &&
                existing.Owner == owner && existing.Name == name &&
                existing.WebUrl == command.WebUrl &&
                existing.OriginalRemoteUrl == command.OriginalRemoteUrl &&
                existing.DefaultBranch == defaultBranch)
                return Result<GitHubRepositoryResponse>.Success(
                    GitHubRepositoryResponse.FromDomain(existing));

            var modifiedAt = timeProvider.GetUtcNow();
            var dto = new GitHubRepositoryDto
            {
                ProjectId = project.Id,
                Project = project,
                Owner = owner,
                Name = name,
                WebUrl = command.WebUrl,
                OriginalRemoteUrl = command.OriginalRemoteUrl,
                DefaultBranch = defaultBranch,
            };
            var repository = existing is null
                ? GitHubRepository.Create(dto, modifiedAt)
                : existing;
            if (existing is not null) repository.Update(dto, modifiedAt);

            project.Update(new ProjectDto
            {
                Name = project.Name,
                Description = project.Description,
                Folder = project.Folder,
                GitRootPath = project.GitRootPath,
                GitHubRepository = repository,
                Streak = project.Streak,
                Lifecycle = project.Lifecycle,
                IsFavorite = project.IsFavorite,
                LastOpenedAt = project.LastOpenedAt,
            }, modifiedAt);
            await projectStore.UpdateAsync(project, cancellationToken);
            return Result<GitHubRepositoryResponse>.Success(
                GitHubRepositoryResponse.FromDomain(repository));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            return Result<GitHubRepositoryResponse>.Failure(
                GitHubRepositoryErrors.CouldNotSave(exception.Message));
        }
    }
}
