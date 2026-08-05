using CommunityToolkit.Mvvm.ComponentModel;

namespace ProjectLauncher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Add a project folder to get started.";
}
