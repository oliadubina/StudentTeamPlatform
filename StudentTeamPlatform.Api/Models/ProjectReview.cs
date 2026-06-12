using System.ComponentModel.DataAnnotations;

namespace StudentTeamPlatform.Api.Models
{
    public class ProjectReview
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int ReviewerId { get; set; }
        public User Reviewer { get; set; } = null!;

        public int RevieweeId { get; set; }
        public User Reviewee { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
