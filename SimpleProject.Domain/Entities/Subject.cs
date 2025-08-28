namespace SimpleProject.Domain.Entities
{
    public class Subject : Entity
    {
        public string Type { get; set; }      
        public string Name { get; set; }      
        public string? Notes { get; set; }    
        public string? FotoUrl { get; set; }  

        public ICollection<Collar> Collars { get; set; } = new List<Collar>();
    }
}
