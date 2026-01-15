using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;

namespace VolunteerSystem.Data.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetConversationAsync(int userId1, int userId2)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                            (m.SenderId == userId2 && m.ReceiverId == userId1))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<List<User>> GetChatContactsAsync(int userId)
        {
            // Logic:
            // If Volunteer: Get Organizers of events they applied to (Accepted or Pending?) - Let's say all.
            // If Organizer: Get Volunteers who applied to their events.
            
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<User>();

            if (user is Volunteer)
            {
                // Find organizers
                return await _context.Applications
                    .Where(a => a.VolunteerId == userId)
                    .Select(a => a.Event.Opportunity.Organizer)
                    .Distinct()
                    .Cast<User>() // Cast to base User
                    .ToListAsync();
            }
            else if (user is Organizer)
            {
                // Find volunteers
                return await _context.Applications
                    .Where(a => a.Event.Opportunity.OrganizerId == userId)
                    .Select(a => a.Volunteer)
                    .Distinct()
                    .Cast<User>()
                    .ToListAsync();
            }

            return new List<User>();
        }
    }
}
