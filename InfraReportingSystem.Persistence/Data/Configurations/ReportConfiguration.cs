using InfraReportingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Data.Configurations {

    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Reports");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(1000);
            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>();
            builder.Property(r => r.Latitude)
                .IsRequired();
            builder.Property(r => r.Longitude)
                .IsRequired();
            builder.Property(r => r.UploadedAt)
                .IsRequired();

            builder.HasMany(r => r.ReportPics)
                .WithOne(rp => rp.Report)
                .HasForeignKey(rp => rp.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.SubmittedBy)
                .WithMany(u => u.SubmittedReports)
                .HasForeignKey(r => r.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.AssignedWorker)
                .WithMany(w => w.AssignedReports)
                .HasForeignKey(r => r.AssignedWorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.AssignedByAuthority)
                .WithMany(a => a.ReportsAssignedByMe)
                .HasForeignKey(r => r.AssignedByAuthorityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}