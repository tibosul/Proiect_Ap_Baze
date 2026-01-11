using System.ComponentModel.DataAnnotations;

namespace VolunteerSystem.Core.Entities
{
    public abstract class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;


    }
}
