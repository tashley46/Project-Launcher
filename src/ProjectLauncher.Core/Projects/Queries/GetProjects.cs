using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Queries;

public sealed record GetProjectsQuery : IQuery<Result<IReadOnlyList<ProjectResponse>>>;

public sealed class GetProjectsQueryHandler(IProjectStore projectStore)
{
    public async Task<Result<IReadOnlyList<ProjectResponse>>> HandleAsync(
        GetProjectsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await projectStore.GetAllAsync(cancellationToken);
            var response = projects
                .Select(ProjectResponse.FromDomain)
                .ToArray();

            return Result<IReadOnlyList<ProjectResponse>>.Success(response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<IReadOnlyList<ProjectResponse>>.Failure(
                ProjectErrors.CouldNotLoad(exception.Message));
        }
    }
}
