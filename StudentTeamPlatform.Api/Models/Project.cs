namespace StudentTeamPlatform.Api.Models
{ 
    public enum ProjectState
    {
        SearchTeam = 0,
        InProcess =  1,
        Done = 2,
        Archieved= 3
    }
    public enum ProjectType
    {
        CourseWork, 
        Hackathon,  
        Startup
    }
    public enum WorkFormat
    {
        Online,
        Offline,
        Hybrid
    }
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }=string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Technology> Technologies { get; set; } = new List<Technology>();
        public int MaxContributors { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ProjectState ProjectState { get; set; }
        public ProjectType ProjectType { get; set; }
        public WorkFormat WorkFormat {  get; set; }
        public string Language { get; set; } = string.Empty;
        public int AuthorId {  get; set; }
        public User Author { get; set; } = new User();
        public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
        public ICollection<User> Contributors { get; set; } = new List<User>();
        
    }
}
