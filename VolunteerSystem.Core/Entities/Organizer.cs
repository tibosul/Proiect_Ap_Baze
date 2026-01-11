namespace VolunteerSystem.Core.Entities
{
    public class Organizer : User
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string OrganizationDescription { get; set; } = string.Empty;
        
        // public ICollection<Opportunity> CreatedOpportunities { get; set; }
    }
}
