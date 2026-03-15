using InfraReportingSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

      
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Authority> Authorities { get; set; }

        public DbSet<Report> Reports { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ReportPic> ReportPics { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration classes automatically
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}