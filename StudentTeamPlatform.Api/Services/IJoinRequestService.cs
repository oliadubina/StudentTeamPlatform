using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.Services
{
    public interface IJoinRequestService
    {
        Task<bool> ApplyForProjectAsync(int projectId, int studentId);
        Task<ICollection<JoinRequestDTO>> GetProjectRequestsAsync(int projectId, int authorId);
        Task<bool> RespondToRequestAsync(int requestId, int authorId, RequestStatus status);
        Task<List<StudentSpaceRequestDTO>> GetMySubmittedRequestsAsync(int studentId);
        Task<bool> CancelRequestAsync(int requestId, int studentId);
    }
}
