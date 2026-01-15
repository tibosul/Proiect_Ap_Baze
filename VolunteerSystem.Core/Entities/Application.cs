using System;

namespace VolunteerSystem.Core.Entities
{
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected,
        Withdrawn
    }

    public class Application
    {
        public int Id { get; set; }
        
        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;

        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public DateTime AppliedDate { get; set; } = DateTime.Now;
        public bool IsPresent { get; set; } // Marked by organizer
        
        public bool CanWithdraw => Status == ApplicationStatus.Pending;
        public bool CanMarkPresent => !IsPresent && Status != ApplicationStatus.Withdrawn;
    }
}
