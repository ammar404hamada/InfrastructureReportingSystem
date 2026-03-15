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
    public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
    {
        public void Configure(EntityTypeBuilder<OtpVerification> builder)
        {
            // OTP code is required and limited to 20 characters
            builder.Property(o => o.OtpCode)
                   .IsRequired()
                   .HasMaxLength(20);

            // Expiration time of the OTP
            builder.Property(o => o.ExpiresAt)
                   .IsRequired();

            // Indicates whether the OTP has already been used
            builder.Property(o => o.IsUsed)
                   .IsRequired()
                   .HasDefaultValue(false);

            // Index to speed up queries filtering by UserId
            builder.HasIndex(o => o.UserId)
                   .HasDatabaseName("IX_OtpVerification_UserId");

            // Composite index for queries filtering by user, usage status, and expiration
            builder.HasIndex(o => new { o.UserId, o.IsUsed, o.ExpiresAt })
                   .HasDatabaseName("IX_OtpVerification_UserId_IsUsed_ExpiresAt");

            // Relationship: each OTP belongs to one User
            builder.HasOne(o => o.User)
                   .WithMany(u => u.OtpVerifications)
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
