using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectLaunch.Core.Domain;

namespace ProjectLauncher.Data.EF.EntityConfigurations;

public class GitHubRepositoryConfiguration : IEntityTypeConfiguration<GitHubRepository>
{
    public void Configure(EntityTypeBuilder<GitHubRepository> builder)
    {
        builder.HasKey(repository => repository.Id);
        builder.Property(repository => repository.Owner).IsRequired();
        builder.Property(repository => repository.Name).IsRequired();
        builder.Property(repository => repository.WebUrl).IsRequired();
        builder.Property(repository => repository.DefaultBranch).HasMaxLength(255);
        builder.HasIndex(repository => repository.ProjectId).IsUnique();
    }
}
