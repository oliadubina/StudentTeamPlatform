using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.DTO
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(8, ErrorMessage = "Пароль має містити мінімум 8 символів")]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; }= string.Empty;
        
        

    }
}
