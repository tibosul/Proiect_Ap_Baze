using System;

namespace VolunteerSystem.Core.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }
        
        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public int ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
