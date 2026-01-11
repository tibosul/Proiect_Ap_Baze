using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.Core.Entities;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class AdminDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAuthenticationService _authService;
        private readonly IOpportunityService _opportunityService;
        private readonly Admin _admin;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome, Administrator!";

        public AdminDashboardViewModel(MainViewModel mainViewModel, IAuthenticationService authService, IOpportunityService opportunityService, Admin admin)
        {
            _mainViewModel = mainViewModel;
            _authService = authService;
            _opportunityService = opportunityService;
            _admin = admin;
        }

        [RelayCommand]
        private void Logout()
        {
            _mainViewModel.CurrentView = new LoginViewModel(_authService, _opportunityService, _mainViewModel);
        }
    }
}
