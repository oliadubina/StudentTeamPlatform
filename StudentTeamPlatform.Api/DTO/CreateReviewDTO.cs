using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.DTO
{
    public class CreateReviewDTO
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int RevieweeId { get; set; }

        [Range(1, 5, ErrorMessage = "Оцінка має бути від 1 до 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Коментар не може бути довшим за 1000 символів")]
        public string Comment { get; set; } = string.Empty;
    }
}
