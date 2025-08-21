using System.Drawing;

namespace SimpleProject.Domain.Entities
{
    public class Pet : Entity
    {
        public int OwnerId { get; set; }     
        public string Name { get; set; }        
        public string Species { get; set; }   
        public string? Breed { get; set; }        
        public string? Color { get; set; }       
        public string? Sex { get; set; }       
        public DateTime? BirthDate { get; set; }  
        public string? PrimaryImage { get; set; }  
        public int Status { get; set; }  

        // nav
        public AppUser Owner { get; set; }
        public ICollection<PetImage> Images { get; set; } = new List<PetImage>();
        public ICollection<Collar> Collars { get; set; } = new List<Collar>();
        public ICollection<LostReport> LostReports { get; set; } = new List<LostReport>();
        public ICollection<FoundReport> FoundReports { get; set; } = new List<FoundReport>();
        public ICollection<ScanEvent> ScanEvents { get; set; } = new List<ScanEvent>();
    }
}
