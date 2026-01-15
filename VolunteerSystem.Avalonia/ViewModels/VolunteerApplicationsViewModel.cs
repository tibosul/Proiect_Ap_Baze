using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Added for Any()

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class VolunteerApplicationsViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly VolunteerDashboardViewModel _dashboardViewModel;
        private readonly IOpportunityService _opportunityService;
        private readonly Volunteer _volunteer;

        [ObservableProperty]
        private IEnumerable<Application> _applications = new List<Application>();

        [ObservableProperty]
        private string _message = string.Empty;

        public VolunteerApplicationsViewModel(IOpportunityService opportunityService, MainViewModel mainViewModel, VolunteerDashboardViewModel dashboardViewModel, Volunteer volunteer)
        {
            _opportunityService = opportunityService;
            _mainViewModel = mainViewModel;
            _dashboardViewModel = dashboardViewModel;
            _volunteer = volunteer;

            _ = LoadApplicationsAsync();
        }

        private async Task LoadApplicationsAsync()
        {
            try 
            {
                Applications = await _opportunityService.GetVolunteerApplicationsAsync(_volunteer.Id);
                if (Applications == null || !((List<Application>)Applications).Any())
                {
                    Message = "You haven't applied to any opportunities yet.";
                }
            }
            catch(System.Exception ex)
            {
                Message = $"Error loading applications: {ex.Message}";
            }
        }

        [RelayCommand]
        private void GiveFeedback(Application app)
        {
            if (app != null)
            {
                _mainViewModel.CurrentView = new FeedbackViewModel(_opportunityService, _mainViewModel, this, app);
            }
        }

        [RelayCommand]
        private async Task Withdraw(Application app)
        {
             if (app != null)
            {
                try
                {
                    await _opportunityService.WithdrawApplicationAsync(app.Id);
                    await LoadApplicationsAsync();
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine($"[Withdraw] Error: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _mainViewModel.CurrentView = _dashboardViewModel;
        }
    }
}
