using StudentTeamPlatform.Api.Models;
using System.ComponentModel.DataAnnotations;
namespace StudentTeamPlatform.Api.DTO
{
    public class UserProfileResponseDTO
    {
        [Required]
        public string EmailAddress { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string University { get; set; } = string.Empty;
        [Required]
        public string Speciality { get; set; } = string.Empty;
        [Required]
        public int Course { get; set; }
        [Required]
        public ICollection<Skill>? Skills { get; set; }


    }
}
