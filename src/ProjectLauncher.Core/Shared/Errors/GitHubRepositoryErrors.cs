using ProjectLauncher.Core.Shared;

namespace ProjectLauncher.Core.Shared.Errors;

public static class GitHubRepositoryErrors
{
    public static readonly Error ProjectIdMustBePositive = new(
        "GitHubRepository.ProjectIdMustBePositive",
        "The project identifier must be greater than zero.");

    public static readonly Error OwnerRequired = new(
        "GitHubRepository.OwnerRequired",
        "The GitHub repository owner is required.");

    public static readonly Error NameRequired = new(
        "GitHubRepository.NameRequired",
        "The GitHub repository name is required.");

    public static readonly Error UrlInvalid = new(
        "GitHubRepository.UrlInvalid",
        "The repository URL must be a valid HTTPS GitHub URL.");

    public static Error ProjectNotFound(int projectId) => new(
        "GitHubRepository.ProjectNotFound",
        $"Project {projectId} could not be found.");

    public static Error CouldNotSave(string reason) => new(
        "GitHubRepository.CouldNotSave",
        $"The GitHub connection could not be saved. {reason}");

    public static Error CouldNotLoad(string reason) => new(
        "GitHubRepository.CouldNotLoad",
        $"GitHub repository details could not be loaded. {reason}");
}
