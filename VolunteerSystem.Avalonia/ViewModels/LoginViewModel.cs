using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.Core.Entities;
using System.Threading.Tasks;
using System;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;
        private readonly IOpportunityService _opportunityService;
        private readonly IChatService _chatService;
        private readonly IPointsService _pointsService;
        private readonly IReportService _reportService;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(IAuthenticationService authService, IUserService userService, IOpportunityService opportunityService, IChatService chatService, IPointsService pointsService, IReportService reportService, MainViewModel mainViewModel)
        {
            _authService = authService;
            _userService = userService;
            _opportunityService = opportunityService;
            _chatService = chatService;
            _pointsService = pointsService;
            _reportService = reportService;
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            Console.WriteLine($"[LoginViewModel] Login Clicked. Email: '{Email}'");
            try
            {
                var user = await _authService.LoginAsync(Email, Password);
                if (user != null)
                {
                    Console.WriteLine($"[Login] User found. Type: {user.GetType().Name}");
                    // Navigate to Dashboard based on Role
                    if (user is Admin admin)
                    {
                        _mainViewModel.CurrentView = new AdminDashboardViewModel(_mainViewModel, _authService, _userService, _opportunityService, _chatService, _pointsService, _reportService, admin);
                    }
                    else if (user is Volunteer volunteer)
                    {
                        _mainViewModel.CurrentView = new VolunteerDashboardViewModel(_mainViewModel, _authService, _userService, _opportunityService, _chatService, _pointsService, _reportService, volunteer);
                    }
                    else if (user is Organizer organizer)
                    {
                        _mainViewModel.CurrentView = new OrganizerDashboardViewModel(_mainViewModel, _authService, _userService, _opportunityService, _chatService, _pointsService, _reportService, organizer);
                    }
                    else
                    {
                        ErrorMessage = $"Unknown user role. Type: {user.GetType().Name}";
                    }
                }
                else
                {
                    ErrorMessage = "Invalid credentials.";
                }
            }
            catch (Exception ex)
            {
                // Detailed error for debugging
                ErrorMessage = $"Login Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    ErrorMessage += $"\nInner: {ex.InnerException.Message}";
                }
                Console.WriteLine($"[LoginError] {ex}");
            }
        }

        [RelayCommand]
        private void GoToRegister()
        {
            _mainViewModel.CurrentView = new RegistrationViewModel(_authService, _mainViewModel, this);
        }
    }
}
