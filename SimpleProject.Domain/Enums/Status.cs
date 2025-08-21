using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Enums;

public enum Status
{
    [Display(Name = "Pasif")]
    PASSIVE,

    [Display(Name = "Aktif")]
    ACTIVE
}
