using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(permission => permission.PermissionId);
        builder.Property(permission => permission.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(permission => permission.Name).IsUnique();
        builder.Property(permission => permission.Description).HasMaxLength(250);
    }
}