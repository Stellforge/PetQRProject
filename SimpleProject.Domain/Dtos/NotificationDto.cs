namespace SimpleProject.Domain.Dtos
{
    public class NotificationDto : EntityDto
    {
        public int UserId { get; set; }
        public string Type { get; set; } = null!;
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Payload { get; set; }
        public bool IsRead { get; set; }
        public DateTime? SentDate { get; set; }
    }
}
