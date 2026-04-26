using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Map the User entity to the Identity users table
            builder.ToTable("AspNetUsers");

            // The "UserType" column determines which type each record represents
            builder.HasDiscriminator<string>("UserType")
                   .HasValue<User>("User")
                   .HasValue<Worker>("Worker")
                   .HasValue<Authority>("Authority");

            // Name is required and limited to 200 characters
            builder.Property(u => u.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            // Limit Email length to 200 characters
            // Required validation will be handled in the application layer
            builder.Property(u => u.Email)
                   .HasMaxLength(200);

            // Profile picture URL with a maximum length
            builder.Property(u => u.ProfilePictureUrl)
                   .HasMaxLength(2048);

            // Store UserStatus enum as an integer with default value Inactive
            builder.Property(u => u.Status)
                   .HasConversion<int>()
                   .IsRequired()
                   .HasDefaultValue(UserStatus.Inactive);

            builder.Property(u => u.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");
        }
    }


}


