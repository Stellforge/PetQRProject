using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Dtos;

public class CustomFieldDto : EntityDto
{
    [Display(Name = "Tablo"), MaxLength(50)]
    public string? TableName { get; set; }
    
    [Display(Name = "Kod"), MaxLength(50)]
    public string? Code { get; set; }
    
    [Display(Name = "Ad"), Required, MaxLength(500)]
    public string? Name { get; set; }
    
    [Display(Name = "Açýklama")]
    public string? Description { get; set; }
    
    [Display(Name = "Alan tipi"), Required]
    public FieldType FieldType { get; set; }
    
    [Display(Name = "Zorunlu"), Required]
    public bool Required { get; set; }
    
    [Display(Name = "Sýra"), Required]
    public int DisplayOrder { get; set; }
    
    [Display(Name = "Durum"), Required]
    public Status Status { get; set; }

    [Display(Name = "Seçenekler")]
    public List<CustomFieldOptionDto>? CustomFieldOptions { get; set; }

    public CustomFieldDto() : base()
    {
        Status = Status.ACTIVE;
    }
    
    public static implicit operator CustomField(CustomFieldDto data) => new()
    {
        Id = data.Id,
        TableName = data.TableName,
        Code = data.Code,
        Name = data.Name,
        Description = data.Description,
        FieldType = data.FieldType,
        Required = data.Required,
        DisplayOrder = data.DisplayOrder,
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };


    public static explicit operator CustomFieldDto(CustomField data) => new()
    {
        Id = data.Id,
        TableName = data.TableName,
        Code = data.Code,
        Name = data.Name,
        Description = data.Description,
        FieldType = data.FieldType,
        Required = data.Required,
        DisplayOrder = data.DisplayOrder,
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate,
        CustomFieldOptions = data.CustomFieldOptions?.Select(a=> (CustomFieldOptionDto)a).ToList()
    };
}