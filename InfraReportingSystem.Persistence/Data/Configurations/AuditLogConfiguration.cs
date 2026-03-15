using InfraReportingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            // Store ActionType enum as an integer
            builder.Property(a => a.ActionType)
                   .HasConversion<int>()
                   .IsRequired();

            // Details of the action (required, max length 2000)
            builder.Property(a => a.Details)
                   .IsRequired()
                   .HasMaxLength(2000);

            // Name of the affected entity (e.g., "Report", "User")
            builder.Property(a => a.EntityName)
                   .HasMaxLength(100);

            // ID of the affected entity
            builder.Property(a => a.EntityId)
                   .HasMaxLength(450);

            // Timestamp of when the log entry was created
            builder.Property(a => a.Timestamp)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            // ----- Indexes -----

            // Index to speed up queries ordered or filtered by timestamp
            builder.HasIndex(a => a.Timestamp)
                   .HasDatabaseName("IX_AuditLog_Timestamp");

            // Index for queries filtering logs by user
            builder.HasIndex(a => a.UserId)
                   .HasDatabaseName("IX_AuditLog_UserId");

            // Composite index for queries filtering by action type and time
            builder.HasIndex(a => new { a.ActionType, a.Timestamp })
                   .HasDatabaseName("IX_AuditLog_ActionType_Timestamp");

            // ----- Relationship -----

            // Each log may reference a User
            builder.HasOne(a => a.User)
                   .WithMany(u => u.AuditLogs)
                   .HasForeignKey(a => a.UserId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
