using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;
using System.Threading.Tasks;
using System;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class OrganizerProfileViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly MainViewModel _mainViewModel;
        private readonly OrganizerDashboardViewModel _dashboardViewModel;
        private readonly Organizer _organizer;

        [ObservableProperty]
        private string _organizationName;

        [ObservableProperty]
        private string _organizationDescription;

        [ObservableProperty]
        private string _message;

        public OrganizerProfileViewModel(IUserService userService, MainViewModel mainViewModel, OrganizerDashboardViewModel dashboardViewModel, Organizer organizer)
        {
            _userService = userService;
            _mainViewModel = mainViewModel;
            _dashboardViewModel = dashboardViewModel;
            _organizer = organizer;

            // Initialize
            OrganizationName = organizer.OrganizationName;
            OrganizationDescription = organizer.OrganizationDescription;
            Message = "";
        }

        [RelayCommand]
        private async Task SaveProfileAsync()
        {
            try
            {
                // Update entity
                _organizer.OrganizationName = OrganizationName;
                _organizer.OrganizationDescription = OrganizationDescription;

                var result = await _userService.UpdateOrganizerProfileAsync(_organizer);
                if (result)
                {
                    Message = "Profile updated successfully!";
                    // Update dashboard welcome message if name changed
                    _dashboardViewModel.WelcomeMessage = $"Welcome, {_organizer.OrganizationName}!";
                    _dashboardViewModel.OrganizationName = _organizer.OrganizationName;
                }
                else
                {
                    Message = "Failed to update profile.";
                }
            }
            catch (Exception ex)
            {
                Message = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _mainViewModel.CurrentView = _dashboardViewModel;
        }
    }
}
