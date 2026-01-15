using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.Data;

namespace VolunteerSystem.Data.Services
{
    public class OpportunityService : IOpportunityService
    {
        private readonly ApplicationDbContext _context;

        public OpportunityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Opportunity>> GetAllOpportunitiesAsync()
        {
            return await _context.Opportunities
                .Include(o => o.Organizer)
                .Include(o => o.Events)
                .ToListAsync();
        }

        public async Task<List<Opportunity>> GetOrganizerOpportunitiesAsync(int organizerId)
        {
            return await _context.Opportunities
                .Where(o => o.OrganizerId == organizerId)
                .Include(o => o.Events)
                .ThenInclude(e => e.Applications)
                .ThenInclude(a => a.Volunteer)
                .ToListAsync();
        }

        public async Task<List<Application>> GetVolunteerApplicationsAsync(int volunteerId)
        {
            return await _context.Applications
                .Where(a => a.VolunteerId == volunteerId)
                .Include(a => a.Event)
                .ThenInclude(e => e.Opportunity)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();
        }

        public async Task CreateOpportunityAsync(Opportunity opportunity)
        {
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOpportunityAsync(Opportunity opportunity)
        {
            _context.Opportunities.Update(opportunity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOpportunityAsync(int id)
        {
            var opportunity = await _context.Opportunities.FindAsync(id);
            if (opportunity != null)
            {
                _context.Opportunities.Remove(opportunity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ApplyToEventAsync(int volunteerId, int eventId)
        {
            var existingApp = await _context.Applications
                .FirstOrDefaultAsync(a => a.VolunteerId == volunteerId && a.EventId == eventId);
            
            if (existingApp == null)
            {
                var app = new Application
                {
                    VolunteerId = volunteerId,
                    EventId = eventId,
                    Status = ApplicationStatus.Pending,
                    AppliedDate = DateTime.Now
                };
                _context.Applications.Add(app);
                await _context.SaveChangesAsync();
            }
        }
        public async Task MarkAttendanceAsync(int applicationId, bool isPresent)
        {
            var app = await _context.Applications.FindAsync(applicationId);
            if (app != null)
            {
                app.IsPresent = isPresent;
                if (isPresent)
                {
                    app.Status = ApplicationStatus.Approved; 
                }
                _context.Applications.Update(app);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SubmitFeedbackAsync(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Opportunity>> SearchOpportunitiesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllOpportunitiesAsync();
            }

            query = query.ToLower();
            return await _context.Opportunities
                .Include(o => o.Organizer)
                .Include(o => o.Events)
                .Where(o => o.Title.ToLower().Contains(query) || 
                            o.Description.ToLower().Contains(query) || 
                            o.Location.ToLower().Contains(query) ||
                            o.RequiredSkills.ToLower().Contains(query))
                .ToListAsync();
        }

        public async Task WithdrawApplicationAsync(int applicationId)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application != null)
            {
                _context.Applications.Remove(application);
                await _context.SaveChangesAsync();
            }
        }
    }
}
