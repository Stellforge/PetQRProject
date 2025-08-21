
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Dtos.Admin;

public class LoginRequest
{
    [Display(Name = "UserName"), Required]
    public string UserName { get; set; } = "";

    [Display(Name = "Password"), Required]
    public string Password { get; set; } = "";

    [Display(Name = "ReturnUrl")]
    public string? ReturnUrl { get; set; }
}
