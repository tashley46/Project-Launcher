using ProjectLauncher.Core.Shared;

namespace ProjectLauncher.Core.Shared.Errors;

public static class ProjectErrors
{
    public static readonly Error FolderRequired = new(
        "Project.FolderRequired",
        "Choose a project folder before continuing.");

    public static readonly Error FolderMustBeAbsolute = new(
        "Project.FolderMustBeAbsolute",
        "The project folder must use an absolute path.");

    public static readonly Error FolderPathInvalid = new(
        "Project.FolderPathInvalid",
        "The selected project folder path is not valid.");

    public static readonly Error FolderNotFound = new(
        "Project.FolderNotFound",
        "The selected project folder no longer exists.");

    public static readonly Error FolderNotAccessible = new(
        "Project.FolderNotAccessible",
        "The selected project folder cannot be accessed.");

    public static readonly Error NameCouldNotBeDerived = new(
        "Project.NameCouldNotBeDerived",
        "A project name could not be derived from the selected folder.");

    public static readonly Error IdMustBePositive = new(
        "Project.IdMustBePositive",
        "The project identifier must be greater than zero.");

    public static readonly Error NameRequired = new(
        "Project.NameRequired",
        "Enter a project name before saving.");

    public static readonly Error NameTooLong = new(
        "Project.NameTooLong",
        "The project name cannot exceed 120 characters.");

    public static readonly Error DescriptionTooLong = new(
        "Project.DescriptionTooLong",
        "The project description cannot exceed 1,000 characters.");

    public static readonly Error LifecycleInvalid = new(
        "Project.LifecycleInvalid",
        "Choose Active or Paused. Use Archive project to archive it safely.");

    public static Error AlreadyArchived(int projectId) => new(
        "Project.AlreadyArchived",
        $"Project {projectId} is already archived.");

    public static Error NotArchived(int projectId) => new(
        "Project.NotArchived",
        $"Project {projectId} is not archived.");

    public static Error AlreadyExists(string path) => new(
        "Project.AlreadyExists",
        $"A project using '{path}' is already on the dashboard.");

    public static Error NotFound(int projectId) => new(
        "Project.NotFound",
        $"Project {projectId} could not be found.");

    public static Error CouldNotSave(string reason) => new(
        "Project.CouldNotSave",
        $"The project could not be saved. {reason}");

    public static Error CouldNotCheckForDuplicate(string reason) => new(
        "Project.CouldNotCheckForDuplicate",
        $"Existing projects could not be checked. {reason}");

    public static Error CouldNotLoad(string reason) => new(
        "Project.CouldNotLoad",
        $"The project could not be loaded. {reason}");
}
