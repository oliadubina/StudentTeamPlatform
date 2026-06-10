namespace StudentTeamPlatform.Api.Models
{   public enum UserRole {
        Student,
        Admin
    }
    public class User
    {
        public int Id { get; set; }
        public string FullName {  get; set; }= string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public string Speciality { get; set; } = string.Empty;
        public int Course { get; set; } = 0;
        public UserRole Role { get; set; } 
        public WorkFormat? WorkFormat { get; set; }
        public string PreferredLanguage { get; set; } = string.Empty;
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
    }
}
