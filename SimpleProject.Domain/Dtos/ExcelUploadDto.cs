using SimpleProject.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Dtos;

public class ExcelUploadDto : EntityDto
{
    [Display(Name = "Kullanıcı id"), Required]
    public int AdminUserId { get; set; }

    [Display(Name = "Ad"), Required, MaxLength(500)]
    public string? Name { get; set; }

    [Display(Name = "Tip"), Required, MaxLength(500)]
    public string? UploadType { get; set; }

    [Display(Name = "Interface"), Required, MaxLength(500)]
    public string? InterfaceName { get; set; }

    [Display(Name = "Metod"), Required, MaxLength(500)]
    public string? Method { get; set; }

    [Display(Name = "Ekstra parametere"), MaxLength(500)]
    public string? ExtraParameter { get; set; }

    [Display(Name = "Toplam"), Required]
    public int Total { get; set; }

    [Display(Name = "Başarılı"), Required]
    public int Success { get; set; }

    [Display(Name = "Hatalı"), Required]
    public int Fail { get; set; }

    [Display(Name = "Dosya yolu"), Required, MaxLength(500)]
    public string? FilePath { get; set; }

    [Display(Name = "Hatalı dosya yolu"), Required, MaxLength(500)]
    public string? ErrorFilePath { get; set; }

    [Display(Name = "Hata mesajı"), MaxLength(500)]
    public string? ErrorMessage { get; set; }

    [Display(Name = "Tamamlandı"), Required]
    public bool Completed { get; set; }

    public ExcelUploadDto() : base()
    {
    }

    public static implicit operator ExcelUpload(ExcelUploadDto data) => new()
    {
        Id = data.Id,
        AdminUserId = data.AdminUserId,
        ErrorFilePath = data.ErrorFilePath,
        ErrorMessage = data.ErrorMessage,
        Fail = data.Fail,
        FilePath = data.FilePath,
        InterfaceName = data.InterfaceName,
        Completed = data.Completed,
        Method = data.Method,
        ExtraParameter = data.ExtraParameter,
        Name = data.Name,
        Success = data.Success,
        Total = data.Total,
        UploadType = data.UploadType,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };


    public static explicit operator ExcelUploadDto(ExcelUpload data) => new()
    {
        Id = data.Id,
        AdminUserId = data.AdminUserId,
        ErrorFilePath = data.ErrorFilePath,
        ErrorMessage = data.ErrorMessage,
        Fail = data.Fail,
        FilePath = data.FilePath,
        InterfaceName = data.InterfaceName,
        Completed = data.Completed,
        Method = data.Method,
        ExtraParameter = data.ExtraParameter,
        Name = data.Name,
        Success = data.Success,
        Total = data.Total,
        UploadType = data.UploadType,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };
}

