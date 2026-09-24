using System.ComponentModel.DataAnnotations;

namespace UserAPI.DTOs
{
    public class LoginDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = "User";
    }
}
