using SimpleProject.Domain.Enums;

namespace SimpleProject.Domain.Entities;

public class CustomFieldOption : Entity
{
    public int CustomFieldId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public Status Status { get; set; }

    public CustomField? CustomField { get; set; }

    public List<CustomFieldValue>? CustomFieldValues { get; set; }

    public CustomFieldOption() : base()
    {
        Status = Status.ACTIVE;
    }
}