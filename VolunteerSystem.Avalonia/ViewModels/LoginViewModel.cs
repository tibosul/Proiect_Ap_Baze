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
        private readonly IOpportunityService _opportunityService;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(IAuthenticationService authService, IOpportunityService opportunityService, MainViewModel mainViewModel)
        {
            _authService = authService;
            _opportunityService = opportunityService;
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var user = await _authService.LoginAsync(Email, Password);
                if (user != null)
                {
                    // Navigate to Dashboard based on Role
                    if (user is Admin admin)
                    {
                        _mainViewModel.CurrentView = new AdminDashboardViewModel(_mainViewModel, _authService, _opportunityService, admin);
                    }
                    else if (user is Volunteer volunteer)
                    {
                        _mainViewModel.CurrentView = new VolunteerDashboardViewModel(_mainViewModel, _authService, _opportunityService, volunteer);
                    }
                    else if (user is Organizer organizer)
                    {
                        _mainViewModel.CurrentView = new OrganizerDashboardViewModel(_mainViewModel, _authService, _opportunityService, organizer);
                    }
                    else
                    {
                        ErrorMessage = "Unknown user role.";
                    }
                }
                else
                {
                    ErrorMessage = "Invalid credentials.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private void GoToRegister()
        {
            _mainViewModel.CurrentView = new RegistrationViewModel(_authService, _mainViewModel, this);
        }
    }
}
