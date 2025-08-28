namespace SimpleProject.Domain.Entities
{
    public class Collar : Entity
    {
        public int? PetId { get; set; }                
        public int QrCodeId { get; set; }             
        public string? SerialNumber { get; set; }     
        public bool IsActive { get; set; }

        public int? SubjectId { get; set; }               
        public string AssetType { get; set; }             
        public string? FriendlyName { get; set; }

        public Pet Pet { get; set; }
        public QrCode QrCode { get; set; }
        public Subject? Subject { get; set; }
    }
}
