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
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public ProjectFolder Folder { get; init; } = new(string.Empty);

    public string? GitRootPath { get; init; }

    public GitHubRepository? GitHubRepository { get; init; }

    public ProjectStreak Streak { get; init; } = new();

    public ProjectLifecycle Lifecycle { get; init; } = ProjectLifecycle.Active;

    public bool IsFavorite { get; init; }

    public DateTimeOffset? LastOpenedAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
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

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public static Project Create(ProjectDto dto)
    {
        var project = new Project();
        project.Update(dto);
        return project;
    }

    public void Update(ProjectDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Description = dto.Description;
        Folder = dto.Folder;
        GitRootPath = dto.GitRootPath;
        GitHubRepository = dto.GitHubRepository;
        Streak = dto.Streak;
        Lifecycle = dto.Lifecycle;
        IsFavorite = dto.IsFavorite;
        LastOpenedAt = dto.LastOpenedAt;
        CreatedAt = dto.CreatedAt;
        UpdatedAt = dto.UpdatedAt;

        if (GitHubRepository is not null)
        {
            GitHubRepository.Project = this;
        }

        Streak.Project = this;
    }
}

public sealed record ProjectFolder(string Path);
