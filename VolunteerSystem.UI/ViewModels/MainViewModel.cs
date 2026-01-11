using VolunteerSystem.UI.MVVM;

namespace VolunteerSystem.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentView;

        public ViewModelBase? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainViewModel()
        {
            // Initial view could be Login
            // CurrentView = new LoginViewModel();
        }
    }
}
