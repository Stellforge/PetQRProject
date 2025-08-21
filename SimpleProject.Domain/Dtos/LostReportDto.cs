namespace SimpleProject.Domain.Dtos
{
    public class LostReportDto : EntityDto
    {
        public int PetId { get; set; }
        public int OwnerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LostDateTime { get; set; }
        public decimal? LostLat { get; set; }
        public decimal? LostLng { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public DateTime? ResolvedDate { get; set; }

        public ICollection<int> FoundReportIds { get; set; } = new List<int>();
        public ICollection<int> ScanEventIds { get; set; } = new List<int>();
    }
}
