namespace SimpleProject.Domain.Dtos
{
    public class QrCodeDto : EntityDto
    {
        public string Code { get; set; } = null!;
        public string? Secret { get; set; }
        public bool IsActive { get; set; }
    }
}
