namespace SimpleProject.Domain.Entities
{
    public class AppUser : Entity
    {
        public string Name { get; set; }       
        public string? Surname { get; set; }       
        public string? Email { get; set; }    
        public string? Phone { get; set; }      
        public string? Password { get; set; }     
        public int Status { get; set; }          

        // nav
        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
        public ICollection<LostReport> LostReports { get; set; } = new List<LostReport>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
