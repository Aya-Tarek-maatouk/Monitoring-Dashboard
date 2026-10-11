using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public sealed class ServerConfiguration : IEntityTypeConfiguration<Server>
{
    public void Configure(EntityTypeBuilder<Server> builder)
    {
        builder.ToTable("Servers");
        builder.HasKey(server => server.ServerId);
        builder.Property(server => server.Name).HasMaxLength(100).IsRequired();
        builder.Property(server => server.IPAddress).HasMaxLength(50);
        builder.Property(server => server.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(ServerStatus.Up);
        builder.Property(server => server.Description).HasMaxLength(250);
        builder.Property(server => server.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
    }
}