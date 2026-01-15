using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VolunteerSystem.Core.Interfaces;

namespace VolunteerSystem.Data.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrganizerReport> GenerateOrganizerReportAsync(int organizerId)
        {
            var report = new OrganizerReport();

            var opportunities = await _context.Opportunities
                .Where(o => o.OrganizerId == organizerId)
                .Include(o => o.Events)
                .ThenInclude(e => e.Applications)
                .ToListAsync();

            report.TotalOpportunities = opportunities.Count;
            report.TotalEvents = opportunities.Sum(o => o.Events.Count);
            
            var allEvents = opportunities.SelectMany(o => o.Events).ToList();
            var allApps = allEvents.SelectMany(e => e.Applications).ToList();

            report.TotalApplications = allApps.Count;
            report.TotalVolunteersPresent = allApps.Count(a => a.IsPresent);

            // Calculate hours (EndTime - StartTime) for present volunteers
            foreach (var evt in allEvents)
            {
                var presentCount = evt.Applications.Count(a => a.IsPresent);
                if (presentCount > 0)
                {
                    var duration = (evt.EndTime - evt.StartTime).TotalHours;
                    if (duration > 0)
                    {
                        report.TotalHours += (int)(duration * presentCount);
                    }
                }
            }

            return report;
        }

        public async Task<SystemReport> GenerateSystemReportAsync()
        {
            var report = new SystemReport();

            report.TotalUsers = await _context.Users.CountAsync();
            report.TotalVolunteers = await _context.Volunteers.CountAsync();
            report.TotalOrganizers = await _context.Organizers.CountAsync();
            report.TotalOpportunities = await _context.Opportunities.CountAsync();
            report.TotalApplications = await _context.Applications.CountAsync();

            return report;
        }
    }
}
