using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Models;
namespace StudentTeamPlatform.Api.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<User>Users { get; set; }
        public DbSet<Skill>Skills { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Technology> Technologies { get; set; }
        public DbSet<ProjectRole> ProjectRoles { get; set; }
        public DbSet<JoinRequest> JoinRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(user => user.Role).HasConversion<string>();
            modelBuilder.Entity<Project>().Property(project => project.ProjectState).HasConversion<string>();
            modelBuilder.Entity<User>().Property(user => user.WorkFormat).HasConversion<string>();
            modelBuilder.Entity<Project>().Property(p => p.WorkFormat).HasConversion<string>();
            modelBuilder.Entity<Project>().Property(p => p.ProjectType).HasConversion<string>();
            modelBuilder.Entity<Project>().HasOne(p=>p.Author).WithMany(p=>p.CreatedProjects).HasForeignKey(p=>p.AuthorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Project>().HasMany(p => p.Contributors).WithMany(u => u.Projects);
            modelBuilder.Entity<JoinRequest>().Property(j=>j.Status).HasConversion<string>();
        }
    }
}
