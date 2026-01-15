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
        private readonly IPointsService _pointsService;

        public OpportunityService(ApplicationDbContext context, IPointsService pointsService)
        {
            _context = context;
            _pointsService = pointsService;
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
                .Include(o => o.Events)
                .ThenInclude(e => e.Feedbacks)
                .ThenInclude(f => f.Volunteer)
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
            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null) throw new Exception("Event not found.");

            if (evt.StartTime < DateTime.Now)
            {
                throw new Exception("Cannot apply to past events.");
            }

            var existingApp = await _context.Applications
                .FirstOrDefaultAsync(a => a.VolunteerId == volunteerId && a.EventId == eventId);
            
            if (existingApp != null)
            {
                 if (existingApp.Status == ApplicationStatus.Withdrawn)
                 {
                     // Optional: Allow re-applying if withdrawn? For now, throw to be safe/simple as per request.
                     // Or maybe reactivate it? Let's throw "Already applied" as requested to prevent "many times".
                     // Actually, if it's withdrawn, maybe they want to re-apply. 
                     // But the user said "pot aplica de mai multe ori" (can apply multiple times) is bad.
                     // So strict check is better.
                     throw new Exception("You have already applied to this event (Status: " + existingApp.Status + ").");
                 }
                 throw new Exception("You have already applied to this event.");
            }

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
        public async Task MarkAttendanceAsync(int applicationId, bool isPresent)
        {
            var app = await _context.Applications
                .Include(a => a.Event)
                .ThenInclude(e => e.Opportunity)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app != null)
            {
                if (app.Status == ApplicationStatus.Withdrawn)
                {
                    throw new Exception("Cannot mark a withdrawn volunteer as present.");
                }

                // Prevent duplicate points/marking if already present
                if (app.IsPresent && isPresent) return;

                app.IsPresent = isPresent;
                if (isPresent)
                {
                    app.Status = ApplicationStatus.Approved;
                    
                    // Award points
                    if (app.Event?.Opportunity != null) 
                    {
                        var points = app.Event.Opportunity.Points;
                        await _pointsService.AwardPointsAsync(app.VolunteerId, points, PointsReason.CompletedEvent, app.EventId);
                    }
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
                // Soft delete / Status update
                application.Status = ApplicationStatus.Withdrawn;
                _context.Applications.Update(application);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Opportunity>> GetRecommendedOpportunitiesAsync(int volunteerId)
        {
            var volunteer = await _context.Volunteers.FindAsync(volunteerId);
            if (volunteer == null || string.IsNullOrWhiteSpace(volunteer.Skills))
            {
                return new List<Opportunity>();
            }

            var volunteerSkills = volunteer.Skills.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            // This logic is simple: if opportunity requires ANY of the volunteer's skills
            var opportunities = await GetAllOpportunitiesAsync();
            
            return opportunities.Where(o => 
                !string.IsNullOrWhiteSpace(o.RequiredSkills) &&
                volunteerSkills.Any(s => o.RequiredSkills.ToLower().Contains(s))
            ).ToList();
        }
    }
}
