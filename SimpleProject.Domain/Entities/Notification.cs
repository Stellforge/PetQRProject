namespace SimpleProject.Domain.Entities
{
    public class Notification : Entity
    {
        public int UserId { get; set; }            
        public string Type { get; set; }         
        public string? Title { get; set; }        
        public string? Body { get; set; }    
        public string? Payload { get; set; }   
        public bool IsRead { get; set; }       
        public DateTime? SentDate { get; set; }  

        public AppUser User { get; set; }
    }
}
