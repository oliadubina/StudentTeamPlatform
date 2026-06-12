using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Data;
using StudentTeamPlatform.Api.DTO;
using StudentTeamPlatform.Api.Models;

namespace StudentTeamPlatform.Api.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _appDbContext;

        public ReviewService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<PendingReviewUserDTO>> GetPendingReviewsAsync(int projectId, int reviewerId)
        {
            var project = await _appDbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(project => project.Id == projectId);

            if (project == null || project.ProjectState != ProjectState.Done)
            {
                return new List<PendingReviewUserDTO>();
            }

            var isReviewerContributor = await _appDbContext.Projects
                .AsNoTracking()
                .AnyAsync(project =>
                    project.Id == projectId &&
                    project.Contributors.Any(contributor => contributor.Id == reviewerId));

            if (project.AuthorId != reviewerId && !isReviewerContributor)
            {
                return new List<PendingReviewUserDTO>();
            }

            var reviewedUserIds = await _appDbContext.ProjectReviews
                .AsNoTracking()
                .Where(review => review.ProjectId == projectId && review.ReviewerId == reviewerId)
                .Select(review => review.RevieweeId)
                .ToListAsync();

            var contributorUsers = await _appDbContext.Users
                .AsNoTracking()
                .Where(user => user.Projects.Any(project => project.Id == projectId))
                .Select(user => new PendingReviewUserDTO
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email
                })
                .ToListAsync();

            var authorUser = await _appDbContext.Users
                .AsNoTracking()
                .Where(user => user.Id == project.AuthorId)
                .Select(user => new PendingReviewUserDTO
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email
                })
                .FirstOrDefaultAsync();

            var projectMembers = contributorUsers;
            if (authorUser != null)
            {
                projectMembers.Add(authorUser);
            }

            return projectMembers
                .GroupBy(user => user.UserId)
                .Select(group => group.First())
                .Where(user => user.UserId != reviewerId && !reviewedUserIds.Contains(user.UserId))
                .ToList();
        }

        public async Task<List<ReviewResponseDTO>> GetProjectReviewsAsync(int projectId, int currentUserId)
        {
            var project = await _appDbContext.Projects
                .Include(project => project.Contributors)
                .FirstOrDefaultAsync(project => project.Id == projectId);

            if (project == null || !IsProjectMember(project, currentUserId))
            {
                return new List<ReviewResponseDTO>();
            }

            return await _appDbContext.ProjectReviews
                .Include(review => review.Project)
                .Include(review => review.Reviewer)
                .Include(review => review.Reviewee)
                .Where(review => review.ProjectId == projectId)
                .OrderByDescending(review => review.CreatedAt)
                .Select(review => new ReviewResponseDTO
                {
                    Id = review.Id,
                    ProjectId = review.ProjectId,
                    ProjectTitle = review.Project.Title,
                    ReviewerId = review.ReviewerId,
                    ReviewerName = review.Reviewer.FullName,
                    RevieweeId = review.RevieweeId,
                    RevieweeName = review.Reviewee.FullName,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> CreateReviewAsync(CreateReviewDTO createReviewDTO, int reviewerId)
        {
            if (createReviewDTO == null || reviewerId == 0 || createReviewDTO.RevieweeId == reviewerId)
            {
                return false;
            }

            var project = await _appDbContext.Projects
                .Include(project => project.Author)
                .Include(project => project.Contributors)
                .FirstOrDefaultAsync(project => project.Id == createReviewDTO.ProjectId);

            if (project == null || project.ProjectState != ProjectState.Done)
            {
                return false;
            }

            if (!IsProjectMember(project, reviewerId) || !IsProjectMember(project, createReviewDTO.RevieweeId))
            {
                return false;
            }

            var reviewExists = await _appDbContext.ProjectReviews.AnyAsync(review =>
                review.ProjectId == createReviewDTO.ProjectId &&
                review.ReviewerId == reviewerId &&
                review.RevieweeId == createReviewDTO.RevieweeId);

            if (reviewExists)
            {
                return false;
            }

            var review = new ProjectReview
            {
                ProjectId = createReviewDTO.ProjectId,
                ReviewerId = reviewerId,
                RevieweeId = createReviewDTO.RevieweeId,
                Rating = createReviewDTO.Rating,
                Comment = (createReviewDTO.Comment ?? string.Empty).Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _appDbContext.ProjectReviews.AddAsync(review);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        private static bool IsProjectMember(Project project, int userId)
        {
            return project.AuthorId == userId || project.Contributors.Any(contributor => contributor.Id == userId);
        }

        private static List<User> GetProjectMembers(Project project)
        {
            return new[] { project.Author }
                .Concat(project.Contributors)
                .GroupBy(user => user.Id)
                .Select(group => group.First())
                .ToList();
        }
    }
}
