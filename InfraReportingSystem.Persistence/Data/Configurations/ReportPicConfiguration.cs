using InfraReportingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Data.Configurations {

    public class ReportPicConfiguration : IEntityTypeConfiguration<ReportPic>
    {
        public void Configure(EntityTypeBuilder<ReportPic> builder)
        {
            // Set PicId as the primary key
            builder.HasKey(p => p.PicId);

            // Picture URL is required and limited to 2048 characters
            builder.Property(p => p.PicUrl)
                   .IsRequired()
                   .HasMaxLength(2048);

            // Each picture belongs to one report
            // When a report is deleted, its pictures are also deleted
            builder.HasOne(p => p.Report)
                   .WithMany(r => r.ReportPics)
                   .HasForeignKey(p => p.ReportId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}