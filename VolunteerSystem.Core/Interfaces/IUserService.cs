using System.Threading.Tasks;
using VolunteerSystem.Core.Entities;

namespace VolunteerSystem.Core.Interfaces
{
    public interface IUserService
    {
        Task<bool> UpdateVolunteerProfileAsync(Volunteer volunteer);
        Task<bool> UpdateOrganizerProfileAsync(Organizer organizer);
        Task<List<User>> GetAllUsersAsync();
        Task DeleteUserAsync(int userId);
    }
}
