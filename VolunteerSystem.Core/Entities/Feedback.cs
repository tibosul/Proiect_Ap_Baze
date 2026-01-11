using System;

namespace VolunteerSystem.Core.Entities
{
    public class Feedback
    {
        public int Id { get; set; }
        
        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;

        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public int Rating { get; set; } // 1-5
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
