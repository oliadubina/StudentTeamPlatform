namespace StudentTeamPlatform.Api.DTO
{
    public class SkillDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        // Тут ми НЕ повертаємо об'єкт User, тому циклу не буде!
    }
}