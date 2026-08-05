using ProjectLauncher.Core.Projects;

namespace ProjectLauncher.ViewModels;

public sealed record ProjectCardViewModel(
    int Id,
    string Name,
    string FolderPath,
    string Lifecycle,
    bool IsFavorite,
    int CurrentStreakDays,
    string RepositoryStatus,
    string? GitHubUrl)
{
    public string StreakLabel => CurrentStreakDays == 1
        ? "1 day streak"
        : $"{CurrentStreakDays} day streak";

    public static ProjectCardViewModel FromResponse(ProjectResponse response) => new(
        response.Id,
        response.Name,
        response.FolderPath,
        response.Lifecycle.ToString(),
        response.IsFavorite,
        response.CurrentStreakDays,
        response.GitHubUrl is null ? "Git scan pending" : "GitHub connected",
        response.GitHubUrl);
}

