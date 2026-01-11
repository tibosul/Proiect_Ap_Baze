using System.Threading.Tasks;
using VolunteerSystem.Core.Entities;

namespace VolunteerSystem.Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<bool> RegisterVolunteerAsync(string email, string password, string fullName, string skills);
        Task<bool> RegisterOrganizerAsync(string email, string password, string fullName, string organizationName);
    }
}
