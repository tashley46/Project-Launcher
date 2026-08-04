namespace ProjectLauncher.Features;

public sealed class Project
{
    private Project()
    {
        // Required by persistence.
    }

    public Project(string name, ProjectFolder folder, DateTimeOffset createdAt)
    {
        Name = ValidateName(name);
        Folder = folder ?? throw new ArgumentNullException(nameof(folder));
        Lifecycle = ProjectLifecycle.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ProjectFolder Folder { get; private set; } = null!;

    public string? GitRootPath { get; private set; }

    public GitHubRepository? GitHub { get; private set; }

    public ProjectLifecycle Lifecycle { get; private set; }

    public bool IsFavorite { get; private set; }

    public DateTimeOffset? LastOpenedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Rename(string name, DateTimeOffset updatedAt)
    {
        Name = ValidateName(name);
        Touch(updatedAt);
    }

    public void SetDescription(string? description, DateTimeOffset updatedAt)
    {
        Description = NormalizeOptionalText(description);
        Touch(updatedAt);
    }

    public void Relocate(ProjectFolder folder, DateTimeOffset updatedAt)
    {
        Folder = folder ?? throw new ArgumentNullException(nameof(folder));
        GitRootPath = null;
        Touch(updatedAt);
    }

    public void SetGitRootPath(string? gitRootPath, DateTimeOffset updatedAt)
    {
        GitRootPath = NormalizeOptionalPath(gitRootPath);
        Touch(updatedAt);
    }

    public void SetGitHubRepository(GitHubRepository? repository, DateTimeOffset updatedAt)
    {
        GitHub = repository;
        Touch(updatedAt);
    }

    public void SetLifecycle(ProjectLifecycle lifecycle, DateTimeOffset updatedAt)
    {
        Lifecycle = lifecycle;
        Touch(updatedAt);
    }

    public void SetFavorite(bool isFavorite, DateTimeOffset updatedAt)
    {
        IsFavorite = isFavorite;
        Touch(updatedAt);
    }

    public void RecordOpened(DateTimeOffset openedAt)
    {
        LastOpenedAt = openedAt;
        Touch(openedAt);
    }

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeOptionalPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (!System.IO.Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("Git root path must be absolute.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

    private void Touch(DateTimeOffset updatedAt)
    {
        if (updatedAt < CreatedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(updatedAt),
                "The update time cannot be earlier than the project creation time.");
        }

        UpdatedAt = updatedAt;
    }
}

public sealed record ProjectFolder
{
    public ProjectFolder(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!System.IO.Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("Project folder path must be absolute.", nameof(path));
        }

        Path = System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(path));
    }

    public string Path { get; }

    public string Name => System.IO.Path.GetFileName(Path);

    public override string ToString() => Path;
}

public sealed record GitHubRepository
{
    public GitHubRepository(string owner, string name, Uri webUrl, string? originalRemoteUrl = null)
    {
        Owner = ValidateSegment(owner, nameof(owner));
        Name = ValidateSegment(name, nameof(name));
        WebUrl = ValidateWebUrl(webUrl);
        OriginalRemoteUrl = string.IsNullOrWhiteSpace(originalRemoteUrl)
            ? null
            : originalRemoteUrl.Trim();
    }

    public string Owner { get; }

    public string Name { get; }

    public Uri WebUrl { get; }

    public string? OriginalRemoteUrl { get; }

    private static string ValidateSegment(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    private static Uri ValidateWebUrl(Uri webUrl)
    {
        ArgumentNullException.ThrowIfNull(webUrl);

        if (!webUrl.IsAbsoluteUri ||
            webUrl.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(webUrl.Host, "github.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "GitHub web URL must be an absolute HTTPS github.com URL.",
                nameof(webUrl));
        }

        return webUrl;
    }
}

public enum ProjectLifecycle
{
    Active,
    Paused,
    Archived,
}
