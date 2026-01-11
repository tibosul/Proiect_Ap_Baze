using System.Collections.Generic;
using System.Threading.Tasks;
using VolunteerSystem.Core.Entities;

namespace VolunteerSystem.Core.Interfaces
{
    public interface IOpportunityService
    {
        Task<List<Opportunity>> GetAllOpportunitiesAsync();
        Task<List<Opportunity>> GetOrganizerOpportunitiesAsync(int organizerId);
        Task CreateOpportunityAsync(Opportunity opportunity);
        Task UpdateOpportunityAsync(Opportunity opportunity);
        Task DeleteOpportunityAsync(int id);
        Task ApplyToEventAsync(int volunteerId, int eventId);
    }
}
