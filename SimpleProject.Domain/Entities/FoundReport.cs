namespace SimpleProject.Domain.Entities
{
    public class FoundReport : Entity
    {
        public int PetId { get; set; }               
        public int LostReportId { get; set; }          
        public string? FinderName { get; set; }       
        public string? FinderPhone { get; set; }     
        public string? Message { get; set; }           
        public decimal? FoundLat { get; set; }         
        public decimal? FoundLng { get; set; }       
        public DateTime? FoundDateTime { get; set; }  
        public string? FinderPhoto { get; set; }     
        public bool IsContactShared { get; set; }   

        public Pet Pet { get; set; }
        public LostReport LostReport { get; set; }
    }
}
