namespace SimpleProject.Domain.Entities
{
    public class QrOwnership : Entity
    {
        public int CollarId { get; set; }           
        public int OwnerUserId { get; set; }        
        public DateTime ActivatedAt { get; set; }   
        public bool IsActive { get; set; }          

        public Collar Collar { get; set; }
        public AppUser OwnerUser { get; set; }
    }
}
