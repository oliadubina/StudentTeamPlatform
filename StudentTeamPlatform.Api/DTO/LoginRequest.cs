using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.DTO
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }=string.Empty;
        [Required]
        [StringLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
