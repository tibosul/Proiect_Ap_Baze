using System.Windows.Input;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.UI.MVVM;

namespace VolunteerSystem.UI.ViewModels
{
    public class AdminDashboardViewModel : ViewModelBase
    {
        private readonly Admin _currentUser;
        private readonly MainViewModel _mainViewModel;

        public string WelcomeMessage => $"Admin Panel: {_currentUser.FullName}";

        public ICommand LogoutCommand { get; }

        public AdminDashboardViewModel(Admin currentUser, MainViewModel mainViewModel)
        {
            _currentUser = currentUser;
            _mainViewModel = mainViewModel;
            LogoutCommand = new RelayCommand(_ => _mainViewModel.CurrentView = new LoginViewModel(new Services.AuthenticationService(new Data.ApplicationDbContext()), new Services.OpportunityService(new Data.ApplicationDbContext()), _mainViewModel));
        }
    }
}
