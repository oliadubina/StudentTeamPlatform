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
            var user =  await _appDBContext.Users.Include(u => u.Skills).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) { return null; }
            var skills=user.Skills.Select(skill=>new SkillDTO
            {
                Id = skill.Id,
                Name = skill.Name,
                Category=skill.Category
            }).ToList();
            UserProfileResponseDTO profileResponseDTO = new UserProfileResponseDTO()
            {
                EmailAddress=user.Email,
                FullName=user.FullName,
                University=user.University,
                Speciality=user.Speciality,
                Course=user.Course,
                Skills=skills,
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
            var skills = updateUserProfile.Skills.Select(skill => new Skill
            {
                Id = skill.Id,
                Name = skill.Name,
                Category=skill.Category
            }).ToList();
            user.FullName=updateUserProfile.FullName;
                user.Course=updateUserProfile.Course;
                user.University= updateUserProfile.University;
                user.Speciality = updateUserProfile.Speciality;
                user.Skills=skills;   
                user.WorkFormat=updateUserProfile.WorkFormat;
                user.PreferredLanguage=updateUserProfile.PreferredLanguage;
            await _appDBContext.SaveChangesAsync();
            return true;
            
        }
    }
}
