using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraReportingSystem.Persistence.Data.Configurations
{
    public class ReportAffectedUserConfiguration : IEntityTypeConfiguration<ReportAffectedUser>
    {
        public void Configure(EntityTypeBuilder<ReportAffectedUser> builder)
        {
            builder.Property(r => r.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(r => r.UserId)
                   .IsRequired();

            builder.HasIndex(r => new { r.ReportId, r.UserId })
                   .IsUnique()
                   .HasDatabaseName("IX_ReportAffectedUser_ReportId_UserId");

            builder.HasIndex(r => r.UserId)
                   .HasDatabaseName("IX_ReportAffectedUser_UserId");

            builder.HasOne(r => r.Report)
                   .WithMany(r => r.AffectedUsers)
                   .HasForeignKey(r => r.ReportId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                   .WithMany(u => u.AffectedReports)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
