namespace SimpleProject.Domain.Entities
{
    public class LostReport : Entity
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


        public Pet Pet { get; set; }
        public AppUser Owner { get; set; }
        public ICollection<FoundReport> FoundReports { get; set; } = new List<FoundReport>();
        public ICollection<ScanEvent> ScanEvents { get; set; } = new List<ScanEvent>();
    }
}
