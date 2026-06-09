using System.Globalization;

namespace StudentTeamPlatform.Api.Models
{
    public class Technology
    {
        public int Id { get; set; }
        public string Name {  get; set; }= string.Empty;
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
