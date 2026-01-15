using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using VolunteerSystem.Core.Interfaces;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class ReportViewModel : ViewModelBase
    {
        private readonly IReportService _reportService;
        private readonly MainViewModel _mainViewModel;
        private readonly ViewModelBase _returnViewModel;
        private readonly int _organizerId;

        [ObservableProperty]
        private OrganizerReport _report;

        [ObservableProperty]
        private bool _isLoading;

        public ReportViewModel(IReportService reportService, MainViewModel mainViewModel, ViewModelBase returnViewModel, int organizerId)
        {
            _reportService = reportService;
            _mainViewModel = mainViewModel;
            _returnViewModel = returnViewModel;
            _organizerId = organizerId;
            
            _ = LoadReportAsync();
        }

        private async Task LoadReportAsync()
        {
            IsLoading = true;
            Report = await _reportService.GenerateOrganizerReportAsync(_organizerId);
            IsLoading = false;
        }

        [RelayCommand]
        private void GoBack()
        {
            _mainViewModel.CurrentView = _returnViewModel;
        }
    }
}
