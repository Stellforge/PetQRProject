using SimpleProject.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Dtos;

public class CustomFieldValueDto : EntityDto
{
    [Display(Name = "Tablo"), Required]
    public int TableId { get; set; }
    
    [Display(Name = "Özel alan id")]
    public int? CustomFieldId { get; set; }
    
    [Display(Name = "Özel alan kod"), MaxLength(50)]
    public string? CustomFieldCode { get; set; }
    
    [Display(Name = "Özel alan ad"), MaxLength(500)]
    public string? CustomFieldName { get; set; }
    
    [Display(Name = "Özel alan seçenek id")]
    public int? CustomFieldOptionId { get; set; }
    
    [Display(Name = "Özel alan seçenek kod"), MaxLength(50)]
    public string? CustomFieldOptionCode { get; set; }
    
    [Display(Name = "Özel alan seçenek ad"), MaxLength(500)]
    public string? CustomFieldOptionName { get; set; }
    
    [Display(Name = "Value")]
    public string? Value { get; set; }

    public CustomFieldValueDto() : base()
    {

    }

    public static implicit operator CustomFieldValue(CustomFieldValueDto data) => new()
    {
        Id = data.Id,
        TableId = data.TableId,
        CustomFieldId = data.CustomFieldId.GetValueOrDefault(),
        CustomFieldOptionId = data.CustomFieldOptionId,
        Value = data.Value,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate,
    };

    public static explicit operator CustomFieldValueDto(CustomFieldValue data) => new()
    {
        Id = data.Id,
        TableId = data.TableId,
        CustomFieldId = data.CustomFieldId,
        CustomFieldCode = data.CustomField?.Code,
        CustomFieldName = data.CustomField?.Name,
        CustomFieldOptionId = data.CustomFieldOptionId,
        CustomFieldOptionCode = data.CustomFieldOption?.Code,
        CustomFieldOptionName = data.CustomFieldOption?.Name,
        Value = data.Value,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };

    public bool EqualsWith(CustomField other)
    {
        if (CustomFieldId.HasValue)
        {
            return CustomFieldId == other.Id;
        }
        else if (!string.IsNullOrEmpty(CustomFieldCode))
        {
            return CustomFieldCode == other.Code;
        }
        else if (!string.IsNullOrEmpty(CustomFieldName))
        {
            return CustomFieldName == other.Name;
        }

        return false;
    }

    public bool EqualsWith(CustomFieldValue other)
    {
        if (CustomFieldId.HasValue)
        {
            return CustomFieldId == other.CustomFieldId;
        }
        else if (!string.IsNullOrEmpty(CustomFieldCode))
        {
            return CustomFieldCode == other.CustomField?.Code;
        }
        else if (!string.IsNullOrEmpty(CustomFieldName))
        {
            return CustomFieldName == other.CustomField?.Name;
        }
        return false;
    }
}