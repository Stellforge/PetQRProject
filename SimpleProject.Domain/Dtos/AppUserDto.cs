namespace SimpleProject.Domain.Dtos
{
    public class AppUserDto : EntityDto
    {
        public string Name { get; set; } = null!;
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public int Status { get; set; }

        public ICollection<int> PetIds { get; set; } = new List<int>();
        public ICollection<int> LostReportIds { get; set; } = new List<int>();
        public ICollection<int> NotificationIds { get; set; } = new List<int>();
    }
}
