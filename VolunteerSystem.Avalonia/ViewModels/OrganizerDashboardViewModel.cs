using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class OrganizerDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;
        private readonly IOpportunityService _opportunityService;
        private readonly IChatService _chatService;
        private readonly IPointsService _pointsService;
        private readonly IReportService _reportService;
        private readonly Organizer _organizer;

        [ObservableProperty]
        private string _welcomeMessage;

        [ObservableProperty]
        private string _organizationName;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isStatusMessageError = false;

        [ObservableProperty]
        private IEnumerable<Opportunity> _opportunities = new List<Opportunity>();

        public OrganizerDashboardViewModel(MainViewModel mainViewModel, IAuthenticationService authService, IUserService userService, IOpportunityService opportunityService, IChatService chatService, IPointsService pointsService, IReportService reportService, Organizer organizer)
        {
            _mainViewModel = mainViewModel;
            _authService = authService;
            _userService = userService;
            _opportunityService = opportunityService;
            _chatService = chatService;
            _pointsService = pointsService;
            _reportService = reportService;
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
                try
                {
                    StatusMessage = string.Empty;
                    await _opportunityService.MarkAttendanceAsync(app.Id, true);
                    app.IsPresent = true; // Update local model
                    
                    // Force property change notification for 'CanMarkPresent' if it wasn't automatic
                    // But since we are replacing the list or binding, checking if we need to refresh.
                    // The 'app' object inside 'Opportunities' list is updated. 
                    // However, 'CanMarkPresent' depends on 'IsPresent'.
                    
                    StatusMessage = $"Marked {app.Volunteer?.FullName ?? "Volunteer"} as present.";
                    IsStatusMessageError = false;

                    Console.WriteLine($"[INFO] {StatusMessage}");
                    await RefreshOpportunitiesAsync();
                }
                catch (System.Exception ex)
                {
                     StatusMessage = ex.Message;
                     IsStatusMessageError = true;
                     Console.WriteLine($"[ERROR] Failed to mark attendance: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void GoToReports()
        {
            _mainViewModel.CurrentView = new ReportViewModel(_reportService, _mainViewModel, this, _organizer.Id);
        }



        [RelayCommand]
        private void Logout()
        {
             _mainViewModel.CurrentView = new LoginViewModel(_authService, _userService, _opportunityService, _chatService, _pointsService, _reportService, _mainViewModel);
        }
    }
}
