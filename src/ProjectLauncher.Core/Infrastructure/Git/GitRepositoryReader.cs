using System.Diagnostics;
using System.Globalization;

namespace ProjectLauncher.Core.Infrastructure.Git;

public sealed class GitRepositoryReader(TimeProvider timeProvider) : IGitRepositoryReader
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(5);

    public async Task<GitRepositorySnapshot> ReadAsync(
        int projectId,
        string folderPath,
        CancellationToken cancellationToken = default)
    {
        var refreshedAt = timeProvider.GetUtcNow();
        try
        {
            var inside = await RunGitAsync(folderPath, ["rev-parse", "--is-inside-work-tree"], cancellationToken);
            if (inside.ExitCode != 0 || !string.Equals(inside.Output.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                return GitRepositorySnapshot.NotGit(projectId, refreshedAt);

            var rootTask = RunGitAsync(folderPath, ["rev-parse", "--show-toplevel"], cancellationToken);
            var branchTask = RunGitAsync(folderPath, ["branch", "--show-current"], cancellationToken);
            var statusTask = RunGitAsync(folderPath, ["status", "--porcelain=v1", "--branch"], cancellationToken);
            var remotesTask = RunGitAsync(folderPath, ["remote", "-v"], cancellationToken);
            var commitTask = RunGitAsync(folderPath, ["log", "-1", "--all", "--format=%H%x1f%cI%x1f%s"], cancellationToken);
            await Task.WhenAll(rootTask, branchTask, statusTask, remotesTask, commitTask);

            var root = (await rootTask).Output.Trim();
            var branch = (await branchTask).Output.Trim();
            var status = ParseStatus((await statusTask).Output);
            var remotes = ParseRemotes((await remotesTask).Output);
            var preferredRemote = remotes.FirstOrDefault(remote => remote.Name == "origin")
                ?? remotes.FirstOrDefault();
            var github = NormalizeGitHubUrl(preferredRemote?.Url);
            var commit = ParseCommit((await commitTask).Output);

            return new GitRepositorySnapshot(
                projectId,
                true,
                root,
                string.IsNullOrWhiteSpace(branch) ? null : branch,
                string.IsNullOrWhiteSpace(branch),
                status.IsDirty,
                status.Staged,
                status.Modified,
                status.Untracked,
                remotes,
                preferredRemote?.Url,
                github.Url,
                github.Owner,
                github.Name,
                commit.Hash,
                commit.Summary,
                commit.Timestamp,
                refreshedAt,
                null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            return GitRepositorySnapshot.Unavailable(projectId, refreshedAt, exception.Message);
        }
    }

    private static async Task<GitCommandResult> RunGitAsync(
        string folderPath,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(folderPath);
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start()) throw new InvalidOperationException("Git could not be started.");
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CommandTimeout);
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            try { process.Kill(true); } catch (InvalidOperationException) { }
            throw new TimeoutException("Git inspection exceeded five seconds.");
        }
        return new GitCommandResult(process.ExitCode, await outputTask, await errorTask);
    }

    private static (bool IsDirty, int Staged, int Modified, int Untracked) ParseStatus(string output)
    {
        var staged = 0; var modified = 0; var untracked = 0;
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("##", StringComparison.Ordinal) || line.Length < 2) continue;
            if (line.StartsWith("??", StringComparison.Ordinal)) { untracked++; continue; }
            if (line[0] != ' ') staged++;
            if (line[1] != ' ') modified++;
        }
        return (staged + modified + untracked > 0, staged, modified, untracked);
    }

    private static IReadOnlyList<GitRemote> ParseRemotes(string output) => output
        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(line => line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        .Where(parts => parts.Length >= 2)
        .Select(parts => new GitRemote(parts[0], parts[1]))
        .GroupBy(remote => remote.Name, StringComparer.Ordinal)
        .Select(group => group.First())
        .ToArray();

    private static (string? Url, string? Owner, string? Name) NormalizeGitHubUrl(string? remote)
    {
        if (string.IsNullOrWhiteSpace(remote)) return (null, null, null);
        var value = remote.Trim();
        string? path = null;
        if (value.StartsWith("git@github.com:", StringComparison.OrdinalIgnoreCase))
            path = value["git@github.com:".Length..];
        else if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                 string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
            path = uri.AbsolutePath.Trim('/');
        if (path is null) return (null, null, null);
        if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase)) path = path[..^4];
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? ($"https://github.com/{parts[0]}/{parts[1]}", parts[0], parts[1])
            : (null, null, null);
    }

    private static (string? Hash, DateTimeOffset? Timestamp, string? Summary) ParseCommit(string output)
    {
        var parts = output.Trim().Split('\u001f', 3);
        if (parts.Length != 3) return (null, null, null);
        return (parts[0], DateTimeOffset.TryParse(parts[1], CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var timestamp) ? timestamp : null, parts[2]);
    }

    private sealed record GitCommandResult(int ExitCode, string Output, string Error);
}
