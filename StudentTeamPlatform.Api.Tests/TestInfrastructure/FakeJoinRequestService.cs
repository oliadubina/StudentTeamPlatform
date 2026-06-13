using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
using StudentTeamPlatform.Api.Services;

namespace StudentTeamPlatform.Api.Tests.TestInfrastructure;

public class FakeJoinRequestService : IJoinRequestService
{
    public ICollection<JoinRequestDTO> ProjectRequests { get; set; } = new List<JoinRequestDTO>();
    public List<StudentSpaceRequestDTO> SubmittedRequests { get; set; } = new();
    public bool ApplyResult { get; set; } = true;
    public bool RespondResult { get; set; } = true;
    public bool CancelResult { get; set; } = true;

    public int? LastProjectId { get; private set; }
    public int? LastStudentId { get; private set; }
    public int? LastRoleId { get; private set; }
    public RequestStatus? LastStatus { get; private set; }

    public Task<bool> ApplyForProjectAsync(int projectId, int studentId, int roleId)
    {
        LastProjectId = projectId;
        LastStudentId = studentId;
        LastRoleId = roleId;
        return Task.FromResult(ApplyResult);
    }

    public Task<ICollection<JoinRequestDTO>> GetProjectRequestsAsync(int projectId, int authorId)
    {
        LastProjectId = projectId;
        LastStudentId = authorId;
        return Task.FromResult(ProjectRequests);
    }

    public Task<bool> RespondToRequestAsync(int requestId, int authorId, RequestStatus status)
    {
        LastStudentId = authorId;
        LastStatus = status;
        return Task.FromResult(RespondResult);
    }

    public Task<List<StudentSpaceRequestDTO>> GetMySubmittedRequestsAsync(int studentId)
    {
        LastStudentId = studentId;
        return Task.FromResult(SubmittedRequests);
    }

    public Task<bool> CancelRequestAsync(int requestId, int studentId)
    {
        LastStudentId = studentId;
        return Task.FromResult(CancelResult);
    }
}
