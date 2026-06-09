using StudentTeamPlatform.Api.DTO;
namespace StudentTeamPlatform.Api.Services
{
    public interface IProfileService
    {
        Task<UserProfileResponseDTO> GetUserProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(UpdateUserProfileDTO updateUserProfile, int id);

    }
}
