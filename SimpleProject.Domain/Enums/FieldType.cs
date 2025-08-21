using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Enums;

public enum FieldType
{
    [Display(Name = "Unknown")]
    UNKNOWN,

    [Display(Name = "String")]
    STRING,

    [Display(Name = "Int")]
    INT,

    [Display(Name = "Decimal")]
    DECIMAL,

    [Display(Name = "Bool")]
    BOOL,

    [Display(Name = "Datetime")]
    DATETIME,

    [Display(Name = "Date")]
    DATE,

    [Display(Name = "FieldSelect")]
    SELECT,

    [Display(Name = "Textarea")]
    TEXTAREA
}