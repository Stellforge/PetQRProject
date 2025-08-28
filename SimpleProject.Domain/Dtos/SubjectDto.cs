
namespace SimpleProject.Domain.Dtos
{
    public class SubjectDto : EntityDto
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string? Notes { get; set; }
        public string? FotoUrl { get; set; }
    }
}
