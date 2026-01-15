using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class LeaderboardViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly MainViewModel _mainViewModel;
        private readonly ViewModelBase _returnViewModel;

        [ObservableProperty]
        private ObservableCollection<Volunteer> _topVolunteers = new();

        public LeaderboardViewModel(IUserService userService, MainViewModel mainViewModel, ViewModelBase returnViewModel)
        {
            _userService = userService;
            _mainViewModel = mainViewModel;
            _returnViewModel = returnViewModel;

            _ = LoadLeaderboardAsync();
        }

        private async Task LoadLeaderboardAsync()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            var volunteers = allUsers.OfType<Volunteer>().OrderByDescending(v => v.Points).Take(10);
            
            TopVolunteers = new ObservableCollection<Volunteer>(volunteers);
        }

        [RelayCommand]
        private void GoBack()
        {
            _mainViewModel.CurrentView = _returnViewModel;
        }
    }
}
