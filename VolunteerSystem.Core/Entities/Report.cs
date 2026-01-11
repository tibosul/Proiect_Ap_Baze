using System;

namespace VolunteerSystem.Core.Entities
{
    public class Report
    {
        public int Id { get; set; }
        public int ReporterId { get; set; } // User who reported
        public User Reporter { get; set; } = null!;
        
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; }
        
        // Optional: Target of report (Opportunity or Organization)
        // For simplicity, just text description or nullable IDs
        public int? ReportedOpportunityId { get; set; }
    }
}
