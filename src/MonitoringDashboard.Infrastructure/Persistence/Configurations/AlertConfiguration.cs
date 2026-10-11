using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");
        builder.HasKey(alert => alert.AlertId);
        builder.Property(alert => alert.MetricType).HasMaxLength(50).IsRequired();
        builder.Property(alert => alert.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(AlertStatus.Triggered);
        builder.Property(alert => alert.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(alert => new { alert.ServerId, alert.CreatedAt });
        builder.HasOne(alert => alert.Server)
            .WithMany(server => server.Alerts)
            .HasForeignKey(alert => alert.ServerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}