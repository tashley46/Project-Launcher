using System;

namespace ProjectLaunch.Core.Domain;

public enum ProjectLifecycle
{
    Active,
    Paused,
    Archived,
}

public sealed record ProjectDto
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public ProjectFolder Folder { get; init; } = new(string.Empty);

    public string? GitRootPath { get; init; }

    public GitHubRepository? GitHubRepository { get; init; }

    public ProjectStreak Streak { get; init; } = new();

    public ProjectLifecycle Lifecycle { get; init; } = ProjectLifecycle.Active;

    public bool IsFavorite { get; init; }

    public DateTimeOffset? LastOpenedAt { get; init; }

}

public class Project : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProjectFolder Folder { get; set; } = new(string.Empty);

    public string? GitRootPath { get; set; }

    public GitHubRepository? GitHubRepository { get; set; }

    public ProjectStreak Streak { get; set; } = new();

    public ProjectLifecycle Lifecycle { get; set; } = ProjectLifecycle.Active;

    public bool IsFavorite { get; set; }

    public DateTimeOffset? LastOpenedAt { get; set; }

    public static Project Create(ProjectDto dto, DateTimeOffset createdDateTime)
    {
        var project = new Project();
        project.Apply(dto);
        project.SetCreatedDateTime(createdDateTime);
        return project;
    }

    public void Update(ProjectDto dto, DateTimeOffset modifiedDateTime)
    {
        Apply(dto);
        SetModifiedDateTime(modifiedDateTime);
    }

    private void Apply(ProjectDto dto)
    {
        Name = dto.Name;
        Description = dto.Description;
        Folder = dto.Folder;
        GitRootPath = dto.GitRootPath;
        GitHubRepository = dto.GitHubRepository;
        Streak = dto.Streak;
        Lifecycle = dto.Lifecycle;
        IsFavorite = dto.IsFavorite;
        LastOpenedAt = dto.LastOpenedAt;
        if (GitHubRepository is not null)
        {
            GitHubRepository.Project = this;
        }

        Streak.Project = this;
    }
}

public sealed record ProjectFolder(string Path);
