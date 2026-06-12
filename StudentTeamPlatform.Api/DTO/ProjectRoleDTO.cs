using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.DTO
{
    public class ProjectRoleDTO
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 10, ErrorMessage = "Кількість місць на одну роль має бути від 1 до 10")]
        public int SlotsCount { get; set; }
    }
}
