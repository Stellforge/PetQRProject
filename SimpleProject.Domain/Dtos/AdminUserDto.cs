using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Dtos;

public class AdminUserDto : EntityDto
{
    [Display(Name = "Rol id"), Required]
    public int AdminRoleId { get; set; }

    [Display(Name = "Rol ad")]
    public string? AdminRoleName { get; set; }

    [Display(Name = "Kullanıcı ad"), Required, MaxLength(500)]
    public string? UserName { get; set; }

    [Display(Name = "Şifre"), Required, MaxLength(50)]
    public string? Password { get; set; }

    [Display(Name = "Ad"), Required, MaxLength(500)]
    public string? Name { get; set; }

    [Display(Name = "Soyad"), Required, MaxLength(500)]
    public string? Surname { get; set; }

    [Display(Name = "E-posta"), Required, EmailAddress, MaxLength(500)]
    public string? Email { get; set; }

    [Display(Name = "Durum"), Required]
    public Status Status { get; set; }

    [Display(Name = "Rol")]
    public AdminRoleDto? AdminRole { get; set; }

    public AdminUserDto() : base()
    {
        Status = Status.ACTIVE;
    }

    public static implicit operator AdminUser(AdminUserDto data) => new()
    {
        Id = data.Id,
        AdminRoleId = data.AdminRoleId,
        UserName = data.UserName,
        Password = data.Password,
        Name = data.Name,
        Surname = data.Surname,
        Email = data.Email,
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate
    };

    public static explicit operator AdminUserDto(AdminUser data) => new()
    {
        Id = data.Id,
        AdminRoleId = data.AdminRoleId,
        AdminRoleName = data.AdminRole?.Name,
        UserName = data.UserName,
        Password = data.Password,
        Name = data.Name,
        Surname = data.Surname,
        Email = data.Email,
        Status = data.Status,
        CreateDate = data.CreateDate,
        UpdateDate = data.UpdateDate,
        AdminRole = data.AdminRole != null ? (AdminRoleDto)data.AdminRole : null
    };
}
