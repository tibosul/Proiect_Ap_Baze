using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Interfaces;
using System.Threading.Tasks;
using System;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class RegistrationViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authService;
        private readonly MainViewModel _mainViewModel;
        private readonly LoginViewModel _loginViewModel;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private bool _isOrganizer;

        [ObservableProperty]
        private string _organizationName = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public RegistrationViewModel(IAuthenticationService authService, MainViewModel mainViewModel, LoginViewModel loginViewModel)
        {
            _authService = authService;
            _mainViewModel = mainViewModel;
            _loginViewModel = loginViewModel;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                if (IsOrganizer)
                {
                    if (string.IsNullOrWhiteSpace(OrganizationName))
                    {
                        ErrorMessage = "Organization Name is required.";
                        return;
                    }
                    await _authService.RegisterOrganizerAsync(Email, Password, OrganizationName, FullName);
                }
                else
                {
                    await _authService.RegisterVolunteerAsync(Email, Password, FullName, "General");
                }

                // Metadata: Show success or login automatically?
                // For now, go back to login with a message? Or just login.
                // Let's go back to Login so they can verify.
                _loginViewModel.ErrorMessage = "Registration successful! Please login.";
                _mainViewModel.CurrentView = _loginViewModel;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _mainViewModel.CurrentView = _loginViewModel;
        }
    }
}
