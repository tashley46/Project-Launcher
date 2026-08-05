using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace ProjectLauncher.Services;

public sealed class ProjectFolderPicker
{
    private Window? _owner;

    public void SetOwner(Window owner) => _owner = owner;

    public async Task<string?> PickAsync(string title)
    {
        if (_owner is null) return null;
        var folders = await _owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
        });
        return folders.Count == 0 ? null : folders[0].TryGetLocalPath();
    }
}
