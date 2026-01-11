using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.UI.MVVM;

namespace VolunteerSystem.UI.ViewModels
{
    public class VolunteerDashboardViewModel : ViewModelBase
    {
        private readonly IOpportunityService _opportunityService;
        private readonly Volunteer _currentUser;
        private readonly MainViewModel _mainViewModel;

        public string WelcomeMessage => $"Welcome, {_currentUser.FullName} (Points: {_currentUser.Points})";

        private ObservableCollection<Opportunity> _opportunities = new ObservableCollection<Opportunity>();
        public ObservableCollection<Opportunity> Opportunities
        {
            get => _opportunities;
            set => SetProperty(ref _opportunities, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterOpportunities();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ApplyCommand { get; }
        public ICommand LogoutCommand { get; }

        public VolunteerDashboardViewModel(IOpportunityService opportunityService, Volunteer currentUser, MainViewModel mainViewModel)
        {
            _opportunityService = opportunityService;
            _currentUser = currentUser;
            _mainViewModel = mainViewModel;

            RefreshCommand = new RelayCommand(async _ => await LoadOpportunities());
            ApplyCommand = new RelayCommand(ExecuteApply);
            LogoutCommand = new RelayCommand(_ => _mainViewModel.CurrentView = new LoginViewModel(new Services.AuthenticationService(new Data.ApplicationDbContext()), _mainViewModel));

            // Initial load
            _ = LoadOpportunities();
        }

        private async Task LoadOpportunities()
        {
            var list = await _opportunityService.GetAllOpportunitiesAsync();
            
            // Simple Recommendation Algorithm: Sort by matching skills
            if (!string.IsNullOrEmpty(_currentUser.Skills))
            {
                var userSkills = _currentUser.Skills.Split(',').Select(s => s.Trim().ToLower()).ToList();
                list = list.OrderByDescending(o => 
                {
                    if (string.IsNullOrEmpty(o.RequiredSkills)) return 0;
                    var oppSkills = o.RequiredSkills.Split(',').Select(s => s.Trim().ToLower());
                    return oppSkills.Count(os => userSkills.Contains(os));
                }).ToList();
            }

            Opportunities = new ObservableCollection<Opportunity>(list);
            FilterOpportunities();
        }

        private async void FilterOpportunities()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Reload original list
                await LoadOpportunities();
            }
            else
            {
                var filtered = _opportunities.Where(o => 
                    o.Title.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) || 
                    o.Description.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                    o.Location.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)
                ).ToList();
                
                Opportunities = new ObservableCollection<Opportunity>(filtered);
            }
        }

        private async void ExecuteApply(object? parameter)
        {
            if (parameter is Event evt)
            {
                await _opportunityService.ApplyToEventAsync(_currentUser.Id, evt.Id);
                // Show success message or refresh
            }
        }
    }
}
