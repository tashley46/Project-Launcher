namespace ProjectLaunch.Core.Domain;

public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProjectFolder Folder { get; set; } = new(string.Empty);

    public string? GitRootPath { get; set; }

    public GitHubRepository? GitHubRepository { get; set; }

    public ProjectStreak Streak { get; set; } = new();

    public ProjectLifecycle Lifecycle { get; set; } = ProjectLifecycle.Active;

    public bool IsFavorite { get; set; }

    public DateTimeOffset? LastOpenedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed record ProjectFolder(string Path);

public enum ProjectLifecycle
{
    Active,
    Paused,
    Archived,
}

