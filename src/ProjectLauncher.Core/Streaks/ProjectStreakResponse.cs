using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Core.Streaks;

public sealed record ProjectStreakResponse(
    int CurrentDays,
    int LongestDays,
    DateTimeOffset? LastCommitByUserAt,
    int ActiveCommitDaysLast30,
    DateTimeOffset? CalculatedAt)
{
    public static ProjectStreakResponse FromDomain(ProjectStreak streak) => new(
        streak.CurrentDays,
        streak.LongestDays,
        streak.LastCommitByUserAt,
        streak.ActiveCommitDaysLast30,
        streak.CalculatedAt);
}
