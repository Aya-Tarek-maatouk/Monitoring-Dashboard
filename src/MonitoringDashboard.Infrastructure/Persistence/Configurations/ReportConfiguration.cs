using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(report => report.ReportId);
        builder.Property(report => report.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(ReportStatus.Pending);
        builder.Property(report => report.FilePath).HasMaxLength(500);
        builder.Property(report => report.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(report => new { report.ServerId, report.StartTime, report.EndTime });
        builder.HasOne(report => report.Server)
            .WithMany(server => server.Reports)
            .HasForeignKey(report => report.ServerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}