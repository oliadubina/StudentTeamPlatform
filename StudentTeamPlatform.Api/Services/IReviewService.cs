using StudentTeamPlatform.Api.DTO;

namespace StudentTeamPlatform.Api.Services
{
    public interface IReviewService
    {
        Task<List<PendingReviewUserDTO>> GetPendingReviewsAsync(int projectId, int reviewerId);
        Task<List<ReviewResponseDTO>> GetProjectReviewsAsync(int projectId, int currentUserId);
        Task<bool> CreateReviewAsync(CreateReviewDTO createReviewDTO, int reviewerId);
    }
}
