namespace SimpleProject.Domain.Dtos
{
    public class QrOwnershipDto : EntityDto
    {
        public int CollarId { get; set; }
        public int OwnerUserId { get; set; }
        public DateTime ActivatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
