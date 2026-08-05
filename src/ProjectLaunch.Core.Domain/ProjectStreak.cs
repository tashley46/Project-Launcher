using System;

namespace ProjectLaunch.Core.Domain;

public sealed record ProjectStreakDto
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public Project Project { get; init; } = null!;

    public int CurrentDays { get; init; }

    public int LongestDays { get; init; }

    public DateTimeOffset? LastCommitByUserAt { get; init; }

    public int ActiveCommitDaysLast30 { get; init; }

    public DateTimeOffset? CalculatedAt { get; init; }
}

public class ProjectStreak : EntityBase
{

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public int CurrentDays { get; set; }

    public int LongestDays { get; set; }

    public DateTimeOffset? LastCommitByUserAt { get; set; }

    public int ActiveCommitDaysLast30 { get; set; }

    public DateTimeOffset? CalculatedAt { get; set; }

    public static ProjectStreak Create(ProjectStreakDto dto)
    {
        var streak = new ProjectStreak();
        streak.Update(dto);
        return streak;
    }

    public void Update(ProjectStreakDto dto)
    {
        Id = dto.Id;
        ProjectId = dto.ProjectId;
        Project = dto.Project;
        CurrentDays = dto.CurrentDays;
        LongestDays = dto.LongestDays;
        LastCommitByUserAt = dto.LastCommitByUserAt;
        ActiveCommitDaysLast30 = dto.ActiveCommitDaysLast30;
        CalculatedAt = dto.CalculatedAt;
    }
}
