namespace ProjectLauncher.Core.Infrastructure.Git;

public interface IGitRepositoryReader
{
    Task<GitRepositorySnapshot> ReadAsync(
        int projectId,
        string folderPath,
        CancellationToken cancellationToken = default);

    Task<GitCommitHistory> ReadCommitHistoryAsync(
        string folderPath,
        CancellationToken cancellationToken = default);
}

public sealed record GitCommit(string Hash, DateTimeOffset AuthoredAt, string AuthorEmail);

public sealed record GitCommitHistory(
    bool IsGitRepository,
    IReadOnlyList<string> IdentityEmails,
    IReadOnlyList<GitCommit> Commits,
    string? Error);

public sealed record GitRemote(string Name, string Url);

public sealed record GitRepositorySnapshot(
    int ProjectId,
    bool IsGitRepository,
    string? RepositoryRoot,
    string? CurrentBranch,
    bool IsDetached,
    bool IsDirty,
    int StagedFileCount,
    int ModifiedFileCount,
    int UntrackedFileCount,
    IReadOnlyList<GitRemote> Remotes,
    string? PreferredRemoteUrl,
    string? GitHubUrl,
    string? GitHubOwner,
    string? GitHubRepositoryName,
    string? DefaultBranch,
    string? LastCommitHash,
    string? LastCommitSummary,
    DateTimeOffset? LastCommitAt,
    DateTimeOffset RefreshedAt,
    string? Error)
{
    public static GitRepositorySnapshot NotGit(int projectId, DateTimeOffset refreshedAt) =>
        new(projectId, false, null, null, false, false, 0, 0, 0, [], null,
            null, null, null, null, null, null, null, refreshedAt, null);

    public static GitRepositorySnapshot Unavailable(
        int projectId,
        DateTimeOffset refreshedAt,
        string error) =>
        new(projectId, false, null, null, false, false, 0, 0, 0, [], null,
            null, null, null, null, null, null, null, refreshedAt, error);
}
