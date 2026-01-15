using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VolunteerSystem.Core.Interfaces;
using VolunteerSystem.Core.Entities;
using System.Threading.Tasks;

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class AdminDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;
        private readonly IOpportunityService _opportunityService;
        private readonly IChatService _chatService;
        private readonly Admin _admin;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome, Administrator!";

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<User> _users = new();

        public AdminDashboardViewModel(MainViewModel mainViewModel, IAuthenticationService authService, IUserService userService, IOpportunityService opportunityService, IChatService chatService, Admin admin)
        {
            _mainViewModel = mainViewModel;
            _authService = authService;
            _userService = userService;
            _opportunityService = opportunityService;
            _chatService = chatService;
            _admin = admin;
            
            _ = LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            Users = new System.Collections.ObjectModel.ObservableCollection<User>(users);
        }

        [RelayCommand]
        private async Task DeleteUserAsync(User user)
        {
            if (user != null && user.Id != _admin.Id) // Prevent deleting self
            {
                await _userService.DeleteUserAsync(user.Id);
                Users.Remove(user);
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _mainViewModel.CurrentView = new LoginViewModel(_authService, _userService, _opportunityService, _chatService, _mainViewModel);
        }
    }
}
