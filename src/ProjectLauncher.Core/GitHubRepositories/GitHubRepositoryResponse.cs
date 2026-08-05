using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Core.GitHubRepositories;

public sealed record GitHubRepositoryResponse(
    bool IsConnected,
    string? Owner,
    string? Name,
    string? WebUrl,
    string? OriginalRemoteUrl)
{
    public static GitHubRepositoryResponse NotConnected { get; } =
        new(false, null, null, null, null);

    public static GitHubRepositoryResponse FromDomain(GitHubRepository repository) => new(
        true,
        repository.Owner,
        repository.Name,
        repository.WebUrl,
        repository.OriginalRemoteUrl);
}
