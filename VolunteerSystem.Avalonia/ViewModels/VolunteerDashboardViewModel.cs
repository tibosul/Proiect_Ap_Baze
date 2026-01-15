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
        private readonly IUserService _userService;
        private readonly IOpportunityService _opportunityService;
        private readonly IChatService _chatService;
        private readonly IPointsService _pointsService;
        private readonly IReportService _reportService;
        private readonly Volunteer _volunteer;

        [ObservableProperty]
        private string _welcomeMessage;

        [ObservableProperty]
        private int _points;

        [ObservableProperty]
        private IEnumerable<Opportunity> _opportunities = new List<Opportunity>();

        [ObservableProperty]
        private List<Opportunity> _recommendedOpportunities = new List<Opportunity>();

        [ObservableProperty]
        private IEnumerable<PointsTransaction> _transactions = new List<PointsTransaction>();

        public VolunteerDashboardViewModel(MainViewModel mainViewModel, IAuthenticationService authService, IUserService userService, IOpportunityService opportunityService, IChatService chatService, IPointsService pointsService, IReportService reportService, Volunteer volunteer)
        {
            _mainViewModel = mainViewModel;
            _authService = authService;
            _userService = userService;
            _opportunityService = opportunityService;
            _chatService = chatService;
            _pointsService = pointsService;
            _reportService = reportService;
            _volunteer = volunteer;
            WelcomeMessage = $"Welcome, {volunteer.FullName}!";
            
            // Initial points from user object
            Points = volunteer.Points;

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadOpportunitiesAsync();
            await LoadRecommendedOpportunitiesAsync();
            await LoadPointsDataAsync();
        }

        private async Task LoadRecommendedOpportunitiesAsync()
        {
            RecommendedOpportunities = await _opportunityService.GetRecommendedOpportunitiesAsync(_volunteer.Id);
        }

        [RelayCommand]
        private async Task RefreshPoints()
        {
            await LoadPointsDataAsync();
        }

        private async Task LoadPointsDataAsync()
        {
             // Refresh points total
             Points = await _pointsService.GetTotalPointsAsync(_volunteer.Id);
             Transactions = await _pointsService.GetTransactionsAsync(_volunteer.Id);
        }

        [RelayCommand]
        private void GoToChat()
        {
            _mainViewModel.CurrentView = new ChatViewModel(_chatService, _mainViewModel, _volunteer, this);
        }

        [ObservableProperty]
        private string _searchText = string.Empty;

        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadOpportunitiesAsync();
            }
            else
            {
                Opportunities = await _opportunityService.SearchOpportunitiesAsync(SearchText);
            }
        }
        
        [RelayCommand]
        private async Task ClearSearch()
        {
            SearchText = string.Empty;
            await LoadOpportunitiesAsync();
        }

        [RelayCommand]
        private void GoToProfile()
        {
            _mainViewModel.CurrentView = new VolunteerProfileViewModel(_userService, _mainViewModel, this, _volunteer);
        }

        [RelayCommand]
        private void GoToApplications()
        {
            _mainViewModel.CurrentView = new VolunteerApplicationsViewModel(_opportunityService, _mainViewModel, this, _volunteer);
        }

        [RelayCommand]
        private void GoToLeaderboard()
        {
            _mainViewModel.CurrentView = new LeaderboardViewModel(_userService, _mainViewModel, this);
        }

        private async Task LoadOpportunitiesAsync()
        {
            Opportunities = await _opportunityService.GetAllOpportunitiesAsync();
        }
        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isStatusMessageError = false; // To color code the message

        [RelayCommand]
        private async Task ApplyToEvent(Event evt)
        {
            if (evt != null)
            {
                try
                {
                    StatusMessage = string.Empty; // Clear previous
                    await _opportunityService.ApplyToEventAsync(_volunteer.Id, evt.Id);
                    
                    StatusMessage = "Successfully applied!";
                    IsStatusMessageError = false;
                    
                    // Refresh to potentially show update status (if we loaded applications)
                    await LoadOpportunitiesAsync();
                }
                catch (System.Exception ex)
                {
                     StatusMessage = ex.Message;
                     IsStatusMessageError = true;
                     Console.WriteLine($"[ERROR] Failed to apply: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _mainViewModel.CurrentView = new LoginViewModel(_authService, _userService, _opportunityService, _chatService, _pointsService, _reportService, _mainViewModel);
        }
    }
}
