using System.Collections.Generic;
using System.Threading.Tasks;
using VolunteerSystem.Core.Entities;

namespace VolunteerSystem.Core.Interfaces
{
    public interface IChatService
    {
        Task SendMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetConversationAsync(int userId1, int userId2);
        Task<List<User>> GetChatContactsAsync(int userId);
    }
}
