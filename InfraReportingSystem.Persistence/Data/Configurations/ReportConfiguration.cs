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
            // ----- Properties -----

            // Description is required and limited to 1000 characters
            builder.Property(r => r.Description)
                   .IsRequired()
                   .HasMaxLength(1000);

            // Store ReportStatus enum as an integer
            builder.Property(r => r.Status)
                   .HasConversion<int>()
                   .IsRequired();

            // Optional rejection reason with max length 1000
            builder.Property(r => r.RejectionReason)
                   .HasMaxLength(1000);

            // Latitude stored as SQL float and required
            builder.Property(r => r.Latitude)
                   .HasColumnType("float")
                   .IsRequired();

            // Longitude stored as SQL float and required
            builder.Property(r => r.Longitude)
                   .HasColumnType("float")
                   .IsRequired();

            // Ensure latitude and longitude stay within valid geographic ranges
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Report_Latitude", "[Latitude] >= -90.0 AND [Latitude] <= 90.0");
                t.HasCheckConstraint("CK_Report_Longitude", "[Longitude] >= -180.0 AND [Longitude] <= 180.0");
            });

            // Set default value for UploadedAt in the database
            builder.Property(r => r.UploadedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            // ----- Indexes -----

            // Indexes to speed up common queries
            builder.HasIndex(r => r.Status);
            builder.HasIndex(r => r.CategoryId);
            builder.HasIndex(r => r.SubmittedById);
            builder.HasIndex(r => r.AssignedWorkerId);
            builder.HasIndex(r => r.UploadedAt);

            // Composite index for queries filtering by worker and status
            builder.HasIndex(r => new { r.AssignedWorkerId, r.Status })
                   .HasDatabaseName("IX_Report_AssignedWorkerId_Status");

            // Composite index for queries filtering by submitter and status
            builder.HasIndex(r => new { r.SubmittedById, r.Status })
                   .HasDatabaseName("IX_Report_SubmittedById_Status");

            // ----- Relationships -----

            // Report belongs to one Category
            builder.HasOne(r => r.Category)
                   .WithMany(c => c.Reports)
                   .HasForeignKey(r => r.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Report was submitted by one User
            builder.HasOne(r => r.SubmittedBy)
                   .WithMany(u => u.SubmittedReports)
                   .HasForeignKey(r => r.SubmittedById)
                   .OnDelete(DeleteBehavior.Restrict);

            // Report may be assigned to a Worker
            builder.HasOne(r => r.AssignedWorker)
                   .WithMany(w => w.AssignedReports)
                   .HasForeignKey(r => r.AssignedWorkerId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Authority who assigned the worker
            builder.HasOne(r => r.AssignedByAuthority)
                   .WithMany(a => a.ReportsAssignedByMe)
                   .HasForeignKey(r => r.AssignedByAuthorityId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Report can have many pictures
            builder.HasMany(r => r.ReportPics)
                   .WithOne(p => p.Report)
                   .HasForeignKey(p => p.ReportId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
