using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class FeedbackViewModel : ViewModelBase
    {
        private readonly IOpportunityService _opportunityService;
        private readonly MainViewModel _mainViewModel;
        private readonly ViewModelBase _returnViewModel;
        private readonly Application _application;

        [ObservableProperty]
        private int _rating = 5;

        [ObservableProperty]
        private string _comment = string.Empty;

        public FeedbackViewModel(IOpportunityService opportunityService, MainViewModel mainViewModel, ViewModelBase returnViewModel, Application application)
        {
            _opportunityService = opportunityService;
            _mainViewModel = mainViewModel;
            _returnViewModel = returnViewModel;
            _application = application;
        }

        [RelayCommand]
        private async Task SubmitFeedback()
        {
            var feedback = new Feedback
            {
                VolunteerId = _application.VolunteerId,
                EventId = _application.EventId,
                Rating = Rating,
                Comment = Comment,
                CreatedAt = DateTime.Now
            };

            await _opportunityService.SubmitFeedbackAsync(feedback);
            
            // Return
            _mainViewModel.CurrentView = _returnViewModel;
        }

        [RelayCommand]
        private void Cancel()
        {
            _mainViewModel.CurrentView = _returnViewModel;
        }
    }
}
