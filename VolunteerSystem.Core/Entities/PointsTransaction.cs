using System;

namespace VolunteerSystem.Core.Entities
{
    public class PointsTransaction
    {
        public int Id { get; set; }
        public int VolunteerId { get; set; }
        public int? EventId { get; set; }
        public int Points { get; set; }
        public PointsReason Reason { get; set; } // 0=CompletedEvent, 1=Bonus, 2=Penalty
        
        public string ReasonString => Reason switch
        {
            PointsReason.CompletedEvent => "Event Completion",
            PointsReason.Bonus => "Bonus",
            PointsReason.Penalty => "Penalty",
            _ => "Other"
        };

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        // public Volunteer Volunteer { get; set; } // Circular dependency potential if not careful, keeping simple for now or use User
        public Event? Event { get; set; }
    }
}
