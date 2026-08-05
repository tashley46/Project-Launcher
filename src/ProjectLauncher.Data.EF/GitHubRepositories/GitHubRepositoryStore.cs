using Microsoft.EntityFrameworkCore;
using ProjectLaunch.Core.Domain;
using ProjectLauncher.Core.GitHubRepositories;

namespace ProjectLauncher.Data.EF.GitHubRepositories;

public sealed class GitHubRepositoryStore(
    IDbContextFactory<ApplicationDbContext> contextFactory) : IGitHubRepositoryStore
{
    public async Task<GitHubRepository?> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.GitHubRepositories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                repository => repository.ProjectId == projectId && !repository.IsDeleted,
                cancellationToken);
    }
}
