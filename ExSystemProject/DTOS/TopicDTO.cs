namespace ExSystemProject.DTOS
{
    public class TopicDTO
    {
        public int? TopicId { get; set; }
        public string? TopicName { get; set; }
        public string? Description { get; set; }
        public int? CrsId { get; set; }
        public bool? IsActive { get; set; } = true;
        public string? CourseName { get; set; }
    }
}
