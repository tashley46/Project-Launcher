namespace ProjectLaunch.Core.Domain;

public class GitHubRepository
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public string Owner { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string WebUrl { get; set; } = string.Empty;

    public string? OriginalRemoteUrl { get; set; }
}

