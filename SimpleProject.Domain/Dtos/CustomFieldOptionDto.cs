using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Dtos;

public class CustomFieldOptionDto : EntityDto
{
    [Display(Name = "Özel alan id")]
    public int? CustomFieldId { get; set; }

    [Display(Name = "Özel alan kod"), MaxLength(50)]
    public string? CustomFieldCode { get; set; }

    [Display(Name = "Özel alan ad"), MaxLength(500)]
    public string? CustomFieldName { get; set; }

    [Display(Name = "Kod"), MaxLength(50)]
    public string? Code { get; set; }

    [Display(Name = "Ad"), Required, MaxLength(500)]
    public string? Name { get; set; }

    [Display(Name = "Açýklama")]
    public string? Description { get; set; }

    [Display(Name = "Sýra"), Required]
    public int DisplayOrder { get; set; }

    [Display(Name = "Durum"), Required]
    public Status Status { get; set; }

    public CustomFieldOptionDto() : base()
    {
        Status = Status.ACTIVE;
    }

    public static implicit operator CustomFieldOption(CustomFieldOptionDto data) => new()
    {
        Id = data.Id,
        CustomFieldId = data.CustomFieldId.GetValueOrDefault(),
        Code = data.Code,
        Name = data.Name,
        Description = data.Description,
        DisplayOrder = data.DisplayOrder,
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };

    public static explicit operator CustomFieldOptionDto(CustomFieldOption data) => new()
    {
        Id = data.Id,
        CustomFieldId = data.CustomFieldId,
        CustomFieldCode = data.CustomField?.Code,
        CustomFieldName = data.CustomField?.Name,
        Code = data.Code,
        Name = data.Name,
        Description = data.Description,
        DisplayOrder = data.DisplayOrder,
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };
}