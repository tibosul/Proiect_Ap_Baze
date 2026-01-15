using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;

namespace VolunteerSystem.Data.Services
{
    public class PointsService : IPointsService
    {
        private readonly ApplicationDbContext _context;

        public PointsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AwardPointsAsync(int volunteerId, int points, PointsReason reason, int? eventId = null)
        {
            var volunteer = await _context.Volunteers.FindAsync(volunteerId);
            if (volunteer == null) return;

            // Update user total
            volunteer.Points += points;
            _context.Volunteers.Update(volunteer); // Profile points

            // Add transaction record
            var transaction = new PointsTransaction
            {
                VolunteerId = volunteerId,
                EventId = eventId,
                Points = points,
                Reason = reason, // 0=CompletedEvent, 1=Bonus, 2=Penalty (Mapped to Enum later if needed)
                CreatedAt = DateTime.UtcNow
            };
            
            _context.PointsTransactions.Add(transaction);
            
            await _context.SaveChangesAsync();
        }

        public async Task<List<PointsTransaction>> GetTransactionsAsync(int volunteerId)
        {
            return await _context.PointsTransactions
                .Where(t => t.VolunteerId == volunteerId)
                .Include(t => t.Event)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalPointsAsync(int volunteerId)
        {
            var volunteer = await _context.Volunteers.FindAsync(volunteerId);
            return volunteer?.Points ?? 0;
        }
    }
}
