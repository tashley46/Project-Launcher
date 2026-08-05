using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ProjectLauncher.ViewModels;

namespace ProjectLauncher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Opened += MainWindow_OnOpened;
    }

    private async void MainWindow_OnOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.LoadProjectsAsync();
        }
    }

    private async void AddProject_OnClick(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Choose a project folder",
                AllowMultiple = false,
            });

        if (folders.Count == 0 || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var localPath = folders[0].TryGetLocalPath();
        if (localPath is not null)
        {
            await viewModel.AddProjectAsync(localPath);
        }
    }

    private void DismissError_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.DismissError();
        }
    }
}
