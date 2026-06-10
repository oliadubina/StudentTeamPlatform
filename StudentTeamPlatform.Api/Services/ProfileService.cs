using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Data;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;
namespace StudentTeamPlatform.Api.Services
{
    public class ProfileService: IProfileService
    {
        private readonly AppDbContext _appDBContext;
        public ProfileService(AppDbContext appDbContext)
        {
            _appDBContext = appDbContext;
        }
        public async Task<UserProfileResponseDTO>GetUserProfileAsync(int userId)
        {
            var user =  await _appDBContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) { return null; }
            UserProfileResponseDTO profileResponseDTO = new UserProfileResponseDTO()
            {
                EmailAddress=user.Email,
                FullName=user.FullName,
                University=user.University,
                Speciality=user.Speciality,
                Course=user.Course,
                Skills=user.Skills,
                PreferredLanguage=user.PreferredLanguage,
                WorkFormat=user.WorkFormat

            };
            return profileResponseDTO;
        }
        public async Task<bool> UpdateProfileAsync(UpdateUserProfileDTO updateUserProfile, int id)
        {
            if(updateUserProfile == null || id ==0) {return false;}
            var user = await _appDBContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) { return false;}
                user.FullName=updateUserProfile.FullName;
                user.Course=updateUserProfile.Course;
                user.University= updateUserProfile.University;
                user.Speciality = updateUserProfile.Speciality;
                user.Skills=updateUserProfile.Skills;   
                user.WorkFormat=updateUserProfile.WorkFormat;
                user.PreferredLanguage=updateUserProfile.PreferredLanguage;
            await _appDBContext.SaveChangesAsync();
            return true;
            
        }
    }
}
