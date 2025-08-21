using SimpleProject.Domain.Validations;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Dtos.Admin;
public class ResetPasswordRequest
{
    [Display(Name = "Kullanıcı"), Required]
    public int UserId { get; set; }

    [Display(Name = "Şifre"), Required, MaxLength(50), Password]
    public string? Password { get; set; }

    [Display(Name = "Token"), Required, MaxLength(500)]
    public string? UToken { get; set; }
}
