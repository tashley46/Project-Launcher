using CommunityToolkit.Mvvm.Input;
using ProjectLauncher.Core.GitHubRepositories;
using ProjectLauncher.Core.Infrastructure.Git;
using ProjectLauncher.Core.Projects;
using ProjectLauncher.Core.Streaks;

namespace ProjectLauncher.ViewModels;

public sealed class ProjectCardViewModel : ViewModelBase
{
    private readonly Func<ProjectCardViewModel, Task> _loadDetails;
    private readonly Func<ProjectCardViewModel, Task> _saveEdit;
    private readonly Func<ProjectCardViewModel, Task> _changeArchiveState;
    private readonly Func<ProjectCardViewModel, Task> _refreshStreak;
    private bool _isDetailsVisible;
    private bool _isEditing;
    private bool _hasLoadedDetails;
    private GitRepositorySnapshot? _gitSnapshot;
    private string _name;
    private string _lifecycle;
    private int _currentStreakDays;

    private ProjectCardViewModel(
        ProjectResponse response,
        Func<ProjectCardViewModel, Task> loadDetails,
        Func<ProjectCardViewModel, Task> saveEdit,
        Func<ProjectCardViewModel, Task> changeArchiveState,
        Func<ProjectCardViewModel, Task> refreshStreak)
    {
        Id = response.Id;
        IsArchived = response.IsDeleted;
        _name = response.Name;
        FolderPath = response.FolderPath;
        _lifecycle = response.Lifecycle.ToString();
        IsFavorite = response.IsFavorite;
        _currentStreakDays = response.CurrentStreakDays;
        Description = response.Description ?? "No description has been added.";
        EditName = response.Name;
        EditDescription = response.Description ?? string.Empty;
        EditLifecycle = response.Lifecycle.ToString();
        RepositoryStatus = response.GitHubUrl is null ? "GitHub not connected" : "GitHub connected";
        GitHubUrl = response.GitHubUrl;
        _loadDetails = loadDetails;
        _saveEdit = saveEdit;
        _changeArchiveState = changeArchiveState;
        _refreshStreak = refreshStreak;

        ViewProjectCommand = new AsyncRelayCommand(ToggleDetailsAsync);
        BeginEditCommand = new RelayCommand(BeginEdit);
        CancelEditCommand = new RelayCommand(CancelEdit);
        SaveEditCommand = new AsyncRelayCommand(() => _saveEdit(this));
        ChangeArchiveStateCommand = new AsyncRelayCommand(() => _changeArchiveState(this));
        RefreshStreakCommand = new AsyncRelayCommand(() => _refreshStreak(this));
    }

    public int Id { get; }
    public bool IsArchived { get; }
    public bool IsActiveProject => !IsArchived;
    public string FolderPath { get; }
    public bool IsFavorite { get; }
    public int CurrentStreakDays { get => _currentStreakDays; private set => SetProperty(ref _currentStreakDays, value); }
    public string Name { get => _name; private set => SetProperty(ref _name, value); }
    public string Lifecycle { get => _lifecycle; private set => SetProperty(ref _lifecycle, value); }
    public string RepositoryStatus { get; private set; }
    public string? GitHubUrl { get; private set; }
    public string Description { get; private set; }
    public string EditName { get; set; }
    public string EditDescription { get; set; }
    public string EditLifecycle { get; set; }
    public IReadOnlyList<string> LifecycleOptions { get; } = ["Active", "Paused"];
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
    public string BranchLabel { get; private set; } = "Checking Git…";
    public string WorkingTreeStatus { get; private set; } = "Status pending";
    public string FileChangeSummary { get; private set; } = string.Empty;
    public string LatestCommitSummary { get; private set; } = "No commits yet";
    public string LatestCommitDateLabel { get; private set; } = string.Empty;
    public string RemoteLabel { get; private set; } = "No remote";
    public string DefaultBranchLabel { get; private set; } = "Unknown";
    public string ArchiveButtonLabel => IsArchived ? "Restore project" : "Archive project";
    public string StreakLabel => CurrentStreakDays == 1 ? "1 day streak" : $"{CurrentStreakDays} day streak";
    public string DetailsButtonLabel => IsDetailsVisible ? "Hide details" : "View project";

    public bool IsDetailsVisible
    {
        get => _isDetailsVisible;
        private set { if (SetProperty(ref _isDetailsVisible, value)) OnPropertyChanged(nameof(DetailsButtonLabel)); }
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set => SetProperty(ref _isEditing, value);
    }

    public IAsyncRelayCommand ViewProjectCommand { get; }
    public IRelayCommand BeginEditCommand { get; }
    public IRelayCommand CancelEditCommand { get; }
    public IAsyncRelayCommand SaveEditCommand { get; }
    public IAsyncRelayCommand ChangeArchiveStateCommand { get; }
    public IAsyncRelayCommand RefreshStreakCommand { get; }

    public static ProjectCardViewModel FromResponse(
        ProjectResponse response,
        Func<ProjectCardViewModel, Task> loadDetails,
        Func<ProjectCardViewModel, Task> saveEdit,
        Func<ProjectCardViewModel, Task> changeArchiveState,
        Func<ProjectCardViewModel, Task> refreshStreak) =>
        new(response, loadDetails, saveEdit, changeArchiveState, refreshStreak);

    public void ApplyEdit(ProjectResponse project)
    {
        Name = project.Name;
        Lifecycle = project.Lifecycle.ToString();
        Description = project.Description ?? "No description has been added.";
        EditName = project.Name;
        EditDescription = project.Description ?? string.Empty;
        EditLifecycle = project.Lifecycle.ToString();
        IsEditing = false;
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(EditName));
        OnPropertyChanged(nameof(EditDescription));
        OnPropertyChanged(nameof(EditLifecycle));
    }

    public void SetDetails(ProjectResponse project, GitHubRepositoryResponse repository, ProjectStreakResponse streak)
    {
        ApplyEdit(project);
        GitRootPath = project.GitRootPath ?? "Not detected";
        CreatedLabel = project.CreatedDateTime.ToLocalTime().ToString("MMM d, yyyy");
        ModifiedLabel = project.ModifiedDateTime.ToLocalTime().ToString("MMM d, yyyy");
        LastOpenedLabel = project.LastOpenedAt?.ToLocalTime().ToString("MMM d, yyyy h:mm tt") ?? "Not opened yet";
        RepositoryStatus = repository.IsConnected ? "GitHub connected" : "GitHub not connected";
        GitHubRepositoryLabel = repository.IsConnected ? $"{repository.Owner}/{repository.Name}" : "Not connected";
        GitHubUrl = repository.WebUrl;
        OriginalRemoteUrl = repository.OriginalRemoteUrl ?? "No GitHub remote detected";
        DefaultBranchLabel = repository.DefaultBranch ?? "Unknown";
        LongestStreakLabel = $"{streak.LongestDays} {(streak.LongestDays == 1 ? "day" : "days")}";
        LastCommitLabel = streak.LastCommitByUserAt?.ToLocalTime().ToString("MMM d, yyyy h:mm tt") ?? "No matching commits yet";
        ActiveDaysLabel = $"{streak.ActiveCommitDaysLast30} active days in the last 30";
        CalculatedLabel = streak.CalculatedAt?.ToLocalTime().ToString("MMM d, yyyy h:mm tt") ?? "Not calculated yet";

        foreach (var property in new[] { nameof(GitRootPath), nameof(CreatedLabel), nameof(ModifiedLabel), nameof(LastOpenedLabel), nameof(RepositoryStatus), nameof(GitHubRepositoryLabel), nameof(GitHubUrl), nameof(OriginalRemoteUrl), nameof(DefaultBranchLabel), nameof(LongestStreakLabel), nameof(LastCommitLabel), nameof(ActiveDaysLabel), nameof(CalculatedLabel) })
            OnPropertyChanged(property);

        _hasLoadedDetails = true;
        IsDetailsVisible = true;
        if (_gitSnapshot is not null) SetGitSnapshot(_gitSnapshot);
    }

    public void SetGitSnapshot(GitRepositorySnapshot snapshot)
    {
        _gitSnapshot = snapshot;
        if (snapshot.Error is not null)
        {
            BranchLabel = "Git unavailable";
            WorkingTreeStatus = "Unavailable";
            LatestCommitSummary = snapshot.Error;
        }
        else if (!snapshot.IsGitRepository)
        {
            BranchLabel = "Not a Git repository";
            WorkingTreeStatus = "Not Git";
            LatestCommitSummary = "Git details are not available for this folder.";
        }
        else
        {
            BranchLabel = snapshot.IsDetached ? "Detached HEAD" : snapshot.CurrentBranch ?? "Unknown branch";
            WorkingTreeStatus = snapshot.IsDirty ? "Dirty" : "Clean";
            FileChangeSummary = snapshot.IsDirty
                ? $"{snapshot.StagedFileCount} staged · {snapshot.ModifiedFileCount} modified · {snapshot.UntrackedFileCount} untracked"
                : "No local changes";
            LatestCommitSummary = snapshot.LastCommitSummary ?? "No commits yet";
            LatestCommitDateLabel = snapshot.LastCommitAt?.ToLocalTime().ToString("MMM d, yyyy h:mm tt") ?? string.Empty;
            RemoteLabel = snapshot.Remotes.Count == 0
                ? "No remote"
                : string.Join(" · ", snapshot.Remotes.Select(remote => remote.Name));
            if (snapshot.GitHubUrl is not null)
            {
                RepositoryStatus = "GitHub connected";
                GitHubUrl = snapshot.GitHubUrl;
                GitHubRepositoryLabel = $"{snapshot.GitHubOwner}/{snapshot.GitHubRepositoryName}";
                OriginalRemoteUrl = snapshot.PreferredRemoteUrl ?? "No GitHub remote detected";
                DefaultBranchLabel = snapshot.DefaultBranch ?? "Unknown";
            }
            GitRootPath = snapshot.RepositoryRoot ?? GitRootPath;
        }

        foreach (var property in new[] { nameof(BranchLabel), nameof(WorkingTreeStatus), nameof(FileChangeSummary), nameof(LatestCommitSummary), nameof(LatestCommitDateLabel), nameof(RemoteLabel), nameof(RepositoryStatus), nameof(GitHubUrl), nameof(GitHubRepositoryLabel), nameof(OriginalRemoteUrl), nameof(DefaultBranchLabel), nameof(GitRootPath) })
            OnPropertyChanged(property);
    }

    public void SetGitHubConnection(GitHubRepositoryResponse repository)
    {
        RepositoryStatus = repository.IsConnected ? "GitHub connected" : "GitHub not connected";
        GitHubRepositoryLabel = repository.IsConnected ? $"{repository.Owner}/{repository.Name}" : "Not connected";
        GitHubUrl = repository.WebUrl;
        OriginalRemoteUrl = repository.OriginalRemoteUrl ?? "No GitHub remote detected";
        DefaultBranchLabel = repository.DefaultBranch ?? "Unknown";
        OnPropertyChanged(nameof(RepositoryStatus));
        OnPropertyChanged(nameof(GitHubRepositoryLabel));
        OnPropertyChanged(nameof(GitHubUrl));
        OnPropertyChanged(nameof(OriginalRemoteUrl));
        OnPropertyChanged(nameof(DefaultBranchLabel));
    }

    public void SetStreak(ProjectStreakResponse streak)
    {
        CurrentStreakDays = streak.CurrentDays;
        LongestStreakLabel = $"{streak.LongestDays} {(streak.LongestDays == 1 ? "day" : "days")}";
        LastCommitLabel = streak.LastCommitByUserAt?.ToLocalTime().ToString("MMM d, yyyy h:mm tt")
            ?? "No matching commits yet";
        ActiveDaysLabel = $"{streak.ActiveCommitDaysLast30} active days in the last 30";
        CalculatedLabel = streak.CalculatedAt?.ToLocalTime().ToString("MMM d, yyyy h:mm tt")
            ?? "Not calculated yet";
        OnPropertyChanged(nameof(StreakLabel));
        OnPropertyChanged(nameof(LongestStreakLabel));
        OnPropertyChanged(nameof(LastCommitLabel));
        OnPropertyChanged(nameof(ActiveDaysLabel));
        OnPropertyChanged(nameof(CalculatedLabel));
    }

    public void SetGitError(string message)
    {
        BranchLabel = "Git unavailable";
        WorkingTreeStatus = "Unavailable";
        LatestCommitSummary = message;
        OnPropertyChanged(nameof(BranchLabel));
        OnPropertyChanged(nameof(WorkingTreeStatus));
        OnPropertyChanged(nameof(LatestCommitSummary));
    }

    private void BeginEdit()
    {
        EditName = Name;
        EditDescription = Description == "No description has been added." ? string.Empty : Description;
        EditLifecycle = Lifecycle;
        IsEditing = true;
        IsDetailsVisible = true;
    }

    private void CancelEdit() => IsEditing = false;

    private async Task ToggleDetailsAsync()
    {
        if (IsDetailsVisible) { IsDetailsVisible = false; return; }
        if (_hasLoadedDetails) { IsDetailsVisible = true; return; }
        await _loadDetails(this);
    }
}
