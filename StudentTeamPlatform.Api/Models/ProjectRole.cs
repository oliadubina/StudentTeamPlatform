namespace StudentTeamPlatform.Api.Models
{
    public class ProjectRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId {  get; set; }
        public int SlotsCount {  get; set; }
    }
}
