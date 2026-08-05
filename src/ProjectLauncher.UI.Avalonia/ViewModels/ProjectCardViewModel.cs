using CommunityToolkit.Mvvm.Input;
using ProjectLauncher.Core.GitHubRepositories;
using ProjectLauncher.Core.Projects;
using ProjectLauncher.Core.Streaks;

namespace ProjectLauncher.ViewModels;

public sealed class ProjectCardViewModel : ViewModelBase
{
    private readonly Func<ProjectCardViewModel, Task> _loadDetails;
    private bool _isDetailsVisible;
    private bool _hasLoadedDetails;

    private ProjectCardViewModel(
        ProjectResponse response,
        Func<ProjectCardViewModel, Task> loadDetails)
    {
        Id = response.Id;
        Name = response.Name;
        FolderPath = response.FolderPath;
        Lifecycle = response.Lifecycle.ToString();
        IsFavorite = response.IsFavorite;
        CurrentStreakDays = response.CurrentStreakDays;
        RepositoryStatus = response.GitHubUrl is null
            ? "GitHub not connected"
            : "GitHub connected";
        GitHubUrl = response.GitHubUrl;
        _loadDetails = loadDetails;
        ViewProjectCommand = new AsyncRelayCommand(ToggleDetailsAsync);
    }

    public int Id { get; }

    public string Name { get; }

    public string FolderPath { get; }

    public string Lifecycle { get; }

    public bool IsFavorite { get; }

    public int CurrentStreakDays { get; }

    public string RepositoryStatus { get; private set; }

    public string? GitHubUrl { get; private set; }

    public string Description { get; private set; } = "No description has been added.";

    public string GitRootPath { get; private set; } = "Not detected";

    public string CreatedLabel { get; private set; } = string.Empty;

    public string ModifiedLabel { get; private set; } = string.Empty;

    public string LastOpenedLabel { get; private set; } = "Not opened yet";

    public string GitHubRepositoryLabel { get; private set; } = "Not connected";

    public string OriginalRemoteUrl { get; private set; } = "No GitHub remote detected";

    public string LongestStreakLabel { get; private set; } = "0 days";

    public string LastCommitLabel { get; private set; } = "No matching commits yet";

    public string ActiveDaysLabel { get; private set; } = "0 active days in the last 30";

    public string CalculatedLabel { get; private set; } = "Not calculated yet";

    public bool IsDetailsVisible
    {
        get => _isDetailsVisible;
        private set
        {
            if (SetProperty(ref _isDetailsVisible, value))
            {
                OnPropertyChanged(nameof(DetailsButtonLabel));
            }
        }
    }

    public string DetailsButtonLabel => IsDetailsVisible ? "Hide details" : "View project";

    public IAsyncRelayCommand ViewProjectCommand { get; }

    public string StreakLabel => CurrentStreakDays == 1
        ? "1 day streak"
        : $"{CurrentStreakDays} day streak";

    public static ProjectCardViewModel FromResponse(
        ProjectResponse response,
        Func<ProjectCardViewModel, Task> loadDetails) => new(response, loadDetails);

    public void SetDetails(
        ProjectResponse project,
        GitHubRepositoryResponse repository,
        ProjectStreakResponse streak)
    {
        Description = string.IsNullOrWhiteSpace(project.Description)
            ? "No description has been added."
            : project.Description;
        GitRootPath = project.GitRootPath ?? "Not detected";
        CreatedLabel = project.CreatedDateTime.ToLocalTime().ToString("MMM d, yyyy");
        ModifiedLabel = project.ModifiedDateTime.ToLocalTime().ToString("MMM d, yyyy");
        LastOpenedLabel = project.LastOpenedAt?.ToLocalTime().ToString("MMM d, yyyy h:mm tt")
            ?? "Not opened yet";

        RepositoryStatus = repository.IsConnected ? "GitHub connected" : "GitHub not connected";
        GitHubRepositoryLabel = repository.IsConnected
            ? $"{repository.Owner}/{repository.Name}"
            : "Not connected";
        GitHubUrl = repository.WebUrl;
        OriginalRemoteUrl = repository.OriginalRemoteUrl ?? "No GitHub remote detected";

        LongestStreakLabel = $"{streak.LongestDays} {(streak.LongestDays == 1 ? "day" : "days")}";
        LastCommitLabel = streak.LastCommitByUserAt?.ToLocalTime().ToString("MMM d, yyyy h:mm tt")
            ?? "No matching commits yet";
        ActiveDaysLabel = $"{streak.ActiveCommitDaysLast30} active days in the last 30";
        CalculatedLabel = streak.CalculatedAt?.ToLocalTime().ToString("MMM d, yyyy h:mm tt")
            ?? "Not calculated yet";

        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(GitRootPath));
        OnPropertyChanged(nameof(CreatedLabel));
        OnPropertyChanged(nameof(ModifiedLabel));
        OnPropertyChanged(nameof(LastOpenedLabel));
        OnPropertyChanged(nameof(RepositoryStatus));
        OnPropertyChanged(nameof(GitHubRepositoryLabel));
        OnPropertyChanged(nameof(GitHubUrl));
        OnPropertyChanged(nameof(OriginalRemoteUrl));
        OnPropertyChanged(nameof(LongestStreakLabel));
        OnPropertyChanged(nameof(LastCommitLabel));
        OnPropertyChanged(nameof(ActiveDaysLabel));
        OnPropertyChanged(nameof(CalculatedLabel));

        _hasLoadedDetails = true;
        IsDetailsVisible = true;
    }

    private async Task ToggleDetailsAsync()
    {
        if (IsDetailsVisible)
        {
            IsDetailsVisible = false;
            return;
        }

        if (_hasLoadedDetails)
        {
            IsDetailsVisible = true;
            return;
        }

        await _loadDetails(this);
    }
}
