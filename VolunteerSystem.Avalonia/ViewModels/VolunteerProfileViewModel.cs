using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;
using System.Threading.Tasks;
using System;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class VolunteerProfileViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly MainViewModel _mainViewModel;
        private readonly VolunteerDashboardViewModel _dashboardViewModel;
        private readonly Volunteer _volunteer;

        [ObservableProperty]
        private string _fullName;

        [ObservableProperty]
        private string _skills;

        [ObservableProperty]
        private string _message;

        public VolunteerProfileViewModel(IUserService userService, MainViewModel mainViewModel, VolunteerDashboardViewModel dashboardViewModel, Volunteer volunteer)
        {
            _userService = userService;
            _mainViewModel = mainViewModel;
            _dashboardViewModel = dashboardViewModel;
            _volunteer = volunteer;

            // Initialize fields
            FullName = volunteer.FullName;
            Skills = volunteer.Skills;
            Message = "";
        }

        [RelayCommand]
        private async Task SaveProfileAsync()
        {
            try
            {
                // Update entity
                _volunteer.FullName = FullName;
                _volunteer.Skills = Skills;

                var result = await _userService.UpdateVolunteerProfileAsync(_volunteer);
                if (result)
                {
                    Message = "Profile updated successfully!";
                    // Update dashboard welcome message if name changed
                    _dashboardViewModel.WelcomeMessage = $"Welcome, {_volunteer.FullName}!";
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
