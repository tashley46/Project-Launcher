using System.Collections.ObjectModel;
using ProjectLauncher.Core.GitHubRepositories.Queries;
using ProjectLauncher.Core.Projects;
using ProjectLauncher.Core.Projects.Commands;
using ProjectLauncher.Core.Projects.Queries;
using ProjectLauncher.Core.Streaks.Queries;

namespace ProjectLauncher.ViewModels;

public sealed class MainViewModel(
    AddProjectCommandHandler addProjectHandler,
    GetProjectQueryHandler getProjectHandler,
    GetProjectsQueryHandler getProjectsHandler,
    GetGitHubRepositoryQueryHandler getGitHubRepositoryHandler,
    GetProjectStreakQueryHandler getProjectStreakHandler) : ViewModelBase
{
    private bool _isBusy;
    private bool _hasLoaded;
    private string? _errorMessage;

    public ObservableCollection<ProjectCardViewModel> Projects { get; } = [];

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasProjects => Projects.Count > 0;

    public bool HasNoProjects => !HasProjects;

    public int ProjectCount => Projects.Count;

    public int ActiveProjectCount => Projects.Count(project => project.Lifecycle == "Active");

    public int CurrentStreakDays => Projects.Count == 0
        ? 0
        : Projects.Max(project => project.CurrentStreakDays);

    public async Task LoadProjectsAsync(CancellationToken cancellationToken = default)
    {
        if (_hasLoaded)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await getProjectsHandler.HandleAsync(
                new GetProjectsQuery(),
                cancellationToken);

            if (!result.IsSuccess || result.Value is null)
            {
                ErrorMessage = result.Error?.Message ?? "Saved projects could not be loaded.";
                return;
            }

            Projects.Clear();
            foreach (var project in result.Value)
            {
                Projects.Add(CreateProjectCard(project));
            }

            _hasLoaded = true;
            NotifyDashboardChanged();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task AddProjectAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var addResult = await addProjectHandler.HandleAsync(
                new AddProjectCommand(folderPath),
                cancellationToken);

            if (!addResult.IsSuccess || addResult.Value is null)
            {
                ErrorMessage = addResult.Error?.Message ?? "The project could not be added.";
                return;
            }

            var getResult = await getProjectHandler.HandleAsync(
                new GetProjectQuery(addResult.Value.Id),
                cancellationToken);

            if (!getResult.IsSuccess || getResult.Value is null)
            {
                ErrorMessage = getResult.Error?.Message ?? "The saved project could not be loaded.";
                return;
            }

            Projects.Add(CreateProjectCard(getResult.Value));
            NotifyDashboardChanged();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void DismissError() => ErrorMessage = null;

    private ProjectCardViewModel CreateProjectCard(ProjectResponse response) =>
        ProjectCardViewModel.FromResponse(response, LoadProjectDetailsAsync);

    private async Task LoadProjectDetailsAsync(ProjectCardViewModel card)
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var projectTask = getProjectHandler.HandleAsync(new GetProjectQuery(card.Id));
            var repositoryTask = getGitHubRepositoryHandler.HandleAsync(
                new GetGitHubRepositoryQuery(card.Id));
            var streakTask = getProjectStreakHandler.HandleAsync(
                new GetProjectStreakQuery(card.Id));

            await Task.WhenAll(projectTask, repositoryTask, streakTask);

            var projectResult = await projectTask;
            var repositoryResult = await repositoryTask;
            var streakResult = await streakTask;

            var error = projectResult.Error ?? repositoryResult.Error ?? streakResult.Error;
            if (error is not null ||
                projectResult.Value is null ||
                repositoryResult.Value is null ||
                streakResult.Value is null)
            {
                ErrorMessage = error?.Message ?? "Project details could not be loaded.";
                return;
            }

            card.SetDetails(projectResult.Value, repositoryResult.Value, streakResult.Value);
        }
        finally
        {
            IsBusy = false;
        }
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
