using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Core.Projects;

public sealed record ProjectResponse(
    int Id,
    string Name,
    string FolderPath,
    ProjectLifecycle Lifecycle,
    bool IsFavorite,
    int CurrentStreakDays,
    string? GitHubUrl)
{
    public static ProjectResponse FromDomain(Project project) => new(
        project.Id,
        project.Name,
        project.Folder.Path,
        project.Lifecycle,
        project.IsFavorite,
        project.Streak.CurrentDays,
        project.GitHubRepository?.WebUrl);
}

