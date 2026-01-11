using System;
using System.Windows.Input;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.UI.MVVM;

namespace VolunteerSystem.UI.ViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authService;
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

        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private bool _isOrganizer;
        public bool IsOrganizer
        {
            get => _isOrganizer;
            set
            {
                if (SetProperty(ref _isOrganizer, value))
                {
                    OnPropertyChanged(nameof(IsVolunteer));
                }
            }
        }

        public bool IsVolunteer => !IsOrganizer;

        // Volunteer specific
        private string _skills = string.Empty;
        public string Skills
        {
            get => _skills;
            set => SetProperty(ref _skills, value);
        }

        // Organizer specific
        private string _organizationName = string.Empty;
        public string OrganizationName
        {
            get => _organizationName;
            set => SetProperty(ref _organizationName, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public RegistrationViewModel(IAuthenticationService authService, MainViewModel mainViewModel)
        {
            _authService = authService;
            _mainViewModel = mainViewModel;
            RegisterCommand = new RelayCommand(ExecuteRegister);
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
        }

        private async void ExecuteRegister(object? parameter)
        {
            ErrorMessage = string.Empty;
            bool success = false;
            try
            {
                if (IsOrganizer)
                {
                    success = await _authService.RegisterOrganizerAsync(Email, Password, FullName, OrganizationName);
                }
                else
                {
                    success = await _authService.RegisterVolunteerAsync(Email, Password, FullName, Skills);
                }

                if (success)
                {
                    // Navigate to login
                    _mainViewModel.CurrentView = new LoginViewModel(_authService, _mainViewModel);
                }
                else
                {
                    ErrorMessage = "Registration failed. Email might already be taken.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred during registration.";
            }
        }

        private void ExecuteNavigateToLogin(object? parameter)
        {
            _mainViewModel.CurrentView = new LoginViewModel(_authService, _mainViewModel);
        }
    }
}
