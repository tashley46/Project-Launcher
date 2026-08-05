using ProjectLauncher.Core.Shared;

namespace ProjectLauncher.Core.Shared.Errors;

public static class GitHubRepositoryErrors
{
    public static readonly Error ProjectIdMustBePositive = new(
        "GitHubRepository.ProjectIdMustBePositive",
        "The project identifier must be greater than zero.");

    public static Error CouldNotLoad(string reason) => new(
        "GitHubRepository.CouldNotLoad",
        $"GitHub repository details could not be loaded. {reason}");
}
