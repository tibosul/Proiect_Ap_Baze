using System.Threading.Tasks;
using VolunteerSystem.Core.Entities;
using System.Collections.Generic;

namespace VolunteerSystem.Core.Interfaces
{
    public class OrganizerReport
    {
        public int TotalEvents { get; set; }
        public int TotalOpportunities { get; set; }
        public int TotalApplications { get; set; }
        public int TotalVolunteersPresent { get; set; }
        public int TotalHours { get; set; } // Simplified: 1 event = duration hours
    }

    public class SystemReport
    {
        public int TotalUsers { get; set; }
        public int TotalVolunteers { get; set; }
        public int TotalOrganizers { get; set; }
        public int TotalOpportunities { get; set; }
        public int TotalApplications { get; set; }
    }

    public interface IReportService
    {
        Task<OrganizerReport> GenerateOrganizerReportAsync(int organizerId);
        Task<SystemReport> GenerateSystemReportAsync();
    }
}
