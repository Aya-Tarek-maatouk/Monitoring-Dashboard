using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public sealed class MetricConfiguration : IEntityTypeConfiguration<Metric>
{
    public void Configure(EntityTypeBuilder<Metric> builder)
    {
        builder.ToTable("Metrics");
        builder.HasKey(metric => metric.MetricId);
        builder.Property(metric => metric.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(metric => metric.Timestamp).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(metric => new { metric.ServerId, metric.Timestamp });
        builder.HasOne(metric => metric.Server)
            .WithMany(server => server.Metrics)
            .HasForeignKey(metric => metric.ServerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}