using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Dtos;

public class BrandDto : EntityDto
{
    [Display(Name = "Kod"), MaxLength(50)]
    public string? Code { get; set; }

    [Display(Name = "Ad"), Required, MaxLength(500)]
    public string? Name { get; set; }

    [Display(Name = "Görsel"), MaxLength(500)]
    public string? Image { get; set; }

    [Display(Name = "Görsel (küçük)"), MaxLength(500)]
    public string? Thumbnail { get; set; }

    [Display(Name = "Sıra"), Required]
    public int DisplayOrder { get; set; }

    [Display(Name = "Durum"), Required]
    public Status Status { get; set; }

    public BrandDto() : base()
    {
        Status = Status.ACTIVE;
    }
    public static implicit operator Brand(BrandDto data) => new()
    {
        Id = data.Id,
        Code = data.Code,
        Name = data.Name,
        Image = data.Image,
        Thumbnail = data.Thumbnail,
        DisplayOrder = data.DisplayOrder,
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };

    public static explicit operator BrandDto(Brand data) => new()
    {
        Id = data.Id,
        Code = data.Code,
        Name = data.Name,
        Image = data.Image,
        Thumbnail = data.Thumbnail,
        DisplayOrder = data.DisplayOrder,
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };
}
