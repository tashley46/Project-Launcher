using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Core.GitHubRepositories;

public interface IGitHubRepositoryStore
{
    Task<GitHubRepository?> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken);
}
