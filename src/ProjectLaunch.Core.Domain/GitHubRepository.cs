namespace ProjectLaunch.Core.Domain;

public sealed record GitHubRepositoryDto
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public Project Project { get; init; } = null!;

    public string Owner { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string WebUrl { get; init; } = string.Empty;

    public string? OriginalRemoteUrl { get; init; }
}

public class GitHubRepository : EntityBase
{

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public string Owner { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string WebUrl { get; set; } = string.Empty;

    public string? OriginalRemoteUrl { get; set; }

    public static GitHubRepository Create(GitHubRepositoryDto dto)
    {
        var repository = new GitHubRepository();
        repository.Update(dto);
        return repository;
    }

    public void Update(GitHubRepositoryDto dto)
    {
        Id = dto.Id;
        ProjectId = dto.ProjectId;
        Project = dto.Project;
        Owner = dto.Owner;
        Name = dto.Name;
        WebUrl = dto.WebUrl;
        OriginalRemoteUrl = dto.OriginalRemoteUrl;
    }
}
