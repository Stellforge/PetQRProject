namespace SimpleProject.Domain.Entities
{
    public class ScanEvent : Entity
    {
        public int QrCodeId { get; set; }
        public int? PetId { get; set; }          
        public int LostReportId { get; set; }   
        public int ResultType { get; set; }        
        public string? IpAddress { get; set; }      
        public string? UserAgent { get; set; }        
        public decimal? ScanLat { get; set; }          
        public decimal? ScanLng { get; set; }       
        public string? Notes { get; set; }            
        public string? FinderPhone { get; set; }    
        public string? FinderPhoto { get; set; }      

   
        public QrCode QrCode { get; set; }
        public Pet? Pet { get; set; }
        public LostReport LostReport { get; set; }
    }
}
