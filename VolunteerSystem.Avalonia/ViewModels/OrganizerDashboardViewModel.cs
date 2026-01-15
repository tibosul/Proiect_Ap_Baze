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
        private readonly IUserService _userService;
        private readonly IOpportunityService _opportunityService;
        private readonly IChatService _chatService;
        private readonly Organizer _organizer;

        [ObservableProperty]
        private string _welcomeMessage;

        [ObservableProperty]
        private string _organizationName;

        [ObservableProperty]
        private IEnumerable<Opportunity> _opportunities = new List<Opportunity>();

        public OrganizerDashboardViewModel(MainViewModel mainViewModel, IAuthenticationService authService, IUserService userService, IOpportunityService opportunityService, IChatService chatService, Organizer organizer)
        {
            _mainViewModel = mainViewModel;
            _authService = authService;
            _userService = userService;
            _opportunityService = opportunityService;
            _chatService = chatService;
            _organizer = organizer;
            WelcomeMessage = $"Welcome, {organizer.OrganizationName}!";
            OrganizationName = organizer.OrganizationName;

            // Load initial data
            _ = RefreshOpportunitiesAsync();
        }

        [RelayCommand]
        private void GoToProfile()
        {
            _mainViewModel.CurrentView = new OrganizerProfileViewModel(_userService, _mainViewModel, this, _organizer);
        }

        [RelayCommand]
        private void GoToChat()
        {
            _mainViewModel.CurrentView = new ChatViewModel(_chatService, _mainViewModel, _organizer, this);
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
        private async Task MarkPresent(Application app)
        {
            if (app != null)
            {
                await _opportunityService.MarkAttendanceAsync(app.Id, true);
                app.IsPresent = true; // Update local model
                // Maybe show a message?
            }
        }

        [RelayCommand]
        private void Logout()
        {
             _mainViewModel.CurrentView = new LoginViewModel(_authService, _userService, _opportunityService, _chatService, _mainViewModel);
        }
    }
}
