using System;
using System.Windows.Input;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.UI.MVVM;

namespace VolunteerSystem.UI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IOpportunityService _opportunityService;
        private readonly MainViewModel _mainViewModel;

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(IAuthenticationService authService, IOpportunityService opportunityService, MainViewModel mainViewModel)
        {
            _authService = authService;
            _opportunityService = opportunityService;
            _mainViewModel = mainViewModel;
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);
        }

        private bool CanExecuteLogin(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private async void ExecuteLogin(object? parameter)
        {
            ErrorMessage = string.Empty;
            try
            {
                var user = await _authService.LoginAsync(Email, Password);
                if (user != null)
                {
                    if (user is Volunteer volunteer)
                    {
                        _mainViewModel.CurrentView = new VolunteerDashboardViewModel(_opportunityService, volunteer, _mainViewModel);
                    }
                    else if (user is Organizer organizer)
                    {
                        _mainViewModel.CurrentView = new OrganizerDashboardViewModel(_opportunityService, organizer, _mainViewModel);
                    }
                    else if (user is Admin admin)
                    {
                        _mainViewModel.CurrentView = new AdminDashboardViewModel(admin, _mainViewModel);
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
                ErrorMessage = "An error occurred during login.";
            }
        }

        private void ExecuteNavigateToRegister(object? parameter)
        {
            _mainViewModel.CurrentView = new RegistrationViewModel(_authService, _mainViewModel);
        }
    }
}
