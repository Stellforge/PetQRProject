namespace SimpleProject.Domain.Dtos
{
    public class CodeBatchDto : EntityDto
    {
        public int DealerId { get; set; }
        public string BatchCode { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
