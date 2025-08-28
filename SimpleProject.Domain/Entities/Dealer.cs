namespace SimpleProject.Domain.Entities
{
    public class Dealer : Entity
    {
        public string Code { get; set; }        
        public string Name { get; set; }        
        public string? Contact { get; set; }    

        // nav
        public ICollection<CodeBatch> Batches { get; set; } = new List<CodeBatch>();
    }
}
