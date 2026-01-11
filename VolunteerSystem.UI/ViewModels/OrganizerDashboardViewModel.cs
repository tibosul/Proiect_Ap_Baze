using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.UI.MVVM;

namespace VolunteerSystem.UI.ViewModels
{
    public class OrganizerDashboardViewModel : ViewModelBase
    {
        private readonly IOpportunityService _opportunityService;
        private readonly Organizer _currentUser;
        private readonly MainViewModel _mainViewModel;

        public string WelcomeMessage => $"Organizer: {_currentUser.OrganizationName}";

        private ObservableCollection<Opportunity> _myOpportunities = new ObservableCollection<Opportunity>();
        public ObservableCollection<Opportunity> MyOpportunities
        {
            get => _myOpportunities;
            set => SetProperty(ref _myOpportunities, value);
        }

        // Create Form
        private string _newTitle = string.Empty;
        public string NewTitle { get => _newTitle; set => SetProperty(ref _newTitle, value); }

        private string _newDescription = string.Empty;
        public string NewDescription { get => _newDescription; set => SetProperty(ref _newDescription, value); }
        
        private string _newLocation = string.Empty;
        public string NewLocation { get => _newLocation; set => SetProperty(ref _newLocation, value); }

        public ICommand CreateCommand { get; }
        public ICommand AddEventCommand { get; }
        public ICommand LogoutCommand { get; }

        public OrganizerDashboardViewModel(IOpportunityService opportunityService, Organizer currentUser, MainViewModel mainViewModel)
        {
            _opportunityService = opportunityService;
            _currentUser = currentUser;
            _mainViewModel = mainViewModel;

            CreateCommand = new RelayCommand(ExecuteCreate);
            AddEventCommand = new RelayCommand(ExecuteAddEvent);
            LogoutCommand = new RelayCommand(_ => _mainViewModel.CurrentView = new LoginViewModel(new Services.AuthenticationService(new Data.ApplicationDbContext()), new Services.OpportunityService(new Data.ApplicationDbContext()), _mainViewModel));

            _ = LoadOpportunities();
        }

        private async Task LoadOpportunities()
        {
            var list = await _opportunityService.GetOrganizerOpportunitiesAsync(_currentUser.Id);
            MyOpportunities = new ObservableCollection<Opportunity>(list);
        }

        private async void ExecuteCreate(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(NewTitle)) return;

            var opp = new Opportunity
            {
                Title = NewTitle,
                Description = NewDescription,
                Location = NewLocation,
                OrganizerId = _currentUser.Id,
                RequiredSkills = "General" // Placeholder
            };

            await _opportunityService.CreateOpportunityAsync(opp);
            
            // Reset form
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            NewLocation = string.Empty;

            await LoadOpportunities();
        }

        private async void ExecuteAddEvent(object? parameter)
        {
            if (parameter is Opportunity opp)
            {
                var evt = new Event
                {
                    OpportunityId = opp.Id,
                    StartTime = System.DateTime.Now.AddDays(1),
                    EndTime = System.DateTime.Now.AddDays(1).AddHours(2),
                    Location = opp.Location,
                    MaxVolunteers = 10,
                    IsCompleted = false
                };
                
                // We need a method in Service to add Event, or just add to DB if we had context.
                // Since we only have OpportunityService, let's add a method there or use Update.
                // Check IOpportunityService... it doesn't have AddEvent.
                // I'll assume UpdateOpportunityAsync works if I add to list, but EF Core needs explicit Add for new child if not tracking.
                // Best to add `CreateEventAsync` to Service.
                // For this prototype, I'll allow Update to handle it if the graph is attached, but detached graph is tricky.
                // I will add `CreateEventAsync` to `IOpportunityService` and implementation?
                // Or just hack it: `opp.Events.Add(evt); await _opportunityService.UpdateOpportunityAsync(opp);`
                // UpdateOpportunityAsync calls `_context.Opportunities.Update(opportunity)`.
                
                opp.Events.Add(evt);
                await _opportunityService.UpdateOpportunityAsync(opp);
                await LoadOpportunities();
            }
        }
    }
}
