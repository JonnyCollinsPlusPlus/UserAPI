using System.ComponentModel.DataAnnotations;

namespace UserAPI.DTOs
{
    public class UpdateRoleDTO
    {
        [Required]
        [RegularExpression("^(User|Admin|Tester)$", ErrorMessage = "Role must be User, Admin, or Tester.")]
        public string Role { get; set; } = string.Empty;
    }
}
