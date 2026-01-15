using System;
using System.Collections.Generic;

namespace VolunteerSystem.Core.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public int OpportunityId { get; set; }
        public Opportunity Opportunity { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxVolunteers { get; set; }
        public bool IsCompleted { get; set; }

        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
