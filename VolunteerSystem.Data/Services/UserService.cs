using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;

namespace VolunteerSystem.Data.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateVolunteerProfileAsync(Volunteer volunteer)
        {
            var existing = await _context.Volunteers.FindAsync(volunteer.Id);
            if (existing == null) return false;

            existing.FullName = volunteer.FullName;
            existing.Skills = volunteer.Skills;
            existing.Points = volunteer.Points;
            // Add other fields as needed (Bio, City, etc. if added to entity)

            _context.Volunteers.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrganizerProfileAsync(Organizer organizer)
        {
            var existing = await _context.Organizers.FindAsync(organizer.Id);
            if (existing == null) return false;

            existing.OrganizationName = organizer.OrganizationName;
            existing.OrganizationDescription = organizer.OrganizationDescription;

            _context.Organizers.Update(existing);
            await _context.SaveChangesAsync();
            _context.Organizers.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
