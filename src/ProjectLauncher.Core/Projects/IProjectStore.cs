using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Core.Projects;

public interface IProjectStore
{
    Task<bool> ExistsByFolderPathAsync(
        string normalizedPath,
        int? excludedProjectId,
        CancellationToken cancellationToken);

    Task AddAsync(Project project, CancellationToken cancellationToken);

    Task<Project?> GetByIdAsync(int projectId, CancellationToken cancellationToken);

    Task<Project?> GetByIdIncludingDeletedAsync(int projectId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> GetArchivedAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> GetFavoritesAsync(CancellationToken cancellationToken);

    Task UpdateAsync(Project project, CancellationToken cancellationToken);
}
