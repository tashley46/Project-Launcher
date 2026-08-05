using ProjectLauncher.Core.Infrastructure.Git;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Queries;

public sealed record GetProjectGitStatusQuery(int ProjectId, string FolderPath)
    : IQuery<Result<GitRepositorySnapshot>>;

public sealed class GetProjectGitStatusQueryHandler(IGitRepositoryReader repositoryReader)
{
    public async Task<Result<GitRepositorySnapshot>> HandleAsync(
        GetProjectGitStatusQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ProjectId <= 0)
            return Result<GitRepositorySnapshot>.Failure(ProjectErrors.IdMustBePositive);
        if (string.IsNullOrWhiteSpace(query.FolderPath))
            return Result<GitRepositorySnapshot>.Failure(ProjectErrors.FolderRequired);
        if (!Path.IsPathFullyQualified(query.FolderPath))
            return Result<GitRepositorySnapshot>.Failure(ProjectErrors.FolderMustBeAbsolute);
        if (!Directory.Exists(query.FolderPath))
            return Result<GitRepositorySnapshot>.Failure(ProjectErrors.FolderNotFound);

        var snapshot = await repositoryReader.ReadAsync(
            query.ProjectId,
            query.FolderPath,
            cancellationToken);
        return Result<GitRepositorySnapshot>.Success(snapshot);
    }
}
