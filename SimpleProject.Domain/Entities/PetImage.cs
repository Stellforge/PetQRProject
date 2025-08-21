namespace SimpleProject.Domain.Entities
{
    public class PetImage : Entity
    {
        public int PetId { get; set; }       
        public string Url { get; set; }       
        public bool IsPrimary { get; set; } 

        public Pet Pet { get; set; }
    }
}
