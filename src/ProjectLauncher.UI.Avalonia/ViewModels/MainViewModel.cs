using System.Collections.ObjectModel;
using ProjectLauncher.Core.GitHubRepositories.Queries;
using ProjectLauncher.Core.GitHubRepositories.Commands;
using ProjectLauncher.Core.Projects;
using ProjectLauncher.Core.Projects.Commands;
using ProjectLauncher.Core.Projects.Queries;
using ProjectLauncher.Core.Shared;
using ProjectLauncher.Core.Streaks.Queries;
using ProjectLauncher.Core.Streaks.Commands;

namespace ProjectLauncher.ViewModels;

public sealed class MainViewModel(
    AddProjectCommandHandler addProjectHandler,
    UpdateProjectCommandHandler updateProjectHandler,
    ArchiveProjectCommandHandler archiveProjectHandler,
    RestoreProjectCommandHandler restoreProjectHandler,
    GetProjectQueryHandler getProjectHandler,
    GetProjectIncludingDeletedQueryHandler getProjectIncludingDeletedHandler,
    GetProjectsQueryHandler getProjectsHandler,
    GetArchivedProjectsQueryHandler getArchivedProjectsHandler,
    GetFavoriteProjectsQueryHandler getFavoriteProjectsHandler,
    GetProjectGitStatusQueryHandler getProjectGitStatusHandler,
    ConnectGitHubRepositoryCommandHandler connectGitHubRepositoryHandler,
    GetGitHubRepositoryQueryHandler getGitHubRepositoryHandler,
    GetProjectStreakQueryHandler getProjectStreakHandler,
    RefreshProjectStreakCommandHandler refreshProjectStreakHandler,
    SetProjectFavoriteCommandHandler setProjectFavoriteHandler) : ViewModelBase
{
    private readonly List<ProjectCardViewModel> _allProjects = [];
    private bool _isBusy;
    private bool _hasLoaded;
    private bool _isArchiveView;
    private bool _isFavoritesView;
    private string? _errorMessage;
    private string _searchText = string.Empty;
    private string _selectedStatusFilter = "All";

    public ObservableCollection<ProjectCardViewModel> Projects { get; } = [];
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public bool IsArchiveView { get => _isArchiveView; private set { if (SetProperty(ref _isArchiveView, value)) NotifyViewChanged(); } }
    public bool IsProjectView => !IsArchiveView;
    public bool IsFavoritesView { get => _isFavoritesView; private set { if (SetProperty(ref _isFavoritesView, value)) NotifyViewChanged(); } }
    public string WorkspaceTitle => IsArchiveView ? "Archived projects" : IsFavoritesView ? "Favorite projects" : "Your workspace";
    public string WorkspaceSubtitle => IsArchiveView
        ? "Restore a project whenever you are ready to return to it."
        : IsFavoritesView ? "Your pinned projects, ordered by recent activity."
        : "Your saved projects are restored each time you launch.";
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasProjects => Projects.Count > 0;
    public bool HasNoProjects => !HasProjects;
    public int ProjectCount => _allProjects.Count;
    public int ActiveProjectCount => _allProjects.Count(project => project.Lifecycle == "Active");
    public int CurrentStreakDays => _allProjects.Count == 0 ? 0 : _allProjects.Max(project => project.CurrentStreakDays);
    public IReadOnlyList<string> StatusFilters { get; } = ["All", "Active", "Paused", "Clean", "Dirty", "Not Git", "GitHub connected"];

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilters(); }
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set { if (SetProperty(ref _selectedStatusFilter, value)) ApplyFilters(); }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set { if (SetProperty(ref _errorMessage, value)) OnPropertyChanged(nameof(HasError)); }
    }

    public async Task LoadProjectsAsync(CancellationToken cancellationToken = default)
    {
        if (_hasLoaded) return;
        await ShowProjectsAsync(cancellationToken);
        _hasLoaded = true;
    }

    public async Task ShowProjectsAsync(CancellationToken cancellationToken = default)
    {
        IsArchiveView = false;
        IsFavoritesView = false;
        var result = await RunBusyAsync(() => getProjectsHandler.HandleAsync(new GetProjectsQuery(), cancellationToken));
        if (!result.IsSuccess || result.Value is null) { ShowError(result.Error?.Message); return; }
        ReplaceProjects(result.Value);
        await RefreshGitStatusesAsync(cancellationToken);
    }

    public async Task ShowArchivedProjectsAsync(CancellationToken cancellationToken = default)
    {
        IsArchiveView = true;
        IsFavoritesView = false;
        var result = await RunBusyAsync(() => getArchivedProjectsHandler.HandleAsync(new GetArchivedProjectsQuery(), cancellationToken));
        if (!result.IsSuccess || result.Value is null) { ShowError(result.Error?.Message); return; }
        ReplaceProjects(result.Value);
    }

    public async Task ShowFavoriteProjectsAsync(CancellationToken cancellationToken = default)
    {
        IsArchiveView = false;
        IsFavoritesView = true;
        var result = await RunBusyAsync(() => getFavoriteProjectsHandler.HandleAsync(
            new GetFavoriteProjectsQuery(), cancellationToken));
        if (!result.IsSuccess || result.Value is null) { ShowError(result.Error?.Message); return; }
        ReplaceProjects(result.Value);
        await RefreshGitStatusesAsync(cancellationToken);
    }

    public async Task AddProjectAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var result = await RunBusyAsync(() => addProjectHandler.HandleAsync(new AddProjectCommand(folderPath), cancellationToken));
        if (!result.IsSuccess || result.Value is null) { ShowError(result.Error?.Message); return; }
        if (IsArchiveView || IsFavoritesView) await ShowProjectsAsync(cancellationToken);
        else
        {
            var card = CreateProjectCard(result.Value);
            _allProjects.Add(card);
            ApplyFilters();
            NotifyDashboardChanged();
            await RefreshGitStatusAsync(card, cancellationToken);
        }
    }

    public void DismissError() => ErrorMessage = null;

    private ProjectCardViewModel CreateProjectCard(ProjectResponse response) =>
        ProjectCardViewModel.FromResponse(
            response,
            LoadProjectDetailsAsync,
            SaveProjectEditAsync,
            ChangeArchiveStateAsync,
            RefreshProjectStreakAsync,
            ToggleProjectFavoriteAsync);

    private async Task ToggleProjectFavoriteAsync(ProjectCardViewModel card)
    {
        var result = await RunBusyAsync(() => setProjectFavoriteHandler.HandleAsync(
            new SetProjectFavoriteCommand(card.Id, !card.IsFavorite)));
        if (!result.IsSuccess || result.Value is null) { ShowError(result.Error?.Message); return; }
        card.ApplyFavorite(result.Value);
        if (IsFavoritesView && !card.IsFavorite) _allProjects.Remove(card);
        _allProjects.Sort((left, right) => right.IsFavorite.CompareTo(left.IsFavorite));
        ApplyFilters();
    }

    private async Task RefreshProjectStreakAsync(ProjectCardViewModel card)
    {
        var result = await RunBusyAsync(() => refreshProjectStreakHandler.HandleAsync(
            new RefreshProjectStreakCommand(card.Id)));
        if (!result.IsSuccess || result.Value is null)
        {
            ShowError(result.Error?.Message);
            return;
        }
        card.SetStreak(result.Value);
        NotifyDashboardChanged();
    }

    private async Task SaveProjectEditAsync(ProjectCardViewModel card)
    {
        var result = await RunBusyAsync(() => updateProjectHandler.HandleAsync(
            new UpdateProjectCommand(card.Id, card.EditName, card.EditDescription, card.EditLifecycle)));
        if (!result.IsSuccess || result.Value is null) { ShowError(result.Error?.Message); return; }
        card.ApplyEdit(result.Value);
        NotifyDashboardChanged();
    }

    private async Task ChangeArchiveStateAsync(ProjectCardViewModel card)
    {
        var result = card.IsArchived
            ? await RunBusyAsync(() => restoreProjectHandler.HandleAsync(new RestoreProjectCommand(card.Id)))
            : await RunBusyAsync(() => archiveProjectHandler.HandleAsync(new ArchiveProjectCommand(card.Id)));
        if (!result.IsSuccess) { ShowError(result.Error?.Message); return; }
        _allProjects.Remove(card);
        ApplyFilters();
        NotifyDashboardChanged();
    }

    private async Task LoadProjectDetailsAsync(ProjectCardViewModel card)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var projectTask = card.IsArchived
                ? getProjectIncludingDeletedHandler.HandleAsync(new GetProjectIncludingDeletedQuery(card.Id))
                : getProjectHandler.HandleAsync(new GetProjectQuery(card.Id));
            var repositoryTask = getGitHubRepositoryHandler.HandleAsync(new GetGitHubRepositoryQuery(card.Id));
            var streakTask = getProjectStreakHandler.HandleAsync(new GetProjectStreakQuery(card.Id));
            await Task.WhenAll(projectTask, repositoryTask, streakTask);
            var project = await projectTask;
            var repository = await repositoryTask;
            var streak = await streakTask;
            var error = project.Error ?? repository.Error ?? streak.Error;
            if (error is not null || project.Value is null || repository.Value is null || streak.Value is null)
            { ShowError(error?.Message); return; }
            card.SetDetails(project.Value, repository.Value, streak.Value);
        }
        finally { IsBusy = false; }
    }

    private async Task<Result<T>> RunBusyAsync<T>(Func<Task<Result<T>>> action)
    {
        IsBusy = true;
        ErrorMessage = null;
        try { return await action(); }
        finally { IsBusy = false; }
    }

    private void ReplaceProjects(IEnumerable<ProjectResponse> projects)
    {
        _allProjects.Clear();
        foreach (var project in projects) _allProjects.Add(CreateProjectCard(project));
        SearchText = string.Empty;
        SelectedStatusFilter = "All";
        ApplyFilters();
        NotifyDashboardChanged();
    }

    private async Task RefreshGitStatusesAsync(CancellationToken cancellationToken)
    {
        foreach (var card in _allProjects)
        {
            await RefreshGitStatusAsync(card, cancellationToken);
        }
    }

    private async Task RefreshGitStatusAsync(
        ProjectCardViewModel card,
        CancellationToken cancellationToken)
    {
        var result = await getProjectGitStatusHandler.HandleAsync(
            new GetProjectGitStatusQuery(card.Id, card.FolderPath),
            cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            card.SetGitSnapshot(result.Value);
            var snapshot = result.Value;
            if (snapshot.GitHubUrl is not null && snapshot.GitHubOwner is not null &&
                snapshot.GitHubRepositoryName is not null)
            {
                var connection = await connectGitHubRepositoryHandler.HandleAsync(
                    new ConnectGitHubRepositoryCommand(
                        card.Id,
                        snapshot.GitHubOwner,
                        snapshot.GitHubRepositoryName,
                        snapshot.GitHubUrl,
                        snapshot.PreferredRemoteUrl,
                        snapshot.DefaultBranch),
                    cancellationToken);
                if (connection.IsSuccess && connection.Value is not null)
                    card.SetGitHubConnection(connection.Value);
                else
                    ShowError(connection.Error?.Message);
            }
            ApplyFilters();
        }
        else
            card.SetGitError(result.Error?.Message ?? "Git status could not be read.");
    }

    private void ShowError(string? message) => ErrorMessage = message ?? "The project operation could not be completed.";
    private void ApplyFilters()
    {
        IEnumerable<ProjectCardViewModel> filtered = _allProjects;
        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(project =>
                project.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                project.FolderPath.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        filtered = SelectedStatusFilter switch
        {
            "Active" or "Paused" => filtered.Where(project => project.Lifecycle == SelectedStatusFilter),
            "Clean" or "Dirty" => filtered.Where(project => project.WorkingTreeStatus == SelectedStatusFilter),
            "Not Git" => filtered.Where(project => project.WorkingTreeStatus == "Not Git"),
            "GitHub connected" => filtered.Where(project => project.RepositoryStatus == "GitHub connected"),
            _ => filtered,
        };
        Projects.Clear();
        foreach (var project in filtered) Projects.Add(project);
        NotifyDashboardChanged();
    }
    private void NotifyViewChanged()
    {
        OnPropertyChanged(nameof(IsProjectView));
        OnPropertyChanged(nameof(IsFavoritesView));
        OnPropertyChanged(nameof(WorkspaceTitle));
        OnPropertyChanged(nameof(WorkspaceSubtitle));
    }
    private void NotifyDashboardChanged()
    {
        OnPropertyChanged(nameof(HasProjects));
        OnPropertyChanged(nameof(HasNoProjects));
        OnPropertyChanged(nameof(ProjectCount));
        OnPropertyChanged(nameof(ActiveProjectCount));
        OnPropertyChanged(nameof(CurrentStreakDays));
    }
}
