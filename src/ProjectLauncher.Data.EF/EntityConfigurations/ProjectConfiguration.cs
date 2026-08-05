using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Data.EF.EntityConfigurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Name).IsRequired();
        builder.OwnsOne(project => project.Folder, folder =>
        {
            folder.Property(value => value.Path).HasColumnName("LocalPath").IsRequired();
        });

        builder.HasOne(project => project.GitHubRepository)
            .WithOne(repository => repository.Project)
            .HasForeignKey<GitHubRepository>(repository => repository.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(project => project.Streak)
            .WithOne(streak => streak.Project)
            .HasForeignKey<ProjectStreak>(streak => streak.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

