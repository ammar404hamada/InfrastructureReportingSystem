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
            builder.ToTable("ReportPics");
            builder.HasKey(rp => rp.ReportId);
            builder.Property(rp => rp.PicUrl)
                .IsRequired()
                .HasMaxLength(200);
        }
    }
}