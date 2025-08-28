namespace SimpleProject.Domain.Entities
{
    public class CodeBatch : Entity
    {
        public int DealerId { get; set; }        
        public string BatchCode { get; set; }    
        public int Quantity { get; set; }        
        public DateTime CreatedAt { get; set; }  

        public Dealer Dealer { get; set; }
        public ICollection<CodeAssignment> Assignments { get; set; } = new List<CodeAssignment>();
    }
}
