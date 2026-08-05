using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Data.EF.EntityConfigurations;

public class ProjectStreakConfiguration : IEntityTypeConfiguration<ProjectStreak>
{
    public void Configure(EntityTypeBuilder<ProjectStreak> builder)
    {
        builder.HasKey(streak => streak.Id);
        builder.HasIndex(streak => streak.ProjectId).IsUnique();
    }
}

