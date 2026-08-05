using ProjectLauncher.Core.Shared;

namespace ProjectLauncher.Core.Shared.Errors;

public static class ProjectStreakErrors
{
    public static readonly Error ProjectIdMustBePositive = new(
        "ProjectStreak.ProjectIdMustBePositive",
        "The project identifier must be greater than zero.");

    public static Error NotFound(int projectId) => new(
        "ProjectStreak.NotFound",
        $"Streak details for project {projectId} could not be found.");

    public static Error CouldNotLoad(string reason) => new(
        "ProjectStreak.CouldNotLoad",
        $"Project streak details could not be loaded. {reason}");

    public static readonly Error NotGitRepository = new(
        "ProjectStreak.NotGitRepository",
        "Streaks cannot be calculated because this folder is not a Git repository.");

    public static readonly Error IdentityNotConfigured = new(
        "ProjectStreak.IdentityNotConfigured",
        "Configure a Git user email before calculating commit streaks.");

    public static Error GitHistoryUnavailable(string reason) => new(
        "ProjectStreak.GitHistoryUnavailable",
        $"Git commit history could not be read. {reason}");

    public static Error CouldNotSave(string reason) => new(
        "ProjectStreak.CouldNotSave",
        $"The calculated streak could not be saved. {reason}");
}
