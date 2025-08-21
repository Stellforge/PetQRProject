
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Dtos;

public class EntityDto
{
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Display(Name = "Oluşturulma tarihi")]
    public DateTime CreateDate { get; set; }

    [Display(Name = "Güncellenme tarihi")]
    public DateTime UpdateDate { get; set; }

    public bool Deleted { get; set; }

    public EntityDto()
    {
        CreateDate = DateTime.UtcNow;
        UpdateDate = DateTime.UtcNow;
    }
}
