using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.Core.Entities;
using System.Threading.Tasks;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class AdminDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;
        private readonly IOpportunityService _opportunityService;
        private readonly IChatService _chatService;
        private readonly IPointsService _pointsService;
        private readonly IReportService _reportService;
        private readonly Admin _admin;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome, Administrator!";

        [ObservableProperty]
        private SystemReport _systemReport;

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<User> _users = new();

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<Opportunity> _allOpportunities = new();

        public AdminDashboardViewModel(MainViewModel mainViewModel, IAuthenticationService authService, IUserService userService, IOpportunityService opportunityService, IChatService chatService, IPointsService pointsService, IReportService reportService, Admin admin)
        {
            _mainViewModel = mainViewModel;
            _authService = authService;
            _userService = userService;
            _opportunityService = opportunityService;
            _chatService = chatService;
            _pointsService = pointsService;
            _reportService = reportService;
            _admin = admin;
            
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadUsersAsync();
            await LoadSystemReportAsync();
            await LoadAllOpportunitiesAsync();
        }

        private async Task LoadUsersAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            Users = new System.Collections.ObjectModel.ObservableCollection<User>(users);
        }

        private async Task LoadSystemReportAsync()
        {
            SystemReport = await _reportService.GenerateSystemReportAsync();
        }

        private async Task LoadAllOpportunitiesAsync()
        {
            // We need a method in IOpportunityService to get ALL opportunities (including those by organizers)
            // Existing GetAllOpportunitiesAsync returns all, so we can reuse it.
            var opportunities = await _opportunityService.GetAllOpportunitiesAsync();
            AllOpportunities = new System.Collections.ObjectModel.ObservableCollection<Opportunity>(opportunities);
        }

        [RelayCommand]
        private async Task DeleteUserAsync(User user)
        {
            if (user != null && user.Id != _admin.Id) // Prevent deleting self
            {
                await _userService.DeleteUserAsync(user.Id);
                Users.Remove(user);
                 // Refresh stats
                await LoadSystemReportAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteOpportunityAsync(Opportunity opportunity)
        {
            if (opportunity != null)
            {
                await _opportunityService.DeleteOpportunityAsync(opportunity.Id);
                AllOpportunities.Remove(opportunity);
                // Refresh stats
                await LoadSystemReportAsync();
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _mainViewModel.CurrentView = new LoginViewModel(_authService, _userService, _opportunityService, _chatService, _pointsService, _reportService, _mainViewModel);
        }
    }
}
