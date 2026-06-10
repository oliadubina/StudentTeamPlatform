using StudentTeamPlatform.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.DTO
{
    public class UpdateUserProfileDTO
    {
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
        [Required]
        public WorkFormat? WorkFormat { get; set; }
        [Required]
        public string PreferredLanguage { get; set; } = string.Empty;
    }
}
