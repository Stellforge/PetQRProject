namespace SimpleProject.Domain.Entities
{
    public class CodeAssignment : Entity
    {
        public int BatchId { get; set; }          
        public int CollarId { get; set; }         
        public DateTime AssignedAt { get; set; }  

        public CodeBatch Batch { get; set; }
        public Collar Collar { get; set; }
    }
}
