using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Core.Projects;

public sealed record ProjectResponse(
    int Id,
    string Name,
    string FolderPath,
    string? Description,
    string? GitRootPath,
    ProjectLifecycle Lifecycle,
    bool IsFavorite,
    DateTimeOffset CreatedDateTime,
    DateTimeOffset ModifiedDateTime,
    DateTimeOffset? LastOpenedAt,
    int CurrentStreakDays,
    string? GitHubUrl)
{
    public static ProjectResponse FromDomain(Project project) => new(
        project.Id,
        project.Name,
        project.Folder.Path,
        project.Description,
        project.GitRootPath,
        project.Lifecycle,
        project.IsFavorite,
        project.CreatedDateTime,
        project.ModifiedDateTime,
        project.LastOpenedAt,
        project.Streak.CurrentDays,
        project.GitHubRepository?.WebUrl);
}
