using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Enums;
public enum LogType
{
    [Display(Name = "Insert")]
    INSERT,

    [Display(Name = "Update")]
    UPDATE,

    [Display(Name = "Delete")]
    DELETE
}
