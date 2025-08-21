namespace SimpleProject.Domain.Dtos
{
    public class CollarDto : EntityDto
    {
        public int PetId { get; set; }
        public int QrCodeId { get; set; }
        public string? SerialNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
