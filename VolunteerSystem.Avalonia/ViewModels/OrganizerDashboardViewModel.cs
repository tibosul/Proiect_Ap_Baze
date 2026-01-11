using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class OrganizerDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAuthenticationService _authService;
        private readonly IOpportunityService _opportunityService;
        private readonly Organizer _organizer;

        [ObservableProperty]
        private string _welcomeMessage;

        [ObservableProperty]
        private string _organizationName;

        [ObservableProperty]
        private IEnumerable<Opportunity> _opportunities = new List<Opportunity>();

        public OrganizerDashboardViewModel(MainViewModel mainViewModel, IAuthenticationService authService, IOpportunityService opportunityService, Organizer organizer)
        {
            _mainViewModel = mainViewModel;
            _authService = authService;
            _opportunityService = opportunityService;
            _organizer = organizer;
            WelcomeMessage = $"Welcome, {organizer.OrganizationName}!";
            OrganizationName = organizer.OrganizationName;

            // Load initial data
            _ = RefreshOpportunitiesAsync();
        }

        public async Task RefreshOpportunitiesAsync()
        {
            Opportunities = await _opportunityService.GetOrganizerOpportunitiesAsync(_organizer.Id);
        }

        [RelayCommand]
        private void CreateOpportunity()
        {
            _mainViewModel.CurrentView = new EditOpportunityViewModel(_opportunityService, _mainViewModel, this, _organizer);
        }

        [RelayCommand]
        private void EditOpportunity(Opportunity opportunity)
        {
            if (opportunity != null)
            {
                _mainViewModel.CurrentView = new EditOpportunityViewModel(_opportunityService, _mainViewModel, this, _organizer, opportunity);
            }
        }
        [RelayCommand]
        private void Logout()
        {
            _mainViewModel.CurrentView = new LoginViewModel(_authService, _opportunityService, _mainViewModel);
        }
    }
}
