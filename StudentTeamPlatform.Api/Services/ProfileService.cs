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
            var reviews = await _appDBContext.ProjectReviews
                .AsNoTracking()
                .Include(review => review.Project)
                .Include(review => review.Reviewer)
                .Where(review => review.RevieweeId == userId)
                .OrderByDescending(review => review.CreatedAt)
                .Select(review => new UserReviewDTO
                {
                    Id = review.Id,
                    ProjectId = review.ProjectId,
                    ProjectTitle = review.Project.Title,
                    ReviewerId = review.ReviewerId,
                    ReviewerName = review.Reviewer.FullName,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                })
                .ToListAsync();
            UserProfileResponseDTO profileResponseDTO = new UserProfileResponseDTO()
            {
                EmailAddress=user.Email,
                FullName=user.FullName,
                University=user.University,
                Speciality=user.Speciality,
                Course=user.Course,
                Skills=skills,
                PreferredLanguage=user.PreferredLanguage,
                WorkFormat=user.WorkFormat,
                Reviews = reviews,
                ReviewsCount = reviews.Count,
                AverageRating = reviews.Count > 0 ? Math.Round(reviews.Average(review => review.Rating), 1) : 0

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
