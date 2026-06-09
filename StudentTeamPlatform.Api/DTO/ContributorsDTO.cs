namespace StudentTeamPlatform.Api.DTO
{
    public class ContributorsDTO
    {
        public int Id { get; set; }
        public string FullName {  get; set; }= string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProjectRole { get; set; } = string.Empty;
    }
}
