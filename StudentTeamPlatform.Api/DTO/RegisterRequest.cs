using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.DTO
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        [Required]
        [StringLength(8)]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; }= string.Empty;
        
        

    }
}
