namespace VolunteerSystem.Core.Entities
{
    public class Volunteer : User
    {
        public string FullName { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public int Points { get; set; }
        
        // Navigation properties for future
        // public ICollection<Event> AttendedEvents { get; set; }
    }
}
