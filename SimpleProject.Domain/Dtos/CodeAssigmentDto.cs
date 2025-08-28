namespace SimpleProject.Domain.Dtos
{
    public class CodeAssignmentDto : EntityDto
    {
        public int BatchId { get; set; }
        public int CollarId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
