using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.UserId);
        builder.Property(user => user.UserName).HasMaxLength(50).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(100).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(user => user.RefreshToken).HasMaxLength(500);
        builder.Property(user => user.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(user => user.UserName).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasOne(user => user.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(user => user.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}