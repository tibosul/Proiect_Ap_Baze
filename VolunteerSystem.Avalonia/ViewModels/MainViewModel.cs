using CommunityToolkit.Mvvm.ComponentModel;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? _currentView;

        public MainViewModel()
        {
        }
    }
}
