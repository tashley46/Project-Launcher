using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Core.Projects;

public interface IProjectStore
{
    Task<bool> ExistsByFolderPathAsync(string normalizedPath, CancellationToken cancellationToken);

    Task AddAsync(Project project, CancellationToken cancellationToken);

    Task<Project?> GetByIdAsync(int projectId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken);
}
