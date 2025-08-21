namespace SimpleProject.Domain.Entities;

public class CustomFieldValue : Entity
{
    public int TableId { get; set; }
    public int CustomFieldId { get; set; }
    public int? CustomFieldOptionId { get; set; }
    public string? Value { get; set; }

    //public Product? Product { get; set; }
    public CustomField? CustomField { get; set; }
    public CustomFieldOption? CustomFieldOption { get; set; }

    public CustomFieldValue() : base()
    {
    }
}