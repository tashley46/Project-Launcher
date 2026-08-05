using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Queries;

public sealed record GetFavoriteProjectsQuery
    : IQuery<Result<IReadOnlyList<ProjectResponse>>>;

public sealed class GetFavoriteProjectsQueryHandler(IProjectStore projectStore)
{
    public async Task<Result<IReadOnlyList<ProjectResponse>>> HandleAsync(
        GetFavoriteProjectsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await projectStore.GetFavoritesAsync(cancellationToken);
            return Result<IReadOnlyList<ProjectResponse>>.Success(
                projects.Select(ProjectResponse.FromDomain).ToArray());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            return Result<IReadOnlyList<ProjectResponse>>.Failure(
                ProjectErrors.CouldNotLoad(exception.Message));
        }
    }
}
