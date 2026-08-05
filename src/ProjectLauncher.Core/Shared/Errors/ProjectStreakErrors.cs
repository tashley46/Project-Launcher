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
}
