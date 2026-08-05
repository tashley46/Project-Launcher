using ProjectLauncher.Core.Infrastructure.Git;

namespace ProjectLauncher.Core.Streaks;

public sealed record ProjectStreakCalculation(
    int CurrentDays,
    int LongestDays,
    DateTimeOffset? LastCommitByUserAt,
    int ActiveCommitDaysLast30);

public static class ProjectStreakCalculator
{
    public static ProjectStreakCalculation Calculate(
        IEnumerable<GitCommit> commits,
        IEnumerable<string> identityEmails,
        DateOnly today)
    {
        var identities = identityEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var matching = commits
            .Where(commit => identities.Contains(commit.AuthorEmail))
            .GroupBy(commit => commit.Hash, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
        var days = matching
            .Select(commit => DateOnly.FromDateTime(commit.AuthoredAt.LocalDateTime))
            .Distinct()
            .Order()
            .ToArray();
        var daySet = days.ToHashSet();

        var start = daySet.Contains(today)
            ? today
            : daySet.Contains(today.AddDays(-1)) ? today.AddDays(-1) : (DateOnly?)null;
        var current = 0;
        while (start is not null && daySet.Contains(start.Value.AddDays(-current))) current++;

        var longest = 0;
        var run = 0;
        DateOnly? previous = null;
        foreach (var day in days)
        {
            run = previous is not null && day == previous.Value.AddDays(1) ? run + 1 : 1;
            longest = Math.Max(longest, run);
            previous = day;
        }

        var firstDay = today.AddDays(-29);
        var activeLast30 = days.Count(day => day >= firstDay && day <= today);
        DateTimeOffset? lastCommit = matching.Length == 0
            ? null
            : matching.Max(commit => commit.AuthoredAt);
        return new ProjectStreakCalculation(current, longest, lastCommit, activeLast30);
    }
}
