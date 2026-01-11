using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;
using System.Threading.Tasks;
using System;
using System.Linq; // Added for ToList()

using System.Collections.ObjectModel; // Added
using VolunteerSystem.Avalonia.ViewModels; // Standard namespace

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class EditOpportunityViewModel : ViewModelBase
    {
        private readonly IOpportunityService _opportunityService;
        private readonly MainViewModel _mainViewModel;
        private readonly OrganizerDashboardViewModel _organizerDashboardViewModel; // To go back
        private readonly Organizer _organizer;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _location = string.Empty;

        [ObservableProperty]
        private string _requiredSkills = string.Empty;

        // Event Management
        [ObservableProperty]
        private ObservableCollection<Event> _events = new ObservableCollection<Event>();

        [ObservableProperty]
        private DateTimeOffset _newEventDate = DateTimeOffset.Now;

        [ObservableProperty]
        private TimeSpan _newEventStartTime = TimeSpan.FromHours(9); // Default 9 AM

        [ObservableProperty]
        private TimeSpan _newEventEndTime = TimeSpan.FromHours(17); // Default 5 PM

        [ObservableProperty]
        private int _newEventMaxVolunteers = 10;


        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private Opportunity? _existingOpportunity;

        public EditOpportunityViewModel(
            IOpportunityService opportunityService, 
            MainViewModel mainViewModel, 
            OrganizerDashboardViewModel organizerDashboardViewModel,
            Organizer organizer,
            Opportunity? existingOpportunity = null)
        {
            _opportunityService = opportunityService;
            _mainViewModel = mainViewModel;
            _organizerDashboardViewModel = organizerDashboardViewModel;
            _organizer = organizer;
            _existingOpportunity = existingOpportunity;

            if (_existingOpportunity != null)
            {
                Title = _existingOpportunity.Title;
                Description = _existingOpportunity.Description;
                Location = _existingOpportunity.Location;
                RequiredSkills = _existingOpportunity.RequiredSkills;

                if (_existingOpportunity.Events != null)
                {
                    foreach (var evt in _existingOpportunity.Events)
                    {
                        Events.Add(evt);
                    }
                }
            }
        }

        [RelayCommand]
        private void AddEvent()
        {
            try 
            {
                // Combine Date + Time
                var startDateTime = NewEventDate.Date + NewEventStartTime;
                var endDateTime = NewEventDate.Date + NewEventEndTime;

                if (endDateTime <= startDateTime)
                {
                    ErrorMessage = "End time must be after start time.";
                    return;
                }

                var newEvent = new Event
                {
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    MaxVolunteers = NewEventMaxVolunteers,
                    IsCompleted = false
                };

                Events.Add(newEvent);
                ErrorMessage = string.Empty; // Clear errors
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Invalid date/time: {ex.Message}";
            }
        }

        [RelayCommand]
        private void RemoveEvent(Event eventToDelete)
        {
            if (Events.Contains(eventToDelete))
            {
                Events.Remove(eventToDelete);
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            Console.WriteLine("[DEBUG] SaveAsync invoked.");
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Title and Description are required.";
                return;
            }

            // Validation for Organizer
            if (_organizer == null)
            {
                ErrorMessage = "Error: Organizer session is null.";
                return;
            }
            if (_organizer.Id <= 0)
            {
                ErrorMessage = $"Error: Invalid Organizer ID ({_organizer.Id}). Please log out and back in.";
                return;
            }

            try
            {
                if (_existingOpportunity == null)
                {
                    // Create New
                    var opportunity = new Opportunity
                    {
                        Title = Title,
                        Description = Description,
                        Location = Location,
                        RequiredSkills = RequiredSkills,
                        OrganizerId = _organizer.Id,
                        Events = Events.ToList()
                    };

                    Console.WriteLine($"[DEBUG] Creating new opportunity...");
                    await _opportunityService.CreateOpportunityAsync(opportunity);
                }
                else
                {
                    // Update Existing
                    _existingOpportunity.Title = Title;
                    _existingOpportunity.Description = Description;
                    _existingOpportunity.Location = Location;
                    _existingOpportunity.RequiredSkills = RequiredSkills;
                    _existingOpportunity.Events = Events.ToList(); // Note: This might need better handling for deletions

                    Console.WriteLine($"[DEBUG] Updating opportunity ID: {_existingOpportunity.Id}");
                    await _opportunityService.UpdateOpportunityAsync(_existingOpportunity);
                }

                Console.WriteLine("[DEBUG] Save successful.");
                
                // Navigate back
                await _organizerDashboardViewModel.RefreshOpportunitiesAsync(); // Refresh list
                _mainViewModel.CurrentView = _organizerDashboardViewModel;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving: {ex.Message}";
                Console.WriteLine($"[DEBUG] Error saving: {ex}");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _mainViewModel.CurrentView = _organizerDashboardViewModel;
        }
    }
}
