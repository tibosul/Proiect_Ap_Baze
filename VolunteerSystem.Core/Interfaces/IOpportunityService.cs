using System.Collections.Generic;
using System.Threading.Tasks;
using VolunteerSystem.Core.Entities;

namespace VolunteerSystem.Core.Interfaces
{
    public interface IOpportunityService
    {
        Task<List<Opportunity>> GetAllOpportunitiesAsync();
        Task<List<Opportunity>> GetOrganizerOpportunitiesAsync(int organizerId);
        Task<List<Application>> GetVolunteerApplicationsAsync(int volunteerId);
        Task CreateOpportunityAsync(Opportunity opportunity);
        Task UpdateOpportunityAsync(Opportunity opportunity);
        Task DeleteOpportunityAsync(int id);
        Task ApplyToEventAsync(int volunteerId, int eventId);
        Task MarkAttendanceAsync(int applicationId, bool isPresent);
        Task SubmitFeedbackAsync(Feedback feedback);
        Task<List<Opportunity>> SearchOpportunitiesAsync(string query);
        Task WithdrawApplicationAsync(int applicationId);
        Task<List<Opportunity>> GetRecommendedOpportunitiesAsync(int volunteerId);
    }
}
