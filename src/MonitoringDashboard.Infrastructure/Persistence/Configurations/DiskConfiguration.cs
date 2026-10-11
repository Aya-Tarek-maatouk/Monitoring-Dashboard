using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public sealed class DiskConfiguration : IEntityTypeConfiguration<Disk>
{
    public void Configure(EntityTypeBuilder<Disk> builder)
    {
        builder.ToTable("Disks");
        builder.HasKey(disk => disk.DiskId);
        builder.Property(disk => disk.DriveLetter).HasMaxLength(5).IsRequired();
        builder.Property(disk => disk.Timestamp).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(disk => new { disk.ServerId, disk.Timestamp });
        builder.HasOne(disk => disk.Server)
            .WithMany(server => server.Disks)
            .HasForeignKey(disk => disk.ServerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}