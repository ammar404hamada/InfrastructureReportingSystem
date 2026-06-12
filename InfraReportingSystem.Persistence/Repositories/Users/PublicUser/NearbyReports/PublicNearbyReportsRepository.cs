using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.NearbyReports;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.NearbyReports;
using Microsoft.EntityFrameworkCore;


namespace InfraReportingSystem.Persistence.Repositories.Users.PublicUser.NearbyReports
{
    public class PublicNearbyReportsRepository : IPublicNearbyReportsRepository
    {
        private static readonly HashSet<ReportStatus> VisibleStatuses = new()
        {
            ReportStatus.Submitted,
            ReportStatus.InProgress,
            ReportStatus.Blocked,
            ReportStatus.PendingConfirmation,
        };

        private const double EarthRadiusKm = 6371.0;

        private readonly AppDbContext _context;

        public PublicNearbyReportsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NearbyReportDto>> GetNearbyReportsAsync(
            double latitude,
            double longitude,
            double radiusInKm)
        {
            var userLatRad = ToRadians(latitude);
            var userLonRad = ToRadians(longitude);

            var latDelta = radiusInKm / 111.0;
            var lonDelta = radiusInKm / (111.0 * Math.Cos(ToRadians(latitude)));

            var minLat = latitude - latDelta;
            var maxLat = latitude + latDelta;
            var minLon = longitude - lonDelta;
            var maxLon = longitude + lonDelta;

            var candidates = await _context.Reports
                .AsNoTracking()
                .Where(r =>
                    VisibleStatuses.Contains(r.Status) &&
                    r.Latitude >= minLat && r.Latitude <= maxLat &&
                    r.Longitude >= minLon && r.Longitude <= maxLon)
                .Select(r => new
                {
                    r.Id,
                    r.Latitude,
                    r.Longitude,
                    r.Status,
                    CategoryName = r.Category != null ? r.Category.Name : string.Empty
                })
                .ToListAsync();

            var results = candidates
                .Select(r => new NearbyReportDto
                {
                    ReportId = r.Id,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    CategoryName = r.CategoryName,
                    Status = r.Status.ToString(),
                    DistanceInKm = HaversineKm(userLatRad, userLonRad, r.Latitude, r.Longitude)
                })
                .Where(r => r.DistanceInKm <= radiusInKm)
                .OrderBy(r => r.DistanceInKm)
                .ToList();

            return results;
        }

        private static double HaversineKm(
            double userLatRad, double userLonRad,
            double reportLat, double reportLon)
        {
            var reportLatRad = ToRadians(reportLat);
            var reportLonRad = ToRadians(reportLon);

            var dLat = reportLatRad - userLatRad;
            var dLon = reportLonRad - userLonRad;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(userLatRad) * Math.Cos(reportLatRad)
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
