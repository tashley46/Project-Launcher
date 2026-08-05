namespace ProjectLaunch.Core.Domain;

public class ProjectStreak
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public int CurrentDays { get; set; }

    public int LongestDays { get; set; }

    public DateTimeOffset? LastCommitByUserAt { get; set; }

    public int ActiveCommitDaysLast30 { get; set; }

    public DateTimeOffset? CalculatedAt { get; set; }
}

