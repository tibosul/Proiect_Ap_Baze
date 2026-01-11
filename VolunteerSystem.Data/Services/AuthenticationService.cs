using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Helpers;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.Data;

namespace VolunteerSystem.Data.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;

        public AuthenticationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return null;
            }

            if (PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }

        public async Task<bool> RegisterVolunteerAsync(string email, string password, string fullName, string skills)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return false;

            var volunteer = new Volunteer
            {
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                FullName = fullName,
                Skills = skills,
                Points = 0
            };

            _context.Users.Add(volunteer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegisterOrganizerAsync(string email, string password, string fullName, string organizationName)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return false;

            var organizer = new Organizer
            {
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),

                OrganizationName = organizationName,
                OrganizationDescription = "New Organization" // Default
            };

            _context.Users.Add(organizer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
