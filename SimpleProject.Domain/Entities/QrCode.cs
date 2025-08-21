namespace SimpleProject.Domain.Entities
{
    public class QrCode : Entity
    {
        public string Code { get; set; }
        public string? Secret { get; set; }
        public bool IsActive { get; set; }
    }
}