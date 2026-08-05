using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.GitHubRepositories.Queries;

public sealed record GetGitHubRepositoryQuery(int ProjectId)
    : IQuery<Result<GitHubRepositoryResponse>>;

public sealed class GetGitHubRepositoryQueryHandler(IGitHubRepositoryStore repositoryStore)
{
    public async Task<Result<GitHubRepositoryResponse>> HandleAsync(
        GetGitHubRepositoryQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ProjectId <= 0)
        {
            return Result<GitHubRepositoryResponse>.Failure(
                GitHubRepositoryErrors.ProjectIdMustBePositive);
        }

        try
        {
            var repository = await repositoryStore.GetByProjectIdAsync(
                query.ProjectId,
                cancellationToken);

            return Result<GitHubRepositoryResponse>.Success(repository is null
                ? GitHubRepositoryResponse.NotConnected
                : GitHubRepositoryResponse.FromDomain(repository));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<GitHubRepositoryResponse>.Failure(
                GitHubRepositoryErrors.CouldNotLoad(exception.Message));
        }
    }
}
