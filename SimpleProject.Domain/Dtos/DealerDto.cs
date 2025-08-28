namespace SimpleProject.Domain.Dtos
{
    public class DealerDto : EntityDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Contact { get; set; }
    }
}
