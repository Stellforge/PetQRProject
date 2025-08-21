using SimpleProject.Domain.Enums;

namespace SimpleProject.Domain.Entities;

public class CustomField : Entity
{
    public string? TableName { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public FieldType FieldType { get; set; }
    public bool Required { get; set; }
    public int DisplayOrder { get; set; }
    public Status Status { get; set; }

    public List<CustomFieldOption>? CustomFieldOptions { get; set; }
    public List<CustomFieldValue>? CustomFieldValues { get; set; }

    public CustomField() : base()
    {
        Status = Status.ACTIVE;
    }
}