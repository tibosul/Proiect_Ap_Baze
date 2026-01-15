using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VolunteerSystem.Core.Entities;
using VolunteerSystem.Core.Interfaces;
using System;
using System.Linq; // Added for sort logic

namespace VolunteerSystem.Avalonia.ViewModels
{
    public partial class ChatViewModel : ViewModelBase
    {
        private readonly IChatService _chatService;
        private readonly MainViewModel _mainViewModel;
        private readonly User _currentUser;
        private readonly ViewModelBase _returnViewModel;

        [ObservableProperty]
        private ObservableCollection<User> _contacts = new();

        [ObservableProperty]
        private User? _selectedContact;

        [ObservableProperty]
        private ObservableCollection<ChatMessage> _messages = new();

        [ObservableProperty]
        private string _newMessageText = string.Empty;

        public ChatViewModel(IChatService chatService, MainViewModel mainViewModel, User currentUser, ViewModelBase returnViewModel)
        {
            _chatService = chatService;
            _mainViewModel = mainViewModel;
            _currentUser = currentUser;
            _returnViewModel = returnViewModel;

            _ = LoadContactsAsync();
        }

        private async Task LoadContactsAsync()
        {
            var contacts = await _chatService.GetChatContactsAsync(_currentUser.Id);
            Contacts = new ObservableCollection<User>(contacts);
        }

        async partial void OnSelectedContactChanged(User? value)
        {
            if (value != null)
            {
                await LoadConversationAsync(value.Id);
            }
            else
            {
                Messages.Clear();
            }
        }

        private async Task LoadConversationAsync(int otherUserId)
        {
            var msgs = await _chatService.GetConversationAsync(_currentUser.Id, otherUserId);
            Messages = new ObservableCollection<ChatMessage>(msgs);
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (SelectedContact == null || string.IsNullOrWhiteSpace(NewMessageText)) return;

            var msg = new ChatMessage
            {
                SenderId = _currentUser.Id,
                ReceiverId = SelectedContact.Id,
                Content = NewMessageText,
                Timestamp = DateTime.Now
            };

            await _chatService.SendMessageAsync(msg);
            
            // Optimistic update
            Messages.Add(msg);
            NewMessageText = string.Empty;
        }

        [RelayCommand]
        private void GoBack()
        {
            _mainViewModel.CurrentView = _returnViewModel;
        }
    }
}
