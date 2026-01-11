using System.Collections.Generic;

namespace VolunteerSystem.Core.Entities
{
    public class Opportunity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RequiredSkills { get; set; } = string.Empty; // Comma separated for now
        public string Location { get; set; } = string.Empty;
        
        public int OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
