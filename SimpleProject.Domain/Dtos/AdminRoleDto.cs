using SimpleProject.Domain.Dtos.Settings;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SimpleProject.Domain.Dtos;

public class AdminRoleDto : EntityDto
{
    [Display(Name = "Kod"), MaxLength(50)]
    public string? Code { get; set; }

    [Display(Name = "Ad"), Required, MaxLength(500)]
    public string? Name { get; set; }

    [Display(Name = "Ayarlar")]
    public RoleSettings? Settings { get; set; }

    [Display(Name = "Ayarlar (Raw)")]
    public string? SettingsRaw { get; set; }

    [Display(Name = "Durum"), Required]
    public Status Status { get; set; }


    public AdminRoleDto() : base()
    {
        Status = Status.ACTIVE;
    }

    public static implicit operator AdminRole(AdminRoleDto data) => new()
    {
        Id = data.Id,
        Code = data.Code,
        Name = data.Name,
        Settings = data.Settings != null ? JsonSerializer.Serialize(data.Settings, JsonOptions.Default) : data.SettingsRaw,
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };

    public static explicit operator AdminRoleDto(AdminRole data) => new()
    {
        Id = data.Id,
        Code = data.Code,
        Name = data.Name,
        SettingsRaw = data.Settings,
        Settings = string.IsNullOrEmpty(data.Settings) ? null : JsonSerializer.Deserialize<RoleSettings>(data.Settings, JsonOptions.Default),
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };
}
