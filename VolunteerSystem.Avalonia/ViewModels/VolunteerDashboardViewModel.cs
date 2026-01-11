using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; // Added for Console

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class VolunteerDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAuthenticationService _authService;
        private readonly IOpportunityService _opportunityService;
        private readonly Volunteer _volunteer;

        [ObservableProperty]
        private string _welcomeMessage;

        [ObservableProperty]
        private int _points;

        [ObservableProperty]
        private IEnumerable<Opportunity> _opportunities = new List<Opportunity>();

        public VolunteerDashboardViewModel(MainViewModel mainViewModel, IAuthenticationService authService, IOpportunityService opportunityService, Volunteer volunteer)
        {
            _mainViewModel = mainViewModel;
            _authService = authService;
            _opportunityService = opportunityService;
            _volunteer = volunteer;
            WelcomeMessage = $"Welcome, {volunteer.FullName}!";
            Points = volunteer.Points;

            _ = LoadOpportunitiesAsync();
        }

        private async Task LoadOpportunitiesAsync()
        {
            Opportunities = await _opportunityService.GetAllOpportunitiesAsync();
        }
        [RelayCommand]
        private async Task ApplyToEvent(Event evt)
        {
            if (evt != null)
            {
                try
                {
                    await _opportunityService.ApplyToEventAsync(_volunteer.Id, evt.Id);
                    // In a real app, show a success dialog or toast
                    Console.WriteLine($"[INFO] Applied to event {evt.Id} successfully.");
                    
                    // Refresh to potentially show update status (if we loaded applications)
                    await LoadOpportunitiesAsync();
                }
                catch (System.Exception ex)
                {
                     Console.WriteLine($"[ERROR] Failed to apply: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _mainViewModel.CurrentView = new LoginViewModel(_authService, _opportunityService, _mainViewModel);
        }
    }
}
