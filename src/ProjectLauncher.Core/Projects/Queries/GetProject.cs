using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Shared.Errors;

namespace ProjectLauncher.Core.Projects.Queries;

public sealed record GetProjectQuery(int ProjectId) : IQuery<Result<ProjectResponse>>;

public sealed class GetProjectQueryHandler(IProjectStore projectStore)
{
    public async Task<Result<ProjectResponse>> HandleAsync(
        GetProjectQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ProjectId <= 0)
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.IdMustBePositive);
        }

        try
        {
            var project = await projectStore.GetByIdAsync(query.ProjectId, cancellationToken);
            return project is null
                ? Result<ProjectResponse>.Failure(ProjectErrors.NotFound(query.ProjectId))
                : Result<ProjectResponse>.Success(ProjectResponse.FromDomain(project));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.CouldNotLoad(exception.Message));
        }
    }
}

