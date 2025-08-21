namespace SimpleProject.Domain.Dtos
{
    public class PetImageDto : EntityDto
    {
        public int PetId { get; set; }
        public string Url { get; set; } = null!;
        public bool IsPrimary { get; set; }
    }
}
