namespace SimpleProject.Domain.Dtos
{
    public class PetDto : EntityDto 
    {
        public int OwnerId { get; set; }
        public string Name { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string? Breed { get; set; }
        public string? Color { get; set; }
        public string? Sex { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? PrimaryImage { get; set; }
        public int Status { get; set; }

        public ICollection<int> ImageIds { get; set; } = new List<int>();
        public ICollection<int> CollarIds { get; set; } = new List<int>();
        public ICollection<int> LostReportIds { get; set; } = new List<int>();
        public ICollection<int> FoundReportIds { get; set; } = new List<int>();
        public ICollection<int> ScanEventIds { get; set; } = new List<int>();
    }
}
